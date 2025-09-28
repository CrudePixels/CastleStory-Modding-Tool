# Castle Story Modding Tool - Project Structure

## 📁 Root Directory
```
CastleStoryModdingTool/
├── 📄 CastleStoryModdingTool.sln     # Visual Studio solution file
├── 📄 build-all.bat                  # Build all components script
├── 📄 CreateRelease.bat              # Create release package script
├── 📄 Launch.bat                     # Main launcher script
├── 📄 README.md                      # Project documentation
├── 📄 version.txt                    # Current version (1.2.0)
├── 📄 .gitignore                     # Git ignore rules
├── 📁 .github/                       # GitHub Actions workflows
│   └── 📁 workflows/
│       └── 📄 release.yml            # Automated release workflow
├── 📁 Components/                    # All modding tool components
├── 📁 EasyLauncher/                  # Main launcher application
├── 📁 logs/                          # Application logs
└── 📁 Release/                       # Generated release package
```

## 🧩 Components Directory
```
Components/
├── 📁 CastleStoryLauncher/           # Main mod manager and Lua editor
│   ├── 📄 CastleStoryLauncher.csproj
│   ├── 📄 App.xaml / App.xaml.cs
│   ├── 📄 MainWindow.xaml / MainWindow.xaml.cs
│   ├── 📄 LuaEditorWindow.xaml / LuaEditorWindow.xaml.cs
│   ├── 📄 MemoryPatcher.cs
│   ├── 📄 launcher.ico
│   ├── 📄 launcher_settings.json
│   └── 📁 Mods/                      # Mod storage directory
├── 📁 LANServer/                     # LAN multiplayer server
│   ├── 📄 LANServer.csproj
│   ├── 📄 Program.cs
│   ├── 📄 LANServerGUI.cs
│   ├── 📄 ClientHandler.cs
│   └── 📄 server.ico
├── 📁 LANClient/                     # LAN multiplayer client
│   ├── 📄 LANClient.csproj
│   ├── 📄 Program.cs
│   ├── 📄 LANClientGUI.cs
│   └── 📄 client.ico
├── 📁 MultiplayerServer/             # Additional multiplayer server
│   ├── 📄 MultiplayerServer.csproj
│   └── 📄 Program.cs
├── 📁 Mods/                          # Mod development
│   └── 📁 MultiplayerMod/
│       ├── 📄 MultiplayerMod.csproj
│       ├── 📄 MemoryPatcher.cs
│       ├── 📄 mod.json
│       ├── 📄 SimpleTestMod.cs
│       └── 📄 TestMultiplayerMod.cs
└── 📁 ExampleMods/                   # Example mods
    ├── 📄 mod.json
    └── 📄 MultiplayerMod.cs
```

## 🚀 EasyLauncher Directory
```
EasyLauncher/
├── 📄 EasyLauncher.csproj
└── 📄 Program.cs                      # Main launcher logic with auto-update
```

## 📋 Key Files Description

### **Core Application Files**
- **`CastleStoryModdingTool.sln`**: Visual Studio solution file for the entire project
- **`build-all.bat`**: Builds all components in the correct order
- **`CreateRelease.bat`**: Creates a complete release package for distribution
- **`Launch.bat`**: Main entry point that launches the Easy Launcher

### **Configuration Files**
- **`version.txt`**: Current version number (used by auto-updater)
- **`.gitignore`**: Git ignore rules to exclude build artifacts
- **`README.md`**: Comprehensive project documentation

### **GitHub Integration**
- **`.github/workflows/release.yml`**: Automated build and release workflow
- **Auto-updater**: Checks GitHub releases for updates

## 🎯 Component Responsibilities

### **EasyLauncher** (Main Entry Point)
- **Purpose**: Primary launcher with auto-update functionality
- **Features**: 
  - GitHub integration for updates
  - One-click access to all components
  - Clean, user-friendly interface

### **CastleStoryLauncher** (Mod Manager)
- **Purpose**: Core modding functionality
- **Features**:
  - Memory patching for game limits
  - Advanced Lua editor with Easy/Advanced modes
  - Multi-file support (Lua, JSON, XML, CSV, TXT, PNG)
  - Dynamic form generation for Castle Story configs
  - File validation system

### **LANServer** (Multiplayer Server)
- **Purpose**: Host LAN multiplayer games
- **Features**:
  - UDP discovery system
  - Real-time client management
  - GUI interface
  - Configurable settings

### **LANClient** (Multiplayer Client)
- **Purpose**: Connect to LAN servers
- **Features**:
  - Auto-discovery of servers
  - Connection management
  - GUI interface
  - Real-time status updates

### **MultiplayerServer** (Additional Server)
- **Purpose**: Alternative multiplayer server implementation
- **Features**: Additional server functionality

### **Mods/MultiplayerMod** (Game Modifications)
- **Purpose**: Runtime game modifications
- **Features**:
  - Memory patching
  - Player/team limit modifications
  - Runtime configuration

## 🔧 Build Process

1. **Development**: Edit source files in respective component directories
2. **Build**: Run `build-all.bat` to build all components
3. **Test**: Test individual components or use `Launch.bat`
4. **Release**: Run `CreateRelease.bat` to create distribution package
5. **Deploy**: Upload release package to GitHub releases

## 📦 Release Package Structure

When `CreateRelease.bat` is run, it creates:
```
Release/
├── 📄 EasyLauncher.exe               # Main launcher
├── 📄 EasyLauncher.dll
├── 📄 README.md                      # Documentation
├── 📄 version.txt                    # Version info
├── 📁 Components/                    # All components
│   ├── 📁 CastleStoryLauncher/
│   ├── 📁 LANServer/
│   ├── 📁 LANClient/
│   ├── 📁 MultiplayerServer/
│   └── 📁 Mods/
└── 📁 logs/                          # Log directory
```

## 🎮 Usage Flow

1. **User downloads** release package from GitHub
2. **Runs** `EasyLauncher.exe` (main entry point)
3. **Auto-updater** checks for newer versions
4. **User selects** desired functionality:
   - Mod Manager (Castle Story Launcher)
   - Editor (Lua Editor)
   - LAN Server
   - LAN Client
5. **Components launch** with full functionality

This structure provides a clean, organized, and maintainable codebase that's easy to understand and extend.
