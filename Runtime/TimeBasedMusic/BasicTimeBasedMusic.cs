using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    /// <summary>
    /// <para>The API can be used in Awake, OnEnable or Start. It will be initialized in time.</para>
    /// </summary>
    public class BasicTimeBasedMusic : TimeBasedMusicBase
    {
        [Tooltip("Must not be null.")]
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
