using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ChangeMusicDescriptorOnInteract : UdonSharpBehaviour
    {
        public bool changeFadeInSeconds = false;
        public bool changeFadeOutSeconds = false;
        public bool changeUseDifferentFadeForFirstPlay = false;
        public bool changeFirstFadeInSeconds = false;
        [Space(8f)]
        public float fadeInSeconds = 1f;
        public float fadeOutSeconds = 1f;
        public bool useDifferentFadeForFirstPlay = false;
        public float firstFadeInSeconds = 0.5f;
        [Space(8f)]
        public MusicDescriptor[] targets;

        public override void Interact()
        {
            if (targets == null)
            {
                Debug.LogWarning($"[MusicControl] The {nameof(targets)} array for "
                    + $"{nameof(ChangeMusicDescriptorOnInteract)} {name} is null, doing nothing.");
                return;
            }
            foreach (MusicDescriptor target in targets)
            {
                if (target == null)
                {
                    Debug.LogWarning($"[MusicControl] The {nameof(targets)} array for "
                        + $"{nameof(ChangeMusicDescriptorOnInteract)} {name} contains a null element, "
                        + $"ignoring it.");
                    continue;
                }
                if (changeFadeInSeconds)
                    target.FadeInSeconds = fadeInSeconds;
                if (changeFadeOutSeconds)
                    target.FadeOutSeconds = fadeOutSeconds;
                if (changeUseDifferentFadeForFirstPlay)
                    target.UseDifferentFadeForFirstPlay = useDifferentFadeForFirstPlay;
                if (changeFirstFadeInSeconds)
                    target.FirstFadeInSeconds = firstFadeInSeconds;
            }
        }
    }
}
