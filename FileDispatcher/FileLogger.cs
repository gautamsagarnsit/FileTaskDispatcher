using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDispatcher
{
    public class FileLogger
    {
        private readonly string _logFilePath;

        public FileLogger(string logFilePath = "C:\\Users\\gauta\\source\\repos\\AutoFileDispatcher\\FileDispatcher.log")
        {
            _logFilePath = logFilePath;

            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(_logFilePath));
        }

        public void Log(string message)
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}";
            File.AppendAllText(_logFilePath, logEntry);
        }

    }
}
