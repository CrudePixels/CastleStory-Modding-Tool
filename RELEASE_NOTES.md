# Release Notes

## Version 1.7.0 - Jason's Enhancements Mod

### New Features

#### 🎮 Jason's Enhancements Mod
- **New Mod Type**: File copy mod that copies all files from a source directory to the Castle Story game directory
- **Source Location**: `Components/Mods/Castle Story/`
- **Features**:
  - Recursive directory copying
  - Automatic backup of original files before overwriting
  - Preserves full directory structure
  - Simple enable/disable toggle
  - Detailed logging of all operations

#### 📁 Mod Content Included
- Complete Castle Story mod content including:
  - Gamemode configurations (Conquer mode)
  - Lua scripts for game logic
  - UI panels and menus
  - Language files
  - Game assets (images, JSON configs)

### Improvements
- Updated `.gitignore` to properly track mod source files while excluding build artifacts
- Enhanced mod manager to support file copy mods
- Improved project structure organization

### Technical Details
- **Mod Name**: Jason's Enhancements
- **Mod Type**: AssetReplacement (File Copy)
- **Source Path**: `Components/Mods/Castle Story` (relative to project root)
- **Integration**: Full integration with ModManager system

### Usage
1. Place your modded files in: `Components/Mods/Castle Story/`
2. Enable "Jason's Enhancements" in the mod manager
3. Launch Castle Story - files will be copied automatically
4. Original files are backed up before overwriting

### Files Changed
- Added `Components/Mods/JasonsEnhancements/` - New mod project
- Added `Components/Mods/Castle Story/` - Mod content directory
- Updated `Components/CastleStoryLauncher/ModManager.cs` - Mod registration
- Updated `Components/CastleStoryLauncher/ModDefinitions/JasonsEnhancementsDefinition.cs` - Mod definition
- Updated `.gitignore` - Allow mod source files

---

## Previous Releases

### Version 1.6.0
- Enhanced LAN multiplayer support
- Improved mod management system
- Better error handling

### Version 1.5.0
- Added Electron-based Lua editor
- Improved file handling
- Enhanced UI/UX

### Version 1.4.0
- Initial release with core modding features
- Lua editor support
- Basic mod management
