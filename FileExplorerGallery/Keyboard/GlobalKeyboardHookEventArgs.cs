using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FileExplorerGallery.Helpers.Win32Apis;

namespace FileExplorerGallery.Keyboard
{
    internal class GlobalKeyboardHookEventArgs : HandledEventArgs
    {
        internal GlobalKeyboardHook.KeyboardState KeyboardState { get; private set; }
        internal LowLevelKeyboardInputEvent KeyboardData { get; private set; }

        internal GlobalKeyboardHookEventArgs(
            LowLevelKeyboardInputEvent keyboardData,
            GlobalKeyboardHook.KeyboardState keyboardState)
        {
            KeyboardData = keyboardData;
            KeyboardState = keyboardState;
        }
    }
}
