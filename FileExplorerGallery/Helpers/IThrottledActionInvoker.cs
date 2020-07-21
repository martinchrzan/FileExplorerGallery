using System;

namespace FileExplorerGallery.Helpers
{
    public interface IThrottledActionInvoker : IDisposable
    {
        void ScheduleAction(Action action, int miliseconds);
    }
}
