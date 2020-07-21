using System.ComponentModel.Composition;

namespace FileExplorerGallery.Settings
{
    [Export(typeof(IUserSettings))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class UserSettings : IUserSettings
    {
        [ImportingConstructor]
        public UserSettings()
        {
            var settings = Properties.Settings.Default;
            RunOnStartup = new SettingItem<bool>(settings.RunOnStartup, (currentValue) => { settings.RunOnStartup = currentValue; SaveSettings(); });
            AutomaticUpdates = new SettingItem<bool>(settings.AutomaticUpdates, (currentValue) => { settings.AutomaticUpdates = currentValue; SaveSettings(); });
            ActivationShortcut = new SettingItem<string>(settings.ActivationShortcut, (currentValue) => { settings.ActivationShortcut = currentValue; SaveSettings(); });
            SlideshowDuration = new SettingItem<int>(settings.SlideshowDuration, (currentValue) => { settings.SlideshowDuration = currentValue; SaveSettings(); });
        }

        public SettingItem<bool> RunOnStartup { get; }

        public SettingItem<string> ActivationShortcut { get; }

        public SettingItem<bool> AutomaticUpdates { get; }

        public SettingItem<int> SlideshowDuration { get; }

        private void SaveSettings()
        {
            Properties.Settings.Default.Save();
        }
    }
}
