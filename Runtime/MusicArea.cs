using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using JetBrains.Annotations;

namespace JanSharp
{
    #if !AdvancedMusicManager
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    #endif
    public class MusicArea : UdonSharpBehaviour
    {
        #if AdvancedMusicManager
        [Header("The sync mode must either be Manual or None.", order = 0)]
        [Space(16f, order = 1)]
        #endif
        [Tooltip("When false it still keeps track of the player, but music will not be played. "
            + "Generally speaking use this instead of toggling the entire object or toggling this component "
            + "- unless you really, really know what you're doing.")]
        [SerializeField] private bool isActive = true;
        [PublicAPI] public bool IsActive
        {
            get => isActive;
            set
            {
                if (value == isActive)
                    return;
                isActive = value;
                if (IsInArea)
                    if (value)
                        AddMusicToManager();
                    else
                        RemoveMusicFromManager();
                CheckSync();
            }
        }
        [Tooltip("Must never be null.")]
        [SerializeField] private MusicDescriptor musicForThisArea;
        [PublicAPI] public MusicDescriptor MusicForThisArea
        {
            get => musicForThisArea;
            set
            {
                if (value == null)
                {
                    Debug.LogError($"[MusicControl] Attempt to set {nameof(MusicForThisArea)} for "
                        + $"{nameof(MusicArea)} {name} to null. It must never be null, ignoring.");
                    return;
                }
                if (value == musicForThisArea)
                    return;
                if (value.Manager != Manager)
                {
                    Debug.LogError($"[MusicControl] Changing the music for an area to a music descriptor "
                        + $"which is apart of a different manager is not supported. Ignoring this attempt "
                        + $"at changing music.", this);
                    return;
                }
                if (IsInArea && IsActive)
                    RemoveMusicFromManager();
                musicForThisArea = value;
                if (IsInArea && IsActive)
                    AddMusicToManager();
                CheckSync();
            }
        }
        [PublicAPI] public MusicManager Manager => MusicForThisArea.Manager;

        [Tooltip("When true, the Default Priority from the Music Descriptor is used, "
            + "otherwise the priority defined below is used.")]
        [SerializeField] private bool useDefaultPriority = true;
        [PublicAPI] public bool UseDefaultPriority
        {
            get => useDefaultPriority;
            set
            {
                if (value == useDefaultPriority)
                    return;
                useDefaultPriority = value;
                UseUpdatedPriorityIfInArea();
                CheckSync();
            }
        }

        [Tooltip("Only used if 'Use Default Priority' is false.")]
        [SerializeField] private int priority = 0;
        [PublicAPI] public int Priority
        {
            get => priority;
            set
            {
                if (value == priority)
                    return;
                priority = value;
                UseUpdatedPriorityIfInArea();
                CheckSync();
            }
        }

        [Tooltip("When enabled, all values above will be synced whenever they are changed on any client. "
            + "This setting does not affect the network impact of this script when the values are never "
            + "changed at runtime.")]
        [SerializeField] private bool syncCurrentMusicAndPriority = true;
        [PublicAPI] public bool SyncCurrentMusicAndPriority => syncCurrentMusicAndPriority;
        private bool receivingData;

        // The music index itself is left shifted by 2 and the first 2 bits are actually used for the IsActive
        // and UseDefaultPriority flags. Synced booleans would be such a waste of space, and at the end of the
        // day this still supports 1 million music descriptors.
        [UdonSynced] private uint syncedMusicIndex;
        [UdonSynced] private int syncedPriority;

        private uint id;
        // To support having multiple trigger colliders on one object, as the enter and exit events get fired
        // for each collider on the object, so this keeps track of how many colliders the player is in.
        private int triggerCount;
        [PublicAPI] public bool IsInArea => triggerCount != 0;

        private void CheckSync()
        {
            if (!syncCurrentMusicAndPriority || receivingData)
                return;
            syncedMusicIndex = (((uint)MusicForThisArea.Index) << 2)
                | (UseDefaultPriority ? 2u : 0u)
                | (IsActive ? 1u : 0u);
            syncedPriority = Priority;
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            RequestSerialization();
        }

        public override void OnDeserialization()
        {
            // Multiple MusicAreas may be on the same object and may have different sync settings.
            // Ignore the sync if this area here does not actually do syncing.
            if (!syncCurrentMusicAndPriority)
                return;
            receivingData = true;
            IsActive = (syncedMusicIndex & 1u) != 0;
            UseDefaultPriority = (syncedMusicIndex & 2u) != 0;
            MusicForThisArea = Manager.Descriptors[syncedMusicIndex >> 1];
            Priority = syncedPriority;
            receivingData = false;
        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (!player.isLocal || (++triggerCount) > 1)
                return;
            if (IsActive)
                AddMusicToManager();
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            // triggerCount == 0 is just there to make sure this script doesn't break when it is placed at the
            // spawn point of the world. It still has undefined behaviour when doing that though.
            if (!player.isLocal || triggerCount == 0 || (--triggerCount) > 0)
                return;
            if (IsActive)
                RemoveMusicFromManager();
        }

        private void AddMusicToManager()
        {
            id = UseDefaultPriority
                ? MusicForThisArea.AddThisMusic()
                : MusicForThisArea.AddThisMusic(Priority);
        }

        private void RemoveMusicFromManager()
        {
            Manager.RemoveMusic(id);
        }

        private void UseUpdatedPriorityIfInArea()
        {
            if (!IsInArea || !IsActive)
                return;
            RemoveMusicFromManager();
            AddMusicToManager();
        }
    }
}
