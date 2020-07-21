namespace FileExplorerGallery.Helpers
{
    public interface IThrottledActionInvokerFactory
    {
        IThrottledActionInvoker CreateThrottledActionInvoker();
    }
}
