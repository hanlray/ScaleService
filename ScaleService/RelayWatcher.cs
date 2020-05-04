using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScaleService
{
    public class RelayWatcher
    {
        //private TcpClient _tcpClient;
        private string _host;
        private int _port;
        public delegate void InChangedEventHandler(object sender, InChangedEventArgs e);
        private Dictionary<int, InChangedEventHandler> _dic = new Dictionary<int, InChangedEventHandler>();
        private ILogger<RelayWatcher> _logger;

        //private StreamReader streamReader;

        public RelayWatcher(IConfiguration configuration, ILogger<RelayWatcher> logger)
        {
            this._logger = logger;

            var section = configuration.GetSection("UpRelay");
            _host = section["IP"];
            _port = Convert.ToInt32(section["Port"]);
        }

        public async void StartAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                using var tcpClient = new TcpClient();
                _logger.LogDebug("正在建立连接");
                try
                {
                    await tcpClient.ConnectAsync(_host, _port);
                }
                catch (Exception e)
                {
                    //wait 2 second to reconnect
                    _logger.LogDebug("建立连接失败，稍后会重试：{0}", e.Message);
                    await Task.Delay(2000);
                    continue;
                }

                _logger.LogDebug("连接成功");

                try
                {
                    using var stream = tcpClient.GetStream();
                    byte[] buf = new byte[256];
                    int i = 0;
                    while (stream.CanRead)
                    {
                        _logger.LogTrace("正在读取");
                        int n = await stream.ReadAsync(buf, i, 1, stoppingToken);
                        if (n != 1) throw new Exception("网络读取失败");

                        _logger.LogTrace("读到一个字节");
                        char c = (char)buf[i];
                        if (c != '\n')
                        {
                            i++;
                            if (i >= 256) throw new Exception("没有读到一行");
                            continue;
                        }

                        string message = Encoding.ASCII.GetString(buf, 0, i);

                        Process(message);

                        i = 0;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogDebug("网络抛出异常:{0}", e);
                    //need to reconnect
                    //tcpClient.Close();
                    //continue;
                }
            }
        }

        private void Process(string message)
        {
            _logger.LogDebug("收到消息：{0}", message);
            try
            {
                int index = message.IndexOf(':');
                if (index < 0) return;
                var s = message.Substring(index + 1).Split(',');
                for (var i = 0; i < s.Length; i++)
                {
                    int inIndex = i + 1;
                    if (_dic.ContainsKey(inIndex))
                    {
                        var v = Convert.ToInt32(s[i]);
                        _logger.LogTrace("通知In{0}变化：{1}", inIndex, v);
                        _dic[inIndex].Invoke(this, new InChangedEventArgs(v));
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("处理上行消息失败:" + e.Message);
            }
        }

        public void Subscribe(int inPort, InChangedEventHandler eh)
        {
            if (_dic.ContainsKey(inPort))
            {
                _dic[inPort] += eh;
            }
            else
            {
                _dic.Add(inPort, eh);
            }
        }
    }

    public class InChangedEventArgs : EventArgs
    {
        public int Value { get; }

        public InChangedEventArgs(int v)
        {
            this.Value = v;
        }
    }
}
