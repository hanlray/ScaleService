using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ScaleService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddTransient<ScaleOperator>();
                    services.AddTransient<ScaleOperator.Builder>();
                    services.AddTransient<BidirectCoordinator>();
                    services.AddTransient<BidirectCoordinator.Builder>();
                    services.AddSingleton<RelayWatcher>();
                    services.AddSingleton<ScaleRestClient>();
                    services.AddHostedService<Worker>();
                });
    }
}
