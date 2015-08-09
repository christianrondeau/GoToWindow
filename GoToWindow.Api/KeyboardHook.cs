using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

#if(DEBUG_KEYS)
using System.Collections.Generic;
#endif

namespace GoToWindow.Api
{
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

		[DllImport("user32.dll")]
		static extern short GetAsyncKeyState(int vKey);

		// ReSharper disable InconsistentNaming
		// ReSharper disable UnusedMember.Local
		private const int HC_ACTION = 0;
		private const int WH_KEYBOARD_LL = 13;
		private const int WM_KEYDOWN = 0x0100;
		private const int WM_KEYUP = 0x0101;
		private const int WM_SYSKEYDOWN = 0x0104;
		private const int WM_SYSKEYUP = 0x0105;
		private static IntPtr _hookID = IntPtr.Zero;
		// ReSharper restore UnusedMember.Local
		// ReSharper restore InconsistentNaming

		#if(DEBUG_KEYS)
		private static readonly Dictionary<int, string> WMKeyNames = new Dictionary<int, string>
		{
			{ WM_KEYDOWN, "WM_KEYDOWN" },
			{ WM_KEYUP, "WM_KEYUP" },
			{ WM_SYSKEYDOWN, "WM_SYSKEYDOWN" },
			{ WM_SYSKEYUP, "WM_SYSKEYUP" },
		};
		#endif

		private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

		private int _downCounter;

		public static KeyboardHook Hook(KeyboardShortcut shortcut, Action callback)
		{
			return new KeyboardHook(shortcut, callback);
		}

		// ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
		private readonly LowLevelKeyboardProc _proc;

		private KeyboardHook(KeyboardShortcut shortcut, Action callback)
		{
			_shortcut = shortcut;
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
			if (nCode != HC_ACTION)
				return CallNextHookEx(_hookID, nCode, wParam, lParam);

			var keyInfo = (Kbdllhookstruct)Marshal.PtrToStructure(lParam, typeof(Kbdllhookstruct));

			#if(DEBUG_KEYS)
			Console.WriteLine("Keys: 0x{0:X2}\t0x{1:X2}\t{2}\t{3}", keyInfo.VkCode, keyInfo.Flags, WMKeyNames[(int)wParam], GetAsyncKeyState(_shortcut.ControlVirtualKeyCode));
			#endif

			var expectedDownWParam = _shortcut.ControlVirtualKeyCode == (int)KeyboardControlKeys.LAlt ? WM_SYSKEYDOWN : WM_KEYDOWN;

			if ((int) wParam != expectedDownWParam)
				return CallNextHookEx(_hookID, nCode, wParam, lParam);

			if (keyInfo.VkCode == _shortcut.VirtualKeyCode && (GetAsyncKeyState(_shortcut.ControlVirtualKeyCode) < 0))
			{
				_downCounter++;

				if (_downCounter < _shortcut.ShortcutPressesBeforeOpen)
					return CallNextHookEx(_hookID, nCode, wParam, lParam);

				_callback();
				return (IntPtr) 1;
			}

			_downCounter = 0;
			return CallNextHookEx(_hookID, nCode, wParam, lParam);
		}
	}
}
