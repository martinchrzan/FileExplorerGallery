using FileExplorerGallery.Helpers;
using FileExplorerGallery.Keyboard;
using Squirrel;
using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

namespace FileExplorerGallery
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private Mutex _instanceMutex = null;
        private NotifyIcon _notifyIcon = null;

        [STAThread]
        public static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            try
            {
                var application = new App();
                application.InitializeComponent();
                application.Run();
            }
            catch (Exception ex)
            {
                Logger.LogError("Unhandled exception", ex);
            }
        }

        [Import]
        public KeyboardMonitor KeyboardMonitor { get; set; }

        [Import]
        public SettingsWindowHelper SettingsWindowHelper { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            // allow only one instance of file explorer gallery
            bool createdNew;
            _instanceMutex = new Mutex(true, @"Global\FileExplorerGallery", out createdNew);
            if (!createdNew)
            {
                _instanceMutex = null;
                Current.Shutdown();
                return;
            }

            using (var mgr = new UpdateManager(""))
            {
                // Note, in most of these scenarios, the app exits after this method
                // completes!
                SquirrelAwareApp.HandleEvents(
                  onInitialInstall: v => mgr.CreateShortcutForThisExe(),
                  onAppUpdate: v => mgr.CreateShortcutForThisExe(),
                  onAppUninstall: v => mgr.RemoveShortcutForThisExe(),
                  onFirstRun: () => ShowWelcome());
            }

            base.OnStartup(e);

            Bootstrapper.InitializeContainer(this);
            MigrateUserSettings();
            SetupTrayIcon();
            KeyboardMonitor.Start();
            Current.MainWindow = null;
        }

        private void ShowWelcome()
        {
            var welcomeWindow = new WelcomeWindow();
            welcomeWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_instanceMutex != null)
                _instanceMutex.ReleaseMutex();
            base.OnExit(e);
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.LogError("Unhandled exception", (e.ExceptionObject is Exception) ? (e.ExceptionObject as Exception) : new Exception());
        }

        private void MigrateUserSettings()
        {
            if (FileExplorerGallery.Properties.Settings.Default.UpdateSettings)
            {
                FileExplorerGallery.Properties.Settings.Default.Upgrade();
                FileExplorerGallery.Properties.Settings.Default.UpdateSettings = false;
                FileExplorerGallery.Properties.Settings.Default.Save();
            }
        }

        private void SetupTrayIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = new System.Drawing.Icon("Resources\\icon.ico"),
                Text = "File Explorer Gallery",
                ContextMenu = new ContextMenu()
            };
            _notifyIcon.Visible = true;

            _notifyIcon.ContextMenu.MenuItems.Add(new MenuItem("Settings", (s, e) => { SettingsWindowHelper.ShowSettings(); }) { ShowShortcut = false });
            _notifyIcon.ContextMenu.MenuItems.Add(new MenuItem("Close", (s, e) => { Current.Shutdown(); }) { ShowShortcut = false });
            _notifyIcon.MouseClick += (s, e) => SettingsWindowHelper.ShowSettings();
        }
    }
}
