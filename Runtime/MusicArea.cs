using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MusicArea : UdonSharpBehaviour
    {
        [SerializeField] private MusicDescriptor musicForThisArea;
        public MusicDescriptor MusicForThisArea
        {
            get => musicForThisArea;
            set
            {
                if (value == musicForThisArea)
                    return;
                if (IsInArea)
                    RemoveMusicFromManager();
                musicForThisArea = value;
                if (IsInArea)
                    AddMusicToManager();
            }
        }

        [SerializeField] private bool useDefaultPriority = true;
        public bool UseDefaultPriority
        {
            get => useDefaultPriority;
            set
            {
                if (value == useDefaultPriority)
                    return;
                useDefaultPriority = value;
                UseUpdatedPriorityIfInArea();
            }
        }

        [SerializeField] private int priority = 0;
        public int Priority
        {
            get => priority;
            set
            {
                if (value == priority)
                    return;
                priority = value;
                UseUpdatedPriorityIfInArea();
            }
        }

        private uint id;
        // To support having multiple trigger colliders on one object, as the enter and exit events get fired
        // for each collider on the object, so this keeps track of how many colliders the player is in.
        private int triggerCount;
        private bool IsInArea => triggerCount != 0;

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (!player.isLocal || (++triggerCount) > 1)
                return;
            AddMusicToManager();
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            // triggerCount == 0 is just there to make sure this script doesn't break when it is placed at the
            // spawn point of the world. It still has undefined behaviour when doing that though.
            if (!player.isLocal || triggerCount == 0 || (--triggerCount) > 0)
                return;
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
            MusicForThisArea.Manager.RemoveMusic(id);
        }

        private void UseUpdatedPriorityIfInArea()
        {
            if (!IsInArea)
                return;
            RemoveMusicFromManager();
            AddMusicToManager();
        }
    }
}
