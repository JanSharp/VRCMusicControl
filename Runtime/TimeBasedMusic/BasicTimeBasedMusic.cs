using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class BasicTimeBasedMusic : TimeBasedMusicBase
    {
        [SerializeField] private BasicMusicControlTimer sharedTimer;

        protected override void Start()
        {
            base.Start();
            sharedTimer.RegisterOnTimerReady(this);
        }

        /// <summary>
        /// This is not public API, do not call this function.
        /// </summary>
        public void OnTimerReady() => UpdateCurrentMusic();

        protected override float GetTime() => sharedTimer.CurrentTime;
    }
}
