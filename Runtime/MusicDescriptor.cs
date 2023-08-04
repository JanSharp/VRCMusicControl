using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    public enum MusicStartType
    {
        GlobalTimeSinceFirstPlay,
        GlobalTimeSinceWorldStart,
        GlobalTimeSinceWorldStartSynced,
        Restart,
        Pause,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MusicDescriptor : UdonSharpBehaviour
    {
        [Tooltip(@"A music descriptor describing the absence of music. When true, other properties get ignored, except for default priority.")]
        [SerializeField] private bool isSilenceDescriptor;
        public bool IsSilenceDescriptor => isSilenceDescriptor;

        [SerializeField] private float fadeInSeconds = 1f;
        [SerializeField] private float fadeOutSeconds = 1f;
        [HideInInspector] [SerializeField] private float fadeInInterval;
        [HideInInspector] [SerializeField] private float fadeOutInterval;
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

        [Tooltip(
@"When starting to play this music, where in the audio clip should it start?
- Global Time Since First Play: It starts that the beginning of the clip the very first time, after that it calculates at which timestamp music would be if it kept running constantly.
- Global Time Since World Start: The same as GlobalTimeSinceFirstPlay the very first time doesn't get special handling, it instead pretends the music started at world start.
- Global Time Since World Start Synced: The same as GlobalTimeSinceWorldStart, but it uses a synced world start time, which ultimately means the music will be truly the same for everyone whenever this music is  playing.
- Restart: It'll restart at the beginning of the clip every time it starts playing, unless it was still fading out.
- Pause: It starts the the beginning of the clip the very first time, after that whenever it stops it remembers where it stopped and picks back up from there.")]
        [SerializeField] private MusicStartType startType = MusicStartType.GlobalTimeSinceFirstPlay;
        public MusicStartType StartType => startType;

        [Tooltip("When true, the below fade in seconds are used the very first time this music is played.")]
        [SerializeField] private bool useDifferentFadeForFirstPlay = false;
        [Tooltip("Likely makes sense to use with 'Global Time Since First Play' or 'Pause'.")]
        [SerializeField] private float firstFadeInSeconds = 0.5f;
        [HideInInspector] [SerializeField] private float firstFadeInInterval;
        public float FirstFadeInSeconds
        {
            get => firstFadeInSeconds;
            set
            {
                firstFadeInSeconds = value;
                CalculateUpdateInterval(firstFadeInSeconds);
            }
        }

        private float CurrentFadeInSeconds => useDifferentFadeForFirstPlay && isFirstPlay
            ? FirstFadeInSeconds
            : FadeInSeconds;
        private float CurrentFadeInInterval => useDifferentFadeForFirstPlay && isFirstPlay
            ? firstFadeInInterval
            : fadeInInterval;

        [HideInInspector] [SerializeField] private MusicManager manager;
        [HideInInspector] [SerializeField] private int index;
        public MusicManager Manager => manager;
        public int Index => index;
        [HideInInspector] [SerializeField] private AudioSource audioSource;
        [HideInInspector] [SerializeField] private float maxVolume;
        private float lastFadeInTime;
        private bool fadingIn;
        private float lastFadeOutTime;
        private bool fadingOut;

        private bool isPlaying;
        private bool waitingOnGlobalTimeSync;
        public bool IsPlaying => isPlaying || waitingOnGlobalTimeSync;

        private float pausedTime = 0f;
        private float globalTimeStart;
        private bool isFirstPlay = true;

        public static float CalculateUpdateInterval(float fadeSeconds)
        {
            // At 1 fade second, 15 updates per second.
            // At 10 fade seconds, 50 updates per second.
            // Updates per second is clamped between 15 and 50.
            return fadeSeconds / Mathf.Clamp((fadeSeconds - 1f) / 9f * 35f + 15f, 15f, 50f);
        }

        public void ReceivedGlobalStartTime()
        {
            globalTimeStart = -(Time.time + Manager.GlobalStartTimeOffset);
            if (waitingOnGlobalTimeSync)
            {
                waitingOnGlobalTimeSync = false;
                Play();
            }
        }

        public uint AddThisMusic() => Manager.AddMusic(this);
        public uint AddThisMusic(int priority) => Manager.AddMusic(this, priority);
        public void SetAsDefault() => Manager.DefaultMusic = this;

        public void Play()
        {
            if (startType == MusicStartType.GlobalTimeSinceWorldStartSynced
                && !Manager.HasReceivedGlobalStartTime)
            {
                waitingOnGlobalTimeSync = true;
                return;
            }
            if (isSilenceDescriptor)
                return;
            if (!isPlaying)
            {
                if (isFirstPlay)
                    audioSource.volume = 0;
                if (isFirstPlay && startType == MusicStartType.GlobalTimeSinceFirstPlay)
                    globalTimeStart = Time.time;
                audioSource.Play();
                if (startType == MusicStartType.Pause)
                    audioSource.time = pausedTime;
                else if (startType != MusicStartType.Restart)
                    audioSource.time = (Time.time - globalTimeStart) % audioSource.clip.length;
            }
            isPlaying = true;
            fadingIn = true;
            fadingOut = false;
            lastFadeInTime = Time.time - CurrentFadeInInterval;
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
            float volumePerSecond = maxVolume / CurrentFadeInSeconds;
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
            SendCustomEventDelayedSeconds(nameof(FadeIn), CurrentFadeInInterval);
        }

        public void Stop()
        {
            if (waitingOnGlobalTimeSync)
            {
                waitingOnGlobalTimeSync = false;
                return;
            }
            if (isSilenceDescriptor || !isPlaying)
                return;
            fadingIn = false;
            fadingOut = true;
            lastFadeOutTime = Time.time - fadeOutInterval;
            isFirstPlay = false;
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
            float volumePerSecond = maxVolume / FadeOutSeconds;
            float currentTime = Time.time;
            float deltaTime = currentTime - lastFadeOutTime;
            lastFadeOutTime = currentTime;
            float step = volumePerSecond * deltaTime;
            currentVolume = Mathf.Max(currentVolume - step, 0);
            audioSource.volume = currentVolume;
            if (currentVolume == 0)
            {
                fadingOut = false;
                if (startType == MusicStartType.Pause)
                    pausedTime = audioSource.time;
                audioSource.Stop();
                isPlaying = false;
                return;
            }
            SendCustomEventDelayedSeconds(nameof(FadeOut), fadeOutInterval);
        }
    }
}
