using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace TcpClientTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync("localhost", 13000);
            Console.WriteLine("Connected");
            byte[] buf = new byte[500_000_000];
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(10000));
            try
            {
                await tcpClient.GetStream().WriteAsync(buf, 0, buf.Length, cts.Token);
                Console.WriteLine("Here");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception happened");
            }
        }
    }
}
