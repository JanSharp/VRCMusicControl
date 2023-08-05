
# Migrating to v1.0.0

There are 2 changes with this version which may require manual migration:

- Assignment of Music Descriptors to Music Managers
- Change of Music Start Type for music descriptors

Previously the **assignment of Music Descriptors** to Managers was done 100% manually using an array in the Music Manager inspector. This has been removed, and now Managers automatically manage all Descriptors that are children - or nested children - of the Game Object the Manager is on. Therefore to **restore previous behaviour** every Music Descriptor must be moved to be a child of the Music Manager it was apart of previously.

This version introduces **Music Start Types** for Music Descriptors. The previous behaviour was that it would always `Restart` the audio clip from the beginning. This is still an option, however the **new default is different**, which is `Global Time Since First Play`. Since previously there weren't any options at all, it may be preferable to check all Music Descriptors and see which start type would be best. If simply **restoring the previous behaviour** is desired, follow these steps:

1. In the hierarchy search for `MusicDescriptor`
2. Select all found objects
3. Change the "Music Start Type" dropdown to `Restart`
