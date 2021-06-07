using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using umamusumeKeyCtl.Util;

namespace umamusumeKeyCtl.CaptureScene
{
    public class VirtualKeyPushExecutor : Singleton<VirtualKeyPushExecutor>, IDisposable
    {
        private ConcurrentQueue<Task> _queue;
        public void EnQueue(Task task) => _queue.Enqueue(task);
        private CancellationTokenSource _cancellationTokenSource;

        public VirtualKeyPushExecutor()
        {
            _queue = new ConcurrentQueue<Task>();
            _cancellationTokenSource = new CancellationTokenSource();
            ExecuteQueue(_cancellationTokenSource.Token);
        }

        private async void ExecuteQueue(CancellationToken token)
        {
            try
            {
                while (token.IsCancellationRequested == false)
                {
                    while (_queue.Count == 0 && token.IsCancellationRequested == false)
                    {
                        await Task.Delay(1);
                    }

                    Task task = Task.CompletedTask;
                    while (!_queue.TryDequeue(out task))
                    {
                        await Task.Delay(1);
                    }

                    await task;
                }
            }
            catch (Exception e)
            {
                Debug.Write(e);
                throw;
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}