
# Changelog

## [0.2.1] - 2023-07-16

_First version of this package that is in the VCC listing._

### Changed

- **Breaking:** Separate Music Control into its own repo ([`a6cc91f`](https://github.com/JanSharp/VRCMusicControl/commit/a6cc91f52c408783a78230ed199c62908310dc18))
- Add second arg `this` to Debug.Log calls, maybe enables clicking on runtime log messages ([`ee4ffb5`](https://github.com/JanSharp/VRCMusicControl/commit/ee4ffb5ffe6218097cd01b94becc93bafb6ad2ca))

### Added

- Add readme ([`a5cad57`](https://github.com/JanSharp/VRCMusicControl/commit/a5cad5711ebe43e1956d227ea73025d4cf6a5575))

## [0.2.0] - 2022-06-12

### Changed

- Rename MusicToggle to MusicArea ([`6307354`](https://github.com/JanSharp/VRCMusicControl/commit/63073543fe2baf2b7a6af39f0adaeb759a12541d))
- Improve input validation and error handling ([`a0be987`](https://github.com/JanSharp/VRCMusicControl/commit/a0be98784c5afd607a4088c2763927ad5ed0312e))
- Migrate to VRChat Creator Companion ([`9ae838c`](https://github.com/JanSharp/VRCMusicControl/commit/9ae838cf1d6280c64c607559fb3ae9967b52bd99), [`78b73b6`](https://github.com/JanSharp/VRCMusicControl/commit/78b73b6816612602b04daafeb4097351f087c01a))

### Fixed

- Fix respawning while in a music trigger zone breaking the MusicManager ([`a0be987`](https://github.com/JanSharp/VRCMusicControl/commit/a0be98784c5afd607a4088c2763927ad5ed0312e))
- Fix using the same MusicDescriptor as default and for an area always having lowest priority ([`b9975e6`](https://github.com/JanSharp/VRCMusicControl/commit/b9975e64b54aa0d7e1fd5a28aee46d5e29a861c1))
- Fix MusicToggle not checking colliding player being local ([`b8d5dad`](https://github.com/JanSharp/VRCMusicControl/commit/b8d5dadd665fbc464737bbd33eee66ffe8dc947c))

## [0.1.1] - 2022-09-03

### Fixed

- Fix MusicToggle not checking colliding player being local ([`b8d5dad`](https://github.com/JanSharp/VRCMusicControl/commit/b8d5dadd665fbc464737bbd33eee66ffe8dc947c))

## [0.1.0] - 2022-08-25

### Added

- Add concept of default music which plays by default, like at spawn and outside of any trigger ([`3c33f1d`](https://github.com/JanSharp/VRCMusicControl/commit/3c33f1d1b8cf1bc5da6cb86c8ca8d8fdebf58822), [`52c8c19`](https://github.com/JanSharp/VRCMusicControl/commit/52c8c192bd5302f7e109583f716602d5b645ee8e), [`fcf3d5a`](https://github.com/JanSharp/VRCMusicControl/commit/fcf3d5aca13fcd57cc91993a2bccae7409bdfcef))
- Add concept of a music stack, used by trigger zones which change music ([`3c33f1d`](https://github.com/JanSharp/VRCMusicControl/commit/3c33f1d1b8cf1bc5da6cb86c8ca8d8fdebf58822), [`eb6ebd3`](https://github.com/JanSharp/VRCMusicControl/commit/eb6ebd399a66753a392cf45b57a4b6eda4267dd5), [`ec5a4c7`](https://github.com/JanSharp/VRCMusicControl/commit/ec5a4c762d25ae5e80ec96be3ada19906f506722), [`4211fe2`](https://github.com/JanSharp/VRCMusicControl/commit/4211fe2ad233802a516cb3560102a5c5a4721b84), [`b4707fb`](https://github.com/JanSharp/VRCMusicControl/commit/b4707fbeed41edfc805a1fd94c6d80befc1648f5), [`9f72b56`](https://github.com/JanSharp/VRCMusicControl/commit/9f72b56ece6257f22cd21710a65ababbcf5d360d))
- Add script to change default music ([`c5f49d9`](https://github.com/JanSharp/VRCMusicControl/commit/c5f49d9a0fbd3eff0578b1d8afe69f0165c05c64))

[0.2.1]: https://github.com/JanSharp/VRCMusicControl/releases/tag/v0.2.1
[0.2.0]: https://github.com/JanSharp/VRCMusicControl/releases/tag/MusicControl_v0.2.0
[0.1.1]: https://github.com/JanSharp/VRCMusicControl/releases/tag/MusicControl_v0.1.1
[0.1.0]: https://github.com/JanSharp/VRCMusicControl/releases/tag/MusicControl_v0.1.0