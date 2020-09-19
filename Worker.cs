using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dementor
{
    //TOOD: installer
    //TODO: sqlite db
    //TODO: API/service for distributed logging
    //TODO: support for blacklisting browser tabs
    //TODO: restructure blacklist to support schedules
    //TODO: roles, security

    public class Worker : BackgroundService
    {

        private readonly ILogger<Worker> logger;
        private DementorSettings appsettings;

        public Worker(ILogger<Worker> logger, IOptions<DementorSettings> appsettings)
        {
            this.logger = logger;
            this.appsettings = appsettings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var pm = new ProcessMonitor(appsettings, logger);
                pm.Scan();

                await Task.Delay(appsettings.ProcessPollingInterval*1000, stoppingToken);
            }
        }
    }
}
