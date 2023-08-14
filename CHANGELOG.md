
# Changelog

## [1.1.0] - 2023-08-14

_Several changes could be breaking changes in specific situations._

### Changed

- Change OnTimerReady to guarantee all values have been initialized before it is raised ([`a223bc6`](https://github.com/JanSharp/VRCMusicControl/commit/a223bc665eb0018d41a96affde47ca5e412c62f6))
- Change dependency on `com.jansharp.common` to `0.2.0` for editor scripting ([`c5cc6cd`](https://github.com/JanSharp/VRCMusicControl/commit/c5cc6cd74e8554510082af2b039b9bebcf0e41d5))
- Ensure every API is usable at any point, even in Awake ([`4e2a14b`](https://github.com/JanSharp/VRCMusicControl/commit/4e2a14bfb171f27fa94640382e6ab80f5e99c879))
- Change BasicMusicControlTimer to ignore writes when not ready ([`71bff5b`](https://github.com/JanSharp/VRCMusicControl/commit/71bff5bca5ddf5efd6109c1d479b1bf9a641833c))
- Ensure references to Shared Timer exist at build time ([`b070000`](https://github.com/JanSharp/VRCMusicControl/commit/b0700007690e970f6e6e08cc5576c04f97562390))
- Change events to be raised 1 frame delayed to prevent recursion ([`a223bc6`](https://github.com/JanSharp/VRCMusicControl/commit/a223bc665eb0018d41a96affde47ca5e412c62f6), [`e16f175`](https://github.com/JanSharp/VRCMusicControl/commit/e16f175d138e2be5ba12edce1a0e4bb9e0bcc8d5))
- Improve summaries for BasicMusicControlTimer event register functions ([`a223bc6`](https://github.com/JanSharp/VRCMusicControl/commit/a223bc665eb0018d41a96affde47ca5e412c62f6))
- Rename preprocessor define from `AdvancedMusicManager` to `AdvancedMusicControl` ([`a70e7cf`](https://github.com/JanSharp/VRCMusicControl/commit/a70e7cf434596b8fafe8c37f5a290f4c1a588671))
- Disable UI navigation on HiddenOverlay button ([`8714647`](https://github.com/JanSharp/VRCMusicControl/commit/8714647031537299f81920e77431d078e99c0b69))

### Added

- Add MusicAreaToggleOnInteract plus validation at build time ([`acc938f`](https://github.com/JanSharp/VRCMusicControl/commit/acc938fce31a3797b97ac91821d7129e8c632658))
- Add OnDefaultMusicChanged event on MusicManager ([`9b1671e`](https://github.com/JanSharp/VRCMusicControl/commit/9b1671e9b0a83f27be2358f940758eb5351b949b), [`e16f175`](https://github.com/JanSharp/VRCMusicControl/commit/e16f175d138e2be5ba12edce1a0e4bb9e0bcc8d5))
- Add tooltips to TimeBasedMusicBase ([`e5545d3`](https://github.com/JanSharp/VRCMusicControl/commit/e5545d3b038c5238778fd21dd220441209204cee))
- Add note in summaries about setting BasicMusicControlTimer values raising event ([`d3bd98c`](https://github.com/JanSharp/VRCMusicControl/commit/d3bd98c598b8dd8719a334b83ad28158b351fb44))
- Add editor scripting for TimeBasedMusicBase to validate data at build time ([`c5cc6cd`](https://github.com/JanSharp/VRCMusicControl/commit/c5cc6cd74e8554510082af2b039b9bebcf0e41d5))
- Add debug only debug messages enabled with the MusicControlDebug preprocessor define ([`996ac02`](https://github.com/JanSharp/VRCMusicControl/commit/996ac029eee5b34412993a61164c2e6035b2b667))

### Removed

- Remove DefaultExecutionOrder attribute, I'm pretty sure it doesn't do anything ([`05f69b7`](https://github.com/JanSharp/VRCMusicControl/commit/05f69b78ab23bf6ff1a2564896185bbe1f5eb07f))

### Fixed

- Refactor global time syncing. It may have been broken, it is now fixed ([`b656c40`](https://github.com/JanSharp/VRCMusicControl/commit/b656c404b9fce6bc842424752d684f6dcef02dad))
- Fix time syncing not including speed in calc ([`8551fa4`](https://github.com/JanSharp/VRCMusicControl/commit/8551fa4e3737fe10aff6567f298fea13e79c7caa))

## [1.0.1] - 2023-08-07

### Changed

- Use first time stamp as start in time based music instead of assuming 0 is start ([`91f9cb8`](https://github.com/JanSharp/VRCMusicControl/commit/91f9cb888dcc8c947f848d073c03d7d8786293c5))
- Improve aesthetics of basic timer UI ([`f97b741`](https://github.com/JanSharp/VRCMusicControl/commit/f97b741765604207c9c4d0fb97d8201be6b82799), [`004f1a9`](https://github.com/JanSharp/VRCMusicControl/commit/004f1a9abe225e42bdffe7d715c71c3a7f43ee11))
- Use OnPreSerialization to ensure that other scripts won't break this system's syncing ([`e231360`](https://github.com/JanSharp/VRCMusicControl/commit/e231360d27f970018980c042b5f7b5190c1847f3))
- Improve music descriptor start time tooltip ([`8538dcc`](https://github.com/JanSharp/VRCMusicControl/commit/8538dcc9532f80e20637edb19dd5dc12530ddea8))

### Added

- Add option for basic timer to not be synced ([`3df525a`](https://github.com/JanSharp/VRCMusicControl/commit/3df525a71050c767345d4671ce044343e7f3c489), [`b3a9f98`](https://github.com/JanSharp/VRCMusicControl/commit/b3a9f98b867bd36651ec7d251199a4c12c99ac0a))
- Add tooltips to basic timer UI ([`49036d8`](https://github.com/JanSharp/VRCMusicControl/commit/49036d89be22016323375f12248f4e202aef9e0f))

### Removed

- Remove unused objects from basic timer UI ([`788174d`](https://github.com/JanSharp/VRCMusicControl/commit/788174df7e0638e3785cde7491ef75120dc81b07))
- Remove residual debug log from timer UI ([`7e7a996`](https://github.com/JanSharp/VRCMusicControl/commit/7e7a996240c982282c00c788f99d839e3a6a9911))

### Fixed

- Fix syncing for music for an area off by 1 shift ([`5eb157b`](https://github.com/JanSharp/VRCMusicControl/commit/5eb157b5f686e00bdff20437df1e7fbd40ddc05f))
- Fix timer UI update loop potentially running twice ([`195a72a`](https://github.com/JanSharp/VRCMusicControl/commit/195a72a0075c72815930278fcaa6c35a8f028d71))

## [1.0.0] - 2023-08-06

_For more info about breaking changes see [Migration Instructions](Documentation~/Migration.md#migrating-to-v100)._

### Changed

- **Breaking:** Change manager to automatically use all child descriptors ([`691a14e`](https://github.com/JanSharp/VRCMusicControl/commit/691a14ed723e62e83ec93840e218b3d22de4f5cc), [`69f14e2`](https://github.com/JanSharp/VRCMusicControl/commit/69f14e29073a6d210ea752cfe042e07d8ea9003d), [`bf62597`](https://github.com/JanSharp/VRCMusicControl/commit/bf6259753000b66ed53305a95b6fce45004016b1), [`7c8c601`](https://github.com/JanSharp/VRCMusicControl/commit/7c8c601ad990bc74d929afce091ecdadd56ccffa))
- **Breaking:** Add `[DisallowMultipleComponent]` to manager and descriptor ([`7c06205`](https://github.com/JanSharp/VRCMusicControl/commit/7c06205e8347c1c838274ecbeb0c52907b99dd3f))
- Support changing MusicArea music at runtime ([`8cd413d`](https://github.com/JanSharp/VRCMusicControl/commit/8cd413d3637f1a98403983d884b41b48123f817f))
- Support default music being null ([`1f02dcd`](https://github.com/JanSharp/VRCMusicControl/commit/1f02dcd326f147fa369523f6ff992becdfab3946))
- Support pitch, including reverse pitch ([`7ec563b`](https://github.com/JanSharp/VRCMusicControl/commit/7ec563b8ecfc7e413b2d3158ed95d78d7dd70d9a))
- Add Min attribute to all fade values to prevent them from being negative ([`6c91da1`](https://github.com/JanSharp/VRCMusicControl/commit/6c91da157a84c8d95d3885d7d2f53594f9c28126))
- Improve tooltip for default music ([`6978b59`](https://github.com/JanSharp/VRCMusicControl/commit/6978b597c93a99552acdab196eb0de40ac4814dc))
- Change MusicManager to sync by default ([`ad6ede5`](https://github.com/JanSharp/VRCMusicControl/commit/ad6ede5d7616db8d1491131e5aaa5aa86d93607f))
- Add argument validation to MusicManager.AddMusic ([`c1e9f43`](https://github.com/JanSharp/VRCMusicControl/commit/c1e9f43d79f0b2819ffa44fd5b9fee14b6a478ef))
- Change log messages to use `[MusicControl]` prefix ([`fa4dda9`](https://github.com/JanSharp/VRCMusicControl/commit/fa4dda9d9cc253a0124520597c69e678850a7a43))
- Update and improve package short descriptions ([`f3db390`](https://github.com/JanSharp/VRCMusicControl/commit/f3db3904b44e8f177fda8a744e4c71b3b2e98e63))
- Update readme features, docs and limitations ([`76f814b`](https://github.com/JanSharp/VRCMusicControl/commit/76f814b483d597e830407f152006a9d15e128d6b), [`6d6f0ab`](https://github.com/JanSharp/VRCMusicControl/commit/6d6f0ab3888fa901029284beebdd8b81ebeefdd0))
- Change LICENSE.txt to LICENSE.md so Unity sees it in the package manager window ([`d02a797`](https://github.com/JanSharp/VRCMusicControl/commit/d02a797e7c34c7004d0185ae2d679985eaa9a57f))

### Added

- **Breaking:** Add different start types for MusicDescriptors ([`cd9beff`](https://github.com/JanSharp/VRCMusicControl/commit/cd9beff6de7af96452d7484849e964b06e5dbed4), [`f3933a2`](https://github.com/JanSharp/VRCMusicControl/commit/f3933a2d4a9594769c65aa97049ee99b9f281137), [`a2d5730`](https://github.com/JanSharp/VRCMusicControl/commit/a2d57304907ed26607cbb5d4b95893d7bf891a78), [`3a69ca6`](https://github.com/JanSharp/VRCMusicControl/commit/3a69ca6b1ed455b9b49a5606da4803956e91abcd))
- Add priority overrides and use them on music areas ([`fc83087`](https://github.com/JanSharp/VRCMusicControl/commit/fc83087719338f3ebdf604406fb75a3396a88d83))
- Support MusicArea priority changing at runtime ([`91bfceb`](https://github.com/JanSharp/VRCMusicControl/commit/91bfcebcfb8e665f0724f73a6b8296b6022d2b5e))
- Add IsActive flag to MusicArea to toggle its music ([`47ce57e`](https://github.com/JanSharp/VRCMusicControl/commit/47ce57e964d5df8d6a20b19f57b3ae9322078bec), [`ef087d0`](https://github.com/JanSharp/VRCMusicControl/commit/ef087d0eb7f455cd21dd4b2c3505714e3b8571ea))
- Add utility scripts to change areas or descriptors at runtime ([`e3e0d17`](https://github.com/JanSharp/VRCMusicControl/commit/e3e0d172bbbb419c7166299867e4c5df289b381b))
- Add option to automatically sync runtime changeable fade values on descriptor ([`d96dc6e`](https://github.com/JanSharp/VRCMusicControl/commit/d96dc6ef45689feed7075de16e4284cc2954214f), [`5df4d18`](https://github.com/JanSharp/VRCMusicControl/commit/5df4d1811b0f3f508bdbe9a72560c10f40b4e2b3))
- Add syncing of music and priority to MusicArea ([`0f8dee2`](https://github.com/JanSharp/VRCMusicControl/commit/0f8dee235232805e693d8d0a69712204ad284d58))
- Add abstract time based music base, basic implementation and UI ([`0d50230`](https://github.com/JanSharp/VRCMusicControl/commit/0d5023091bdbcf167e325491202eeda0b1f992a4), [`40d93a6`](https://github.com/JanSharp/VRCMusicControl/commit/40d93a62a3dc2a8afdf71ef5e47a64d258f06bca))
- Add PublicAPI attribute to members tha are apart of the API and add summaries ([`76b30a8`](https://github.com/JanSharp/VRCMusicControl/commit/76b30a857cf092c26a142fa8d91530df993ae3ae), [`7c0ca43`](https://github.com/JanSharp/VRCMusicControl/commit/7c0ca43c73993ebe5ca4a74a992097fef050e45b))
- Add Reset API to descriptor to pretend the next is the first play ([`c6d9130`](https://github.com/JanSharp/VRCMusicControl/commit/c6d9130b6b60b3381244b15ff120afee7c63afdd))
- Expose sync fields as readonly props at runtime ([`0c0b9e3`](https://github.com/JanSharp/VRCMusicControl/commit/0c0b9e3ea0cab6f407e66dfe878f561345427a12))
- Add DefaultExecutionOrder attribute to MusicManager to allow other scripts to use it in their Start ([`2453f78`](https://github.com/JanSharp/VRCMusicControl/commit/2453f78199d2ab83759003312e9e5bd4ae69aeae))
- Add advanced option to change sync modes ([`ba9f867`](https://github.com/JanSharp/VRCMusicControl/commit/ba9f8673f18ac004d06206baa0cc38de49ed71a3), [`c25c951`](https://github.com/JanSharp/VRCMusicControl/commit/c25c9518a16d8837a76ec35568e7f90dda0226fc))
- Add editor scripting for better error handling and quality of life ([`845c1c3`](https://github.com/JanSharp/VRCMusicControl/commit/845c1c33d406fdc01488c9a0551c01a64b26cfb0), [`0d890e9`](https://github.com/JanSharp/VRCMusicControl/commit/0d890e9cb651712ae67c37d4ff615c0747a284c6), [`95049b3`](https://github.com/JanSharp/VRCMusicControl/commit/95049b3103b3133e4b46093eefeac9d3a576bb35), [`1a51e95`](https://github.com/JanSharp/VRCMusicControl/commit/1a51e95f1a15078cb6f5555b457c0a164a27e545), [`2a0e55a`](https://github.com/JanSharp/VRCMusicControl/commit/2a0e55a25a43732aad7f8a1144c64e77506a89f7), [`e3f408d`](https://github.com/JanSharp/VRCMusicControl/commit/e3f408dce7f3d388e730a8a1663608de139b164a))
- Add vpm dependency on `com.jansharp.common` ([`c353b06`](https://github.com/JanSharp/VRCMusicControl/commit/c353b06e249e1322a231ff45b653983690f3e13c))
- Add vpm dependency on `com.vrchat.worlds` for clarity ([`f0af428`](https://github.com/JanSharp/VRCMusicControl/commit/f0af4288cbab80598413eb5a513cfa548a432f4d))
- Add dependency on `com.unity.textmeshpro` ([`92a874f`](https://github.com/JanSharp/VRCMusicControl/commit/92a874fd685feb442cebef730b1c9164a8ff2572))

### Removed

- Remove fade update interval setting, it is now calculated automatically ([`e9eb304`](https://github.com/JanSharp/VRCMusicControl/commit/e9eb304aa86afbed5705e592271cbae5243f3080), [`029e718`](https://github.com/JanSharp/VRCMusicControl/commit/029e71875af0b9d309140562d63cccde81c44863))
- Remove ideas from readme which definitely will not happen ([`0453789`](https://github.com/JanSharp/VRCMusicControl/commit/0453789f9ca1a3548bff34fbd6259de8565d16a8))

## [0.2.3] - 2023-07-23

### Changed

- **Breaking:** Change assembly definitions to not use GUIDs ([`163b27f`](https://github.com/JanSharp/VRCMusicControl/commit/163b27fb24d3886e33f21d7b336ea696af36c616))

### Added

- Add installation instructions in readme ([`5950826`](https://github.com/JanSharp/VRCMusicControl/commit/5950826e054a7535b26fcc0b8ddf1d61d051b072), [`15e7ce4`](https://github.com/JanSharp/VRCMusicControl/commit/15e7ce44bb4e434fc146b27fc385fda505f3d451))

### Fixed

- Fix assembly definitions not following Unity's naming convention ([`655eda8`](https://github.com/JanSharp/VRCMusicControl/commit/655eda8a5da1e253503e6d971b6a2e33f3add872))

## [0.2.2] - 2023-07-16

### Changed

- Add support for MusicAreas with multiple triggers ([`b429565`](https://github.com/JanSharp/VRCMusicControl/commit/b4295655cc89e0257078f3ffc7a4765f7e365405))

### Added

- Add note about MusicAreas at spawn in readme ([`d75eb40`](https://github.com/JanSharp/VRCMusicControl/commit/d75eb40c39cf9706de123fb7347e2db24000aff6))

### Fixed

- **Breaking:** Fix asemdef still being called com.jansharp.dummy ([`99776cc`](https://github.com/JanSharp/VRCMusicControl/commit/99776cca808e950ed1480af0d34c7b69823b83ca))

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

[1.1.0]: https://github.com/JanSharp/VRCMusicControl/releases/tag/v1.1.0
[1.0.1]: https://github.com/JanSharp/VRCMusicControl/releases/tag/v1.0.1
[1.0.0]: https://github.com/JanSharp/VRCMusicControl/releases/tag/v1.0.0
[0.2.3]: https://github.com/JanSharp/VRCMusicControl/releases/tag/v0.2.3
[0.2.2]: https://github.com/JanSharp/VRCMusicControl/releases/tag/v0.2.2
[0.2.1]: https://github.com/JanSharp/VRCMusicControl/releases/tag/v0.2.1
[0.2.0]: https://github.com/JanSharp/VRCMusicControl/releases/tag/MusicControl_v0.2.0
[0.1.1]: https://github.com/JanSharp/VRCMusicControl/releases/tag/MusicControl_v0.1.1
[0.1.0]: https://github.com/JanSharp/VRCMusicControl/releases/tag/MusicControl_v0.1.0
