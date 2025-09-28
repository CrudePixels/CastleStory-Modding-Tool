using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CastleStoryModding.ExampleMods
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

        public static bool PatchCastleStoryLimits(Process castleStoryProcess)
        {
            try
            {
                string logPath = @"D:\MyProjects\CASTLE STORY\CastleStoryModdingTool\CastleStoryLauncher\MEMORY_PATCH_LOG.txt";
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

                // Get the main module - use the process's main module information
                IntPtr mainModule = IntPtr.Zero;
                int moduleSize = 0;
                
                try
                {
                    // Get the main module from the process
                    var mainProcessModule = castleStoryProcess.MainModule;
                    if (mainProcessModule != null)
                    {
                        mainModule = mainProcessModule.BaseAddress;
                        moduleSize = mainProcessModule.ModuleMemorySize;
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
                            moduleSize = firstModule.ModuleMemorySize;
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

                // Use the module size we already have
                File.AppendAllText(logPath, $"Using module size: {moduleSize} bytes\n");

                // Search for team limit patterns in memory
                bool patchSuccess = SearchAndPatchTeamLimits(hProcess, mainModule, (uint)moduleSize, logPath);

                CloseHandle(hProcess);
                
                File.AppendAllText(logPath, $"Memory patching completed. Success: {patchSuccess}\n");
                return patchSuccess;
            }
            catch (Exception ex)
            {
                string errorLog = @"D:\MyProjects\CASTLE STORY\CastleStoryModdingTool\CastleStoryLauncher\MEMORY_PATCH_ERROR.txt";
                File.WriteAllText(errorLog, $"Memory patching error: {ex.Message}\nStack trace: {ex.StackTrace}\n");
                return false;
            }
        }

        private static bool SearchAndPatchTeamLimits(IntPtr hProcess, IntPtr moduleBase, uint moduleSize, string logPath)
        {
            try
            {
                File.AppendAllText(logPath, "Searching for team limit patterns...\n");

                // Common patterns for team limits (4 = 0x04, 0x00000004)
                byte[][] searchPatterns = {
                    new byte[] { 0x04, 0x00, 0x00, 0x00 }, // 4 as int32 (little endian)
                    new byte[] { 0x00, 0x00, 0x00, 0x04 }, // 4 as int32 (big endian)
                    new byte[] { 0x04, 0x00 },             // 4 as int16
                    new byte[] { 0x00, 0x04 },             // 4 as int16 (big endian)
                    new byte[] { 0x04 },                   // 4 as byte
                };

                byte[] newValue = { 0x10, 0x00, 0x00, 0x00 }; // 16 as int32 (little endian)

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
                        // Search for patterns in this chunk
                        for (int i = 0; i < buffer.Length - 4; i++)
                        {
                            foreach (var pattern in searchPatterns)
                            {
                                if (PatternMatches(buffer, i, pattern))
                                {
                                    IntPtr patchAddress = new IntPtr(currentAddress.ToInt64() + i);
                                    
                                    // Verify this looks like a team limit (context check)
                                    if (IsLikelyTeamLimit(buffer, i))
                                    {
                                        File.AppendAllText(logPath, $"Found potential team limit at 0x{patchAddress.ToInt64():X}\n");
                                        
                                        if (PatchMemoryValue(hProcess, patchAddress, newValue, logPath))
                                        {
                                            totalPatches++;
                                            File.AppendAllText(logPath, $"Successfully patched team limit at 0x{patchAddress.ToInt64():X}\n");
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

        private static bool IsLikelyTeamLimit(byte[] buffer, int offset)
        {
            // Context checks to avoid patching random 4s
            // Look for patterns that suggest this is a team limit
            
            // Check if there are nearby string references or other game-related values
            for (int i = Math.Max(0, offset - 100); i < Math.Min(buffer.Length - 4, offset + 100); i++)
            {
                // Look for common game-related byte patterns
                if (buffer[i] == 0x74 && buffer[i + 1] == 0x65 && buffer[i + 2] == 0x61 && buffer[i + 3] == 0x6D) // "team"
                    return true;
                if (buffer[i] == 0x6C && buffer[i + 1] == 0x69 && buffer[i + 2] == 0x6D && buffer[i + 3] == 0x69) // "limi"
                    return true;
            }
            
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
    }
}
