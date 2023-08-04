using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MusicDescriptor : UdonSharpBehaviour
    {
        [SerializeField] private float fadeInSeconds;
        [SerializeField] private float fadeOutSeconds;
        private float fadeInInterval;
        private float fadeOutInterval;
        public float FadeInSeconds
        {
            get => fadeInSeconds;
            set
            {
                fadeInSeconds = value;
                fadeInInterval = CalculateUpdateInterval(fadeInInterval);
            }
        }
        public float FadeOutSeconds
        {
            get => fadeOutSeconds;
            set
            {
                fadeOutSeconds = value;
                fadeOutInterval = CalculateUpdateInterval(fadeOutInterval);
            }
        }
        [FormerlySerializedAs("priority")]
        [SerializeField] private int defaultPriority;
        public int DefaultPriority => defaultPriority;

        [Tooltip(@"A music descriptor describing the absence of music. When true, other properties get ignored, except for default priority.")]
        [SerializeField] private bool isSilenceDescriptor;
        public bool IsSilenceDescriptor => isSilenceDescriptor;

        public MusicManager Manager { get; private set; }
        public int Index { get; private set; }
        private AudioSource audioSource;
        private float maxVolume;
        private float lastFadeInTime;
        private bool fadingIn;
        private float lastFadeOutTime;
        private bool fadingOut;

        private bool isPlaying;

        private float CalculateUpdateInterval(float fadeSeconds)
        {
            // At 1 fade second, 15 updates per second.
            // At 10 fade seconds, 50 updates per second.
            // Updates per second is clamped between 15 and 50.
            return fadeSeconds / Mathf.Clamp((fadeSeconds - 1f) / 9f * 35f + 15f, 15f, 50f);
        }

        public void Init(MusicManager manager, int index)
        {
            this.Manager = manager;
            this.Index = index;
            if (isSilenceDescriptor)
                return;
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError($"[MusicControl] {nameof(MusicDescriptor)} {name} "
                    + $"is missing an AudioSource component.", this);
                return;
            }
            maxVolume = audioSource.volume;
            audioSource.volume = 0;
            fadeInInterval = CalculateUpdateInterval(FadeInSeconds);
            fadeOutInterval = CalculateUpdateInterval(FadeOutSeconds);
        }

        public uint AddThisMusic() => Manager.AddMusic(this);
        public uint AddThisMusic(int priority) => Manager.AddMusic(this, priority);
        public void SetAsDefault() => Manager.DefaultMusic = this;

        public void Play()
        {
            if (isSilenceDescriptor)
                return;
            if (!isPlaying)
                audioSource.Play();
            isPlaying = true;
            fadingIn = true;
            fadingOut = false;
            lastFadeInTime = Time.time - fadeInInterval;
            FadeIn();
        }

        /// <summary>
        /// Isn't actually public, but has to be because it is invoked by `SendCustomEventDelayedSeconds`.
        /// </summary>
        public void FadeIn()
        {
            if (!fadingIn)
                return;
            float currentVolume = audioSource.volume;
            float volumePerSecond = maxVolume / fadeInSeconds;
            float currentTime = Time.time;
            float deltaTime = currentTime - lastFadeInTime;
            lastFadeInTime = currentTime;
            float step = volumePerSecond * deltaTime;
            currentVolume = Mathf.Min(currentVolume + step, maxVolume);
            audioSource.volume = currentVolume;
            if (currentVolume == maxVolume)
            {
                fadingIn = false;
                return;
            }
            SendCustomEventDelayedSeconds(nameof(FadeIn), fadeInInterval);
        }

        public void Stop()
        {
            if (isSilenceDescriptor || !isPlaying)
                return;
            fadingIn = false;
            fadingOut = true;
            lastFadeOutTime = Time.time - fadeOutInterval;
            FadeOut();
        }

        /// <summary>
        /// Isn't actually public, but has to be because it is invoked by `SendCustomEventDelayedSeconds`.
        /// </summary>
        public void FadeOut()
        {
            if (!fadingOut)
                return;
            float currentVolume = audioSource.volume;
            float volumePerSecond = maxVolume / fadeOutSeconds;
            float currentTime = Time.time;
            float deltaTime = currentTime - lastFadeOutTime;
            lastFadeOutTime = currentTime;
            float step = volumePerSecond * deltaTime;
            currentVolume = Mathf.Max(currentVolume - step, 0);
            audioSource.volume = currentVolume;
            if (currentVolume == 0)
            {
                fadingOut = false;
                audioSource.Stop();
                isPlaying = false;
                return;
            }
            SendCustomEventDelayedSeconds(nameof(FadeOut), fadeOutInterval);
        }
    }
}
