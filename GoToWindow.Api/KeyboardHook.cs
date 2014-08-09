using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace GoToWindow.Api
{
    public class HotKey : IDisposable
    {
        private static Dictionary<int, HotKey> _dictHotKeyToCallBackProc;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, UInt32 fsModifiers, UInt32 vlc);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public const int WmHotKey = 0x0312;

        private bool _disposed;

        public Key Key { get; private set; }
        public KeyModifier KeyModifiers { get; private set; }
        public Action<HotKey> Action { get; private set; }
        public int Id { get; set; }

        public HotKey(Key k, KeyModifier keyModifiers, Action<HotKey> action, bool register = true)
        {
            Key = k;
            KeyModifiers = keyModifiers;
            Action = action;
            if (register)
            {
                Register();
            }
        }

        public void Register()
        {
            var virtualKeyCode = KeyInterop.VirtualKeyFromKey(Key);
            Id = virtualKeyCode + ((int)KeyModifiers * 0x10000);
            var result = RegisterHotKey(IntPtr.Zero, Id, (UInt32)KeyModifiers, (UInt32)virtualKeyCode);

            if(!result)
                throw new Win32Exception();

            if (_dictHotKeyToCallBackProc == null)
            {
                _dictHotKeyToCallBackProc = new Dictionary<int, HotKey>();
                ComponentDispatcher.ThreadFilterMessage += ComponentDispatcherThreadFilterMessage;
            }

            _dictHotKeyToCallBackProc.Add(Id, this);
        }

        public void Unregister()
        {
            HotKey hotKey;
            if (_dictHotKeyToCallBackProc.TryGetValue(Id, out hotKey))
            {
                UnregisterHotKey(IntPtr.Zero, Id);
            }
        }

        private static void ComponentDispatcherThreadFilterMessage(ref MSG msg, ref bool handled)
        {
            if (handled) return;

            if (msg.message != WmHotKey) return;

            HotKey hotKey;

            if (!_dictHotKeyToCallBackProc.TryGetValue((int) msg.wParam, out hotKey)) return;

            if (hotKey.Action != null)
                hotKey.Action.Invoke(hotKey);

            handled = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                Unregister();
            }

            _disposed = true;
        }
    }

    [Flags]
    public enum KeyModifier
    {
        None = 0x0000,
        Alt = 0x0001,
        Ctrl = 0x0002,
        NoRepeat = 0x4000,
        Shift = 0x0004,
        Win = 0x0008
    }

    /*
    public class KeyboardHook : IDisposable
    {
        private readonly KeyboardShortcut _shortcut;
		private readonly Action _callback;

		[StructLayout(LayoutKind.Sequential)]
		public struct Kbdllhookstruct
		{
            public int VkCode;
			public int ScanCode;
			public int Flags;
			public int Time;
			public IntPtr Extra;
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool UnhookWindowsHookEx(IntPtr hhk);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr GetModuleHandle(string lpModuleName);

		// ReSharper disable InconsistentNaming
		private const int WH_KEYBOARD_LL = 13;
		private const int WM_SYSKEYDOWN = 0x0104;
		private static IntPtr _hookID = IntPtr.Zero;
		// ReSharper restore InconsistentNaming

		private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly LowLevelKeyboardProc _proc;
        
		public KeyboardHook(Action callback)
		{
		    _shortcut = new KeyboardShortcut
		    {
		        VirtualKeyCode = KeyboardVirtualCodes.Tab,
		        Modifier = KeyboardVirtualCodes.Modifiers.Alt
		    };

            _callback = callback;
            // ReSharper disable once RedundantDelegateCreation
            _proc = new LowLevelKeyboardProc(HookCallback);
            using (var curProcess = Process.GetCurrentProcess())
            {
                using (var curModule = curProcess.MainModule)
                {
                    _hookID = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(curModule.ModuleName), 0);
                }
            }
		}
        
		public void Dispose()
		{
			UnhookWindowsHookEx(_hookID);
		}
        
		private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
		{
		    if (nCode >= 0)
		    {
		        var keyInfo = (Kbdllhookstruct) Marshal.PtrToStructure(lParam, typeof (Kbdllhookstruct));

                if (_shortcut.IsDown(keyInfo.VkCode, keyInfo.Flags))
		        {
		            if (wParam == (IntPtr) WM_SYSKEYDOWN)
		                _callback();

		            return (IntPtr) 1;
		        }
		    }

		    return CallNextHookEx(_hookID, nCode, wParam, lParam);
		}
	}
     * */
}