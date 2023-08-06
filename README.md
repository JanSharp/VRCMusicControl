
# Music Control

Control music and ambiance in VRChat maps using trigger zones and more. Supports fading, priority, muting, global timing, etc.

# Installing

Head to my [VCC Listing](https://jansharp.github.io/vrc/vcclisting.xhtml) and follow the instructions there.

# Features

- MusicManager - a core script managing a set of MusicDescriptor with optional default music
  - Default music plays when no other music is currently playing
  - Keeps track of all active music, playing one with highest priority
  - Can have its default music changed at runtime
  - Can sync its current default music automatically
  - There can be multiple MusicManager, but each MusicDescriptor can only be managed by one
  - Can be muted for the local player
- MusicDescriptor - a core script wrapping an AudioSource defining fade values, music start type and default priority
  - Can have its fade values changed at runtime
  - Can sync its current fade values automatically
  - A Silence descriptor, using priority, it can override other music
- MusicArea - a trigger zone with an associated MusicDescriptor
  - Can have multiple triggers on the one game object
  - Can override the default priority of the MusicDescriptor
  - Can have its MusicDescriptor and priority changed at runtime
  - Can sync its current MusicDescriptor and priority automatically
- One script for each of the scripts above to change their values at runtime on interact, or using UI
- Time based music
  - An abstract base script to control different music at different times to enable integration with other existing time based systems
  - A basic implementation for said script to use a basic timer which has speed and can be paused
  - A UI for this basic timer implementation
  - There can be multiple time based music controllers and they can all use the same timer
- An API to use at runtime for every script, except for BasicMusicControlTimerUI
- A good amount of editor scripting to assist with catching errors early and improve quality of life

# Limitations

- Every script that has syncing should never be disabled. The MusicManager, MusicDescriptor and MusicArea scripts should not be disabled even when syncing is disabled. Note that MusicArea has its own IsActive field
- There mustn't be a MusicArea at the spawn point of the world, the system wouldn't get the on player trigger enter event (I'm quite certain)
- Each MusicDescriptor can only have 1 AudioSource and each MusicManager can only have 1 MusicDescriptor currently playing. This is a limitation which both significantly simplifies the implementation and encourages people to have fewer audio sources for music active at the same time which is also a performance improvement. However there are times where multiple audio sources are totally necessary to achieve certain effects, like having music and ambiance, in which case using multiple MusicManagers would be required. Note that an object can have multiple MusicArea components, and they can have differing sync settings

# Priority

Priority is pretty straight forward: If music has higher priority, it will get played. If 2 MusicDescriptors get added to the MusicManager's music list with the same priority, the last one added will get played. Negative values work.

# Runtime API

At this time there is no documentation unfortunately, however each script (except BasicMusicControlTimerUI) has each public member that's actually apart of the API marked with teh PublicAPI attribute, and each public member that isn't apart of the API has a summary stating as such, and several of these also have a prefix of "Internal".

There are a few summaries for functions which is a bare minimum of documentation which can be read whenever using the functions or by reading through the source scripts.

# Ideas

- I'm thinking about volume sliders, for each music manager
