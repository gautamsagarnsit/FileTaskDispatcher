using FileDispatcher;
using System.Diagnostics;
using log4net;

namespace FileDispatcherService
{
    public class Worker : BackgroundService
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Worker));
        private readonly string _eventSourceName = "FileDispatcherService";

        public Worker()
        {
            
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {

                _logger.Info("===== Service startup initiated =====");

                var runner = new Runner();
                runner.Run();

                _logger.Info("Service started successfully.");
                WriteEventLog(
                    $"FileDispatcherService Started (Worker) at {DateTimeOffset.Now}",
                    EventLogEntryType.Information
                );

                // Keep service running
                while (!stoppingToken.IsCancellationRequested)
                {
                    //_logger.Info($"Worker running at: {DateTimeOffset.Now}");
                    await Task.Delay(5000, stoppingToken); // Check every 5 seconds instead of 1 second
                }

                _logger.Info("Service stopping...");
                WriteEventLog(
                    $"FileDispatcherService Stopped (Worker) at {DateTimeOffset.Now}",
                    EventLogEntryType.Information
                );
            }
            catch (OperationCanceledException)
            {
                _logger.Info("Service cancellation requested.");
                WriteEventLog(
                    "FileDispatcherService cancelled",
                    EventLogEntryType.Warning
                );
            }
            catch (Exception ex)
            {
                _logger.Info($"ERROR: {ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}");
                WriteEventLog(
                    $"FileDispatcherService Error: {ex.Message}",
                    EventLogEntryType.Error
                );
                throw;
            }
        }

        private void WriteEventLog(string message, EventLogEntryType entryType)
        {
            try
            {
                EventLog.WriteEntry(_eventSourceName, message, entryType);
            }
            catch (Exception ex)
            {
                _logger.Info($"Failed to write to EventLog: {ex.Message}");
            }
        }
    }
}
