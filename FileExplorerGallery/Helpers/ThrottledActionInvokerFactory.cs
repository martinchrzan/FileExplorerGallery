using System.ComponentModel.Composition;

namespace FileExplorerGallery.Helpers
{
    [Export(typeof(IThrottledActionInvokerFactory))]
    public class ThrottledActionInvokerFactory : IThrottledActionInvokerFactory
    {
        public IThrottledActionInvoker CreateThrottledActionInvoker()
        {
            return new ThrottledActionInvoker();
        }
    }
}
