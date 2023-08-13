using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using JetBrains.Annotations;

namespace JanSharp
{
    /// <summary>
    /// <para>The API can be used in Awake, OnEnable or Start. It will be initialized in time.</para>
    /// </summary>
    public abstract class TimeBasedMusicBase : UdonSharpBehaviour
    {
        [Tooltip("Defines the start times at which the corresponding Music At Time Stamps starts playing. "
            + "The first timestamp defines the start of the time range used to determine the total length of "
            + "all time ranges, because when the current time is outside of this time frame it will use "
            + "modulo to move it into the range and then determines what music to play.\n"
            + "Must have the same length as Music At Time Stamps array.\n"
            + "Must be sequential, ascending and not equal to each other.")]
        [SerializeField] private float[] timeStamps;
        [Tooltip("Defines the end time for the last entry in the Time Stamps and Music At Time Stamps array. "
            + "Must be greater than the last entry in Time Stamps.")]
        [SerializeField] private float lastMusicEndTime;
        [Tooltip("Defines what music to play at at the corresponding time in Time Stamps. Null entires have "
            + "have the same effect as a silence MusicDescriptor.\n"
            + "Must have the same length as the Time Stamps array.")]
        [SerializeField] private MusicDescriptor[] musicAtTimeStamps;
        [Space(8f)]
        [PublicAPI] public float updateInterval = 10f;
        [PublicAPI] public bool callResetBeforeEachSwitch = true;
        [Tooltip("Must not be null. All MusicDescriptors in Music At Time Stamps must also be apart of this "
            + "MusicManager.")]
        [SerializeField] private MusicManager targetManager;

        [SerializeField] [HideInInspector] private float musicStartTime;
        [SerializeField] [HideInInspector] private float totalMusicLength;
        // -1 is evaluated at runtime too and means null. -2 here means that if -1 is evaluated as the first
        // index to use, it will actually set the current default music to null instead of not doing anything.
        private int currentIndex = -2;
        private bool isStepLoopRunning = false;

        protected abstract float GetTime();

        protected virtual void Start()
        {
            if ((targetManager.SyncCurrentDefaultMusic && !Networking.LocalPlayer.isMaster)
                || isStepLoopRunning) // It should always be false here, but order of operation trust issues.
            {
                return;
            }
            isStepLoopRunning = true;
            SendCustomEventDelayedSeconds(nameof(InternalStep), updateInterval);
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            SendCustomEventDelayedSeconds(nameof(InternalPlayerLeftDelayed), 2f); // Trust issues.
        }

        /// <summary>
        /// This is not public API, do not call this function.
        /// </summary>
        public void InternalPlayerLeftDelayed()
        {
            if (targetManager.SyncCurrentDefaultMusic && Networking.LocalPlayer.isMaster && !isStepLoopRunning)
            {
                isStepLoopRunning = true;
                SendCustomEventDelayedSeconds(nameof(InternalStep), updateInterval);
            }
        }

        /// <summary>
        /// This is not public API, do not call this function.
        /// </summary>
        public void InternalStep()
        {
            UpdateCurrentMusic();
            SendCustomEventDelayedSeconds(nameof(InternalStep), updateInterval);
        }

        [PublicAPI] public void UpdateCurrentMusic()
        {
            float time = GetTime();
            if (float.IsNaN(time))
                return;
            time = (time - musicStartTime) % totalMusicLength;
            if (time < 0)
                time += totalMusicLength;
            time += musicStartTime;

            int length = timeStamps.Length;
            int indexToUse = length - 1; // Default to last, because the loop below never gets that far.
            for (int i = 0; i < length; i++)
            {
                if (timeStamps[i] > time) // Find the first time that's bigger than current time.
                {
                    // That means the previous one must be used.
                    indexToUse = i - 1;
                    break;
                }
            }
            if (indexToUse != currentIndex)
            {
                MusicDescriptor newMusic = indexToUse == -1 ? null : musicAtTimeStamps[indexToUse];
                if (callResetBeforeEachSwitch && newMusic != null)
                {
                    if (targetManager.SyncCurrentDefaultMusic)
                        newMusic.SendCustomNetworkEvent(NetworkEventTarget.All, nameof(newMusic.Reset));
                    else
                        newMusic.Reset();
                }
                targetManager.DefaultMusic = newMusic;
                currentIndex = indexToUse;
            }
        }
    }
}
