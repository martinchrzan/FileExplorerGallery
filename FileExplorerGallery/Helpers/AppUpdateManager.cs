using Squirrel;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FileExplorerGallery.Helpers
{
    [Export(typeof(AppUpdateManager))]
    public class AppUpdateManager
    {
        private const string UpdateLocation = "https://github.com/martinchrzan/FileExplorerGallery";
        public AppUpdateManager()
        {
        }

        public Task<bool> IsNewUpdateAvailable()
        {
            return Task.Run(async () =>
            {
                try
                {
                    using (var mgr = await UpdateManager.GitHubUpdateManager(UpdateLocation))
                    {
                        var updateInfo = await mgr.CheckForUpdate();
                        if (updateInfo != null && updateInfo.ReleasesToApply != null && updateInfo.ReleasesToApply.Count > 0
                        && updateInfo.ReleasesToApply.Last().Version.Version > Assembly.GetExecutingAssembly().GetName().Version)
                        {
                            return true;
                        }
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("Failed to check for updates", ex);
                }
                return false;
            });
        }

        public Task Update()
        {
            return Task.Run(async () =>
            {
                try
                {
                    using (var mgr = await UpdateManager.GitHubUpdateManager(UpdateLocation))
                    {
                        await mgr.UpdateApp();
                        UpdateManager.RestartApp();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("Failed to download and apply an update", ex);
                }
                return Task.CompletedTask;
            });
        }
    }
}
