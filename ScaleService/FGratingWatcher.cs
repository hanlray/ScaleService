using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ScaleService
{
    public partial class ScaleOperator
    {
        class FGratingWatcher
        {
            public event EventHandler Busy;
            public event EventHandler FakeCar;
            public event EventHandler Error;

            public ScaleOperator Operator { get; internal set; }

            private ScaleOperator scaleOperator;

            //public string Name { get; internal set; }
            public bool Enabled { get; set; } = true;
            public bool IsBusy { get; private set; } = false;

            public FGratingWatcher(ScaleOperator scaleOperator)
            {
                this.scaleOperator = scaleOperator;
            }

            internal async void ProcessAsync(object sender, EventArgs e)
            {
                try
                {
                    if (!Enabled) return;

                    Logger.Debug("{0}光栅触发", scaleOperator.Name);

                    if (IsBusy) return;
                    IsBusy = true;
                    Busy?.Invoke(this, new EventArgs());

                    await scaleOperator.SwitchRelayAsync(false);//turn red

                    double weight = 0;
                    GetWtResponse resp = null;
                    for (int i = 1; i <= 5; i++)
                    {
                        Logger.Debug("[{0}]请求重量", scaleOperator.Name);
                        resp = await scaleOperator.RestClient.GetWtAsync(scaleOperator.ScaleIP, scaleOperator.InOrOut.ToString());
                        Logger.Debug("[{0}]返回重量{1}", scaleOperator.Name, resp.wt_num);
                        /*GetWtResponse resp = new GetWtResponse()
                        {
                            status = "1",
                            tk_no = "辽A 88888888",
                            wt_num = "1000.88"
                        };*/
                        if (resp.status == "0")
                        {
                            //Progress?.Invoke(this, new ProgressEventArgs("请求重量失败"));
                            Logger.Warn("称重失败");
                            await Task.Delay(1000);
                            continue;
                        }
                        var w = Convert.ToDouble(resp.wt_num);
                        if (w > 1000)
                        {
                            Logger.Debug("称重大于1000");
                            weight = w;
                            break;
                        }
                        await Task.Delay(1000);
                    }
                    if (weight < 1000)
                    {
                        Logger.Debug("称重小于1000, 开始抬杆");
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
                        }
                        else if (gpResp.status == "1")
                        {
                            Logger.Info("抬杆成功");
                            //Progress?.Invoke(this, new ProgressEventArgs("抬杆成功"));
                            //SwitchRelay(true);
                        }
                        FakeCar?.Invoke(this, new EventArgs());
                    }
                }
                catch (Exception ex)
                {
                    Error?.Invoke(this, new EventArgs());
                    Logger.Debug("处理前光栅事件发生异常：{0}", ex.Message);
                }
                IsBusy = false;
            }
        }
    }
}
