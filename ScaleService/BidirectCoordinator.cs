using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace ScaleService
{
    class BidirectCoordinator : IDisposable
    {
        public ScaleOperator EntranceOP { get; set; }
        public ScaleOperator ExitOP { get; set; }

        public class Builder
        {
            private readonly Dictionary<int, Switch> _gratingSwitches = new Dictionary<int, Switch>();
            private BidirectCoordinator _co;
            private RelayWatcher _relayWatcher;
            private ScaleOperator.BiBuilder _entranceBuilder;
            private ScaleOperator.BiBuilder _exitBuilder;
            private IServiceProvider serviceProvider;

            public Builder(
                BidirectCoordinator co,
                RelayWatcher relayWatcher,
                ScaleOperator.BiBuilder entranceBuilder,
                ScaleOperator.BiBuilder exitBuilder,
                IServiceProvider serviceProvider)
            {
                this._co = co;
                _relayWatcher = relayWatcher;
                this._entranceBuilder = entranceBuilder;
                this._exitBuilder = exitBuilder;
                this.serviceProvider = serviceProvider;
            }

            public BidirectCoordinator Build(IConfigurationSection biOptions)
            {
                var gratings = biOptions.GetSection("Gratings").Get<int[]>();
                foreach (var grating in gratings)
                {
                    var gSwitch = serviceProvider.GetRequiredService<Switch>();
                    gSwitch.SetInPort(grating);
                    _gratingSwitches.Add(grating, gSwitch);
                }

                _entranceBuilder.Options = biOptions.GetSection("Entrance");
                _co.EntranceOP = _entranceBuilder.Build(_gratingSwitches);

                _exitBuilder.Options = biOptions.GetSection("Exit");
                _co.ExitOP = _exitBuilder.Build(_gratingSwitches);

                _co.EntranceOP.Busy += (s, e) =>
                {
                    _co.ExitOP.Enable(false);
                };
                _co.EntranceOP.Idle += (s, e) =>
                {
                    _co.ExitOP.Enable(true);
                };
                _co.ExitOP.Busy += (s, e) =>
                {
                    _co.EntranceOP.Enable(false);
                };
                _co.ExitOP.Idle += (s, e) =>
                {
                    _co.EntranceOP.Enable(true);
                };

                return _co;
            }
        }

        internal void Start()
        {
            EntranceOP.Start();
            ExitOP.Start();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    EntranceOP.Dispose();
                    ExitOP.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BidirectCoordinator()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
