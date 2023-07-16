
# Music Control

Control music in VRChat maps using trigger zones.

Features:

- MusicArea - a trigger zone with an associated MusicDescriptor
- Default music, while outside of MusicAreas
- Priority
- Silence MusicDescriptor, with priority it can override other music
- One can have multiple MusicControllers
- Changing the default music at runtime with an option to sync said change

# Ideas

- There's an incredibly high chance that I'll change MusicDescriptors to accept multiple AudioSources.
- I've currently got ideas for grouping, which would make using multiple controllers obsolete, but it does add complexity.
- I'm thinking about volume sliders, possibly with category support
