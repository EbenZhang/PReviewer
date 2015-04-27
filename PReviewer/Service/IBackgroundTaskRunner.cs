using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace PReviewer.Service
{
    public interface IBackgroundTaskRunner
    {
        void RunInBackground(Action action);
        void AddToQueue(Func<Task> action);
        void Quit();
    }

    public class BackgroundTaskRunner : IBackgroundTaskRunner
    {
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        readonly BlockingCollection<Func<Task>> _tasks = new BlockingCollection<Func<Task>>();
        private Task _backgroundTask;
        public BackgroundTaskRunner()
        {
            RunTasks();
        }
        public void RunInBackground(Action action)
        {
            Task.Run(action);
        }

        public void AddToQueue(Func<Task> action)
        {
            _tasks.Add(action);
        }

        public void Quit()
        {
            _cancel.Cancel(false);
            if (_backgroundTask != null && !_backgroundTask.IsCompleted)
            {
                _backgroundTask.Wait();
            }
        }

        protected void RunTasks()
        {
            _backgroundTask = Task.Run(async () =>
            {
                while (!_cancel.IsCancellationRequested)
                {
                    try
                    {
                        var task = _tasks.Take(_cancel.Token);
                        if (_cancel.IsCancellationRequested)
                        {
                            return;
                        }
                        try
                        {
                            var t = task();
                            await t;
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        //expected cancellation exceptin.
                    }
                }
                // ReSharper disable once FunctionNeverReturns
            });
        }
    }
}
