﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        public delegate void ProgressEventHandler(object sender, ProgressEventArgs e);
        public delegate void IdleEventHandler(object sender, IdleEventArgs e);

        //public event ProgressEventHandler Progress;
        //public event IdleEventHandler ButtonIdle;
        //public event EventHandler GratingsState;
        public event EventHandler Busy;
        public event EventHandler Idle;

        private System.Timers.Timer _idleTimer = new System.Timers.Timer()
        {
            AutoReset = false
        };

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        ScaleOperator()
        {
        }

        internal void SetupSwitches(Switch frontGSwitch, Switch backGSwitch, Switch buttonSwitch)
        {
            FrontGSwitch = frontGSwitch;
            BackGSwitch = backGSwitch;
            ButtonSwitch = buttonSwitch;

            this._frontGWatcher = new FGratingWatcher(this);
            FrontGSwitch.On += _frontGWatcher.ProcessAsync;
            _frontGWatcher.Busy += OnBusy;
            _frontGWatcher.FakeCar += (s, e) => _idleTimer.Start();
            _frontGWatcher.Error += (s, e) => _idleTimer.Start();

            this._buttonWatcher = new ButtonWatcher(this);
            ButtonSwitch.On += _buttonWatcher.ProcessAsync;
            _buttonWatcher.Busy += OnBusy;
            _buttonWatcher.Completed += (s, e) => _idleTimer.Start();

            //this.ButtonIdle += this.OnIdle;
            _idleTimer.Interval = Timeout;
            _idleTimer.Elapsed += (s, e) => Ready();
        }

        public ScaleOperator(string name)
        {
            this.Name = name;
        }

        private async void Ready()
        {
            await SwitchRelayAsync(true);//turn green
            Display(LEDOptions.WelcomeMessage.Template, LEDOptions.WelcomeMessage.ShowConfig);

            Idle?.Invoke(this, new EventArgs());
        }

        public void Start()
        {
            Ready();
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
                LEDOptions.MaterialUid, //内码文字ID
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
            Logger.Debug("LED {0} 发送失败", LEDOptions.IP);
            return ret;
        }

        private async Task<bool> SwitchRelayAsync(bool isSucceed)
        {
            //Progress?.Invoke(this, new ProgressEventArgs("正在开关继电器"));
            for (int i = 0; i < 3; i++)
            {
                if (await SwitchRelayOnceAsync(isSucceed)) return true;
            }
            //Progress?.Invoke(this, new ProgressEventArgs("开关继电器失败"));
            Logger.Warn("开关继电器失败");
            return false;
        }

        private async Task<bool> SwitchRelayOnceAsync(bool isSucceed)
        {
            bool ret = true;
            foreach (var outGroup in DownRelayOptions.OutGroups)
            {
                bool succOutRet = await SwitchRelayOUTAsync(outGroup.SuccessOut, isSucceed ? true : false);
                Logger.Trace("{0} OUT returns {1}", outGroup.SuccessOut, succOutRet);
                bool failOutRet = await SwitchRelayOUTAsync(outGroup.FailureOut, isSucceed ? false : true);
                Logger.Trace("{0} OUT returns {1}", outGroup.FailureOut, failOutRet);
                if (!succOutRet || !failOutRet) ret = false;
            }
            return ret;
        }

        private async Task<bool> SwitchRelayOUTAsync(int OUT, bool isOn)
        {
            bool ret = false;
            try
            {
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(DownRelayOptions.IP, DownRelayOptions.Port);
                using var stream = tcpClient.GetStream();

                string command = string.Format("AT+STACH{0}={1}\n", OUT, isOn ? 1 : 0);
                byte[] bytSend = Encoding.ASCII.GetBytes(command);
                await stream.WriteAsync(bytSend, 0, bytSend.Length);

                byte[] buf = new byte[256];
                int i = 0;
                string message;
                while (stream.CanRead)
                {
                    int n = await stream.ReadAsync(buf, i, 1);
                    if (n != 1) throw new Exception("网络读取失败");

                    char c = (char)buf[i];
                    if (c != '\n')
                    {
                        i++;
                        if (i >= 256) throw new Exception("没有读到一行");
                        continue;
                    }

                    message = Encoding.ASCII.GetString(buf, 0, i);
                    Logger.Trace("Relay:Got the response " + message);
                    if (message == "OK")
                    {
                        ret = true;
                    }
                    break;
                }
            }
            catch (Exception e)
            {
                Logger.Warn("控制继电器失败:" + e.Message);
            }
            return ret;
        }

        internal void Enable(bool isEnable)
        {
            if (isEnable)
            {
                _frontGWatcher.Enabled = true;
                _buttonWatcher.Enabled = true;
            }
            else
            {
                _frontGWatcher.Enabled = false;
                _buttonWatcher.Enabled = false;

                _idleTimer.Stop();
            }
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

        #endregion
    };
}