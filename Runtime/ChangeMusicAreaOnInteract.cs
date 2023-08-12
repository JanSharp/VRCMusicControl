using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using JetBrains.Annotations;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    /// <summary>
    /// <para>The API can be used in Awake, OnEnable or Start. It will be initialized in time.</para>
    /// </summary>
    public class ChangeMusicAreaOnInteract : UdonSharpBehaviour
    {
        [PublicAPI] public bool changeIsActive = false;
        [PublicAPI] public bool changeMusicForThisArea = false;
        [PublicAPI] public bool changeUseDefaultPriority = false;
        [PublicAPI] public bool changePriority = false;
        [Space(8f)]
        [PublicAPI] public bool isActive = true;
        [Tooltip("Actually changing it to null is not possible, but if 'Change Music For This Area' "
            + "is false, this is not used anyway, so leaving it null is fine in that case.")]
        [PublicAPI] public MusicDescriptor musicForThisArea = null;
        [PublicAPI] public bool useDefaultPriority = true;
        [PublicAPI] public int priority = 0;
        [Space(8f)]
        [PublicAPI] public MusicArea[] targets;

        [PublicAPI] public override void Interact()
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
                if (changeIsActive)
                    target.IsActive = isActive;
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
