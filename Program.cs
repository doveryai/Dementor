﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.EventLog;
using System.ServiceProcess;

namespace Dementor
{
    class Program
    {
        //static void Main(string[] args)
        //{

        //    var pm = new ProcessMonitor();

        //    pm.Start();

        //    while (true)
        //    {
        //        System.Threading.Thread.Sleep(60000);
        //    }

        //    //ServiceBase.Run(new WindowsService());
        //}

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>()
                      .Configure<EventLogSettings>(config =>
                      {
                          config.LogName = "Application";
                          config.SourceName = "Dementor Service";
                      })
                      .Configure<DementorSettings>(hostContext.Configuration.GetSection("DementorSettings"));
                })
                .ConfigureAppConfiguration((hostContext, builder) =>
                {
                    if (hostContext.HostingEnvironment.IsDevelopment())
                    {
                        builder.AddUserSecrets<Program>(false);
                    }
                })
                .UseWindowsService();    
    }
}
