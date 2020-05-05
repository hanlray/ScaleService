using System;
using System.Collections.Generic;
using System.Text;

namespace ScaleService
{
    public partial class ScaleOperator
    {
        class ButtonWatcher
        {
            //public string Name { get; set; }
            public ScaleOperator Operator { get; internal set; }
            public bool IsBusy { get; private set; } = false;
            public bool Enabled { get; set; } = true;

            public event EventHandler Busy;
            public event EventHandler NeedAdjust;
            public event EventHandler Succeed;
            private ScaleOperator scaleOperator;
            private int inPort;
            private RelayWatcher relayWatcher;

            public ButtonWatcher(int inPort, RelayWatcher relayWatcher)
            {
                this.inPort = inPort;
                this.relayWatcher = relayWatcher;

            }

            public ButtonWatcher(ScaleOperator scaleOperator)
            {
                this.scaleOperator = scaleOperator;
            }

            internal async void ProcessAsync(object sender, EventArgs e)
            {
                try
                {
                    if (!Enabled) return;

                    Logger.Debug("{0}按钮触发", scaleOperator.Name);
                    if (IsBusy) return;
                    IsBusy = true;

                    //Progress?.Invoke(this, new ProgressEventArgs("开始工作"));
                    Logger.Debug("开始工作");

                    Busy?.Invoke(this, new EventArgs());

                    if (!scaleOperator.IsGratingsOff())
                    {
                        var config = new LEDMessageShowConfig
                        {
                            FontSize = 0,
                            ShowStyle = 3,
                            ShowSpeed = 8
                        };
                        scaleOperator.Display("调整车辆", config);
                        NeedAdjust?.Invoke(this, new EventArgs());
                        return;
                    }

                    //call remote service to get weight and plate number
                    //Progress?.Invoke(this, new ProgressEventArgs("正在请求重量"));
                    Logger.Debug("[{0}]请求重量", scaleOperator.Name);
                    GetWtResponse resp = await scaleOperator.RestClient.GetWtAsync(scaleOperator.ScaleIP, scaleOperator.InOrOut.ToString());
                    Logger.Debug("[{0}]返回重量{1}", scaleOperator.Name, resp.wt_num);
                    /*GetWtResponse resp = new GetWtResponse()
                    {
                        status = 1,
                        tk_no = "辽A 88888888",
                        wt_num = 888.88
                    };*/
                    if (resp.status == "0")
                    {
                        //Progress?.Invoke(this, new ProgressEventArgs("请求重量失败"));
                        Logger.Warn("称重失败");
                        return;
                    }

                    //Progress?.Invoke(this, new ProgressEventArgs("正在输出到显示屏"));
                    string text = String.Format(scaleOperator.LEDOptions.CPZLMessage.Template, resp.tk_no, resp.wt_num);
                    if (!scaleOperator.Display(text, scaleOperator.LEDOptions.CPZLMessage.ShowConfig))
                    {
                        //Progress?.Invoke(this, new ProgressEventArgs("输出到显示屏失败"));
                        Logger.Warn("LED显示失败");
                        //return;
                    }

                    //Progress?.Invoke(this, new ProgressEventArgs("正在抬杆"));
                    Logger.Debug("[{0}]请求抬杆", scaleOperator.Name);
                    GatePassResponse gpResp = await scaleOperator.RestClient.GatePassAsync(scaleOperator.ScaleIP, scaleOperator.InOrOut.ToString(), resp.tk_no, resp.wt_num);
                    Logger.Debug("[{0}]返回抬杆", scaleOperator.Name);
                    /*GatePassResponse gpResp = new GatePassResponse()
                    {
                        status = 1
                    };*/
                    if (gpResp.status == "0")//抬杆失败
                    {
                        //Progress?.Invoke(this, new ProgressEventArgs("抬杆失败"));
                        Logger.Warn("抬杆失败：" + gpResp.reason);
                        //SwitchRelay(false);//no need to call this, it's already red
                        return;
                    }

                    Logger.Info("抬杆成功：");
                    //Progress?.Invoke(this, new ProgressEventArgs("抬杆成功"));

                    Succeed?.Invoke(this, new EventArgs());
                }
                catch (Exception ex)
                {
                    //Progress?.Invoke(this, new ProgressEventArgs("发生错误：" + ex.Message));
                    Logger.Warn("{0}上的本次操作失败:{1}", scaleOperator.Name, ex.ToString());
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }
    }
}
