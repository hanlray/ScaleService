using System;
using System.Collections.Generic;
using System.Text;

namespace ScaleService
{
    public class LEDMessageShowConfig
    {
        public int FontSize { get; set; }
        public int ShowStyle { get; set; }
        public int ShowSpeed { get; set; }
    }

    public class LEDMessageConfig
    {
        public string Template { get; set; }
        public LEDMessageShowConfig ShowConfig { get; set; }
    }

    public class LEDConfig
    {
        public string IP { get; set; }
        public int MaterialUID { get; set; }
        public int StayTime { get; set; }
        public LEDMessageConfig CPZLMessage { get; set; }
        public LEDMessageConfig WelcomeMessage { get; set; }
    }
    public class OutGroup
    {
        public int SuccessOut { get; set; }
        public int FailureOut { get; set; }
    }
    public class DownRelayOptions
    {
        public string IP { get; set; }
        public int Port { get; set; }
        public IList<OutGroup> OutGroups;
    }
}
