using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace ScaleService
{
    public partial class ScaleOperator
    {
        public class Builder
        {
            private ScaleOperator _scaleOperator;
            private RelayWatcher _relayWatcher;

            public IConfigurationSection Options { get; set; }

            public Builder(
                ScaleOperator scaleOperator,
                RelayWatcher relayWatcher)
            {
                this._scaleOperator = scaleOperator;
                this._relayWatcher = relayWatcher;
            }

            public ScaleOperator BuildUni()
            {
                var gratings = Options.GetSection("Gratings");
                var frontInPort = Convert.ToInt32(gratings["Front"]);
                var frontGSwitch = new Switch(frontInPort, _relayWatcher);

                var backInPort = Convert.ToInt32(gratings["Back"]);
                var backGSwitch = new Switch(backInPort, _relayWatcher);

                return Build(frontGSwitch, backGSwitch);
            }

            private ScaleOperator Build(Switch frontGSwitch, Switch backGSwitch)
            {
                var name = Options["Name"];

                var buttonInPort = Convert.ToInt32(Options["Button"]);
                var buttonSwitch = new Switch(buttonInPort, _relayWatcher);

                _scaleOperator.Name = name;
                _scaleOperator.Timeout = Convert.ToInt32(Options["Timeout"]);
                _scaleOperator.LEDOptions = Options.GetSection("LED").Get<LEDConfig>();
                _scaleOperator.ScaleIP = Options["ScaleIP"];
                _scaleOperator.InOrOut = Convert.ToInt32(Options["InOrOut"]);
                _scaleOperator.DownRelayOptions = Options.GetSection("DownRelay").Get<DownRelayOptions>();

                _scaleOperator.SetupSwitches(frontGSwitch, backGSwitch, buttonSwitch);

                return _scaleOperator;
            }

            internal ScaleOperator BuildBi(Dictionary<int, Switch> gratingSwitches)
            {
                var gratings = Options.GetSection("Gratings");
                var frontInPort = Convert.ToInt32(gratings["Front"]);
                var backInPort = Convert.ToInt32(gratings["Back"]);
                if (gratingSwitches.ContainsKey(frontInPort) &&
                    gratingSwitches.ContainsKey(backInPort))
                {
                    return Build(gratingSwitches[frontInPort], gratingSwitches[backInPort]);
                }
                throw new Exception("配置出错");
            }
        }
    }
}