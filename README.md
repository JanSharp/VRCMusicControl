
# Music Control

Control music in VRChat maps using trigger zones.

# Installing

Head to my [VCC Listing](https://jansharp.github.io/vrc/vcclisting.xhtml) and follow the instructions there.

# Features

- MusicArea - a trigger zone with an associated MusicDescriptor
- A MusicArea can have multiple triggers on the one game object
- Default music, while outside of MusicAreas
- Local muting
- Priority
- Silence MusicDescriptor, with priority it can override other music
- One can have multiple MusicControllers
- Changing the default music at runtime with an option to sync said change

Note that a MusicArea at the spawn point of the world is not supported. Unless we do get the on player trigger enter event for a player joining inside of a trigger, which I'm quite certain we don't.

# Limitations

- Each MusicDescriptor can only have 1 AudioSource and each MusicManager can only have 1 MusicDescriptor currently playing. This is a limitation which both significantly simplifies the implementation and encourages people to have fewer audio sources for music active at the same time which is also a performance improvement. However there are times where multiple audio sources are totally necessary to achieve certain effects, like having music and ambiance, in which case using multiple MusicManagers would be required. Note that an object can have multiple MusicArea components, and they can have differing sync settings.

# Ideas

- There's an incredibly high chance that I'll change MusicDescriptors to accept multiple AudioSources.
- I've currently got ideas for grouping, which would make using multiple controllers obsolete, but it does add complexity.
- I'm thinking about volume sliders, possibly with category support
