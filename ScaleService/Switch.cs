using System;

namespace ScaleService
{
    internal class Switch
    {
        public event EventHandler On;
        public event EventHandler Off;

        private int _inPort;
        private RelayWatcher _relayWatcher;

        public bool IsOn { get; internal set; }

        public Switch(int inPort, RelayWatcher relayWatcher)
        {
            _inPort = inPort;
            _relayWatcher = relayWatcher;

            _relayWatcher.Subscribe(inPort, OnChanged);
        }

        private void OnChanged(object sender, InChangedEventArgs e)
        {
            if (e.Value == 1)
                On?.Invoke(this, new EventArgs());
            else
                Off?.Invoke(this, new EventArgs());
        }
    }
}