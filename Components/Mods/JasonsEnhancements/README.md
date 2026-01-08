# Jason's Enhancements

A simple mod that copies all files from `Components\Mods\Castle Story` to the Castle Story game directory.

## How It Works

When enabled, this mod:
1. Copies all files and folders from `Components\Mods\Castle Story` to the Castle Story game directory
2. Backs up any existing files before overwriting them
3. Preserves the full directory structure

## Setup

1. Place your modded files in: `CastleStoryModdingTool\Components\Mods\Castle Story`
2. Enable "Jason's Enhancements" in the mod manager
3. Launch Castle Story - your files will be copied automatically

## Configuration

The source path is configured in `mod.json`:
- **Source Path**: `Components\Mods\Castle Story` (relative to project root)

## Features

- ✅ Recursive directory copying
- ✅ Automatic backup of original files
- ✅ Preserves directory structure
- ✅ Simple enable/disable toggle
- ✅ Detailed logging

## Notes

- Original files are backed up to: `[GameDirectory]\Info\Lua\ModBackup_[timestamp]\`
- The mod logs all operations to the mod log file
- If a file already exists, it will be backed up before being overwritten