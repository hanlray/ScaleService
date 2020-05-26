using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ScaleService
{
    public class Worker : BackgroundService
    {
        private IConfiguration Configuration;

        private RelayWatcher _relayWatcher;
        private IServiceProvider _serviceProvider;
        private IList<ScaleOperator> _uniOperators = new List<ScaleOperator>();
        private IList<BidirectCoordinator> _biCOs = new List<BidirectCoordinator>();

        public Worker(
            IConfiguration configRoot,
            RelayWatcher relayWatcher,
            IServiceProvider serviceProvider)
        {
            Configuration = (IConfigurationRoot)configRoot;
            _relayWatcher = relayWatcher;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            BuildUniOperators();
            BuildBiOperators();

            await Task.Run(() => _relayWatcher.StartAsync(stoppingToken));
        }

        private void BuildBiOperators()
        {
            foreach (var biOptions in Configuration.GetSection("Bidirectionals").GetChildren())
            {
                var builder = _serviceProvider.GetRequiredService<BidirectCoordinator.Builder>();
                var co = builder.Build(biOptions);
                co.Start();
                _biCOs.Add(co);
            }
        }

        private void BuildUniOperators()
        {
            foreach (var uniOptions in Configuration.GetSection("Unidirectionals").GetChildren())
            {
                var builder = _serviceProvider.GetRequiredService<ScaleOperator.UniBuilder>();
                builder.Options = uniOptions;
                var op = builder.Build();

                op.Start();
                _uniOperators.Add(op);
            }
        }
    }
}
