using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using JetBrains.Annotations;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ChangeMusicDescriptorOnInteract : UdonSharpBehaviour
    {
        [PublicAPI] public bool changeFadeInSeconds = false;
        [PublicAPI] public bool changeFadeOutSeconds = false;
        [PublicAPI] public bool changeUseDifferentFadeForFirstPlay = false;
        [PublicAPI] public bool changeFirstFadeInSeconds = false;
        [Space(8f)]
        [PublicAPI] public float fadeInSeconds = 1f;
        [PublicAPI] public float fadeOutSeconds = 1f;
        [PublicAPI] public bool useDifferentFadeForFirstPlay = false;
        [PublicAPI] public float firstFadeInSeconds = 0.5f;
        [Space(8f)]
        [PublicAPI] public MusicDescriptor[] targets;

        [PublicAPI] public override void Interact()
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
