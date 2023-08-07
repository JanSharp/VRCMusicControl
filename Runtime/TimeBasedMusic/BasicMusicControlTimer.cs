using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;
using JetBrains.Annotations;

namespace JanSharp
{
    [DefaultExecutionOrder(1000)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class BasicMusicControlTimer : UdonSharpBehaviour
    {
        [PublicAPI] public bool IsReady => !float.IsNaN(startTime);
        private float startTime = float.NaN;
        // Offset from Time.time to the proper current time.
        private float currentTimeOffset = float.NaN;
        [SerializeField] private float initialTime = 0f;
        [SerializeField] private float speed = 1f;
        [SerializeField] [UdonSynced] private bool isPaused = false;
        [SerializeField] private bool syncTimer = true;

        [UdonSynced] private Vector2 syncedValues;

        private float StartTime
        {
            get => startTime;
            set
            {
                // Ignore attempts to set it to NaN.
                if (float.IsNaN(value))
                    return;
                // Even if the given value is equal to the current value of startTime, Time.time is used when
                // setting currentTimeOffset, therefore it cannot be ignored.
                bool gotReady = !IsReady;
                startTime = value;
                currentTimeOffset = startTime - Time.time;
                if (gotReady)
                    RaiseOnTimerReady();
                SettingsChanged();
            }
        }
        [PublicAPI] public float CurrentTime
        {
            // Returns NaN if it's not initialized yet, which makes the TimeBasedMusicBase script "wait".
            // Only the time passed since startTime is affected by speed.
            get => isPaused ? startTime : startTime + speed * (Time.time + currentTimeOffset - startTime);
            set => StartTime = value;
        }
        [PublicAPI] public float Speed
        {
            get => speed;
            set
            {
                if (value == speed)
                    return;
                StartTime = CurrentTime; // Before setting speed, because speed affects CurrentTime.
                speed = value;
                SettingsChanged();
            }
        }
        [PublicAPI] public bool IsPaused
        {
            get => isPaused;
            set
            {
                if (value == isPaused)
                    return;
                StartTime = CurrentTime; // Before setting isPaused, because isPaused affects CurrentTime.
                isPaused = value;
                SettingsChanged();
            }
        }

        // Public just so intellisense users can see that this exists.
        [PublicAPI] public const string OnTimerReadyEventName = "OnTimerReady";
        private UdonSharpBehaviour[] onReadyListeners = new UdonSharpBehaviour[ArrList.MinCapacity];
        private int onReadyListenersCount = 0;

        // Public just so intellisense users can see that this exists.
        [PublicAPI] public const string OnTimerSettingsChangedEventName = "OnTimerSettingsChanged";
        private UdonSharpBehaviour[] onSettingsChangedListeners = new UdonSharpBehaviour[ArrList.MinCapacity];
        private int onSettingsChangedListenersCount = 0;

        private bool receivingData;

        private void SettingsChanged()
        {
            if (receivingData)
                return;
            // OnDeserialization raises it manually, otherwise it would raise twice. So only raise it when not
            // receiving data.
            RaiseOnTimerSettingsChanged();
            if (!syncTimer)
                return;
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            RequestSerialization();
        }

        public override void OnPreSerialization()
        {
            if (!syncTimer)
                return;
            syncedValues.x = Time.time + currentTimeOffset;
            syncedValues.y = speed;
            // Paused is synced without special handling.
        }

        public override void OnDeserialization(DeserializationResult result)
        {
            if (!syncTimer)
                return;
            receivingData = true;
            // The fact that it reassigns these values every time it syncs will cause it to jump back and
            // forth in time by a few milliseconds, but it should not be noticeable, hopefully.
            StartTime = syncedValues.x + result.receiveTime - result.sendTime;
            Speed = syncedValues.y;
            receivingData = false;
            RaiseOnTimerSettingsChanged();
        }

        private void Start()
        {
            if (!syncTimer)
            {
                StartTime = initialTime;
                return;
            }

            if (Networking.LocalPlayer.isMaster)
            {
                StartTime = initialTime;
                RequestSerialization();
            }
            else
                SendCustomEventDelayedSeconds(nameof(InternalGlobalStartTimeFallback), 15f);
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            // Make sure new players receive the proper synced value.
            if (syncTimer && player.isLocal)
                RequestSerialization();
        }

        /// <summary>
        /// This is not public API, do not call this function.
        /// </summary>
        public void InternalGlobalStartTimeFallback()
        {
            if (IsReady)
                return;
            // Just in case we just don't receive the synced time, set it to the current time and inform all
            // scripts, that way music is guaranteed to play, be it different for this player.
            StartTime = initialTime;
        }

        /// <summary>
        /// <para>Raises the 'OnTimerReady' event as soon as the timer knows what time it is.</para>
        /// <para>Guaranteed to run before 'OnTimerSettingsChanged', so long as this one is registered first.</para>
        /// <para>By the time this runs CurrentTime is ready to be read, however other values may change
        /// nearly instantly after this event runs. However the OnTimerSettingsChanged event will be raised
        /// for those cases anyway, and even when nothing else changes, the OnTimerSettingsChanged event will
        /// get raised right after this. This isn't the best API at the moment, but it's better than nothing</para>
        /// </summary>
        [PublicAPI] public void RegisterOnTimerReady(UdonSharpBehaviour listener)
        {
            ArrList.Add(ref onReadyListeners, ref onReadyListenersCount, listener);
            if (IsReady) // Just in case something has a higher execution order than 1000.
                listener.SendCustomEvent(OnTimerReadyEventName);
        }

        /// <summary>
        /// Raises the 'OnTimerSettingsChanged' event as soon as the timer knows what time it is.
        /// </summary>
        [PublicAPI] public void RegisterOnTimerSettingsChanged(UdonSharpBehaviour listener)
        {
            ArrList.Add(ref onSettingsChangedListeners, ref onSettingsChangedListenersCount, listener);
            if (IsReady) // Just in case something has a higher execution order than 1000.
                listener.SendCustomEvent(OnTimerSettingsChangedEventName);
        }

        private void RaiseOnTimerReady()
        {
            for (int i = 0; i < onReadyListenersCount; i++)
                onReadyListeners[i].SendCustomEvent(OnTimerReadyEventName);
        }

        private void RaiseOnTimerSettingsChanged()
        {
            for (int i = 0; i < onSettingsChangedListenersCount; i++)
                onSettingsChangedListeners[i].SendCustomEvent(OnTimerSettingsChangedEventName);
        }
    }
}
