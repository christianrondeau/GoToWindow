using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace GoToWindow
{
	public class InterceptAltTab : IDisposable
	{
		private readonly Action _callback;

		[StructLayout(LayoutKind.Sequential)]
		public struct Kbdllhookstruct
		{
			public Key Key;
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
        
		public InterceptAltTab(Action callback)
		{
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

		        if (keyInfo.Key == Key.Tab && HasAltModifier(keyInfo.Flags))
		        {
		            if (wParam == (IntPtr) WM_SYSKEYDOWN)
		                _callback();

		            return (IntPtr) 1;
		        }
		    }

		    return CallNextHookEx(_hookID, nCode, wParam, lParam);
		}

		private static bool HasAltModifier(int flags)
		{
			return (flags & 0x20) == 0x20;
		}
	}
}