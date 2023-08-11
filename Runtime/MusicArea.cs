using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using JetBrains.Annotations;

namespace JanSharp
{
    #if !AdvancedMusicControl
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    #endif
    public class MusicArea : UdonSharpBehaviour
    {
        #if AdvancedMusicControl
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

        private const uint IsActiveFlag = 0b0001;
        private const uint UseDefaultPriorityFlag = 0b0010;
        private const int SyncedMusicIndexShift = 2;
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
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            RequestSerialization();
        }

        public override void OnPreSerialization()
        {
            if (!syncCurrentMusicAndPriority)
                return;
            // Use OnPreSerialization instead of CheckSync for this just in case some other script requested
            // serialization on this object.
            syncedMusicIndex = (((uint)MusicForThisArea.Index) << SyncedMusicIndexShift)
                | (UseDefaultPriority ? UseDefaultPriorityFlag : 0u)
                | (IsActive ? IsActiveFlag : 0u);
            syncedPriority = Priority;
        }

        public override void OnDeserialization()
        {
            // Multiple MusicAreas may be on the same object and may have different sync settings.
            // Ignore the sync if this area here does not actually do syncing.
            if (!syncCurrentMusicAndPriority)
                return;
            receivingData = true;
            IsActive = (syncedMusicIndex & IsActiveFlag) != 0;
            UseDefaultPriority = (syncedMusicIndex & UseDefaultPriorityFlag) != 0;
            MusicForThisArea = Manager.Descriptors[syncedMusicIndex >> SyncedMusicIndexShift];
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
