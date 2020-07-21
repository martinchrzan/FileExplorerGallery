using System;
using System.Threading;
using System.Windows.Threading;

namespace FileExplorerGallery.Helpers
{
    public sealed class ThrottledActionInvoker : IThrottledActionInvoker
    {
        private Action _actionToRun;

        private Timer _timer;

        public ThrottledActionInvoker()
        {
            _timer = new Timer(Callback, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void ScheduleAction(Action action, int miliseconds)
        {
            _actionToRun = action;
            _timer.Change(miliseconds, Timeout.Infinite);
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        private void Callback(object state)
        {
            Dispatcher.CurrentDispatcher.Invoke(_actionToRun);
        }
    }
}
