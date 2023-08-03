using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class MusicManager : UdonSharpBehaviour
    {
        public MusicDescriptor[] descriptors;
        [Tooltip("Null is valid and means it's silent by default.")]
        [SerializeField] private MusicDescriptor defaultMusic;
        [SerializeField] private bool syncCurrentDefaultMusic;
        public MusicDescriptor DefaultMusic
        {
            get => defaultMusic;
            set
            {
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
            }
        }
        [UdonSynced]
        private int defaultMusicIndex;
        private bool receivingData;

        public override void OnDeserialization()
        {
            receivingData = true;
            DefaultMusic = defaultMusicIndex == -1 ? null : descriptors[defaultMusicIndex];
            receivingData = false;
        }

        private MusicDescriptor currentlyPlaying;
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
        public bool Muted
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
                        currentlyPlaying.Play();
                }
                else
                {
                    if (currentlyPlaying != null)
                        currentlyPlaying.Stop();
                }
                muted = value;
            }
        }

        void Start()
        {
            if (descriptors == null || descriptors.Length == 0)
            {
                Debug.LogWarning($"<dlt> {nameof(MusicManager)} {name} is missing {nameof(MusicDescriptor)}s.", this);
                return;
            }
            for (int i = 0; i < descriptors.Length; i++)
                descriptors[i].Init(this, i);
            musicListCount++;
            SetMusic(0, DefaultMusic, int.MinValue, nextMusicId++);
            defaultMusicIndex = GetMusicDescriptorIndex(DefaultMusic);
        }

        private int GetMusicDescriptorIndex(MusicDescriptor descriptor)
        {
            return descriptor == null ? -1 : descriptor.Index;
        }

        // for convenience, specifically for hooking them up with GUI buttons
        public void ToggleMuteMusic() => Muted = !Muted;
        public void MuteMusic() => Muted = true;
        public void UnMuteMusic() => Muted = false;

        private void SetCurrentlyPlaying(MusicDescriptor toSwitchTo)
        {
            if (toSwitchTo == currentlyPlaying)
                return;
            if (currentlyPlaying != null)
                currentlyPlaying.Stop();
            if (toSwitchTo != null && !Muted)
                toSwitchTo.Play();
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
        public uint AddMusic(MusicDescriptor toAdd)
        {
            return AddMusic(toAdd, toAdd.DefaultPriority);
        }

        /// <summary>
        /// Returns an id used by `RemoveMusic` to remove the descriptor from the music list again.
        /// </summary>
        public uint AddMusic(MusicDescriptor toAdd, int priority)
        {
            if (musicListCount == musicList.Length)
                GrowMusicList();
            // figure out where to put the music in the active list based on priority
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
                // move items up as we go so we don't need a second loop
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

        public void RemoveMusic(uint id)
        {
            if (musicListCount == 0)
            {
                Debug.LogWarning($"<dlt> Attempt to {nameof(RemoveMusic)} the id {id} when the music stack is completely empty.", this);
                return;
            }

            musicListCount--;
            MusicDescriptor prevDescriptor = null;
            int prevPriority = 0;
            uint prevId = 0;
            for (int i = musicListCount; i >= 0; i--)
            {
                // move down as we go so we don't need a second loop
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

            Debug.LogWarning($"<dlt> Attempt to {nameof(RemoveMusic)} the id {id} that is not in the music stack.", this);

            // To gracefully handle the error, restore the lists, since the previous loop ultimately removed musicList[0].
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
    }
}
