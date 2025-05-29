using UdonSharp;
using UnityEngine;
using JetBrains.Annotations;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    /// <summary>
    /// <para>The API can be used in Awake, OnEnable or Start. It will be initialized in time.</para>
    /// </summary>
    public class ChangeMusicDescriptorOnInteract : UdonSharpBehaviour
    {
        [PublicAPI] public bool changeFadeInSeconds = false;
        [PublicAPI] public bool changeFadeOutSeconds = false;
        [PublicAPI] public bool changeUseDifferentFadeForFirstPlay = false;
        [PublicAPI] public bool changeFirstFadeInSeconds = false;
        [Space(8f)]
        [PublicAPI] [Min(0f)] public float fadeInSeconds = 1f;
        [PublicAPI] [Min(0f)] public float fadeOutSeconds = 1f;
        [PublicAPI] public bool useDifferentFadeForFirstPlay = false;
        [PublicAPI] [Min(0f)] public float firstFadeInSeconds = 0.5f;
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
