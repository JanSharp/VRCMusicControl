
# Music Control

Control music in VRChat maps using trigger zones.

Features:

- MusicArea - a trigger zone with an associated MusicDescriptor
- Default music, while outside of MusicAreas
- Local muting
- Priority
- Silence MusicDescriptor, with priority it can override other music
- One can have multiple MusicControllers
- Changing the default music at runtime with an option to sync said change

Note that a MusicArea at the spawn point of the world is not supported. Unless we do get the on player trigger enter event for a player joining inside of a trigger, which I'm quite certain we don't.

# Ideas

- There's an incredibly high chance that I'll change MusicDescriptors to accept multiple AudioSources.
- I've currently got ideas for grouping, which would make using multiple controllers obsolete, but it does add complexity.
- I'm thinking about volume sliders, possibly with category support
