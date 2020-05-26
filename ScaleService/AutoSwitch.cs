using System.Threading.Tasks;
using System.Timers;

namespace ScaleService
{
    public class AutoSwitch : Switch
    {
        private Timer timer;

        public AutoSwitch(RelayWatcher relayWatcher) : base(relayWatcher)
        {
            this.timer = new Timer();
            timer.Interval = 25000;
            timer.AutoReset = true;
            timer.Elapsed += async (s, e) =>
            {
                OnChanged(this, new InChangedEventArgs(1));
                await Task.Delay(500);
                OnChanged(this, new InChangedEventArgs(0));
            };
            timer.Start();
        }
    }
}