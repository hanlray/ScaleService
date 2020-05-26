using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace ScaleService
{
    public partial class ScaleOperator
    {
        public class Builder
        {
            private ScaleOperator scaleOperator;
            private RelayWatcher relayWatcher;
            private Switch buttonSwitch;

            public IConfigurationSection Options { get; set; }

            public Builder(ScaleOperator scaleOperator, RelayWatcher relayWatcher, Switch buttonSwitch)
            {
                this.scaleOperator = scaleOperator;
                this.relayWatcher = relayWatcher;
                this.buttonSwitch = buttonSwitch;
            }

            protected ScaleOperator Build(Switch frontGSwitch, Switch backGSwitch)
            {
                var name = Options["Name"];

                var buttonInPort = Convert.ToInt32(Options["Button"]);
                //var buttonSwitch = new Switch(buttonInPort, _relayWatcher);
                buttonSwitch.SetInPort(buttonInPort);

                scaleOperator.Name = name;
                scaleOperator.Timeout = Convert.ToInt32(Options["Timeout"]);
                scaleOperator.LEDOptions = Options.GetSection("LED").Get<LEDConfig>();
                scaleOperator.ScaleIP = Options["ScaleIP"];
                scaleOperator.InOrOut = Convert.ToInt32(Options["InOrOut"]);
                scaleOperator.DownRelayOptions = Options.GetSection("DownRelay").Get<DownRelayOptions>();

                scaleOperator.SetupSwitches(frontGSwitch, backGSwitch, buttonSwitch);

                return scaleOperator;
            }
        }

        public class UniBuilder: Builder
        {
            private Switch _frontGSwitch;
            private Switch _backGSwitch;

            public UniBuilder(
                ScaleOperator scaleOperator,
                RelayWatcher relayWatcher,
                Switch frontGSwitch,
                Switch backGSwitch,
                Switch buttonSwitch
                ): base(scaleOperator, relayWatcher, buttonSwitch)
            {
                _frontGSwitch = frontGSwitch;
                _backGSwitch = backGSwitch;
            }

            public ScaleOperator Build()
            {
                var gratings = Options.GetSection("Gratings");
                var frontInPort = Convert.ToInt32(gratings["Front"]);
                //var frontGSwitch = new Switch(frontInPort, _relayWatcher);
                _frontGSwitch.SetInPort(frontInPort);

                var backInPort = Convert.ToInt32(gratings["Back"]);
                //var backGSwitch = new Switch(backInPort, _relayWatcher);
                _backGSwitch.SetInPort(backInPort);

                return Build(_frontGSwitch, _backGSwitch);
            }
        }

        public class BiBuilder: Builder
        {
            public BiBuilder(
                ScaleOperator scaleOperator,
                RelayWatcher relayWatcher,
                Switch buttonSwitch
                ) : base(scaleOperator, relayWatcher, buttonSwitch)
            {
            }

            public ScaleOperator Build(Dictionary<int, Switch> gratingSwitches)
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