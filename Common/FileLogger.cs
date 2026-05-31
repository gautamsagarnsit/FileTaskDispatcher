using System;
using System.IO;
using System.Text;

namespace AutoFileDispatcher.Common
{
    public class FileLogger
    {
        private readonly string _logFilePath;
        private readonly object _lockObj = new object();
        private const int MaxRetries = 3;
        private const int RetryDelayMs = 100;

        public FileLogger(string logFilePath = @".\Logs\FileDispatcher.log")
        {
            _logFilePath = logFilePath;

            // Ensure directory exists
            var dir = Path.GetDirectoryName(_logFilePath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        public void Log(string message)
        {
            lock (_lockObj)
            {
                int retryCount = 0;
                while (retryCount < MaxRetries)
                {
                    try
                    {
                        string entry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}";

                        // Use FileShare to allow other processes to read while we write
                        using (var fileStream = new FileStream(
                            _logFilePath,
                            FileMode.Append,
                            FileAccess.Write,
                            FileShare.Read,
                            bufferSize: 1024,
                            useAsync: false))
                        {
                            using (var writer = new StreamWriter(fileStream, Encoding.UTF8))
                            {
                                writer.Write(entry);
                                writer.Flush();
                            }
                        }

                        return; // Success
                    }
                    catch (IOException ex) when (retryCount < MaxRetries - 1)
                    {
                        retryCount++;
                        System.Threading.Thread.Sleep(RetryDelayMs);
                    }
                    catch (Exception ex)
                    {
                        return;
                    }
                }
            }
        }

    }
}
