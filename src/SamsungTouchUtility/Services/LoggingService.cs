using System;
using System.IO;

namespace SamsungTouchUtility.Services
{
    public interface ILoggingService
    {
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message, Exception? ex = null);
    }

    public sealed class LoggingService : ILoggingService
    {
        private static readonly object _sync = new object();
        private readonly string _logDirectory;
        private readonly string _logFilePath;

        public LoggingService(string? customLogDir = null)
        {
            _logDirectory = customLogDir ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SamsungTouchUtility",
                "logs");

            Directory.CreateDirectory(_logDirectory);
            _logFilePath = Path.Combine(_logDirectory, $"touch-utility-{DateTime.UtcNow:yyyyMMdd}.log");
            RotateOldLogs();
        }

        public void LogInfo(string message) => WriteEntry("INFO", message);
        public void LogWarning(string message) => WriteEntry("WARN", message);
        public void LogError(string message, Exception? ex = null)
        {
            string details = ex != null ? $"{message} | Exception: {ex}" : message;
            WriteEntry("ERROR", details);
        }

        private void WriteEntry(string level, string message)
        {
            try
            {
                string line = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}{Environment.NewLine}";
                lock (_sync)
                {
                    File.AppendAllText(_logFilePath, line);
                }
            }
            catch
            {
                // Never crash the background application if logging encounters transient filesystem lock
            }
        }

        private void RotateOldLogs()
        {
            try
            {
                var files = Directory.GetFiles(_logDirectory, "touch-utility-*.log");
                DateTime cutoff = DateTime.UtcNow.AddDays(-14); // 14-day retention

                foreach (var file in files)
                {
                    var info = new FileInfo(file);
                    if (info.CreationTimeUtc < cutoff)
                    {
                        info.Delete();
                    }
                }
            }
            catch
            {
            }
        }
    }
}
