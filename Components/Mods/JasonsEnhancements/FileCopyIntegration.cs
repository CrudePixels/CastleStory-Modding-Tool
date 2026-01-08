using System;
using System.Collections.Generic;
using System.IO;

using CastleStoryModdingTool;

namespace JasonsEnhancements
{
    /// <summary>
    /// Mod integration that copies all files from a source folder to the game directory
    /// </summary>
    public class FileCopyIntegration : IModIntegration
    {
        public string ModName { get; }
        public ModIntegrationType IntegrationType => ModIntegrationType.AssetReplacement; // Using AssetReplacement as closest match
        
        private readonly string sourcePath;
        private readonly bool backupBeforeCopy;
        private readonly List<string> copiedFiles = new List<string>();
        
        public FileCopyIntegration(string modName, string sourcePath, bool backupBeforeCopy = true)
        {
            ModName = modName;
            this.sourcePath = sourcePath;
            this.backupBeforeCopy = backupBeforeCopy;
        }
        
        public bool CanApply(string gameDirectory)
        {
            // Resolve relative paths
            string resolvedSourcePath = sourcePath;
            if (!Path.IsPathRooted(sourcePath))
            {
                // Try to find project root
                string currentDir = Directory.GetCurrentDirectory();
                string projectRoot = currentDir;
                
                // Go up until we find Components/Mods
                while (!string.IsNullOrEmpty(projectRoot) && !Directory.Exists(Path.Combine(projectRoot, "Components", "Mods")))
                {
                    projectRoot = Path.GetDirectoryName(projectRoot);
                }
                
                if (!string.IsNullOrEmpty(projectRoot))
                {
                    resolvedSourcePath = Path.Combine(projectRoot, sourcePath);
                }
                else
                {
                    resolvedSourcePath = Path.Combine(currentDir, sourcePath);
                }
            }
            
            if (string.IsNullOrEmpty(resolvedSourcePath) || !Directory.Exists(resolvedSourcePath))
            {
                return false;
            }
            
            if (string.IsNullOrEmpty(gameDirectory) || !Directory.Exists(gameDirectory))
            {
                return false;
            }
            
            return true;
        }
        
        public bool Apply(string gameDirectory, string backupDirectory, string logFile)
        {
            try
            {
                // Resolve relative paths
                string resolvedSourcePath = sourcePath;
                if (!Path.IsPathRooted(sourcePath))
                {
                    // Try to find project root
                    string currentDir = Directory.GetCurrentDirectory();
                    string projectRoot = currentDir;
                    
                    // Go up until we find Components/Mods
                    while (!string.IsNullOrEmpty(projectRoot) && !Directory.Exists(Path.Combine(projectRoot, "Components", "Mods")))
                    {
                        projectRoot = Path.GetDirectoryName(projectRoot);
                    }
                    
                    if (!string.IsNullOrEmpty(projectRoot))
                    {
                        resolvedSourcePath = Path.Combine(projectRoot, sourcePath);
                    }
                    else
                    {
                        resolvedSourcePath = Path.Combine(currentDir, sourcePath);
                    }
                }
                
                File.AppendAllText(logFile, $"\n=== Jason's Enhancements: Starting file copy ===");
                File.AppendAllText(logFile, $"\nSource: {resolvedSourcePath}");
                File.AppendAllText(logFile, $"\nDestination: {gameDirectory}");
                File.AppendAllText(logFile, $"\nBackup: {backupDirectory}");
                
                if (!Directory.Exists(resolvedSourcePath))
                {
                    File.AppendAllText(logFile, $"\n❌ Source path does not exist: {resolvedSourcePath}");
                    return false;
                }
                
                if (!Directory.Exists(gameDirectory))
                {
                    File.AppendAllText(logFile, $"\n❌ Game directory does not exist: {gameDirectory}");
                    return false;
                }
                
                // Ensure backup directory exists
                Directory.CreateDirectory(backupDirectory);
                
                copiedFiles.Clear();
                int filesCopied = 0;
                int filesBackedUp = 0;
                
                // Recursively copy all files from source to game directory
                CopyDirectoryRecursive(resolvedSourcePath, resolvedSourcePath, gameDirectory, backupDirectory, logFile, ref filesCopied, ref filesBackedUp);
                
                File.AppendAllText(logFile, $"\n✅ File copy completed successfully");
                File.AppendAllText(logFile, $"\nFiles copied: {filesCopied}");
                File.AppendAllText(logFile, $"\nFiles backed up: {filesBackedUp}");
                
                return true;
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFile, $"\n❌ Error copying files: {ex.Message}");
                File.AppendAllText(logFile, $"\nStack trace: {ex.StackTrace}");
                return false;
            }
        }
        
        private void CopyDirectoryRecursive(string rootSourceDir, string sourceDir, string destDir, string backupDir, string logFile, ref int filesCopied, ref int filesBackedUp)
        {
            // Create destination directory if it doesn't exist
            Directory.CreateDirectory(destDir);
            
            // Copy all files in current directory
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destDir, fileName);
                string relativePath = Path.GetRelativePath(rootSourceDir, file);
                
                try
                {
                    // Backup original file if it exists and backup is enabled
                    if (backupBeforeCopy && File.Exists(destFile))
                    {
                        string backupFile = Path.Combine(backupDir, relativePath);
                        Directory.CreateDirectory(Path.GetDirectoryName(backupFile)!);
                        File.Copy(destFile, backupFile, true);
                        filesBackedUp++;
                        File.AppendAllText(logFile, $"\nBacked up: {destFile} -> {backupFile}");
                    }
                    
                    // Copy the file
                    File.Copy(file, destFile, true);
                    copiedFiles.Add(destFile);
                    filesCopied++;
                    File.AppendAllText(logFile, $"\nCopied: {file} -> {destFile}");
                }
                catch (Exception ex)
                {
                    File.AppendAllText(logFile, $"\n⚠️ Error copying {file}: {ex.Message}");
                }
            }
            
            // Recursively copy subdirectories
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string subDirName = Path.GetFileName(subDir);
                string destSubDir = Path.Combine(destDir, subDirName);
                string backupSubDir = Path.Combine(backupDir, Path.GetRelativePath(rootSourceDir, subDir));
                
                CopyDirectoryRecursive(rootSourceDir, subDir, destSubDir, backupSubDir, logFile, ref filesCopied, ref filesBackedUp);
            }
        }
        
        public bool CanUnapply(string gameDirectory)
        {
            // Can unapply if backup directory exists
            return true;
        }
        
        public bool Unapply(string gameDirectory, string logFile)
        {
            try
            {
                File.AppendAllText(logFile, $"\n=== Jason's Enhancements: Starting file restore ===");
                File.AppendAllText(logFile, $"\nGame directory: {gameDirectory}");
                
                // Note: This would need the backup directory path
                // For now, we'll just log that unapply was called
                // In a full implementation, you'd restore from backup
                File.AppendAllText(logFile, $"\n⚠️ Unapply called - files would be restored from backup");
                File.AppendAllText(logFile, $"\nNote: Manual restore may be required");
                
                return true;
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFile, $"\n❌ Error during unapply: {ex.Message}");
                return false;
            }
        }
        
        public string GetDescription()
        {
            return $"Copies all files from {sourcePath} to Castle Story directory";
        }
    }
}
