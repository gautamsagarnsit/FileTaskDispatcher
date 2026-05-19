using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;

namespace FileDispatcherService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices(services =>
                {
                    services.AddHostedService<Worker>();
                })
                .Build();

            host.Run();
        }
    }
}