﻿using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class MusicManager : UdonSharpBehaviour
{
    public MusicDescriptor[] descriptors;
    [SerializeField] private MusicDescriptor defaultMusic;
    [SerializeField] private bool syncCurrentDefaultMusic;
    public MusicDescriptor DefaultMusic
    {
        get => defaultMusic;
        set
        {
            if (defaultMusic == value)
                return;
            defaultMusic.isCurrentDefaultMusic = false;
            value.isCurrentDefaultMusic = true;
            ReplaceMusic(0, value);
            defaultMusic = value;
            defaultMusicIndex = value.Index;
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
        DefaultMusic = descriptors[defaultMusicIndex];
        receivingData = false;
    }

    private MusicDescriptor currentlyPlaying;
    /// <summary>
    /// This isn't actually truly a stack. The only real difference is that popping doesn't pop off the top of the stack
    /// but instead it removes a specific descriptor that's closest to top. If it was at top, music switches to the new top.
    /// Default/Default also breaks the rules because it always lives at index 0, even if it is null or when it gets replaced.
    /// </summary>
    private MusicDescriptor[] musicList;
    private uint[] musicListIds;
    private int musicListCount;
    private uint nextMusicId;
    private bool muted;
    public bool Muted
    {
        get => muted;
        set
        {
            if (muted == value)
                return;
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
            Debug.LogError($"MusicManager {name} is missing music descriptors.");
            return;
        }
        for (int i = 0; i < descriptors.Length; i++)
            descriptors[i].Init(this, i);
        musicList = new MusicDescriptor[8];
        musicListIds = new uint[8];
        DefaultMusic.isCurrentDefaultMusic = true;
        musicListCount++;
        SetMusic(0, nextMusicId++, DefaultMusic);
        defaultMusicIndex = DefaultMusic.Index;
    }

    public override void OnPlayerRespawn(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            musicListCount = 1;
            SwitchToTop();
        }
    }

    // for convenience, specifically for hooking them up with GUI buttons
    public void ToggleMuteMusic() => Muted = !Muted;
    public void MuteMusic() => Muted = true;
    public void UnMuteMusic() => Muted = false;

    private void SwitchMusic(MusicDescriptor toSwitchTo)
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
        SwitchMusic(musicList[musicListCount - 1]);
    }

    private void SetMusic(int index, uint id, MusicDescriptor toSet)
    {
        musicList[index] = toSet;
        musicListIds[index] = id;
        if (index == musicListCount - 1)
            SwitchToTop();
    }

    /// <summary>
    /// Returns an id used by `RemoveMusic` to remove the descriptor from the music list again
    /// </summary>
    public uint AddMusic(MusicDescriptor toAdd)
    {
        if (musicListCount == musicList.Length)
            GrowMusicList();
        // figure out where to put the music in the active list based on priority
        for (int i = (musicListCount++) - 1; i >= 0; i--)
        {
            var descriptor = musicList[i];
            // on priority collision the last one added "wins"
            if (toAdd.Priority >= descriptor.Priority)
            {
                SetMusic(i + 1, nextMusicId, toAdd);
                break;
            }
            // move items up as we go so we don't need a second loop
            musicList[i + 1] = descriptor;
            musicListIds[i + 1] = musicListIds[i];
        }
        return nextMusicId++;
    }

    private void GrowMusicList()
    {
        var Length = musicList.Length;
        var newMusicList = new MusicDescriptor[Length * 2];
        var newMusicListIds = new uint[Length * 2];
        for (int i = 0; i < Length; i++)
        {
            newMusicList[i] = musicList[i];
            newMusicListIds[i] = musicListIds[i];
        }
        musicList = newMusicList;
        musicListIds = newMusicListIds;
    }

    public void RemoveMusic(uint id)
    {
        musicListCount--;
        MusicDescriptor prevDescriptor = null;
        uint prevId = 0;
        for (int i = musicListCount; i >= 0; i--)
        {
            // move down as we go so we don't need a second loop
            var currentDescriptor = musicList[i];
            var currentId = musicListIds[i];
            musicList[i] = prevDescriptor;
            musicListIds[i] = prevId;
            if (currentId == id)
            {
                if (i == musicListCount)
                    SwitchToTop();
                return;
            }
            prevDescriptor = currentDescriptor;
            prevId = currentId;
        }
        Debug.LogError($"Attempt to RemoveMusic the id {id} that is not in the music stack.");
    }

    private void ReplaceMusic(int index, MusicDescriptor descriptor)
    {
        musicList[index] = descriptor;
        if (index == musicListCount - 1)
            SwitchToTop();
    }
}