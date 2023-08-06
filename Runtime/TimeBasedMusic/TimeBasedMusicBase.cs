using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using JetBrains.Annotations;

namespace JanSharp
{
    public abstract class TimeBasedMusicBase : UdonSharpBehaviour
    {
        public float[] timeStamps;
        public float lastMusicEndTime;
        public MusicDescriptor[] musicAtTimeStamps;
        [Space(8f)]
        public float updateInterval = 10f;
        public bool callResetBeforeEachSwitch = true;
        public MusicManager targetManager;

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
            time %= lastMusicEndTime;
            if (time < 0)
                time += lastMusicEndTime;

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
