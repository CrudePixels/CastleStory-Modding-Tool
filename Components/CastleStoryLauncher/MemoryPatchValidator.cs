using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CastleStoryLauncher
{
    public class MemoryPatchValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
        public List<string> Errors { get; set; } = new List<string>();
        public bool IsSafe { get; set; } = true;
        public string? Recommendation { get; set; }
    }

    public class MemoryBackup
    {
        public IntPtr Address { get; set; }
        public byte[] OriginalBytes { get; set; } = Array.Empty<byte>();
        public DateTime BackupTime { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class MemoryPatchValidator
    {
        private readonly List<MemoryBackup> backups;
        private readonly Dictionary<string, List<string>> patchHistory;
        private readonly string backupDirectory;

        public MemoryPatchValidator(string gameDirectory)
        {
            backups = new List<MemoryBackup>();
            patchHistory = new Dictionary<string, List<string>>();
            backupDirectory = Path.Combine(gameDirectory, "MemoryBackups");
            
            if (!Directory.Exists(backupDirectory))
            {
                Directory.CreateDirectory(backupDirectory);
            }
        }

        public MemoryPatchValidationResult ValidatePatch(byte[] searchPattern, byte[] replacementPattern, string patchName)
        {
            var result = new MemoryPatchValidationResult { IsValid = true, IsSafe = true };

            // Validate search pattern
            if (searchPattern == null || searchPattern.Length == 0)
            {
                result.IsValid = false;
                result.Errors.Add("Search pattern cannot be empty");
                return result;
            }

            if (searchPattern.Length < 4)
            {
                result.Warnings.Add($"Search pattern is very short ({searchPattern.Length} bytes) - may have false positives");
                result.IsSafe = false;
                result.Recommendation = "Consider using a longer, more unique search pattern (8+ bytes recommended)";
            }

            // Validate replacement pattern
            if (replacementPattern == null || replacementPattern.Length == 0)
            {
                result.IsValid = false;
                result.Errors.Add("Replacement pattern cannot be empty");
                return result;
            }

            if (searchPattern.Length != replacementPattern.Length)
            {
                result.Errors.Add($"Pattern length mismatch: search={searchPattern.Length}, replacement={replacementPattern.Length}");
                result.IsValid = false;
                return result;
            }

            // Check if patterns are identical
            if (searchPattern.SequenceEqual(replacementPattern))
            {
                result.Warnings.Add("Search and replacement patterns are identical - patch has no effect");
                result.Recommendation = "Review patch configuration";
            }

            // Check for dangerous patterns
            if (ContainsDangerousPattern(replacementPattern))
            {
                result.Warnings.Add("Replacement pattern contains potentially dangerous bytes");
                result.IsSafe = false;
                result.Recommendation = "Exercise extreme caution - this may crash the game";
            }

            // Check patch history for conflicts
            if (patchHistory.ContainsKey(patchName))
            {
                result.Warnings.Add($"Patch '{patchName}' has been applied {patchHistory[patchName].Count} time(s) before");
            }

            return result;
        }

        public bool CreateBackup(IntPtr address, byte[] originalBytes, string description)
        {
            try
            {
                var backup = new MemoryBackup
                {
                    Address = address,
                    OriginalBytes = (byte[])originalBytes.Clone(),
                    BackupTime = DateTime.Now,
                    Description = description
                };

                backups.Add(backup);

                // Save to disk
                string backupFile = Path.Combine(backupDirectory, $"backup_{DateTime.Now:yyyyMMdd_HHmmss}_{address.ToInt64():X}.dat");
                File.WriteAllBytes(backupFile, originalBytes);

                // Save metadata
                string metaFile = backupFile + ".meta";
                File.WriteAllText(metaFile, $"Address: 0x{address.ToInt64():X}\nDescription: {description}\nTime: {DateTime.Now}\nSize: {originalBytes.Length} bytes");

                return true;
            }
            catch
            {
                return false;
            }
        }

        public MemoryBackup? FindBackup(IntPtr address)
        {
            return backups.FirstOrDefault(b => b.Address == address);
        }

        public bool RestoreFromBackup(IntPtr address, Process process)
        {
            var backup = FindBackup(address);
            if (backup == null)
                return false;

            try
            {
                // This would need proper implementation with Windows API
                // For now, just return the backup exists
                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<MemoryBackup> GetAllBackups()
        {
            return new List<MemoryBackup>(backups);
        }

        public bool ClearBackups()
        {
            try
            {
                backups.Clear();
                
                // Optionally clear disk backups
                if (Directory.Exists(backupDirectory))
                {
                    var files = Directory.GetFiles(backupDirectory);
                    foreach (var file in files)
                    {
                        try { File.Delete(file); } catch { }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void RecordPatchApplication(string patchName, string details)
        {
            if (!patchHistory.ContainsKey(patchName))
            {
                patchHistory[patchName] = new List<string>();
            }

            patchHistory[patchName].Add($"{DateTime.Now}: {details}");
        }

        public List<string> GetPatchHistory(string patchName)
        {
            return patchHistory.ContainsKey(patchName) 
                ? new List<string>(patchHistory[patchName]) 
                : new List<string>();
        }

        public Dictionary<string, int> GetPatchStatistics()
        {
            var stats = new Dictionary<string, int>();

            stats["Total Backups"] = backups.Count;
            stats["Total Patches"] = patchHistory.Count;
            stats["Total Applications"] = patchHistory.Values.Sum(h => h.Count);
            stats["Backup Size (bytes)"] = backups.Sum(b => b.OriginalBytes.Length);

            return stats;
        }

        private bool ContainsDangerousPattern(byte[] pattern)
        {
            // Check for common dangerous patterns
            // This is a simplified check - real implementation would be more sophisticated

            // Check for executable code signatures that might indicate code injection
            if (pattern.Length >= 2)
            {
                // INT 3 breakpoint
                if (pattern[0] == 0xCC)
                    return true;

                // RET instruction sequences
                if (pattern[0] == 0xC3 || pattern[0] == 0xC2)
                    return true;

                // JMP instructions
                if (pattern[0] == 0xEB || pattern[0] == 0xE9)
                    return true;
            }

            // Check for null pointer patterns
            int zeroCount = pattern.Count(b => b == 0x00);
            if (zeroCount > pattern.Length * 0.8)
                return true;

            return false;
        }

        public bool ValidateProcessState(Process process)
        {
            try
            {
                if (process == null || process.HasExited)
                    return false;

                // Check if process is responding
                if (!process.Responding)
                    return false;

                // Check memory usage
                long memoryUsage = process.WorkingSet64;
                long maxMemory = 4L * 1024 * 1024 * 1024; // 4GB

                if (memoryUsage > maxMemory)
                {
                    return false; // Process using too much memory
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public MemoryPatchValidationResult ValidateProcessForPatching(Process process)
        {
            var result = new MemoryPatchValidationResult { IsValid = true, IsSafe = true };

            if (process == null)
            {
                result.IsValid = false;
                result.Errors.Add("Process is null");
                return result;
            }

            if (process.HasExited)
            {
                result.IsValid = false;
                result.Errors.Add("Process has exited");
                return result;
            }

            if (!process.Responding)
            {
                result.Warnings.Add("Process is not responding");
                result.IsSafe = false;
            }

            // Check process architecture
            try
            {
                bool is64Bit = Environment.Is64BitOperatingSystem;
                // Add architecture check if needed
            }
            catch
            {
                result.Warnings.Add("Could not determine process architecture");
            }

            // Check if debugging is enabled
            if (Debugger.IsAttached)
            {
                result.Warnings.Add("Debugger is attached - patching may behave differently");
            }

            return result;
        }

        public bool ExportBackup(string exportPath)
        {
            try
            {
                using (var writer = new StreamWriter(exportPath))
                {
                    writer.WriteLine("# Memory Patch Backup Export");
                    writer.WriteLine($"# Generated: {DateTime.Now}");
                    writer.WriteLine($"# Total Backups: {backups.Count}");
                    writer.WriteLine();

                    foreach (var backup in backups)
                    {
                        writer.WriteLine($"## Backup: {backup.Description}");
                        writer.WriteLine($"Address: 0x{backup.Address.ToInt64():X16}");
                        writer.WriteLine($"Time: {backup.BackupTime}");
                        writer.WriteLine($"Size: {backup.OriginalBytes.Length} bytes");
                        writer.WriteLine($"Data: {BitConverter.ToString(backup.OriginalBytes).Replace("-", " ")}");
                        writer.WriteLine();
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ImportBackup(string importPath)
        {
            try
            {
                // Simple implementation - in production would parse the export format
                return File.Exists(importPath);
            }
            catch
            {
                return false;
            }
        }

        public void GenerateReport(string reportPath)
        {
            try
            {
                using (var writer = new StreamWriter(reportPath))
                {
                    writer.WriteLine("=== Memory Patch Validator Report ===");
                    writer.WriteLine($"Generated: {DateTime.Now}");
                    writer.WriteLine();

                    writer.WriteLine("=== Statistics ===");
                    var stats = GetPatchStatistics();
                    foreach (var stat in stats)
                    {
                        writer.WriteLine($"{stat.Key}: {stat.Value}");
                    }
                    writer.WriteLine();

                    writer.WriteLine("=== Backups ===");
                    foreach (var backup in backups)
                    {
                        writer.WriteLine($"- {backup.Description}");
                        writer.WriteLine($"  Address: 0x{backup.Address.ToInt64():X}");
                        writer.WriteLine($"  Size: {backup.OriginalBytes.Length} bytes");
                        writer.WriteLine($"  Time: {backup.BackupTime}");
                        writer.WriteLine();
                    }

                    writer.WriteLine("=== Patch History ===");
                    foreach (var patch in patchHistory)
                    {
                        writer.WriteLine($"- {patch.Key}: {patch.Value.Count} application(s)");
                        foreach (var entry in patch.Value.Take(5)) // Last 5 entries
                        {
                            writer.WriteLine($"  {entry}");
                        }
                        if (patch.Value.Count > 5)
                        {
                            writer.WriteLine($"  ... and {patch.Value.Count - 5} more");
                        }
                        writer.WriteLine();
                    }
                }
            }
            catch
            {
                // Ignore errors in report generation
            }
        }
    }
}

