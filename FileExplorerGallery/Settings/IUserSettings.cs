namespace FileExplorerGallery.Settings
{
    public interface IUserSettings
    {
        SettingItem<bool> RunOnStartup { get; }

        SettingItem<string> ActivationShortcut { get; }

        SettingItem<bool> AutomaticUpdates { get; }

        SettingItem<int> SlideshowDuration { get; }

        SettingItem<bool> ShowDeleteConfirmation { get; }

        SettingItem<bool> BackupDeletedImages { get; }
    }
}
