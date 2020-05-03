using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace ScaleService
{
    public partial class ScaleOperator
    {
        private ButtonWatcher _buttonWatcher;
        private FGratingWatcher _frontGWatcher;

        public LEDConfig LEDOptions { get; private set; }
        public string ScaleIP { get; private set; }
        public int InOrOut { get; private set; }
        public DownRelayOptions DownRelayOptions { get; private set; }
        public ScaleRestClient RestClient { get; private set; }

        public class Builder
        {
            private IConfigurationSection uniOptions;

            public Builder(IConfigurationSection uniOptions)
            {
                this.uniOptions = uniOptions;
            }

            public RelayWatcher RelayWatcher { get; internal set; }
            public ScaleRestClient RestClient { get; internal set; }

            public ScaleOperator BuildUni()
            {
                var gratings = uniOptions.GetSection("Gratings");
                var frontInPort = Convert.ToInt32(gratings["Front"]);
                var frontGSwitch = new Switch(frontInPort, RelayWatcher);

                var backInPort = Convert.ToInt32(gratings["Back"]);
                var backGSwitch = new Switch(backInPort, RelayWatcher);

                return Build(frontGSwitch, backGSwitch);
            }

            private ScaleOperator Build(Switch frontGSwitch, Switch backGSwitch)
            {
                var name = uniOptions["Name"];

                var buttonInPort = Convert.ToInt32(uniOptions["Button"]);
                var buttonSwitch = new Switch(buttonInPort, RelayWatcher);

                var op = new ScaleOperator(frontGSwitch, backGSwitch, buttonSwitch)
                {
                    Name = name,
                    LEDOptions = uniOptions.GetSection("LED").Get<LEDConfig>(),
                    ScaleIP = uniOptions["ScaleIP"],
                    InOrOut = Convert.ToInt32(uniOptions["InOrOut"]),
                    DownRelayOptions = uniOptions.GetSection("DownRelay").Get<DownRelayOptions>(),
                    RestClient = RestClient
                };

                return op;
            }

            internal ScaleOperator BuildBi(Dictionary<int, Switch> gratingSwitches)
            {
                var gratings = uniOptions.GetSection("Gratings");
                var frontInPort = Convert.ToInt32(gratings["Front"]);
                var backInPort = Convert.ToInt32(gratings["Back"]);
                if(gratingSwitches.ContainsKey(frontInPort) &&
                    gratingSwitches.ContainsKey(backInPort))
                {
                    return Build(gratingSwitches[frontInPort], gratingSwitches[backInPort]);
                }
                throw new Exception("配置出错");
            }
        }

        private void SetButtonWatcher(ButtonWatcher buttonWatcher)
        {
            this._buttonWatcher = buttonWatcher;
            _buttonWatcher.Operator = this;
        }

        private void SetFrontGWatcher(FGratingWatcher frontGWatcher)
        {
            this._frontGWatcher = frontGWatcher;
            _frontGWatcher.Operator = this;
        }
    }
}