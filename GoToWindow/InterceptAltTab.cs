using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GoToWindow
{
	public class InterceptAltTab : IDisposable
	{
		private readonly Action _callback;

		[StructLayout(LayoutKind.Sequential)]
		public struct Kbdllhookstruct
		{
			public Keys Key;
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

		public InterceptAltTab(Action callback)
		{
			_callback = callback;
			_hookID = SetHook(HookCallback);
		}

		public void Dispose()
		{
			UnhookWindowsHookEx(_hookID);
		}

		private static IntPtr SetHook(LowLevelKeyboardProc proc)
		{
			using (var curProcess = Process.GetCurrentProcess())
			{
				using (var curModule = curProcess.MainModule)
				{
					return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
				}
			}
		}

		private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
		{
			var keyInfo = (Kbdllhookstruct)Marshal.PtrToStructure(lParam, typeof(Kbdllhookstruct));

			if (wParam == (IntPtr) WM_SYSKEYDOWN && keyInfo.Key == Keys.Tab && HasAltModifier(keyInfo.Flags))
			{
				_callback();
				return (IntPtr)1;
			}

			return CallNextHookEx(_hookID, nCode, wParam, lParam);
		}

		private static bool HasAltModifier(int flags)
		{
			return (flags & 0x20) == 0x20;
		}
	}
}