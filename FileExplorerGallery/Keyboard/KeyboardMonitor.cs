using FileExplorerGallery.Helpers;
using FileExplorerGallery.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static FileExplorerGallery.Helpers.Win32Apis;

namespace FileExplorerGallery.Keyboard
{
    [Export(typeof(KeyboardMonitor))]
    public class KeyboardMonitor
    {
        private readonly IUserSettings _userSettings;
        private readonly AppStateHandler _appStateHandler;
        private List<int> _currentlyPressedKeys = new List<int>();
        private List<int> _activationKeys = new List<int>();
        private GlobalKeyboardHook _keyboardHook;
        private object showGalleryLock = new object();

        [ImportingConstructor]
        public KeyboardMonitor(IUserSettings userSettings, AppStateHandler appStateHandler)
        {
            _userSettings = userSettings;
            _appStateHandler = appStateHandler;
            _userSettings.ActivationShortcut.PropertyChanged += ActivationShortcut_PropertyChanged;
            SetActivationKeys();
        }

        public void Start()
        {
            _keyboardHook = new GlobalKeyboardHook();
            _keyboardHook.KeyboardPressed += Hook_KeyboardPressed;
            Logger.LogInfo("Pinging windows api");
            PingWindows();
            Logger.LogInfo("DONE - Pinging windows api");
        }

        // Enumarating currently opened windows for the first time is super slow, ping that API to "warm it up"
        private void PingWindows()
        {
            lock (showGalleryLock)
            {
                try{
                    var t = Type.GetTypeFromProgID("Shell.Application");
                    dynamic o = Activator.CreateInstance(t);
                    var ws = o.Windows();
                }
                catch (Exception ex)
                {
                    Logger.LogError("Failed to ping api to get all opened windows", ex);
                }
            }
        }

        private void SetActivationKeys()
        {
            _activationKeys.Clear();

            if (!string.IsNullOrEmpty(_userSettings.ActivationShortcut.Value))
            {
                var keys = _userSettings.ActivationShortcut.Value.Split('+');
                foreach (var key in keys)
                {
                    if (Enum.TryParse(key.Trim(), out Key parsedKey))
                    {
                        _activationKeys.Add(KeyInterop.VirtualKeyFromKey(parsedKey));
                    }
                }

                _activationKeys.Sort();
            }
        }

        private void ActivationShortcut_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SetActivationKeys();
        }

        private void ShowGallery()
        {

            lock (showGalleryLock)
            {
                if (!_appStateHandler.IsGalleryVisible())
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        IntPtr handle = GetForegroundWindow();

                        Win32Apis.Rect fileExplorerLocation;
                        
                        string explorepath;
                        var t = Type.GetTypeFromProgID("Shell.Application");
                        dynamic o = Activator.CreateInstance(t);
                        try
                        {
                            var ws = o.Windows();
                            for (int i = 0; i < ws.Count; i++)
                            {
                                var ie = ws.Item(i);
                                if (ie == null || ie.hwnd != (long)handle) continue;
                                var path = System.IO.Path.GetFileName((string)ie.FullName);
                                if (path.ToLower() == "explorer.exe")
                                {
                                    if (!GetWindowRect(new HandleRef(this, handle), out fileExplorerLocation))
                                    {
                                        Logger.LogWarning("Failed to find where is current file explorer located");
                                    }

                                    var selectedItem = ie.document.focuseditem;
                                    if(selectedItem != null)
                                    {
                                        selectedItem = selectedItem.path;
                                    }
                                    explorepath = ie.LocationURL;
                                    if (explorepath.StartsWith("file:///"))
                                    {
                                        explorepath = explorepath.Substring(8);
                                        explorepath = explorepath.Replace("/", "\\");
                                    } 
                                    _appStateHandler.ShowGallery(Uri.UnescapeDataString(explorepath), selectedItem, fileExplorerLocation);
                                }
                            }
                        }
                        finally
                        {
                            Marshal.FinalReleaseComObject(o);
                        }
                    });
                }
            }
        }

        private void Hook_KeyboardPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            var virtualCode = e.KeyboardData.VirtualCode;
            if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown || e.KeyboardState == GlobalKeyboardHook.KeyboardState.SysKeyDown)
            {
                if (!_currentlyPressedKeys.Contains(virtualCode))
                {
                    _currentlyPressedKeys.Add(virtualCode);
                }
            }
            else if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyUp || e.KeyboardState == GlobalKeyboardHook.KeyboardState.SysKeyUp)
            {
                if (_currentlyPressedKeys.Contains(virtualCode))
                {
                    _currentlyPressedKeys.Remove(virtualCode);
                }
            }

            _currentlyPressedKeys.Sort();

            if (ArraysAreSame(_currentlyPressedKeys, _activationKeys))
            {
                Task.Run(() =>
                {
                    ShowGallery();            
                });
                _currentlyPressedKeys.Clear();
            }
        }

        private bool ArraysAreSame(List<int> first, List<int> second)
        {
            if (first.Count != second.Count)
            {
                return false;
            }

            for (int i = 0; i < first.Count; i++) 


            {
                if (first[i] != second[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
