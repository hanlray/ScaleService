using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ScaleService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IConfiguration Configuration;

        private ScaleRestClient _restClient;
        private RelayWatcher _relayWatcher;
        private IServiceProvider _serviceProvider;
        private IList<ScaleOperator> _uniOperators = new List<ScaleOperator>();
        private IList<BidirectCoordinator> _biCOs = new List<BidirectCoordinator>();

        public Worker(
            ILogger<Worker> logger, 
            IConfiguration configRoot,
            ScaleRestClient restClient,
            RelayWatcher relayWatcher,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            Configuration = (IConfigurationRoot)configRoot;
            _restClient = restClient;
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
                var builder = _serviceProvider.GetRequiredService<ScaleOperator.Builder>();
                builder.Options = uniOptions;
                var op = builder.BuildUni();

                op.Start();
                _uniOperators.Add(op);
            }
        }
    }
}
