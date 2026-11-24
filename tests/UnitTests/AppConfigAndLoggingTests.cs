using System;
using System.IO;
using Xunit;
using SamsungTouchUtility.Configuration;
using SamsungTouchUtility.Services;

namespace UnitTests
{
    public class AppConfigAndLoggingTests
    {
        [Fact]
        public void AppConfig_DefaultValues_AreCorrect()
        {
            var config = new AppConfig();
            Assert.Equal("Samsung Touchscreen Utility", config.ApplicationName);
            Assert.Equal("1.0.0", config.Version);
            Assert.False(config.StartWithWindows);
            Assert.Null(config.PreferredInstanceId);
        }

        [Fact]
        public void LoggingService_WritesLogEntriesWithoutExceptions()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "SamsungTouchTestLogs_" + Guid.NewGuid().ToString("N"));
            try
            {
                var logger = new LoggingService(tempDir);
                logger.LogInfo("Test Info Message");
                logger.LogWarning("Test Warning Message");
                logger.LogError("Test Error Message", new InvalidOperationException("Test exception"));

                var files = Directory.GetFiles(tempDir, "*.log");
                Assert.Single(files);

                string content = File.ReadAllText(files[0]);
                Assert.Contains("[INFO] Test Info Message", content);
                Assert.Contains("[WARN] Test Warning Message", content);
                Assert.Contains("[ERROR] Test Error Message", content);
                Assert.Contains("Test exception", content);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }
    }
}
