using System;
using System.Text;

namespace ScaleService
{
    public class Switch
    {
        public event EventHandler On;
        public event EventHandler Off;

        private RelayWatcher _relayWatcher;

        public bool IsOn { get; internal set; }

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public Switch(RelayWatcher relayWatcher)
        {
            _relayWatcher = relayWatcher;
        }

        protected void OnChanged(object sender, InChangedEventArgs e)
        {
            if (e.Value == 1)
            {
                //Logger.Debug("In {0} is on", _inPort);
                IsOn = true;
                On?.Invoke(this, new EventArgs());
            }
            else
            {
                IsOn = false;
                //Logger.Debug("In {0} is off", _inPort);
                Off?.Invoke(this, new EventArgs());
            }
        }

        internal void SetInPort(int inPort)
        {
            _relayWatcher.Subscribe(inPort, OnChanged);
        }
    }
}