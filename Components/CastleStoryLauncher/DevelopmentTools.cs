using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace CastleStoryLauncher
{
    public class PerformanceMetric
    {
        public string Name { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public class DebugCommand
    {
        public string Command { get; set; } = string.Empty;
        public Func<string[], string> Handler { get; set; } = args => "Command not implemented";
        public string Description { get; set; } = string.Empty;
        public string Usage { get; set; } = string.Empty;
    }

    public class DevelopmentTools
    {
        private readonly List<PerformanceMetric> metrics;
        private readonly Dictionary<string, DebugCommand> commands;
        private readonly List<string> consoleHistory;
        private readonly string logDirectory;
        private Stopwatch? currentTimer;
        private readonly Dictionary<string, Stopwatch> namedTimers;

        public DevelopmentTools(string logDirectory)
        {
            this.logDirectory = logDirectory;
            metrics = new List<PerformanceMetric>();
            commands = new Dictionary<string, DebugCommand>();
            consoleHistory = new List<string>();
            namedTimers = new Dictionary<string, Stopwatch>();

            RegisterDefaultCommands();
        }

        private void RegisterDefaultCommands()
        {
            RegisterCommand("help", args =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("Available Commands:");
                foreach (var cmd in commands.OrderBy(c => c.Key))
                {
                    sb.AppendLine($"  {cmd.Key}: {cmd.Value.Description}");
                    if (!string.IsNullOrEmpty(cmd.Value.Usage))
                    {
                        sb.AppendLine($"    Usage: {cmd.Value.Usage}");
                    }
                }
                return sb.ToString();
            }, "Show all available commands", "help");

            RegisterCommand("metrics", args =>
            {
                if (metrics.Count == 0)
                    return "No metrics recorded";

                var sb = new StringBuilder();
                sb.AppendLine("Performance Metrics:");
                foreach (var metric in metrics.OrderByDescending(m => m.Timestamp).Take(20))
                {
                    sb.AppendLine($"  [{metric.Timestamp:HH:mm:ss}] {metric.Name}: {metric.Value:F2} {metric.Unit}");
                }
                return sb.ToString();
            }, "Display recorded performance metrics", "metrics");

            RegisterCommand("clear", args =>
            {
                consoleHistory.Clear();
                return "Console history cleared";
            }, "Clear console history", "clear");

            RegisterCommand("history", args =>
            {
                if (consoleHistory.Count == 0)
                    return "No history";

                var sb = new StringBuilder();
                sb.AppendLine("Command History:");
                for (int i = 0; i < consoleHistory.Count; i++)
                {
                    sb.AppendLine($"  {i + 1}. {consoleHistory[i]}");
                }
                return sb.ToString();
            }, "Show command history", "history");

            RegisterCommand("status", args =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("System Status:");
                sb.AppendLine($"  Memory Usage: {GC.GetTotalMemory(false) / 1024 / 1024} MB");
                sb.AppendLine($"  GC Collections (Gen 0): {GC.CollectionCount(0)}");
                sb.AppendLine($"  GC Collections (Gen 1): {GC.CollectionCount(1)}");
                sb.AppendLine($"  GC Collections (Gen 2): {GC.CollectionCount(2)}");
                sb.AppendLine($"  Metrics Recorded: {metrics.Count}");
                sb.AppendLine($"  Active Timers: {namedTimers.Count}");
                return sb.ToString();
            }, "Show system status", "status");

            RegisterCommand("gc", args =>
            {
                long before = GC.GetTotalMemory(false);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                long after = GC.GetTotalMemory(false);
                long freed = before - after;
                return $"Garbage collection completed. Freed: {freed / 1024 / 1024} MB";
            }, "Force garbage collection", "gc");

            RegisterCommand("timer", args =>
            {
                if (args.Length == 0)
                    return "Usage: timer [start|stop|reset]";

                string action = args[0].ToLower();
                switch (action)
                {
                    case "start":
                        if (currentTimer == null || !currentTimer.IsRunning)
                        {
                            currentTimer = Stopwatch.StartNew();
                            return "Timer started";
                        }
                        return "Timer already running";

                    case "stop":
                        if (currentTimer != null && currentTimer.IsRunning)
                        {
                            currentTimer.Stop();
                            return $"Timer stopped at {currentTimer.ElapsedMilliseconds}ms";
                        }
                        return "No timer running";

                    case "reset":
                        currentTimer = null;
                        return "Timer reset";

                    default:
                        return $"Unknown action: {action}";
                }
            }, "Control performance timer", "timer [start|stop|reset]");
        }

        public void RegisterCommand(string name, Func<string[], string> handler, string description, string usage = "")
        {
            commands[name.ToLower()] = new DebugCommand
            {
                Command = name,
                Handler = handler,
                Description = description,
                Usage = usage
            };
        }

        public string ExecuteCommand(string commandLine)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(commandLine))
                    return "";

                consoleHistory.Add(commandLine);

                var parts = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                    return "";

                string commandName = parts[0].ToLower();
                string[] args = parts.Skip(1).ToArray();

                if (commands.ContainsKey(commandName))
                {
                    return commands[commandName].Handler(args);
                }

                return $"Unknown command: {commandName}. Type 'help' for available commands.";
            }
            catch (Exception ex)
            {
                return $"Error executing command: {ex.Message}";
            }
        }

        public void RecordMetric(string name, double value, string unit)
        {
            metrics.Add(new PerformanceMetric
            {
                Name = name,
                Value = value,
                Unit = unit,
                Timestamp = DateTime.Now
            });

            // Keep only recent metrics (last 1000)
            if (metrics.Count > 1000)
            {
                metrics.RemoveAt(0);
            }
        }

        public void StartTimer(string name)
        {
            if (!namedTimers.ContainsKey(name))
            {
                namedTimers[name] = new Stopwatch();
            }
            namedTimers[name].Restart();
        }

        public long StopTimer(string name, bool recordMetric = true)
        {
            if (!namedTimers.ContainsKey(name))
                return 0;

            namedTimers[name].Stop();
            long elapsed = namedTimers[name].ElapsedMilliseconds;

            if (recordMetric)
            {
                RecordMetric(name, elapsed, "ms");
            }

            return elapsed;
        }

        public List<PerformanceMetric> GetMetrics(string? nameFilter = null)
        {
            if (string.IsNullOrEmpty(nameFilter))
                return new List<PerformanceMetric>(metrics);

            return metrics.Where(m => m.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public Dictionary<string, double> GetAverageMetrics()
        {
            return metrics
                .GroupBy(m => m.Name)
                .ToDictionary(g => g.Key, g => g.Average(m => m.Value));
        }

        public void ClearMetrics()
        {
            metrics.Clear();
        }

        public void ExportMetrics(string filePath)
        {
            try
            {
                using (var writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("Timestamp,Name,Value,Unit");
                    foreach (var metric in metrics)
                    {
                        writer.WriteLine($"{metric.Timestamp:yyyy-MM-dd HH:mm:ss.fff},{metric.Name},{metric.Value},{metric.Unit}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to export metrics: {ex.Message}");
            }
        }

        public string ProfileAction(string actionName, Action action)
        {
            StartTimer(actionName);
            try
            {
                action();
                long elapsed = StopTimer(actionName);
                return $"{actionName} completed in {elapsed}ms";
            }
            catch (Exception ex)
            {
                StopTimer(actionName, recordMetric: false);
                return $"{actionName} failed: {ex.Message}";
            }
        }

        public T ProfileFunction<T>(string functionName, Func<T> function)
        {
            StartTimer(functionName);
            try
            {
                T result = function();
                long elapsed = StopTimer(functionName);
                RecordMetric($"{functionName}_success", elapsed, "ms");
                return result;
            }
            catch (Exception ex)
            {
                StopTimer(functionName, recordMetric: false);
                RecordMetric($"{functionName}_error", 1, "count");
                throw;
            }
        }

        public void GeneratePerformanceReport(string reportPath)
        {
            try
            {
                using (var writer = new StreamWriter(reportPath))
                {
                    writer.WriteLine("=== Performance Report ===");
                    writer.WriteLine($"Generated: {DateTime.Now}");
                    writer.WriteLine($"Total Metrics: {metrics.Count}");
                    writer.WriteLine();

                    writer.WriteLine("=== Average Metrics ===");
                    var averages = GetAverageMetrics();
                    foreach (var avg in averages.OrderBy(a => a.Key))
                    {
                        writer.WriteLine($"{avg.Key}: {avg.Value:F2}");
                    }
                    writer.WriteLine();

                    writer.WriteLine("=== Recent Metrics (Last 50) ===");
                    foreach (var metric in metrics.OrderByDescending(m => m.Timestamp).Take(50))
                    {
                        writer.WriteLine($"[{metric.Timestamp:HH:mm:ss}] {metric.Name}: {metric.Value:F2} {metric.Unit}");
                    }
                    writer.WriteLine();

                    writer.WriteLine("=== System Information ===");
                    writer.WriteLine($"Memory Usage: {GC.GetTotalMemory(false) / 1024 / 1024} MB");
                    writer.WriteLine($"GC Gen 0: {GC.CollectionCount(0)} collections");
                    writer.WriteLine($"GC Gen 1: {GC.CollectionCount(1)} collections");
                    writer.WriteLine($"GC Gen 2: {GC.CollectionCount(2)} collections");
                    writer.WriteLine($"Processor Count: {Environment.ProcessorCount}");
                    writer.WriteLine($"OS: {Environment.OSVersion}");
                    writer.WriteLine($".NET Version: {Environment.Version}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to generate performance report: {ex.Message}");
            }
        }

        public void EnableDebugLogging(bool enabled)
        {
            if (enabled)
            {
                Trace.Listeners.Add(new TextWriterTraceListener(Path.Combine(logDirectory, "debug.log")));
                Trace.AutoFlush = true;
            }
            else
            {
                Trace.Listeners.Clear();
            }
        }

        public void LogDebug(string message)
        {
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [DEBUG] {message}";
            Trace.WriteLine(logMessage);
            Debug.WriteLine(logMessage);
        }

        public void LogInfo(string message)
        {
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [INFO] {message}";
            Trace.WriteLine(logMessage);
        }

        public void LogWarning(string message)
        {
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [WARN] {message}";
            Trace.WriteLine(logMessage);
        }

        public void LogError(string message, Exception? ex = null)
        {
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [ERROR] {message}";
            if (ex != null)
            {
                logMessage += $"\n{ex}";
            }
            Trace.WriteLine(logMessage);
        }

        public List<string> GetCommandHistory()
        {
            return new List<string>(consoleHistory);
        }

        public void ClearCommandHistory()
        {
            consoleHistory.Clear();
        }

        public Dictionary<string, string> GetCommandList()
        {
            return commands.ToDictionary(c => c.Key, c => c.Value.Description);
        }

        public void DumpMemoryInfo(string outputPath)
        {
            try
            {
                using (var writer = new StreamWriter(outputPath))
                {
                    writer.WriteLine("=== Memory Dump ===");
                    writer.WriteLine($"Timestamp: {DateTime.Now}");
                    writer.WriteLine();

                    writer.WriteLine($"Total Memory: {GC.GetTotalMemory(false)} bytes ({GC.GetTotalMemory(false) / 1024 / 1024} MB)");
                    writer.WriteLine($"GC Gen 0 Collections: {GC.CollectionCount(0)}");
                    writer.WriteLine($"GC Gen 1 Collections: {GC.CollectionCount(1)}");
                    writer.WriteLine($"GC Gen 2 Collections: {GC.CollectionCount(2)}");
                    writer.WriteLine($"Max GC Generation: {GC.MaxGeneration}");
                    writer.WriteLine();

                    var process = Process.GetCurrentProcess();
                    writer.WriteLine($"Working Set: {process.WorkingSet64 / 1024 / 1024} MB");
                    writer.WriteLine($"Private Memory: {process.PrivateMemorySize64 / 1024 / 1024} MB");
                    writer.WriteLine($"Virtual Memory: {process.VirtualMemorySize64 / 1024 / 1024} MB");
                    writer.WriteLine($"Paged Memory: {process.PagedMemorySize64 / 1024 / 1024} MB");
                    writer.WriteLine($"Thread Count: {process.Threads.Count}");
                    writer.WriteLine($"Handle Count: {process.HandleCount}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to dump memory info: {ex.Message}");
            }
        }
    }
}

