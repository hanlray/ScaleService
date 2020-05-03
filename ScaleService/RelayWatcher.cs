using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace ScaleService
{
    public class RelayWatcher
    {
        private TcpClient _tcpClient;
        private string _host;
        private int _port;
        public delegate void InChangedEventHandler(object sender, InChangedEventArgs e);
        private Dictionary<int, InChangedEventHandler> _dic = new Dictionary<int, InChangedEventHandler>();
        private StreamReader streamReader;

        public RelayWatcher(IConfiguration configuration)
        {
            var section = configuration.GetSection("UpRelay");
            Setup(section["IP"], Convert.ToInt32(section["Port"]));
        }
        
        private void Setup(string host, int port)
        {
            this._tcpClient = new TcpClient();
            this._host = host;
            this._port = port;
        }

        public async void StartAsync()
        {
            await _tcpClient.ConnectAsync(_host, _port);
            var stream = _tcpClient.GetStream();
            streamReader = new StreamReader(stream);
            while (true)
            {
                string message = await streamReader.ReadLineAsync();
                //parse the message
                int index = message.IndexOf('=');
                if (index < 0) continue;
                var s = message.Substring(index + 1).Split(',');
                for(var i=0;i<s.Length;i++)
                {
                    int inIndex = i + 1;
                    if (_dic.ContainsKey(inIndex))
                    {
                        var v = Convert.ToInt32(s[i]);
                        _dic[inIndex].Invoke(this, new InChangedEventArgs(v));
                    }
                }
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
