using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using System;
using System.Diagnostics;
using log4net;
using log4net.Config;

namespace EmailService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            var host = Host.CreateDefaultBuilder(args)
            .UseWindowsService()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<Worker>();
            })
            .Build();

            if (!EventLog.SourceExists("FileEmailService"))
            {
                EventLog.CreateEventSource("FileEmailService", "Application");
            }
            EventLog.WriteEntry("FileEmailService", $"Starting FileEmailService");

            host.Run();
        }
    }
}