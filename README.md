# Castle Story Modding Tool

A comprehensive modding and multiplayer enhancement tool for Castle Story, featuring an advanced Lua editor, LAN multiplayer server, and easy-to-use launcher system.

## 🚀 Features

### 🎮 Easy Launcher
- **One-Click Launch**: Launch all components with a single click
- **Auto-Update**: Automatically checks for and downloads updates from GitHub
- **Clean Interface**: Simple, intuitive design for easy navigation

### 📝 Advanced Lua Editor
- **Dual Mode Interface**: 
  - **Easy Mode**: Dynamic form-based editing with auto-generated input fields
  - **Advanced Mode**: Full code editor with syntax highlighting and line numbers
- **Multi-File Support**: Edit Lua, JSON, XML, CSV, TXT, and PNG files
- **Smart Validation**: File-type specific validation for all supported formats
- **Dynamic Parsing**: Automatically detects and parses Castle Story config files

### 🌐 LAN Multiplayer System
- **LAN Server**: Host local multiplayer games without Steam
- **LAN Client**: Connect to local servers with auto-discovery
- **GUI Interface**: User-friendly graphical interfaces for both server and client
- **Real-time Updates**: Live client list and connection status

### ⚙️ Mod Manager
- **Memory Patching**: Runtime modification of game limits and settings
- **Steam Integration**: Launch Castle Story with custom parameters
- **Settings Management**: Configure launch options and advanced settings

## 🎯 Easy Mode Features

The Easy Mode automatically detects and provides form-based editing for:

### Gamemode Configuration
- **Raid Management**: Player attack intervals, enemy levels, corruptron settings
- **Resources**: Bricktron caps, starting unit counts, resource multipliers
- **Global Settings**: Dig permissions, player relations
- **Time of Day**: Day/night cycle, time factors

### File Templates
- **Gamemode Templates**: Complete config.lua with all variables
- **Preset Templates**: Easy/Normal/Hard difficulty settings
- **Bricktron Names**: Custom name collections for units
- **Language Files**: Multi-language translation support

## 📦 Installation

1. Download the latest release from the [Releases](https://github.com/CrudePixels/CastleStory-Modding-Tool/releases) page
2. Extract the ZIP file to your desired location
3. Run `EasyLauncher.exe` to start the modding tool
4. The launcher will automatically check for updates

## 🚀 Quick Start

1. **Launch the Tool**: Run `EasyLauncher.exe`
2. **Open Editor**: Click "📝 Editor" to open the Lua editor
3. **Load a File**: Browse to your Castle Story directory and load a config file
4. **Easy Editing**: Switch to Easy Mode for form-based editing
5. **Create Templates**: Use the file type buttons to create new configurations

## 🛠️ Development

### Building from Source

1. Clone the repository:
   ```bash
   git clone https://github.com/CrudePixels/CastleStory-Modding-Tool.git
   cd CastleStory-Modding-Tool
   ```

2. Build all components:
   ```bash
   # Build Easy Launcher
   cd EasyLauncher
   dotnet build -c Release

   # Build Castle Story Launcher
   cd ../Components/CastleStoryLauncher
   dotnet build -c Release

   # Build LAN Server
   cd ../LANServer
   dotnet build -c Release

   # Build LAN Client
   cd ../LANClient
   dotnet build -c Release
   ```

3. Create release package:
   ```bash
   cd ..
   CreateRelease.bat
   ```

### Project Structure

```
CastleStoryModdingTool/
├── EasyLauncher/              # Main launcher application
├── Components/
│   ├── CastleStoryLauncher/   # Mod manager and Lua editor
│   ├── LANServer/            # LAN multiplayer server
│   └── LANClient/            # LAN multiplayer client
├── logs/                     # Application logs
├── version.txt              # Version information
└── README.md               # This file
```

## 🎮 Castle Story Integration

This tool is designed to work with Castle Story's modding system:

- **Config Files**: Edit `config.lua` files for gamemode settings
- **Language Files**: Modify translation and localization files
- **Asset Files**: Replace images and other game assets
- **Multiplayer**: Enhance multiplayer experience with LAN support

## 🔧 Configuration

### Game Directory
Set your Castle Story installation directory in the editor to enable:
- File browsing
- Template creation
- Asset replacement

### LAN Server
- **Port**: Default 7777 (configurable)
- **Discovery**: UDP broadcast on port 7778
- **Max Players**: Configurable player limit

## 📋 Requirements

- **.NET 9.0 Runtime**: Required for all components
- **Windows 10/11**: Tested on Windows 10 and 11
- **Castle Story**: Steam version recommended

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is open source. See the repository for license details.

## 🐛 Bug Reports

If you encounter any issues, please report them in the [Issues](https://github.com/CrudePixels/CastleStory-Modding-Tool/issues) section.

## 🎉 Acknowledgments

- Castle Story community for inspiration and feedback
- .NET and WPF for the development framework
- GitHub for hosting and version control

---

**Version**: 1.2.0  
**Last Updated**: January 2025  
**Repository**: [https://github.com/CrudePixels/CastleStory-Modding-Tool](https://github.com/CrudePixels/CastleStory-Modding-Tool)