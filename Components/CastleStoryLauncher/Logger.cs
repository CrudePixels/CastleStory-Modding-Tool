using System;
using System.IO;
using System.Text;
using System.Threading;

namespace CastleStoryLauncher
{
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static string _logDirectory = "";
        private static bool _initialized = false;

        public static void Initialize(string logDirectory)
        {
            lock (_lock)
            {
                if (_initialized) return;

                _logDirectory = logDirectory;
                if (!Directory.Exists(_logDirectory))
                {
                    Directory.CreateDirectory(_logDirectory);
                }
                _initialized = true;
            }
        }

        public static void LogInfo(string message, string component = "General")
        {
            Log(LogLevel.Info, message, component);
        }

        public static void LogWarning(string message, string component = "General")
        {
            Log(LogLevel.Warning, message, component);
        }

        public static void LogError(string message, Exception? ex = null, string component = "General")
        {
            var fullMessage = message;
            if (ex != null)
            {
                fullMessage += $"\nException: {ex.Message}\nStack Trace: {ex.StackTrace}";
            }
            Log(LogLevel.Error, fullMessage, component);
        }

        public static void LogDebug(string message, string component = "General")
        {
            Log(LogLevel.Debug, message, component);
        }

        private static void Log(LogLevel level, string message, string component)
        {
            if (!_initialized)
            {
                Initialize(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CastleStoryModdingTool", "logs"));
            }

            lock (_lock)
            {
                try
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var logEntry = $"[{timestamp}] [{level}] [{component}] {message}";
                    
                    // Write to general log
                    var generalLogFile = Path.Combine(_logDirectory, "general.log");
                    File.AppendAllText(generalLogFile, logEntry + Environment.NewLine);
                    
                    // Write to component-specific log
                    var componentLogFile = Path.Combine(_logDirectory, $"{component.ToLower()}.log");
                    File.AppendAllText(componentLogFile, logEntry + Environment.NewLine);
                    
                    // Also write to console for debugging
                    Console.WriteLine(logEntry);
                }
                catch (Exception ex)
                {
                    // Fallback to console if file logging fails
                    Console.WriteLine($"Logging failed: {ex.Message}");
                    Console.WriteLine($"Original message: [{level}] [{component}] {message}");
                }
            }
        }

        public static void ClearOldLogs(int daysToKeep = 7)
        {
            if (!_initialized) return;

            lock (_lock)
            {
                try
                {
                    var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                    var logFiles = Directory.GetFiles(_logDirectory, "*.log");
                    
                    foreach (var logFile in logFiles)
                    {
                        var fileInfo = new FileInfo(logFile);
                        if (fileInfo.CreationTime < cutoffDate)
                        {
                            File.Delete(logFile);
                            LogInfo($"Deleted old log file: {Path.GetFileName(logFile)}", "Logger");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to clear old logs: {ex.Message}");
                }
            }
        }

        public static string GetLogContent(string component = "General", int maxLines = 1000)
        {
            if (!_initialized) return "";

            try
            {
                var logFile = Path.Combine(_logDirectory, $"{component.ToLower()}.log");
                if (!File.Exists(logFile)) return "";

                var lines = File.ReadAllLines(logFile);
                var startIndex = Math.Max(0, lines.Length - maxLines);
                var recentLines = lines.Skip(startIndex).ToArray();
                
                return string.Join(Environment.NewLine, recentLines);
            }
            catch (Exception ex)
            {
                return $"Failed to read log: {ex.Message}";
            }
        }
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }
}
