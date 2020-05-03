using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ScaleService
{
    public class ProgressEventArgs
    {
        public ProgressEventArgs(string s) { Text = s; }
        public String Text { get; } // readonly
    }

    public class IdleEventArgs
    {
        public IdleEventArgs(bool isTimer) { IsTimer = isTimer; }
        public bool IsTimer { get; }
    }

    public partial class ScaleOperator : IDisposable
    {
        public string Name { get; set; }

        internal Switch FrontGSwitch { get; private set; }
        internal Switch BackGSwitch { get; private set; }
        internal Switch ButtonSwitch { get; private set; }
        public object LEDConfig { get; private set; }

        public delegate void ProgressEventHandler(object sender, ProgressEventArgs e);
        public delegate void IdleEventHandler(object sender, IdleEventArgs e);

        //public event ProgressEventHandler Progress;
        //public event IdleEventHandler ButtonIdle;
        //public event EventHandler GratingsState;
        public event EventHandler Busy;
        public event EventHandler Idle;

        private bool isEnable = false;

        private System.Timers.Timer _idleTimer = new System.Timers.Timer()
        {
            AutoReset = false
        };

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        ScaleOperator(Switch frontGSwitch, Switch backGSwitch, Switch buttonSwitch)
        {
            FrontGSwitch = frontGSwitch;
            BackGSwitch = backGSwitch;
            ButtonSwitch = buttonSwitch;

            this._frontGWatcher = new FGratingWatcher(this);
            FrontGSwitch.On += _frontGWatcher.ProcessAsync;
            _frontGWatcher.Busy += OnBusy;
            _frontGWatcher.FakeCar += (s, e) => _idleTimer.Start();

            this._buttonWatcher = new ButtonWatcher(this);
            ButtonSwitch.On += _buttonWatcher.ProcessAsync;
            _buttonWatcher.Busy += OnBusy;
            _buttonWatcher.Completed += (s, e) => _idleTimer.Start();

            //this.ButtonIdle += this.OnIdle;
            _idleTimer.Interval = LEDOptions.StayTime;
            _idleTimer.Elapsed += (s, e) => Ready();
        }

        public ScaleOperator(string name)
        {
            this.Name = name;
        }

        private void Ready()
        {
            SwitchRelay(true);//turn green
            Display(LEDOptions.WelcomeMessage.Template, LEDOptions.WelcomeMessage.ShowConfig);

            Idle?.Invoke(this, new EventArgs());
        }

        public void Start()
        {
            Ready();

            isEnable = true;
        }

        private bool IsGratingsOff()
        {
            /*
            foreach(var gratingDetector in GratingDetectors)
            {
                if (gratingDetector.IsOn) return false;
            }
            return true;*/
            return !FrontGSwitch.IsOn && !BackGSwitch.IsOn;
        }

        private void OnBusy(object sender, EventArgs e)
        {
            Busy?.Invoke(this, new EventArgs());
            _idleTimer.Stop();
        }

        public void OnIdle(object sender, IdleEventArgs e)
        {
            Logger.Debug("{0}空闲", this.Name);
            if (e.IsTimer)
                _idleTimer.Start();
        }

        private bool Display(string text, LEDMessageShowConfig config)
        {
            for (int i = 0; i < 3; i++)
            {
                if (DisplayOnce(text, config)) return true;
            }
            return false;
        }

        private bool DisplayOnce(string text, LEDMessageShowConfig showConfig)
        {
            Logger.Debug("发送文字到显示屏{0}：{1}", text, LEDOptions.IP);
            byte b = QYLED_DLL.SendInternalText_Net(
                text,
                LEDOptions.IP,
                1, //udp
                64, 32,
                LEDOptions.MaterialUID, //内码文字ID
                0,//单基色
                showConfig.ShowStyle, //立即显示
                showConfig.ShowSpeed, //显示速度
                1, //停留时间
                1, //红色
                1, //宋体
                showConfig.FontSize, //0代表12x12字体大小 1:16x16
                2, //本素材立即更新
                false //掉电不保存
            );
            bool ret = Convert.ToInt32(b) == 0 ? true : false;
            Logger.Debug("LED call returns {0}", ret);
            return ret;
        }

        private bool SwitchRelay(bool isSucceed)
        {
            //Progress?.Invoke(this, new ProgressEventArgs("正在开关继电器"));
            for (int i = 0; i < 3; i++)
            {
                if (SwitchRelayOnce(isSucceed)) return true;
            }
            //Progress?.Invoke(this, new ProgressEventArgs("开关继电器失败"));
            Logger.Warn("开关继电器失败");
            return false;
        }

        private bool SwitchRelayOnce(bool isSucceed)
        {
            bool ret = true;
            foreach (var outGroup in DownRelayOptions.OutGroups)
            {
                bool succOutRet = SwitchRelayOUT(outGroup.SuccessOut, isSucceed ? true : false);
                Logger.Trace("{0} OUT returns {1}", outGroup.SuccessOut, succOutRet);
                bool failOutRet = SwitchRelayOUT(outGroup.FailureOut, isSucceed ? false : true);
                Logger.Trace("{0} OUT returns {1}", outGroup.FailureOut, failOutRet);
                if (!succOutRet || !failOutRet) ret = false;
            }
            return ret;
        }

        private bool SwitchRelayOUT(int OUT, bool isOn)
        {
            bool ret = false;
            try
            {
                TcpClient tcpClient = new TcpClient();
                tcpClient.Connect(IPAddress.Parse(DownRelayOptions.IP), DownRelayOptions.Port);
                NetworkStream ntwStream = tcpClient.GetStream();
                if (ntwStream.CanWrite)
                {
                    String command = String.Format("AT+STACH{0}={1}\n", OUT, isOn ? 1 : 0);
                    byte[] bytSend = Encoding.ASCII.GetBytes(command);
                    ntwStream.Write(bytSend, 0, bytSend.Length);

                    var memStream = new MemoryStream();

                    for (int i = 0; i < 256; i++)
                    {
                        byte b = (byte)ntwStream.ReadByte();
                        memStream.WriteByte(b);
                        if (b == '\n') break;
                    }

                    String result = Encoding.ASCII.GetString(memStream.ToArray());
                    Logger.Trace("Relay:Got the response " + result);
                    if (result == "OK\n") ret = true;
                }
                else
                {
                    throw new Exception("不能向继电器发送数据");
                }
                ntwStream.Close();
                tcpClient.Close();
            }
            catch (Exception e)
            {
                Logger.Warn("控制继电器失败:" + e.Message);
            }
            return ret;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //Stop();
                    _idleTimer.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ScaleOperator()
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

        internal void Enable(bool isEnable)
        {
            this.isEnable = isEnable;
            if (!isEnable)
            {
                _idleTimer.Stop();
            }
        }
        #endregion
    };
}
