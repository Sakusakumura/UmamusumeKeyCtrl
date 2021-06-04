using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using umamusumeKeyCtl.Util;

namespace umamusumeKeyCtl.CaptureScene
{
    public class VirtualKeyPushExecutor : Singleton<VirtualKeyPushExecutor>, IDisposable
    {
        private Queue<Task> _queue;
        public void EnQueue(Task task) => _queue.Enqueue(task);
        private CancellationTokenSource _cancellationTokenSource;
        private bool kill = false;

        public VirtualKeyPushExecutor()
        {
            _queue = new Queue<Task>();
            _cancellationTokenSource = new CancellationTokenSource();
            _ = Task.Run(() => ExecuteQueue(_cancellationTokenSource.Token));
        }

        public void Kill()
        {
            kill = true;
        }

        private async Task ExecuteQueue(CancellationToken token)
        {
            while (token.IsCancellationRequested == false && kill == false)
            {
                while (_queue.Count == 0 && token.IsCancellationRequested == false && kill == false)
                {
                    await Task.Delay(1);
                }

                try
                {
                    await _queue.Dequeue();
                }
                catch (Exception e)
                {
                    Debug.Print(e.ToString());
                    throw;
                }
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}