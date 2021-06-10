using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using umamusumeKeyCtl.Util;

namespace umamusumeKeyCtl.CaptureScene
{
    public class VirtualKeyPushExecutor : Singleton<VirtualKeyPushExecutor>, IDisposable
    {
        private ConcurrentQueue<Func<Dispatcher, Task>> _queue;
        public void EnQueue(Func<Dispatcher, Task> task) => _queue.Enqueue(task);
        private CancellationTokenSource _cancellationTokenSource;
        private Dispatcher _dispatcher;

        public VirtualKeyPushExecutor()
        {
            _dispatcher = Dispatcher.FromThread(Application.Current.Dispatcher.Thread);
            _queue = new ConcurrentQueue<Func<Dispatcher, Task>>();
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => ExecuteQueue(_cancellationTokenSource.Token));
        }

        private async void ExecuteQueue(CancellationToken token)
        {
            try
            {
                Task task;
                
                while (token.IsCancellationRequested == false)
                {
                    while (_queue.Count == 0 && token.IsCancellationRequested == false)
                    {
                        await Task.Delay(1);
                    }

                    Func<Dispatcher, Task> func;
                    
                    while (!_queue.TryDequeue(out func) && token.IsCancellationRequested == false)
                    {
                    }

                    task = func.Invoke(_dispatcher);

                    Debug.Print($"[{this.GetType().Name}] Dequeued. ({_queue.Count} left)");
                    
                    await task;

                    await Task.Delay(30);
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