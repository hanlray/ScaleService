using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    public static class AsyncExtensions
    {
        public static async Task<int> ReadAsync(this NetworkStream stream, byte[] buffer, int offset, int count, int TimeOut)
        {
            var ReciveCount = 0;
            var receiveTask = Task.Run(async () => { ReciveCount = await stream.ReadAsync(buffer, offset, count); });
            var isReceived = await Task.WhenAny(receiveTask, Task.Delay(TimeOut)) == receiveTask;
            if (!isReceived) return -1;
            return ReciveCount;
        }

        public static async Task TimeoutAfter(this Task task, TimeSpan timeout)
        {
            using var timeoutCancellationTokenSource = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
            if (completedTask == task)
            {
                timeoutCancellationTokenSource.Cancel();
                await task;  // Very important in order to propagate exceptions
            }
            else
            {
                throw new TimeoutException("The operation has timed out.");
            }
        }
    }
}
