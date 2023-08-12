using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;
using JetBrains.Annotations;

namespace JanSharp
{
    // 2 on one object are nonsensical because they'd use the same descriptors, which is not supported.
    [DisallowMultipleComponent]
    // NOTE: Given that this class is ultimately used to generate Udon assembly which is then used on the ...
    // component type UdonBehaviour for every single script, the concept of DefaultExecutionOrder doesn't
    // actually exist. Udon would require implementation for that itself, which I don't think it does.
    // [DefaultExecutionOrder(-2000)]
    #if !AdvancedMusicControl
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    #endif
    /// <summary>
    /// <para>The API can be used in Awake, OnEnable or Start. It will be initialized in time.</para>
    /// <para>However the DefaultMusic may not be correct yet if SyncCurrentDefaultMusic is true, since that
    /// gets updated in the OnDeserialization event for this script. Listen to the 'OnDefaultMusicChanged'
    /// event if knowing the current default music is needed.</para>
    /// </summary>
    public class MusicManager : UdonSharpBehaviour
    {
        [SerializeField] [HideInInspector] private MusicDescriptor[] descriptors;
        /// <summary>
        /// Must not modify this array, it is read only.
        /// </summary>
        [PublicAPI] public MusicDescriptor[] Descriptors => descriptors;
        #if AdvancedMusicControl
        [Header("The sync mode must either be Manual or None.", order = 0)]
        [Space(-8f, order = 1)]
        [Header("Note that even when 'Sync Current Default Music'", order = 2)]
        [Space(-8f, order = 3)]
        [Header("is false, this script is still used to sync the", order = 4)]
        [Space(-8f, order = 5)]
        [Header("global start time if any of the descriptors for", order = 6)]
        [Space(-8f, order = 7)]
        [Header("this manager are using the synced music start type.", order = 8)]
        [Space(16f, order = 9)]
        #endif
        [Header("Music Descriptors managed by this Manager must be children of this object.", order = 10)]
        [Space(8f, order = 11)]
        [Tooltip("Music played when nothing else is playing, priority is not used for this. "
            + "Null is valid and means it's silent by default.")]
        [SerializeField] private MusicDescriptor defaultMusic;
        [Tooltip("When true, the current default music is synced whenever it is changed on any "
            + "client. This setting does not affect the network impact of this script when the values "
            + "are never changed at runtime.")]
        [SerializeField] private bool syncCurrentDefaultMusic = true;
        [PublicAPI] public bool SyncCurrentDefaultMusic => syncCurrentDefaultMusic;
        [PublicAPI] public MusicDescriptor DefaultMusic
        {
            get => defaultMusic;
            set
            {
                if (!isInitialized)
                    InternalInitialize();
                if (defaultMusic == value)
                    return;
                ReplaceMusic(0, value);
                defaultMusic = value;
                defaultMusicIndex = GetMusicDescriptorIndex(value);
                if (syncCurrentDefaultMusic && !receivingData)
                {
                    Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
                    RequestSerialization();
                }
                FlagForOnDefaultMusicChanged();
            }
        }
        [UdonSynced] private int defaultMusicIndex;
        [UdonSynced] private float syncedGlobalTime;
        private float globalStartTime = float.NaN;
        // Offset from Time.time to the proper start time (which is also in the Time.time scale) where music
        // started playing on a different client, in order to sync this one up with the other one.
        private float currentGlobalTimeOffset;
        private float GlobalStartTime
        {
            get => globalStartTime;
            set
            {
                globalStartTime = value;
                currentGlobalTimeOffset = globalStartTime - Time.time;
            }
        }
        /// <summary>
        /// This is not public API, do not use this property.
        /// </summary>
        public bool InternalHasReceivedGlobalTime => !float.IsNaN(GlobalStartTime);
        /// <summary>
        /// This is not public API, do not use this property.
        /// </summary>
        public float InternalCurrentGlobalTime
        {
            get => Time.time + currentGlobalTimeOffset;
            set => GlobalStartTime = value;
        }
        private bool receivingData;
        [SerializeField] [HideInInspector] private bool syncGlobalStartTime; // Set in OnBuild.

        private bool isInitialized = false;

        public override void OnPreSerialization()
        {
            if (syncGlobalStartTime)
            {
                syncedGlobalTime = InternalCurrentGlobalTime;
                #if MusicControlDebug
                Debug.Log($"[MusicControl] OnPreSerialization - "
                    + $"syncedGlobalStartTime: {syncedGlobalTime},    "
                    + $"Time.time: {Time.time},    "
                    + $"InternalGlobalStartTimeOffset: {currentGlobalTimeOffset}");
                #endif
            }
        }

        public override void OnDeserialization(DeserializationResult result)
        {
            if (syncCurrentDefaultMusic)
            {
                receivingData = true;
                DefaultMusic = defaultMusicIndex == -1 ? null : descriptors[defaultMusicIndex];
                receivingData = false;
            }

            if (syncGlobalStartTime && !InternalHasReceivedGlobalTime)
            {
                InternalCurrentGlobalTime = syncedGlobalTime + result.receiveTime - result.sendTime;
                #if MusicControlDebug
                Debug.Log($"[MusicControl] OnPreSerialization - "
                    + $"result.receiveTime: {result.receiveTime},    "
                    + $"result.sendTime: {result.sendTime},    "
                    + $"result.receiveTime - result.sendTime: {result.receiveTime - result.sendTime},    "
                    + $"original syncedGlobalStartTime: {syncedGlobalTime},    "
                    + $"modified syncedGlobalStartTime: {syncedGlobalTime + result.receiveTime - result.sendTime},    "
                    + $"GlobalStartTime: {GlobalStartTime},    "
                    + $"currentGlobalTimeOffset: {currentGlobalTimeOffset},    "
                    + $"Time.time: {Time.time},    "
                    + $"InternalCurrentGlobalTime: {InternalCurrentGlobalTime}");
                #endif
                ReceivedGlobalStartTime();
            }
        }

        private MusicDescriptor currentlyPlaying;
        /// <summary>
        /// <para>Can be null, which has the same effect as a silence descriptor.</para>
        /// <para>This is the currently main active music descriptor. The descriptor with highest priority in
        /// the music list at this point in time. This is separate from IsPlaying on the descriptor because
        /// they may be fading in, fading out or muted.</para>
        /// </summary>
        [PublicAPI] public MusicDescriptor CurrentlyPlaying
        {
            get
            {
                if (!isInitialized)
                    InternalInitialize();
                return currentlyPlaying;
            }
        }
        /// <summary>
        /// This isn't actually truly a stack. There's no push/pop, it inserts using priority and removes
        /// based on an id obtained when inserting. Default music always lives at index 0, even when null.
        /// </summary>
        private MusicDescriptor[] musicList = new MusicDescriptor[8];
        private int[] musicListPriorities = new int[8];
        private uint[] musicListIds = new uint[8];
        private int musicListCount = 0;
        private uint nextMusicId;
        private bool muted;
        [PublicAPI] public bool Muted
        {
            get => muted;
            set
            {
                if (muted == value)
                    return;
                // Even though there will always be some music descriptor always playing because default music
                // must not be null, another script running Start before this script would set Muted to true,
                // in which case currentlyPlaying is still null.
                if (muted)
                {
                    if (currentlyPlaying != null)
                        currentlyPlaying.InternalPlay();
                }
                else
                {
                    if (currentlyPlaying != null)
                        currentlyPlaying.InternalStop();
                }
                muted = value;
            }
        }

        // Public just so intellisense users can see that this exists.
        [PublicAPI] public const string OnDefaultMusicChangedEventName = "OnDefaultMusicChanged";
        private UdonSharpBehaviour[] onDefaultMusicListeners = new UdonSharpBehaviour[ArrList.MinCapacity];
        private int onDefaultMusicListenersCount = 0;
        private bool flaggedForOnDefaultMusic = false;

        private void Start() => InternalInitialize();

        /// <summary>
        /// This is not public API, do not call this function.
        /// </summary>
        public void InternalInitialize()
        {
            // If you look at all the usages of this function you'll see everything that requires the system
            // to be initialized before being used. This is important because there is no fixed execution
            // order for UdonBehaviours, and other scripts could also be using this api inside of Awake or
            // OnEnable, which happen before Start regardless. So everything that requires the system to be
            // initialized must make sure itself that it is in fact initialized.

            if (isInitialized)
                return;
            isInitialized = true;

            if (syncGlobalStartTime)
            {
                if (Networking.LocalPlayer.isMaster)
                {
                    GlobalStartTime = 0f;
                    ReceivedGlobalStartTime();
                    RequestSerialization();
                }
                else
                    SendCustomEventDelayedSeconds(nameof(InternalGlobalStartTimeFallback), 15f);
            }

            musicListCount++;
            SetMusic(0, DefaultMusic, int.MinValue, nextMusicId++);
            defaultMusicIndex = GetMusicDescriptorIndex(DefaultMusic);
        }

        /// <summary>
        /// This is not public API, do not call this function.
        /// </summary>
        public void InternalGlobalStartTimeFallback()
        {
            if (InternalHasReceivedGlobalTime)
                return;
            // Just in case we just don't receive the synced time, set it to the current time and inform all
            // scripts, that way music is guaranteed to play, be it different for this player.
            GlobalStartTime = 0f;
            ReceivedGlobalStartTime();
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            // Make sure new players receive the proper synced value.
            if (syncGlobalStartTime && player.isLocal)
                RequestSerialization();
        }

        private void ReceivedGlobalStartTime()
        {
            foreach (MusicDescriptor descriptor in descriptors)
                descriptor.InternalReceivedGlobalStartTime();
        }

        private int GetMusicDescriptorIndex(MusicDescriptor descriptor)
        {
            return descriptor == null ? -1 : descriptor.Index;
        }

        // For convenience, specifically for hooking them up with GUI buttons.
        [PublicAPI] public void ToggleMuteMusic() => Muted = !Muted;
        [PublicAPI] public void MuteMusic() => Muted = true;
        [PublicAPI] public void UnMuteMusic() => Muted = false;

        private void SetCurrentlyPlaying(MusicDescriptor toSwitchTo)
        {
            if (toSwitchTo == currentlyPlaying)
                return;
            if (currentlyPlaying != null)
                currentlyPlaying.InternalStop();
            if (toSwitchTo != null && !Muted)
                toSwitchTo.InternalPlay();
            currentlyPlaying = toSwitchTo;
        }

        private void SwitchToTop()
        {
            SetCurrentlyPlaying(musicList[musicListCount - 1]);
        }

        private void SetMusic(int index, MusicDescriptor toSet, int priority, uint id)
        {
            musicList[index] = toSet;
            musicListPriorities[index] = priority;
            musicListIds[index] = id;
            if (index == musicListCount - 1)
                SwitchToTop();
        }

        /// <summary>
        /// <para>Returns an id used by `RemoveMusic` to remove the descriptor from the music list again.</para>
        /// <para>Uses the default priority of the given music descriptor.</para>
        /// </summary>
        [PublicAPI] public uint AddMusic(MusicDescriptor toAdd)
        {
            return AddMusic(toAdd, toAdd.DefaultPriority);
        }

        /// <summary>
        /// Returns an id used by `RemoveMusic` to remove the descriptor from the music list again.
        /// </summary>
        [PublicAPI] public uint AddMusic(MusicDescriptor toAdd, int priority)
        {
            if (!isInitialized)
                InternalInitialize();
            if (toAdd == null)
            {
                Debug.LogError($"[MusicControl] Attempt to {nameof(AddMusic)} a null "
                    + $"{nameof(MusicDescriptor)} to the {nameof(MusicManager)} {name}. "
                    + $"Ignoring and returning uint.MaxValue.");
                return uint.MaxValue;
            }
            if (toAdd.Manager != this)
            {
                Debug.LogError($"[MusicControl] Attempt to {nameof(AddMusic)} the {nameof(MusicDescriptor)} "
                    + $"{toAdd.name} to the {nameof(MusicManager)} {name}, however the "
                    + $"{nameof(MusicDescriptor)}'s {nameof(MusicManager)} is {toAdd.Manager.name} which is a"
                    + $"mismatch and not allowed. Ignoring and returning uint.MaxValue.");
                return uint.MaxValue;
            }

            if (musicListCount == musicList.Length)
                GrowMusicList();
            // Figure out where to put the music in the active list based on priority.
            for (int i = (musicListCount++) - 1; i >= 0; i--)
            {
                MusicDescriptor descriptor = musicList[i];
                int otherPriority = musicListPriorities[i];
                // On priority collision the last one added "wins".
                // Since default music uses int.MinValue, this condition is guaranteed to be true
                // if i reaches 0, in which case index 1 is used, just above default music.
                if (priority >= otherPriority)
                {
                    SetMusic(i + 1, toAdd, priority, nextMusicId);
                    break;
                }
                // Move items up as we go so we don't need a second loop.
                musicList[i + 1] = descriptor;
                musicListPriorities[i + 1] = otherPriority;
                musicListIds[i + 1] = musicListIds[i];
            }
            return nextMusicId++;
        }

        private void GrowMusicList()
        {
            int newLength = musicList.Length * 2;
            MusicDescriptor[] newMusicList = new MusicDescriptor[newLength];
            int[] newMusicListPriorities = new int[newLength];
            uint[] newMusicListIds = new uint[newLength];
            musicList.CopyTo(newMusicList, 0);
            musicListPriorities.CopyTo(newMusicListPriorities, 0);
            musicListIds.CopyTo(newMusicListIds, 0);
            musicList = newMusicList;
            musicListPriorities = newMusicListPriorities;
            musicListIds = newMusicListIds;
        }

        [PublicAPI] public void RemoveMusic(uint id)
        {
            if (!isInitialized)
                InternalInitialize();
            if (musicListCount == 0)
            {
                Debug.LogWarning($"[MusicControl] Attempt to {nameof(RemoveMusic)} the id {id} "
                    + $"when the music list is completely empty.", this);
                return;
            }

            musicListCount--;
            MusicDescriptor prevDescriptor = null;
            int prevPriority = 0;
            uint prevId = 0;
            for (int i = musicListCount; i >= 0; i--)
            {
                // Move down as we go so we don't need a second loop.
                MusicDescriptor currentDescriptor = musicList[i];
                int currentPriority = musicListPriorities[i];
                uint currentId = musicListIds[i];
                musicList[i] = prevDescriptor;
                musicListPriorities[i] = prevPriority;
                musicListIds[i] = prevId;
                if (currentId == id)
                {
                    if (i == musicListCount)
                        SwitchToTop();
                    return;
                }
                prevDescriptor = currentDescriptor;
                prevPriority = currentPriority;
                prevId = currentId;
            }

            Debug.LogWarning($"[MusicControl] Attempt to {nameof(RemoveMusic)} the id {id} "
                + $"that is not in the music stack.", this);

            // To gracefully handle the error, restore the lists,
            // since the previous loop ultimately removed musicList[0].
            for (int i = musicListCount - 1; i >= 0; i--)
            {
                musicList[i + 1] = musicList[i];
                musicListPriorities[i + 1] = musicListPriorities[i];
                musicListIds[i + 1] = musicListIds[i];
            }
            musicList[0] = prevDescriptor;
            musicListPriorities[0] = prevPriority;
            musicListIds[0] = prevId;
            musicListCount++;
        }

        private void ReplaceMusic(int index, MusicDescriptor descriptor)
        {
            musicList[index] = descriptor;
            if (index == musicListCount - 1)
                SwitchToTop();
        }

        /// <summary>
        /// <para>Register a behaviour for the 'OnDefaultMusicChanged' event.</para>
        /// <para>'OnDefaultMusicChanged' is raised whenever the 'DefaultMusic' is changed, 1 frame delayed in
        /// order to prevent recursion.</para>
        /// <para>Since it is 1 frame delayed, if the default music changes multiple times within a frame,
        /// this event will only be raised once next frame for the latest change.</para>
        /// </summary>
        [PublicAPI] public void RegisterOnDefaultMusicChanged(UdonSharpBehaviour listener)
        {
            ArrList.Add(ref onDefaultMusicListeners, ref onDefaultMusicListenersCount, listener);
        }

        private void FlagForOnDefaultMusicChanged()
        {
            if (flaggedForOnDefaultMusic)
                return;
            flaggedForOnDefaultMusic = true;
            SendCustomEventDelayedFrames(nameof(InternalRaiseOnDefaultMusicChanged), 1);
        }

        /// <summary>
        /// This is not public API, do not call this function.
        /// </summary>
        public void InternalRaiseOnDefaultMusicChanged()
        {
            flaggedForOnDefaultMusic = false;
            for (int i = 0; i < onDefaultMusicListenersCount; i++)
                onDefaultMusicListeners[i].SendCustomEvent(OnDefaultMusicChangedEventName);
        }
    }
}
