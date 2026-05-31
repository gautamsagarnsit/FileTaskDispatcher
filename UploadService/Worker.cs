using log4net;
using System.Diagnostics;
using System.Text;

namespace UploadService
{
    public class Worker : BackgroundService
    {       
        private readonly string _eventSourceName = "FileUploadService";
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Worker));
        private UploadHandler _uploadHandler;

        public Worker(UploadHandler uploadHandler)
        {
            _uploadHandler = uploadHandler;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {

                _logger.Info("===== Upload Service startup initiated =====");


                var runner = new Runner(_uploadHandler);

                await runner.Run();

                _logger.Info("Service started successfully.");
                WriteEventLog(
                    $"FileUploadService Started (Worker) at {DateTimeOffset.Now}",
                    EventLogEntryType.Information
                );

                // Keep service running
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.Info($"Upload Service Worker running at: {DateTimeOffset.Now}");
                    await Task.Delay(5000, stoppingToken); // Check every 5 seconds instead of 1 second
                }

                _logger.Info("Service stopping...");
                WriteEventLog(
                    $"FileUploadService Stopped (Worker) at {DateTimeOffset.Now}",
                    EventLogEntryType.Information
                );
            }
            catch (OperationCanceledException)
            {
                _logger.Info("Service cancellation requested.");
                WriteEventLog(
                    "FileUploadService cancelled",
                    EventLogEntryType.Warning
                );
            }
            catch (Exception ex)
            {
                _logger.Info($"ERROR: {ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}");
                WriteEventLog(
                    $"FileUploadService Error: {ex.Message}",
                    EventLogEntryType.Error
                );
                throw;
            }
        }
        private void WriteEventLog(string message, EventLogEntryType entryType)
        {
            try
            {
                _logger.Info($"Writing to EventLog: {message}");
                EventLog.WriteEntry(_eventSourceName, message, entryType);
            }
            catch (Exception ex)
            {
                _logger.Info($"Failed to write to EventLog: {ex.Message}");
            }
        }
    }
}
