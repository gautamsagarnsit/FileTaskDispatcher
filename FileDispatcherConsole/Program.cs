using Common;
using System.Collections.Generic;
using System.IO;
using log4net;
using log4net.Config;

namespace FileDispatcherConsole
{
    public class Program
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Program));
        public static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            var runner = new Runner();
            _logger.Info("Starting Runner...");
            runner.Run();
        }
    }
}
