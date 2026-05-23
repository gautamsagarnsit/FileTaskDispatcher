using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using System;
using System.Diagnostics;
using log4net;
using log4net.Config;

namespace FileDispatcherService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            var host = Host.CreateDefaultBuilder(args)
                .UseWindowsService()                
                .ConfigureServices(services =>
                {
                    services.AddHostedService<Worker>();
                })
                .Build();

            if (!EventLog.SourceExists("FileDispatcherService"))
            {
                EventLog.CreateEventSource("FileDispatcherService", "Application");
            }
            EventLog.WriteEntry("FileDispatcherService", $"Starting FileDispatcherService");

            host.Run();
        }
    }
}