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
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    //services.AddTransient<Switch, AutoSwitch>();
                    services.AddTransient<Switch>();
                    services.AddTransient<ScaleOperator>();
                    services.AddTransient<ScaleOperator.UniBuilder>();
                    services.AddTransient<ScaleOperator.BiBuilder>();
                    services.AddTransient<BidirectCoordinator>();
                    services.AddTransient<BidirectCoordinator.Builder>();
                    services.AddSingleton<RelayWatcher>();
                    services.AddSingleton<ScaleRestClient>();
                    services.AddHostedService<Worker>();
                });
    }
}
