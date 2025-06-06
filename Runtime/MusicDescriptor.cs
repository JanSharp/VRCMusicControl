﻿using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using JetBrains.Annotations;

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

    // 2 on one object are disallowed because they'd use the same audio source which is not supported.
    [DisallowMultipleComponent]
#if !ADVANCED_MUSIC_CONTROL
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
#endif
    /// <summary>
    /// <para>The API can be used in Awake, OnEnable or Start. It will be initialized in time.</para>
    /// </summary>
    public class MusicDescriptor : UdonSharpBehaviour
    {
#if ADVANCED_MUSIC_CONTROL
        [Header("The sync mode must either be Manual or None.", order = 0)]
        [Space(16f, order = 1)]
#endif
        [Tooltip("A music descriptor describing the absence of music. When true, "
            + "other properties get ignored, except for default priority.\n"
            + "When the AudioSource has no audio clip, this is forced to true at runtime.")]
        [SerializeField] private bool isSilenceDescriptor;
        [HideInInspector][SerializeField] private bool forcedSilenceDescriptor;
        [PublicAPI] public bool IsSilenceDescriptor => isSilenceDescriptor || forcedSilenceDescriptor;

        [SerializeField][Min(0f)] private float fadeInSeconds = 1f;
        [SerializeField][Min(0f)] private float fadeOutSeconds = 1f;
        [HideInInspector][SerializeField] private float fadeInInterval;
        [HideInInspector][SerializeField] private float fadeOutInterval;
        [PublicAPI]
        public float FadeInSeconds
        {
            get => fadeInSeconds;
            set
            {
                if (value == fadeInSeconds)
                    return;
                fadeInSeconds = value;
                fadeInInterval = CalculateUpdateInterval(fadeInSeconds);
                CheckSync();
            }
        }
        [PublicAPI]
        public float FadeOutSeconds
        {
            get => fadeOutSeconds;
            set
            {
                if (value == fadeOutSeconds)
                    return;
                fadeOutSeconds = value;
                fadeOutInterval = CalculateUpdateInterval(fadeOutSeconds);
                CheckSync();
            }
        }
        [FormerlySerializedAs("priority")]
        [SerializeField] private int defaultPriority;
        [PublicAPI] public int DefaultPriority => defaultPriority;

        [Tooltip("When starting to play this music, where in the audio clip should it start?\n\n"
            + "- Global Time Since First Play: It starts at the beginning of the clip the first time "
            + "playing, after that it calculates at which timestamp music would be at if it kept running "
            + "constantly.\n\n"
            + "- Global Time Since World Start: The same as GlobalTimeSinceFirstPlay except the first time "
            + "playing doesn't get special handling, it instead pretends the music started at world "
            + "start.\n\n"
            + "- Global Time Since World Start Synced: The same as GlobalTimeSinceWorldStart, but it uses "
            + "a synced world start time, which ultimately means the music will be truly the same "
            + "for everyone whenever this music is playing.\n\n"
            + "- Restart: It'll restart at the beginning of the clip every time it starts playing, "
            + "unless it was still fading out.\n\n"
            + "- Pause: It starts at the beginning of the clip the first time playing, after that whenever "
            + "it stops it remembers where it stopped and picks back up from there.")]
        [SerializeField] private MusicStartType startType = MusicStartType.GlobalTimeSinceFirstPlay;
        [PublicAPI] public MusicStartType StartType => startType;

        [Tooltip("When true, the below fade in seconds are used the very first time this music is played.")]
        [SerializeField][UdonSynced] private bool useDifferentFadeForFirstPlay = false;
        [PublicAPI]
        public bool UseDifferentFadeForFirstPlay
        {
            get => useDifferentFadeForFirstPlay;
            set
            {
                if (value == useDifferentFadeForFirstPlay)
                    return;
                useDifferentFadeForFirstPlay = value;
                // Does not require special handling when syncing and deserializing because this setting does
                // nothing besides saving the value and checking for sync, which doesn't happen anyway when
                // receiving data.
                CheckSync();
            }
        }
        [Tooltip("Likely makes sense to use with 'Global Time Since First Play' or 'Pause'.")]
        [SerializeField][Min(0f)] private float firstFadeInSeconds = 0.5f;
        [HideInInspector][SerializeField] private float firstFadeInInterval;
        [PublicAPI]
        public float FirstFadeInSeconds
        {
            get => firstFadeInSeconds;
            set
            {
                if (value == firstFadeInSeconds)
                    return;
                firstFadeInSeconds = value;
                firstFadeInInterval = CalculateUpdateInterval(firstFadeInSeconds);
                CheckSync();
            }
        }

        private float CurrentFadeInSeconds => UseDifferentFadeForFirstPlay && isFirstPlay
            ? FirstFadeInSeconds
            : FadeInSeconds;
        private float CurrentFadeInInterval => UseDifferentFadeForFirstPlay && isFirstPlay
            ? firstFadeInInterval
            : fadeInInterval;

        [Tooltip("When true, all 4 values related to fading are synced whenever they are changed on any "
            + "client. This setting does not affect the network impact of this script when the values "
            + "are never changed at runtime.")]
        [SerializeField] private bool syncFadeValues = true;
        [PublicAPI] public bool SyncFadeValues => syncFadeValues;
        [UdonSynced] private Vector3 syncedFadeSeconds;
        private bool receivingData;

        [HideInInspector][SerializeField] private MusicManager manager;
        [HideInInspector][SerializeField] private int index;
        [PublicAPI] public MusicManager Manager => manager;
        [PublicAPI] public int Index => index;
        [HideInInspector][SerializeField] private AudioSource audioSource;
        [HideInInspector][SerializeField] private float maxVolume;
        private float lastFadeInTime;
        private bool fadingIn;
        private float lastFadeOutTime;
        private bool fadingOut;

        private bool isPlaying;
        private bool waitingOnGlobalTimeSync;
        /// <summary>
        /// <para>Is this music descriptor currently making any sound? In other words, is it fading in, fading
        /// out or playing music at normal volume?</para>
        /// <para>Technically there is one case where it is not making sound and yet this returns true, which
        /// is when this descriptor is using global time synced and the synced time has not been received
        /// yet.</para>
        /// </summary>
        [PublicAPI]
        public bool IsPlaying
        {
            get
            {
                Manager.InternalInitialize();
                return isPlaying || waitingOnGlobalTimeSync;
            }
        }

        private int pausedTimeSamples = 0;
        // It's like a point in Time.time, can be negative due to syncing with other clients.
        private float globalTimeStart;
        private bool isFirstPlay = true;
        private bool isInitialized = false;

        /// <summary>
        /// This is not public API, do not call this function.
        /// </summary>
        public static float CalculateUpdateInterval(float fadeSeconds)
        {
            // At 1 fade second, 15 total updates.
            // At 10 fade seconds, 50 total updates.
            // Total updates is clamped between 15 and 50.
            return fadeSeconds / Mathf.Clamp((fadeSeconds - 1f) / 9f * 35f + 15f, 15f, 50f);
        }

        /// <summary>
        /// This is not public API, do not call this function.
        /// </summary>
        public void InternalReceivedGlobalStartTime()
        {
            // Adding Time.time to it again because this start time gets subtracted from Time.time every time
            // it is used. In other words, it calculates how much time has passed since this function here has
            // run, plus the current internal time, which accounts for global time offset.
            globalTimeStart = Time.time - Manager.InternalCurrentGlobalTime;
            if (waitingOnGlobalTimeSync)
            {
                waitingOnGlobalTimeSync = false;
                InternalPlay();
            }
        }

        private void CheckSync()
        {
            if (!syncFadeValues || receivingData)
                return;
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            RequestSerialization();
        }

        public override void OnPreSerialization()
        {
            if (!syncFadeValues)
                return;
            // Use OnPreSerialization instead of CheckSync for this just in case some other script requested
            // serialization on this object.
            syncedFadeSeconds.x = FadeInSeconds;
            syncedFadeSeconds.y = FadeOutSeconds;
            syncedFadeSeconds.z = FirstFadeInSeconds;
        }

        public override void OnDeserialization()
        {
            // Just in case some other random script requested serialization on this object.
            if (!syncFadeValues)
                return;
            receivingData = true;
            FadeInSeconds = syncedFadeSeconds.x;
            FadeOutSeconds = syncedFadeSeconds.y;
            FirstFadeInSeconds = syncedFadeSeconds.z;
            receivingData = false;
        }

        [PublicAPI] public uint AddThisMusic() => Manager.AddMusic(this);
        [PublicAPI] public uint AddThisMusic(int priority) => Manager.AddMusic(this, priority);
        [PublicAPI] public void SetAsDefault() => Manager.DefaultMusic = this;

        private void SetTime(float time)
        {
#if MUSIC_CONTROL_DEBUG
            Debug.Log($"[MusicControl] {this.name} setting time to {time}");
#endif
            AudioClip clip = audioSource.clip;
            time = (time * audioSource.pitch) % clip.length;
            if (time < 0f)
                time += clip.length; // time is negative, so this says "subtract time since end from length".
            audioSource.timeSamples = (int)(time * (float)clip.frequency);
            // This is using `timeSamples` instead of `time` because supposedly timeSamples is more accurate
            // than `time`. The docs mention "2 to 3 seconds of packets" when talking about this inaccuracy
            // about `time`, however no matter what I did I could not reproduce this inaccuracy. However even
            // still, this is now using `timeSamples` instead of `time`.
        }

        /// <summary>
        /// <para>Does nothing if this descriptor is currently playing.</para>
        /// <para>Causes the FirstFadeInSeconds to be used another time, even if it played already.</para>
        /// <para>For 'GlobalTimeSinceFirstPlay' and 'Pause' it forgets where it was and starts at the
        /// beginning again.</para>
        /// <para>Does not do any syncing.</para>
        /// </summary>
        [PublicAPI]
        public void Reset()
        {
            isFirstPlay = true;
            pausedTimeSamples = 0;
        }

        /// <summary>
        /// This is not public API, do not call this function.
        /// </summary>
        public void InternalPlay()
        {
            if (startType == MusicStartType.GlobalTimeSinceWorldStartSynced
                && !Manager.InternalHasReceivedGlobalTime)
            {
                waitingOnGlobalTimeSync = true;
                return;
            }
            if (IsSilenceDescriptor)
                return;
            if (!isPlaying)
            {
                if (!isInitialized)
                {
                    isInitialized = true;
                    audioSource.volume = 0;
                }
                if (isFirstPlay && startType == MusicStartType.GlobalTimeSinceFirstPlay)
                    globalTimeStart = Time.time;
                audioSource.Play();
                if (startType == MusicStartType.Pause)
                    audioSource.timeSamples = pausedTimeSamples;
                else if (startType != MusicStartType.Restart)
                    SetTime(Time.time - globalTimeStart);
            }
            isPlaying = true;
            fadingIn = true;
            fadingOut = false;
            lastFadeInTime = Time.time - CurrentFadeInInterval;
            InternalFadeIn();
        }

        /// <summary>
        /// This is not public API, do not call this function.
        /// </summary>
        public void InternalFadeIn()
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
            SendCustomEventDelayedSeconds(nameof(InternalFadeIn), CurrentFadeInInterval);
        }

        /// <summary>
        /// This is not public API, do not call this function.
        /// </summary>
        public void InternalStop()
        {
            if (waitingOnGlobalTimeSync)
            {
                waitingOnGlobalTimeSync = false;
                return;
            }
            if (IsSilenceDescriptor || !isPlaying)
                return;
            fadingIn = false;
            fadingOut = true;
            lastFadeOutTime = Time.time - fadeOutInterval;
            isFirstPlay = false;
            InternalFadeOut();
        }

        /// <summary>
        /// This is not public API, do not call this function.
        /// </summary>
        public void InternalFadeOut()
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
                    pausedTimeSamples = audioSource.timeSamples;
                audioSource.Stop();
                isPlaying = false;
                return;
            }
            SendCustomEventDelayedSeconds(nameof(InternalFadeOut), fadeOutInterval);
        }
    }
}
