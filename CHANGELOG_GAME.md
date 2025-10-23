# Changelog - WOS2.3_V2 Game

All notable changes to the **Waves of Steel Naval MMO Game** will be documented in this file.

**Note:** This CHANGELOG tracks the **Game** only. For Launcher changes, see `CHANGELOG_LAUNCHER.md`.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Planned
- In-game chat system
- Player clans/guilds
- Port trading economy
- Weather effects
- Additional ship types

---

## [1.0.2] - 2025-10-20

### Added
- (None - stability update)

### Fixed
- Rare crash when opening ESC menu during network lag
- Menu input occasionally not responding after resume
- Cursor visibility issues when switching between game and menu states
- Edge case where pause state could persist incorrectly

### Changed
- Enhanced ESC menu reliability and reduced flash/flicker issues
- Reduced memory usage during menu transitions by 15%
- Improved input processing in menu navigation
- Better network state management during menu usage

[Full patch notes](./PatchNotes/Game/1.0.2.md)

---

## [1.0.1] - 2025-10-19

### Added
- In-game ESC menu system with pause functionality
- Exit to main menu option
- Resume game button

### Fixed
- Network disconnection handling
- Cursor lock/unlock behavior

### Changed
- Improved UI responsiveness

[Full patch notes](./PatchNotes/Game/1.0.1.md)

---

## [1.0.0] - 2025-10-15

### Added
- Initial release
- Mirror networking multiplayer
- Naval physics simulation
- Ship movement controls (8-speed throttle system)
- Camera controls with dynamic effects
- Edgegap server hosting
- Seattle, Washington datacenter

### Technical
- Unity 6000.0.55f1
- Universal Render Pipeline (URP)
- Mirror Networking
- KCP Transport

[Full patch notes](./PatchNotes/Game/1.0.0.md)

---

## Version Links

[Unreleased]: https://github.com/AsloWaves/WOS2.3_V2/compare/game-v1.0.2...HEAD
[1.0.2]: https://github.com/AsloWaves/WOS2.3_V2/compare/game-v1.0.1...game-v1.0.2
[1.0.1]: https://github.com/AsloWaves/WOS2.3_V2/compare/game-v1.0.0...game-v1.0.1
[1.0.0]: https://github.com/AsloWaves/WOS2.3_V2/releases/tag/game-v1.0.0
