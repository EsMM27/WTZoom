using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;

namespace WarThunderZoom
{
    public enum VirtualKeyCode : ushort
    {
        F9 = 0x78, // Virtual key code for F9
        // Add other keys as needed
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LowLevelKeyboardInputEvent
    {
        public int VirtualCode;
        public int HardwareScanCode;
        public int Flags;
        public int TimeStamp;
        public IntPtr AdditionalInformation;
    }

    public class GlobalKeyboardHookEventArgs : HandledEventArgs
    {
        public LowLevelKeyboardInputEvent KeyboardData { get; set; }
        public GlobalKeyboardHookEventArgs(LowLevelKeyboardInputEvent keyboardData)
        {
            KeyboardData = keyboardData;
        }
    }

    public class GlobalKeyboardHook
    {
        private IntPtr _windowsHookHandle;
        private IntPtr _user32LibraryHandle;
        private HookProc _hookProc;
        private static readonly int WH_KEYBOARD_LL = 13;

        public event EventHandler<GlobalKeyboardHookEventArgs> KeyboardPressed;

        public GlobalKeyboardHook()
        {
            _hookProc = LowLevelKeyboardProc;
            _user32LibraryHandle = LoadLibrary("User32");
            _windowsHookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _hookProc, _user32LibraryHandle, 0);
        }

        public void StartListening()
        {
            Application.Current.Exit += (sender, e) => UnhookWindowsHookEx(_windowsHookHandle);
        }

        private IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            bool fEatKeyStroke = false;
            if (nCode >= 0)
            {
                var wParamTyped = wParam.ToInt32();
                if (Enum.IsDefined(typeof(KeyboardState), wParamTyped))
                {
                    var o = Marshal.PtrToStructure<LowLevelKeyboardInputEvent>(lParam);
                    var p = new GlobalKeyboardHookEventArgs(o);

                    KeyboardPressed?.Invoke(this, p);
                    fEatKeyStroke = p.Handled;
                }
            }

            return fEatKeyStroke ? (IntPtr)1 : CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private enum KeyboardState
        {
            KeyDown = 0x0100,
            KeyUp = 0x0101,
            SysKeyDown = 0x0104,
            SysKeyUp = 0x0105
        }

        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, int dwThreadId);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr LoadLibrary(string lpFileName);
    }
}