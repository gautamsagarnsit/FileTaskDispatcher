using log4net;
using System.Diagnostics;
using System.Text;

namespace EmailService
{
    public class Worker : BackgroundService
    {
        private readonly string _eventSourceName = "FileEmailService";
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Worker));

        public Worker()
        {

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {

                _logger.Info("===== Email Service startup initiated =====");

                //Todo: Add logic to check EmailServiceQueue for any new message and invoke handler to process email sending. For now, we will just simulate the service running.
                var runner = new Runner();
                runner.Run();

                _logger.Info("Service started successfully.");
                WriteEventLog(
                    $"FileEmailService Started (Worker) at {DateTimeOffset.Now}",
                    EventLogEntryType.Information
                );

                // Keep service running
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.Info($"Email Service Worker running at: {DateTimeOffset.Now}");
                    await Task.Delay(5000, stoppingToken); // Check every 5 seconds instead of 1 second
                }

                _logger.Info("Service stopping...");
                WriteEventLog(
                    $"FileEmailService Stopped (Worker) at {DateTimeOffset.Now}",
                    EventLogEntryType.Information
                );
            }
            catch (OperationCanceledException)
            {
                _logger.Info("Service cancellation requested.");
                WriteEventLog(
                    "FileEmailService cancelled",
                    EventLogEntryType.Warning
                );
            }
            catch (Exception ex)
            {
                _logger.Info($"ERROR: {ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}");
                WriteEventLog(
                    $"FileEmailService Error: {ex.Message}",
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
