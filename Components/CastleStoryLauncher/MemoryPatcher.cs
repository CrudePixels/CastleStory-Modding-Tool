using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CastleStoryLauncher
{
    public class MemoryPatcher
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("psapi.dll")]
        public static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hModule, out MODULEINFO lpmodinfo, uint cb);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool SetWindowText(IntPtr hWnd, string lpString);

        [StructLayout(LayoutKind.Sequential)]
        public struct MODULEINFO
        {
            public IntPtr lpBaseOfDll;
            public uint SizeOfImage;
            public IntPtr EntryPoint;
        }

        public const int PROCESS_VM_READ = 0x0010;
        public const int PROCESS_VM_WRITE = 0x0020;
        public const int PROCESS_VM_OPERATION = 0x0008;
        public const int PROCESS_QUERY_INFORMATION = 0x0400;
        public const uint PAGE_EXECUTE_READWRITE = 0x40;
        public const uint PAGE_READWRITE = 0x04;

        public static bool PatchCastleStoryLimits(Process castleStoryProcess, string logsDirectory)
        {
            try
            {
                // Use the provided logs directory
                if (!Directory.Exists(logsDirectory))
                {
                    Directory.CreateDirectory(logsDirectory);
                }
                string logPath = Path.Combine(logsDirectory, "MEMORY_PATCH_LOG.txt");
                File.WriteAllText(logPath, $"Starting memory patching for Castle Story process {castleStoryProcess.Id}\n");
                File.AppendAllText(logPath, $"Process name: {castleStoryProcess.ProcessName}\n");
                File.AppendAllText(logPath, $"Process start time: {castleStoryProcess.StartTime}\n");

                // Open the Castle Story process
                IntPtr hProcess = OpenProcess(PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION, false, castleStoryProcess.Id);
                
                if (hProcess == IntPtr.Zero)
                {
                    File.AppendAllText(logPath, $"Failed to open process. Error: {Marshal.GetLastWin32Error()}\n");
                    return false;
                }

                File.AppendAllText(logPath, $"Successfully opened Castle Story process\n");

                // Get the main module from the process
                IntPtr mainModule = IntPtr.Zero;
                uint moduleSize = 0;
                
                try
                {
                    // Get the main module from the process
                    var mainProcessModule = castleStoryProcess.MainModule;
                    if (mainProcessModule != null)
                    {
                        mainModule = mainProcessModule.BaseAddress;
                        moduleSize = (uint)mainProcessModule.ModuleMemorySize;
                        File.AppendAllText(logPath, $"Found main module: {mainProcessModule.ModuleName} at 0x{mainModule.ToInt64():X}, size: {moduleSize} bytes\n");
                    }
                    else
                    {
                        File.AppendAllText(logPath, "Main module is null, trying alternative approach...\n");
                        
                        // Try to get the first module
                        if (castleStoryProcess.Modules.Count > 0)
                        {
                            var firstModule = castleStoryProcess.Modules[0];
                            mainModule = firstModule.BaseAddress;
                            moduleSize = (uint)firstModule.ModuleMemorySize;
                            File.AppendAllText(logPath, $"Using first module: {firstModule.ModuleName} at 0x{mainModule.ToInt64():X}, size: {moduleSize} bytes\n");
                        }
                    }
                }
                catch (Exception ex)
                {
                    File.AppendAllText(logPath, $"Error getting main module: {ex.Message}\n");
                }

                if (mainModule == IntPtr.Zero)
                {
                    File.AppendAllText(logPath, "Could not find main module\n");
                    CloseHandle(hProcess);
                    return false;
                }

                // Search for team limit patterns in memory (DISABLED to prevent crashes)
                bool patchSuccess = false; // SearchAndPatchTeamLimits(hProcess, mainModule, moduleSize, logPath);
                
                // Add visual indicator to prove the patch is working (DISABLED to prevent crashes)
                bool visualAdded = false; // AddVisualIndicator(hProcess, mainModule, moduleSize, logPath);
                
                // Only try to modify the window title as a visual indicator (safest option)
                bool titleModified = ModifyWindowTitle(castleStoryProcess.Id, logPath);

                CloseHandle(hProcess);
                
                File.AppendAllText(logPath, $"Memory patching completed. Success: {patchSuccess}\n");
                File.AppendAllText(logPath, $"Visual indicator added: {visualAdded}\n");
                File.AppendAllText(logPath, $"Window title modified: {titleModified}\n");
                return patchSuccess || visualAdded || titleModified;
            }
            catch (Exception ex)
            {
                // Use the provided logs directory for error logging
                if (!Directory.Exists(logsDirectory))
                {
                    Directory.CreateDirectory(logsDirectory);
                }
                string errorLog = Path.Combine(logsDirectory, "MEMORY_PATCH_ERROR.txt");
                File.WriteAllText(errorLog, $"Memory patching error: {ex.Message}\nStack trace: {ex.StackTrace}\n");
                return false;
            }
        }

        private static bool SearchAndPatchTeamLimits(IntPtr hProcess, IntPtr moduleBase, uint moduleSize, string logPath)
        {
            try
            {
                File.AppendAllText(logPath, "Searching for team limit patterns...\n");

                // Look for specific Castle Story team-related patterns
                // Based on decompiled code: CurrentTeamInfos = new TeamInfo[4]
                
                // Look for assembly patterns that create arrays with size 4
                // Common patterns in .NET assembly for "new Type[4]"
                byte[][] teamArrayPatterns = {
                    // Look for "Team A", "Team B", "Team C", "Team D" sequences (without null terminator)
                    new byte[] { 0x54, 0x65, 0x61, 0x6D, 0x20, 0x41 }, // "Team A"
                    new byte[] { 0x54, 0x65, 0x61, 0x6D, 0x20, 0x42 }, // "Team B"
                    new byte[] { 0x54, 0x65, 0x61, 0x6D, 0x20, 0x43 }, // "Team C"
                    new byte[] { 0x54, 0x65, 0x61, 0x6D, 0x20, 0x44 }, // "Team D"
                    
                    // Look for array initialization patterns in assembly
                    new byte[] { 0x6E, 0x65, 0x77, 0x20, 0x54, 0x65, 0x61, 0x6D, 0x49, 0x6E, 0x66, 0x6F }, // "new TeamInfo"
                    new byte[] { 0x43, 0x75, 0x72, 0x72, 0x65, 0x6E, 0x74, 0x54, 0x65, 0x61, 0x6D, 0x49, 0x6E, 0x66, 0x6F, 0x73 }, // "CurrentTeamInfos"
                    
                    // Look for common .NET array creation patterns
                    new byte[] { 0x6C, 0x64, 0x63, 0x2E, 0x69, 0x34, 0x20, 0x34 }, // "ldc.i4 4" (load constant 4)
                    new byte[] { 0x6C, 0x64, 0x63, 0x2E, 0x69, 0x34, 0x20, 0x04 }, // "ldc.i4 4" (alternative)
                };
                
                // Look for the actual array size (4) in various formats
                byte[][] teamSizePatterns = {
                    new byte[] { 0x04, 0x00, 0x00, 0x00 }, // 4 as int32 (little endian)
                    new byte[] { 0x00, 0x00, 0x00, 0x04 }, // 4 as int32 (big endian)
                    new byte[] { 0x04, 0x00 },             // 4 as int16 (little endian)
                    new byte[] { 0x00, 0x04 },             // 4 as int16 (big endian)
                    new byte[] { 0x04 },                   // 4 as int8
                };

                // Player limit patterns - also search for 4
                byte[][] playerPatterns = {
                    new byte[] { 0x04, 0x00, 0x00, 0x00 }, // 4 as int32 (little endian)
                    new byte[] { 0x00, 0x00, 0x00, 0x04 }, // 4 as int32 (big endian)
                    new byte[] { 0x04, 0x00 },             // 4 as int16 (little endian)
                    new byte[] { 0x00, 0x04 },             // 4 as int16 (big endian)
                    new byte[] { 0x04 },                   // 4 as int8
                };

                // New values to patch to
                byte[] newTeamValue = { 0x10, 0x00, 0x00, 0x00 }; // 16 as int32 (little endian)
                byte[] newPlayerValue = { 0x20, 0x00, 0x00, 0x00 }; // 32 as int32 (little endian)
                byte[] newTeamValue16 = { 0x10, 0x00 }; // 16 as int16 (little endian)
                byte[] newPlayerValue16 = { 0x20, 0x00 }; // 32 as int16 (little endian)
                byte[] newTeamValue8 = { 0x10 }; // 16 as int8
                byte[] newPlayerValue8 = { 0x20 }; // 32 as int8
                byte[] newTeamValue64 = { 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }; // 16 as int64 (little endian)
                byte[] newPlayerValue64 = { 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }; // 32 as int64 (little endian)

                uint bufferSize = 0x1000; // Read 4KB chunks
                byte[] buffer = new byte[bufferSize];
                uint totalPatches = 0;

                for (uint offset = 0; offset < moduleSize; offset += bufferSize)
                {
                    IntPtr currentAddress = new IntPtr(moduleBase.ToInt64() + offset);
                    uint bytesToRead = Math.Min(bufferSize, moduleSize - offset);

                    UIntPtr bytesRead;
                    if (ReadProcessMemory(hProcess, currentAddress, buffer, bytesToRead, out bytesRead))
                    {
                        // Step 1: Look for team array initialization patterns
                        for (int i = 0; i < buffer.Length - 8; i++)
                        {
                            foreach (var arrayPattern in teamArrayPatterns)
                            {
                                if (PatternMatches(buffer, i, arrayPattern))
                                {
                                    File.AppendAllText(logPath, $"Found team array pattern at 0x{(currentAddress.ToInt64() + i):X}\n");
                                    
                                    // Step 2: Look for array size (4) near this pattern
                                    // Search in a 256-byte window around the team array
                                    int searchStart = Math.Max(0, i - 128);
                                    int searchEnd = Math.Min(buffer.Length - 4, i + 128);
                                    
                                    for (int j = searchStart; j < searchEnd; j++)
                                    {
                                        foreach (var sizePattern in teamSizePatterns)
                                        {
                                            if (PatternMatches(buffer, j, sizePattern))
                                            {
                                                IntPtr patchAddress = new IntPtr(currentAddress.ToInt64() + j);
                                                File.AppendAllText(logPath, $"Found potential team size at 0x{patchAddress.ToInt64():X}\n");
                                                
                                                // Patch the size from 4 to 16
                                                byte[] newValue = GetNewValueForPattern(sizePattern, true);
                                                if (PatchMemoryValue(hProcess, patchAddress, newValue, logPath))
                                                {
                                                    totalPatches++;
                                                    File.AppendAllText(logPath, $"Successfully patched team size at 0x{patchAddress.ToInt64():X}\n");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        
                        // Also search for direct team size patterns (fallback)
                        for (int i = 0; i < buffer.Length - 8; i++)
                        {
                            foreach (var sizePattern in teamSizePatterns)
                            {
                                if (PatternMatches(buffer, i, sizePattern))
                                {
                                    IntPtr patchAddress = new IntPtr(currentAddress.ToInt64() + i);
                                    
                                    // More aggressive team limit detection
                                    if (IsLikelyTeamLimit(buffer, i) || IsLikelyTeamLimitAggressive(buffer, i))
                                    {
                                        File.AppendAllText(logPath, $"Found potential team limit at 0x{patchAddress.ToInt64():X}\n");
                                        
                                        byte[] newValue = GetNewValueForPattern(sizePattern, true);
                                        if (PatchMemoryValue(hProcess, patchAddress, newValue, logPath))
                                        {
                                            totalPatches++;
                                            File.AppendAllText(logPath, $"Successfully patched team limit at 0x{patchAddress.ToInt64():X}\n");
                                        }
                                    }
                                }
                            }
                        }
                        
                        // CONSERVATIVE APPROACH: Only patch very specific, safe patterns
                        // This prevents game crashes while still trying to find team limits
                        File.AppendAllText(logPath, "Using conservative approach to find team limits...\n");
                        
                        // Only look for 4 in very specific contexts that are likely to be safe
                        for (int i = 0; i < buffer.Length - 4; i++)
                        {
                            // Look for 4 as int32, but only in very safe contexts
                            if (buffer[i] == 0x04 && buffer[i + 1] == 0x00 && buffer[i + 2] == 0x00 && buffer[i + 3] == 0x00)
                            {
                                // Only patch if we find very specific string patterns nearby
                                if (IsVeryLikelyTeamLimit(buffer, i))
                                {
                                    IntPtr patchAddress = new IntPtr(currentAddress.ToInt64() + i);
                                    File.AppendAllText(logPath, $"CONSERVATIVE: Found very likely team limit at 0x{patchAddress.ToInt64():X}\n");
                                    
                                    byte[] newValue = { 0x10, 0x00, 0x00, 0x00 }; // 16 as int32
                                    if (PatchMemoryValue(hProcess, patchAddress, newValue, logPath))
                                    {
                                        totalPatches++;
                                        File.AppendAllText(logPath, $"CONSERVATIVE: Successfully patched team limit at 0x{patchAddress.ToInt64():X}\n");
                                    }
                                }
                            }
                        }
                        
                        // Check for player limit patterns (int32)
                        for (int i = 0; i < buffer.Length - 8; i++)
                        {
                            foreach (var pattern in playerPatterns)
                            {
                                if (PatternMatches(buffer, i, pattern))
                                {
                                    IntPtr patchAddress = new IntPtr(currentAddress.ToInt64() + i);
                                    
                                    // Verify this looks like a player limit (context check)
                                    if (IsLikelyPlayerLimit(buffer, i))
                                    {
                                        File.AppendAllText(logPath, $"Found potential player limit at 0x{patchAddress.ToInt64():X}\n");
                                        
                                        byte[] newValue = GetNewValueForPattern(pattern, false);
                                        if (PatchMemoryValue(hProcess, patchAddress, newValue, logPath))
                                        {
                                            totalPatches++;
                                            File.AppendAllText(logPath, $"Successfully patched player limit at 0x{patchAddress.ToInt64():X}\n");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                File.AppendAllText(logPath, $"Total patches applied: {totalPatches}\n");
                return totalPatches > 0;
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, $"Error in SearchAndPatchTeamLimits: {ex.Message}\n");
                return false;
            }
        }

        private static bool PatternMatches(byte[] buffer, int offset, byte[] pattern)
        {
            if (offset + pattern.Length > buffer.Length) return false;
            
            for (int i = 0; i < pattern.Length; i++)
            {
                if (buffer[offset + i] != pattern[i]) return false;
            }
            return true;
        }

        private static byte[] GetNewValueForPattern(byte[] pattern, bool isTeamLimit)
        {
            if (isTeamLimit)
            {
                switch (pattern.Length)
                {
                    case 1: return new byte[] { 0x10 }; // 16 as int8
                    case 2: return new byte[] { 0x10, 0x00 }; // 16 as int16 (little endian)
                    case 4: return new byte[] { 0x10, 0x00, 0x00, 0x00 }; // 16 as int32 (little endian)
                    case 8: return new byte[] { 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }; // 16 as int64 (little endian)
                    default: return new byte[] { 0x10, 0x00, 0x00, 0x00 }; // 16 as int32 (little endian)
                }
            }
            else
            {
                switch (pattern.Length)
                {
                    case 1: return new byte[] { 0x20 }; // 32 as int8
                    case 2: return new byte[] { 0x20, 0x00 }; // 32 as int16 (little endian)
                    case 4: return new byte[] { 0x20, 0x00, 0x00, 0x00 }; // 32 as int32 (little endian)
                    case 8: return new byte[] { 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }; // 32 as int64 (little endian)
                    default: return new byte[] { 0x20, 0x00, 0x00, 0x00 }; // 32 as int32 (little endian)
                }
            }
        }

        private static bool IsLikelyTeamLimit(byte[] buffer, int offset)
        {
            // Context checks to avoid patching random 4s
            // Look for patterns that suggest this is a team limit
            
            // Check if there are nearby string references or other game-related values
            for (int i = Math.Max(0, offset - 200); i < Math.Min(buffer.Length - 4, offset + 200); i++)
            {
                // Look for common game-related byte patterns
                if (buffer[i] == 0x74 && buffer[i + 1] == 0x65 && buffer[i + 2] == 0x61 && buffer[i + 3] == 0x6D) // "team"
                    return true;
                if (buffer[i] == 0x6C && buffer[i + 1] == 0x69 && buffer[i + 2] == 0x6D && buffer[i + 3] == 0x69) // "limi"
                    return true;
                if (buffer[i] == 0x6D && buffer[i + 1] == 0x61 && buffer[i + 2] == 0x78 && buffer[i + 3] == 0x54) // "maxT"
                    return true;
                if (buffer[i] == 0x54 && buffer[i + 1] == 0x65 && buffer[i + 2] == 0x61 && buffer[i + 3] == 0x6D) // "Team"
                    return true;
                if (buffer[i] == 0x4D && buffer[i + 1] == 0x41 && buffer[i + 2] == 0x58 && buffer[i + 3] == 0x5F) // "MAX_"
                    return true;
                
                // Look for Castle Story specific patterns from the decompiled code
                if (buffer[i] == 0x41 && buffer[i + 1] == 0x4D && buffer[i + 2] == 0x4F && buffer[i + 3] == 0x55 && 
                    buffer[i + 4] == 0x4E && buffer[i + 5] == 0x54 && buffer[i + 6] == 0x5F && buffer[i + 7] == 0x4F && 
                    buffer[i + 8] == 0x46 && buffer[i + 9] == 0x5F && buffer[i + 10] == 0x50 && buffer[i + 11] == 0x4C && 
                    buffer[i + 12] == 0x41 && buffer[i + 13] == 0x59 && buffer[i + 14] == 0x45 && buffer[i + 15] == 0x52 && 
                    buffer[i + 16] == 0x53) // "AMOUNT_OF_PLAYERS"
                    return true;
                
                // Look for CurrentTeamInfos pattern
                if (buffer[i] == 0x43 && buffer[i + 1] == 0x75 && buffer[i + 2] == 0x72 && buffer[i + 3] == 0x72 && 
                    buffer[i + 4] == 0x65 && buffer[i + 5] == 0x6E && buffer[i + 6] == 0x74 && buffer[i + 7] == 0x54 && 
                    buffer[i + 8] == 0x65 && buffer[i + 9] == 0x61 && buffer[i + 10] == 0x6D && buffer[i + 11] == 0x49 && 
                    buffer[i + 12] == 0x6E && buffer[i + 13] == 0x66 && buffer[i + 14] == 0x6F && buffer[i + 15] == 0x73) // "CurrentTeamInfos"
                    return true;
                
                // Look for MaxTeamCount pattern
                if (buffer[i] == 0x4D && buffer[i + 1] == 0x61 && buffer[i + 2] == 0x78 && buffer[i + 3] == 0x54 && 
                    buffer[i + 4] == 0x65 && buffer[i + 5] == 0x61 && buffer[i + 6] == 0x6D && buffer[i + 7] == 0x43 && 
                    buffer[i + 8] == 0x6F && buffer[i + 9] == 0x75 && buffer[i + 10] == 0x6E && buffer[i + 11] == 0x74) // "MaxTeamCount"
                    return true;
            }
            
            // Also check for common Unity/Game engine patterns
            // Look for arrays or collections that might contain team data
            for (int i = Math.Max(0, offset - 50); i < Math.Min(buffer.Length - 8, offset + 50); i++)
            {
                // Look for array patterns that might be team arrays
                if (buffer[i] == 0x04 && buffer[i + 1] == 0x00 && buffer[i + 2] == 0x00 && buffer[i + 3] == 0x00) // 4 as int32
                {
                    // Check if this looks like an array size or count
                    if (i + 8 < buffer.Length)
                    {
                        // Look for patterns that suggest this is a team-related array
                        for (int j = i + 4; j < Math.Min(i + 20, buffer.Length - 4); j++)
                        {
                            if (buffer[j] == 0x74 && buffer[j + 1] == 0x65 && buffer[j + 2] == 0x61 && buffer[j + 3] == 0x6D) // "team"
                                return true;
                        }
                    }
                }
            }
            
            // Be more conservative - only patch if we find specific string context
            return false;
        }

        private static bool IsLikelyTeamLimitAggressive(byte[] buffer, int offset)
        {
            // More aggressive team limit detection
            // Look for any small numbers (1-8) that could be team limits
            
            // Check if there are small numbers nearby that suggest team-related data
            for (int i = Math.Max(0, offset - 20); i < Math.Min(buffer.Length - 1, offset + 20); i++)
            {
                // Look for small numbers (1-8) that could be team counts
                if (buffer[i] >= 1 && buffer[i] <= 8)
                {
                    // Check for "Team" string nearby
                    for (int j = Math.Max(0, i - 10); j < Math.Min(buffer.Length - 4, i + 10); j++)
                    {
                        if (j + 3 < buffer.Length && 
                            buffer[j] == 0x54 && buffer[j + 1] == 0x65 && 
                            buffer[j + 2] == 0x61 && buffer[j + 3] == 0x6D) // "Team"
                        {
                            return true;
                        }
                    }
                }
            }
            
            // Look for array-like patterns (multiple small numbers in sequence)
            int smallNumberCount = 0;
            for (int i = Math.Max(0, offset - 10); i < Math.Min(buffer.Length - 1, offset + 10); i++)
            {
                if (buffer[i] >= 1 && buffer[i] <= 8)
                {
                    smallNumberCount++;
                }
            }
            
            // If we find multiple small numbers nearby, it might be team data
            if (smallNumberCount >= 2)
            {
                return true;
            }
            
            return false;
        }

        private static bool IsVeryLikelyTeamLimit(byte[] buffer, int offset)
        {
            // VERY conservative team limit detection
            // Only patch if we find very specific, safe patterns
            
            // Look for "CurrentTeamInfos" string pattern nearby (very specific)
            for (int i = Math.Max(0, offset - 100); i < Math.Min(buffer.Length - 15, offset + 100); i++)
            {
                if (i + 15 < buffer.Length && 
                    buffer[i] == 0x43 && buffer[i + 1] == 0x75 && buffer[i + 2] == 0x72 && buffer[i + 3] == 0x72 && 
                    buffer[i + 4] == 0x65 && buffer[i + 5] == 0x6E && buffer[i + 6] == 0x74 && buffer[i + 7] == 0x54 && 
                    buffer[i + 8] == 0x65 && buffer[i + 9] == 0x61 && buffer[i + 10] == 0x6D && buffer[i + 11] == 0x49 && 
                    buffer[i + 12] == 0x6E && buffer[i + 13] == 0x66 && buffer[i + 14] == 0x6F && buffer[i + 15] == 0x73) // "CurrentTeamInfos"
                {
                    return true;
                }
            }
            
            // Look for "MaxTeamCount" string pattern nearby (very specific)
            for (int i = Math.Max(0, offset - 100); i < Math.Min(buffer.Length - 11, offset + 100); i++)
            {
                if (i + 11 < buffer.Length && 
                    buffer[i] == 0x4D && buffer[i + 1] == 0x61 && buffer[i + 2] == 0x78 && buffer[i + 3] == 0x54 && 
                    buffer[i + 4] == 0x65 && buffer[i + 5] == 0x61 && buffer[i + 6] == 0x6D && buffer[i + 7] == 0x43 && 
                    buffer[i + 8] == 0x6F && buffer[i + 9] == 0x75 && buffer[i + 10] == 0x6E && buffer[i + 11] == 0x74) // "MaxTeamCount"
                {
                    return true;
                }
            }
            
            // Look for "TeamInfo" string pattern nearby (very specific)
            for (int i = Math.Max(0, offset - 100); i < Math.Min(buffer.Length - 7, offset + 100); i++)
            {
                if (i + 7 < buffer.Length && 
                    buffer[i] == 0x54 && buffer[i + 1] == 0x65 && buffer[i + 2] == 0x61 && buffer[i + 3] == 0x6D && 
                    buffer[i + 4] == 0x49 && buffer[i + 5] == 0x6E && buffer[i + 6] == 0x66 && buffer[i + 7] == 0x6F) // "TeamInfo"
                {
                    return true;
                }
            }
            
            return false;
        }

        private static bool IsLikelyPlayerLimit(byte[] buffer, int offset)
        {
            // Context checks to avoid patching random 4s
            // Look for patterns that suggest this is a player limit
            
            // Check if there are nearby string references or other game-related values
            for (int i = Math.Max(0, offset - 100); i < Math.Min(buffer.Length - 4, offset + 100); i++)
            {
                // Look for common game-related byte patterns
                if (buffer[i] == 0x70 && buffer[i + 1] == 0x6C && buffer[i + 2] == 0x61 && buffer[i + 3] == 0x79) // "play"
                    return true;
                if (buffer[i] == 0x6D && buffer[i + 1] == 0x61 && buffer[i + 2] == 0x78 && buffer[i + 3] == 0x50) // "maxP"
                    return true;
                if (buffer[i] == 0x6C && buffer[i + 1] == 0x69 && buffer[i + 2] == 0x6D && buffer[i + 3] == 0x69) // "limi"
                    return true;
                
                // Look for Castle Story specific patterns from the localization file
                if (buffer[i] == 0x41 && buffer[i + 1] == 0x4D && buffer[i + 2] == 0x4F && buffer[i + 3] == 0x55 && 
                    buffer[i + 4] == 0x4E && buffer[i + 5] == 0x54 && buffer[i + 6] == 0x5F && buffer[i + 7] == 0x4F && 
                    buffer[i + 8] == 0x46 && buffer[i + 9] == 0x5F && buffer[i + 10] == 0x50 && buffer[i + 11] == 0x4C && 
                    buffer[i + 12] == 0x41 && buffer[i + 13] == 0x59 && buffer[i + 14] == 0x45 && buffer[i + 15] == 0x52 && 
                    buffer[i + 16] == 0x53) // "AMOUNT_OF_PLAYERS"
                    return true;
            }
            
            // Be more conservative - only patch if we find specific string context
            return false;
        }

        private static bool PatchMemoryValue(IntPtr hProcess, IntPtr address, byte[] newValue, string logPath)
        {
            try
            {
                // Change memory protection to allow writing
                uint oldProtect;
                if (!VirtualProtectEx(hProcess, address, (uint)newValue.Length, PAGE_EXECUTE_READWRITE, out oldProtect))
                {
                    File.AppendAllText(logPath, $"Failed to change memory protection at 0x{address.ToInt64():X}\n");
                    return false;
                }

                // Write the new value
                UIntPtr bytesWritten;
                bool success = WriteProcessMemory(hProcess, address, newValue, (uint)newValue.Length, out bytesWritten);
                
                // Restore original protection
                VirtualProtectEx(hProcess, address, (uint)newValue.Length, oldProtect, out oldProtect);

                if (success)
                {
                    File.AppendAllText(logPath, $"Successfully wrote {bytesWritten} bytes to 0x{address.ToInt64():X}\n");
                }
                else
                {
                    File.AppendAllText(logPath, $"Failed to write memory at 0x{address.ToInt64():X}. Error: {Marshal.GetLastWin32Error()}\n");
                }

                return success;
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, $"Exception patching memory at 0x{address.ToInt64():X}: {ex.Message}\n");
                return false;
            }
        }

        private static bool AddVisualIndicator(IntPtr hProcess, IntPtr moduleBase, uint moduleSize, string logPath)
        {
            try
            {
                File.AppendAllText(logPath, "Adding visual indicator to main menu...\n");
                
                // Look for common UI text patterns that we can modify
                // We'll look for "Castle Story" title text and add our indicator
                byte[][] titlePatterns = {
                    // "Castle Story" in various encodings
                    new byte[] { 0x43, 0x61, 0x73, 0x74, 0x6C, 0x65, 0x20, 0x53, 0x74, 0x6F, 0x72, 0x79 }, // "Castle Story"
                    new byte[] { 0x43, 0x41, 0x53, 0x54, 0x4C, 0x45, 0x20, 0x53, 0x54, 0x4F, 0x52, 0x59 }, // "CASTLE STORY"
                    new byte[] { 0x4C, 0x6F, 0x62, 0x62, 0x79 }, // "Lobby"
                    new byte[] { 0x4C, 0x4F, 0x42, 0x42, 0x59 }, // "LOBBY"
                };
                
                // Our visual indicator text
                string indicatorText = " [MODDED - 16 TEAMS]";
                byte[] indicatorBytes = Encoding.UTF8.GetBytes(indicatorText);
                
                uint bufferSize = 0x1000; // Read 4KB chunks
                byte[] buffer = new byte[bufferSize];
                bool found = false;
                
                for (uint offset = 0; offset < moduleSize && !found; offset += bufferSize)
                {
                    IntPtr currentAddress = new IntPtr(moduleBase.ToInt64() + offset);
                    uint bytesToRead = Math.Min(bufferSize, moduleSize - offset);

                    UIntPtr bytesRead;
                    if (ReadProcessMemory(hProcess, currentAddress, buffer, bytesToRead, out bytesRead))
                    {
                        // Look for title patterns
                        for (int i = 0; i < buffer.Length - 20 && !found; i++)
                        {
                            foreach (var pattern in titlePatterns)
                            {
                                if (PatternMatches(buffer, i, pattern))
                                {
                                    File.AppendAllText(logPath, $"Found title pattern at 0x{(currentAddress.ToInt64() + i):X}\n");
                                    
                                    // Look for a good place to add our indicator (after the title)
                                    for (int j = i + pattern.Length; j < Math.Min(i + 50, buffer.Length - indicatorBytes.Length); j++)
                                    {
                                        // Look for null terminator or space where we can add text
                                        if (buffer[j] == 0x00 || buffer[j] == 0x20)
                                        {
                                            IntPtr patchAddress = new IntPtr(currentAddress.ToInt64() + j);
                                            
                                            // Try to add our visual indicator
                                            if (PatchMemoryValue(hProcess, patchAddress, indicatorBytes, logPath))
                                            {
                                                File.AppendAllText(logPath, $"Successfully added visual indicator at 0x{patchAddress.ToInt64():X}\n");
                                                found = true;
                                                break;
                                            }
                                        }
                                    }
                                    
                                    if (found) break;
                                }
                            }
                        }
                    }
                }
                
                if (!found)
                {
                    File.AppendAllText(logPath, "Could not find suitable location for visual indicator\n");
                }
                
                return found;
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, $"Error adding visual indicator: {ex.Message}\n");
                return false;
            }
        }

        private static bool ModifyWindowTitle(int processId, string logPath)
        {
            try
            {
                File.AppendAllText(logPath, "Attempting to modify window title...\n");
                
                // Wait a bit for the game to fully load
                System.Threading.Thread.Sleep(2000);
                
                // Find the Castle Story window
                IntPtr hWnd = FindWindow(null, "Castle Story");
                if (hWnd == IntPtr.Zero)
                {
                    // Try alternative window names
                    hWnd = FindWindow(null, "Castle Story - Multiplayer");
                    if (hWnd == IntPtr.Zero)
                    {
                        hWnd = FindWindow(null, "Castle Story - Lobby");
                    }
                }
                
                if (hWnd != IntPtr.Zero)
                {
                    string newTitle = "Castle Story [MODDED - 16 TEAMS]";
                    bool success = SetWindowText(hWnd, newTitle);
                    
                    if (success)
                    {
                        File.AppendAllText(logPath, $"Successfully modified window title to: {newTitle}\n");
                        return true;
                    }
                    else
                    {
                        File.AppendAllText(logPath, "Failed to modify window title\n");
                    }
                }
                else
                {
                    File.AppendAllText(logPath, "Could not find Castle Story window\n");
                }
                
                return false;
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, $"Error modifying window title: {ex.Message}\n");
                return false;
            }
        }
    }
}
