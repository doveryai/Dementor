using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;

namespace Dementor
{
    class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args);
#if DEBUG
            var task = host.RunConsoleAsync();
            task.Wait();
#else
            host.Build().Run();
#endif
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
                .ConfigureLogging((_, logging) => logging.AddEventLog())
                .UseWindowsService();
    }
}
