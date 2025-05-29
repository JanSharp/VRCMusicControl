using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;
using JetBrains.Annotations;

namespace JanSharp
{
    [DefaultExecutionOrder(1000)] // Udon does actually have this concept and UdonSharp explicitly supports it.
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    /// <summary>
    /// <para>The API can be used in Awake, OnEnable or Start. It will be initialized in time.</para>
    /// </summary>
    public class BasicMusicControlTimer : UdonSharpBehaviour
    {
        /// <summary>
        /// Returns false until right before the 'OnTimerReady' event is raised. Values may already be
        /// initialized before the event is raised, therefore before this value is true, however this is not
        /// part of the specification for this API, so it should not be relied upon. Values are guaranteed to
        /// be correct and readable as soon as 'OnTimerReady' event is raised and this value is true.
        /// </summary>
        [PublicAPI] public bool IsReady => readyEventHasBeenRaised;
        private float startTime = float.NaN;
        // Offset from Time.time to the proper current time.
        private float currentTimeOffset = float.NaN;
        [SerializeField] private float initialTime = 0f;
        [SerializeField] private float speed = 1f;
        [SerializeField] [UdonSynced] private bool isPaused = false;
        [Tooltip("When enabled, all values above will be synced whenever they are changed on any client. "
            + "This setting does not affect the network impact of this script when the values are never "
            + "changed at runtime.")]
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
                startTime = value;
                currentTimeOffset = startTime - Time.time;
                if (!readyEventHasBeenRaised)
                    FlagForOnTimerReady();
                SettingsChanged();
            }
        }
        /// <summary>
        /// <para>Returns the current time, taking start time, 'Speed' and 'IsPaused' into account.</para>
        /// <para>Can be read while 'IsReady' is false, however it will return NaN at the beginning, until at
        /// some point the value is correct. By the time 'IsReady' is true, this is guaranteed to return a
        /// correct value.</para>
        /// <para>Must not be written to until 'IsReady' is true. Note that 'IsReady' is true as soon as
        /// 'OnTimerReady' is being raised.</para>
        /// <para>Attempts to write to this while 'IsReady' is false are ignored.</para>
        /// <para>Attempts to set this value to NaN are ignored.</para>
        /// <para>When set, raises the 'OnSettingsChanged' event, 1 frame delayed to prevent recursion.</para>
        /// </summary>
        [PublicAPI] public float CurrentTime
        {
            // Returns NaN if it's not initialized yet, which makes the TimeBasedMusicBase script "wait".
            // Only the time passed since startTime is affected by speed.
            get => isPaused ? startTime : startTime + speed * (Time.time + currentTimeOffset - startTime);
            set
            {
                if (readyEventHasBeenRaised)
                    StartTime = value;
            }
        }
        /// <summary>
        /// <para>The current speed, 1 == 1 unit per second, 2 == 2 units per second.</para>
        /// <para>Negative values are allowed and cause the timer to run backwards.</para>
        /// <para>Must not be read from until 'IsReady' is true. Note that 'IsReady' is true as soon as
        /// 'OnTimerReady' is being raised.</para>
        /// <para>Attempts to write to this while 'IsReady' is false are ignored.</para>
        /// <para>Attempts to set the value its current value are ignored.</para>
        /// <para>When set, raises the 'OnSettingsChanged' event, 1 frame delayed to prevent recursion.</para>
        /// </summary>
        [PublicAPI] public float Speed
        {
            get => speed;
            set
            {
                if (!readyEventHasBeenRaised || value == speed)
                    return;
                StartTime = CurrentTime; // Before setting speed, because speed affects CurrentTime.
                speed = value;
                SettingsChanged();
            }
        }
        /// <summary>
        /// <para>Must not be read from until 'IsReady' is true. Note that 'IsReady' is true as soon as
        /// 'OnTimerReady' is being raised.</para>
        /// <para>Attempts to write to this while 'IsReady' is false are ignored.</para>
        /// <para>Attempts to set the value its current value are ignored.</para>
        /// <para>When set, raises the 'OnSettingsChanged' event, 1 frame delayed to prevent recursion.</para>
        /// </summary>
        [PublicAPI] public bool IsPaused
        {
            get => isPaused;
            set
            {
                if (!readyEventHasBeenRaised || value == isPaused)
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
        private bool flaggedForOnReady = false;
        /// <summary>Also indicates the current IsReady state.</summary>
        private bool readyEventHasBeenRaised = false;

        // Public just so intellisense users can see that this exists.
        [PublicAPI] public const string OnTimerSettingsChangedEventName = "OnTimerSettingsChanged";
        private UdonSharpBehaviour[] onSettingsChangedListeners = new UdonSharpBehaviour[ArrList.MinCapacity];
        private int onSettingsChangedListenersCount = 0;
        private bool flaggedForOnSettingsChanged = false;

        private bool receivingData;

        private void SettingsChanged()
        {
            if (readyEventHasBeenRaised)
                FlagForOnSettingsChanged();
            if (receivingData || !syncTimer)
                return;
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            RequestSerialization();
        }

        public override void OnPreSerialization()
        {
            if (!syncTimer)
                return;
            syncedValues.x = CurrentTime;
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
            CurrentTime = syncedValues.x + result.receiveTime - result.sendTime;
            Speed = syncedValues.y;
            receivingData = false;
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
        /// <para>Register a behaviour for the 'OnTimerReady' event.</para>
        /// <para>Guaranteed to be raised before 'OnTimerSettingsChanged', so long as this one is registered
        /// first.</para>
        /// <para>By the time 'OnTimerReady' is raised, 'IsReady' is true and all values are correct and
        /// readable.</para>
        /// </summary>
        [PublicAPI] public void RegisterOnTimerReady(UdonSharpBehaviour listener)
        {
            ArrList.Add(ref onReadyListeners, ref onReadyListenersCount, listener);
            // Users are free to register for this event at any point in time. Since this is a one time event,
            // if someone registers for it, they're doing so to get the event. If it has already been raised
            // however, then simply raise it for this newly registered listener.
            if (readyEventHasBeenRaised)
                listener.SendCustomEventDelayedFrames(OnTimerReadyEventName, 1);
        }

        /// <summary>
        /// <para>Register a behaviour for the 'OnTimerSettingsChanged' event.</para>
        /// <para>Guaranteed to be raised after 'OnTimerReady', so long as this one is registered
        /// second.</para>
        /// <para>Is not instantly raised after 'OnTimerReady', only under the conditions below.</para>
        /// <para>'OnTimerSettingsChanged' is raised whenever any of 'CurrentTime', 'Speed' or 'IsPaused' are
        /// changed, 1 frame delayed in order to prevent recursion.</para>
        /// <para>Since it is 1 frame delayed, if the settings change multiple times within a frame, this
        /// event will only be raised once next frame for the latest change.</para>
        /// </summary>
        [PublicAPI] public void RegisterOnTimerSettingsChanged(UdonSharpBehaviour listener)
        {
            ArrList.Add(ref onSettingsChangedListeners, ref onSettingsChangedListenersCount, listener);
        }

        private void FlagForOnTimerReady()
        {
            if (flaggedForOnReady)
                return;
            flaggedForOnReady = true;
            // I sure as hell hope that VRChat actually guarantees that events are fired in the order in which
            // they are sent when they are run on the same frame. I sure hope... you know what that means.
            SendCustomEventDelayedFrames(nameof(InternalRaiseOnTimerReady), 1);
        }

        private void FlagForOnSettingsChanged()
        {
            if (flaggedForOnSettingsChanged)
                return;
            flaggedForOnSettingsChanged = true;
            SendCustomEventDelayedFrames(nameof(InternalRaiseOnTimerSettingsChanged), 1);
        }

        /// <summary>
        /// This is not public API, do not call this function.
        /// </summary>
        public void InternalRaiseOnTimerReady()
        {
            flaggedForOnReady = false;
            readyEventHasBeenRaised = true;
            for (int i = 0; i < onReadyListenersCount; i++)
                onReadyListeners[i].SendCustomEvent(OnTimerReadyEventName);
        }

        /// <summary>
        /// This is not public API, do not call this function.
        /// </summary>
        public void InternalRaiseOnTimerSettingsChanged()
        {
            flaggedForOnSettingsChanged = false;
            for (int i = 0; i < onSettingsChangedListenersCount; i++)
                onSettingsChangedListeners[i].SendCustomEvent(OnTimerSettingsChangedEventName);
        }
    }
}
