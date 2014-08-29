// #define DEBUG_KEYS

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

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

		// ReSharper disable InconsistentNaming
		private const int HC_ACTION = 0;
		private const int WH_KEYBOARD_LL = 13;
		private const int WM_KEYDOWN = 0x0100;
		private const int WM_KEYUP = 0x0101;
		private const int WM_SYSKEYDOWN = 0x0104;
		private const int WM_SYSKEYUP = 0x0105;
		private static IntPtr _hookID = IntPtr.Zero;
		// ReSharper restore InconsistentNaming

		#if(DEBUG_KEYS)
		private static Dictionary<int, string> WMKeyNames = new Dictionary<int, string>
		{
			{ WM_KEYDOWN, "WM_KEYDOWN" },
			{ WM_KEYUP, "WM_KEYUP" },
			{ WM_SYSKEYDOWN, "WM_SYSKEYDOWN" },
			{ WM_SYSKEYUP, "WM_SYSKEYUP" },
		};
		#endif

		private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

		// ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
		private readonly LowLevelKeyboardProc _proc;

		public KeyboardHook(KeyboardShortcut shortcut, Action callback)
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
			/*
				VKCode	Flags	WParam			Desc
				0xA4	0x20	WM_SYSKEYDOWN	Alt Down
				0x09	0x20	WM_SYSKEYDOWN	Tab Down
				0x09	0xA0	WM_SYSKEYUP		Tab Up
				0xA4	0x80	WM_KEYUP		Alt Up
			*/

		    if (nCode != HC_ACTION)
                return CallNextHookEx(_hookID, nCode, wParam, lParam);
		    
            var keyInfo = (Kbdllhookstruct)Marshal.PtrToStructure(lParam, typeof(Kbdllhookstruct));

            #if(DEBUG_KEYS)
			Console.WriteLine("Keys: 0x{0}\t0x{1}\t{2}", keyInfo.VkCode.ToString("X2"), keyInfo.Flags.ToString("X2"), WMKeyNames[(int)wParam]);
			#endif

		    if (_shortcut.IsDown(keyInfo.VkCode, keyInfo.Flags))
		    {
		        if (wParam == (IntPtr)WM_SYSKEYDOWN)
		        {
                    #if(DEBUG_KEYS)
					Debug.WriteLine("Keys: Shortcut down");
					#endif

		            _shortcut.DownCounter++;
		        }

		        if (_shortcut.DownCounter < _shortcut.ShortcutPressesBeforeOpen)
		            return CallNextHookEx(_hookID, nCode, wParam, lParam);
				    
		        if (wParam == (IntPtr)WM_SYSKEYDOWN)
		            _callback();

		        return (IntPtr)1;
		    }
			    
		    if(_shortcut.IsControlKeyReleased(keyInfo.VkCode, keyInfo.Flags))
		    {
                #if(DEBUG_KEYS)
				Debug.WriteLine("Keys: Control key up");
				#endif

		        _shortcut.DownCounter = 0;
		    }

		    return CallNextHookEx(_hookID, nCode, wParam, lParam);
		}
	}
}
