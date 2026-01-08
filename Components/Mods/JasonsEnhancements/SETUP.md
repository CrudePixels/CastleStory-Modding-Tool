# FileCopyMod Setup Guide

## What This Mod Does

This mod simply copies all files and folders from your source directory to the Castle Story game directory when enabled.

**Source Folder**: `C:\Users\wolf0\OneDrive\Desktop\CASTLE STORY\Modded Castle Story\Castle Story`

## How It Works

1. **Enable the mod** in the mod manager
2. When you launch Castle Story, the mod will:
   - Backup any existing files that will be overwritten
   - Copy all files from your source folder to the game directory
   - Preserve the directory structure

## Configuration

The source path is configured in `mod.json`:

```json
{
  "sourcePath": "C:\\Users\\wolf0\\OneDrive\\Desktop\\CASTLE STORY\\Modded Castle Story\\Castle Story"
}
```

To change the source folder, edit the `sourcePath` value in `mod.json`.

## Building

The mod is included in the solution. Build the entire solution or just the FileCopyMod project:

```batch
dotnet build Components\Mods\FileCopyMod\FileCopyMod.csproj
```

## Usage

1. Place your modded files in the source folder
2. Enable "FileCopyMod" in the Castle Story Modding Tool
3. Launch Castle Story - files will be copied automatically
4. Disable the mod to stop copying (original files are backed up)

## Features

- ✅ Recursive directory copying
- ✅ Automatic backup before overwriting
- ✅ Preserves full directory structure
- ✅ Simple enable/disable toggle
- ✅ Detailed logging

## Notes

- Original files are backed up to: `[GameDirectory]\Info\Lua\ModBackup_[timestamp]\`
- The mod logs all operations to the mod log file
- If a file already exists, it will be backed up before being overwritten
