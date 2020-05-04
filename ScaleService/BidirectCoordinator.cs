using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace ScaleService
{
    class BidirectCoordinator : IDisposable
    {
        public ScaleOperator EntranceOP { get; set; }
        public ScaleOperator ExitOP { get; set; }

        public class Builder
        {
            private IConfigurationSection biOptions;
            private readonly Dictionary<int, Switch> _gratingSwitches = new Dictionary<int, Switch>();

            public Builder(IConfigurationSection biOptions)
            {
                this.biOptions = biOptions;
            }

            public RelayWatcher RelayWatcher { get; internal set; }
            public ScaleRestClient RestClient { get; internal set; }

            public BidirectCoordinator Build()
            {
                var gratings = biOptions.GetSection("Gratings").Get<int[]>();
                foreach (var grating in gratings)
                {
                    _gratingSwitches.Add(grating, new Switch(grating, RelayWatcher));
                }

                var co  = new BidirectCoordinator();

                co.EntranceOP = new ScaleOperator.Builder(biOptions.GetSection("Entrance"))
                {
                    RelayWatcher = RelayWatcher,
                    RestClient = RestClient
                }.BuildBi(_gratingSwitches);
                co.ExitOP = new ScaleOperator.Builder(biOptions.GetSection("Exit"))
                {
                    RelayWatcher = RelayWatcher,
                    RestClient = RestClient
                }.BuildBi(_gratingSwitches);

                co.EntranceOP.Busy += (s, e) =>
                {
                    co.ExitOP.Enable(false);
                };
                co.EntranceOP.Idle += (s, e) =>
                {
                    co.ExitOP.Enable(true);
                };
                co.ExitOP.Busy += (s, e) =>
                {
                    co.EntranceOP.Enable(false);
                };
                co.ExitOP.Idle += (s, e) =>
                {
                    co.EntranceOP.Enable(true);
                };

                return co;
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
