using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ChangeMusicAreaOnInteract : UdonSharpBehaviour
    {
        public bool changeMusicForThisArea = false;
        public bool changeUseDefaultPriority = false;
        public bool changePriority = false;
        [Space(8f)]
        [Tooltip("Actually changing it to null is not possible, but if 'Change Music For This Area' "
            + "is false, this is not used anyway, so leaving it null is fine in that case.")]
        public MusicDescriptor musicForThisArea = null;
        public bool useDefaultPriority = true;
        public int priority = 0;
        [Space(8f)]
        public MusicArea[] targets;

        public override void Interact()
        {
            if (targets == null)
            {
                Debug.LogWarning($"[MusicControl] The {nameof(targets)} array for "
                    + $"{nameof(ChangeMusicAreaOnInteract)} {name} is null, doing nothing.");
                return;
            }
            foreach (MusicArea target in targets)
            {
                if (target == null)
                {
                    Debug.LogWarning($"[MusicControl] The {nameof(targets)} array for "
                        + $"{nameof(ChangeMusicAreaOnInteract)} {name} contains a null element, "
                        + $"ignoring it.");
                    continue;
                }
                if (changeMusicForThisArea)
                    target.MusicForThisArea = musicForThisArea;
                if (changeUseDefaultPriority)
                    target.UseDefaultPriority = useDefaultPriority;
                if (changePriority)
                    target.Priority = priority;
            }
        }
    }
}
