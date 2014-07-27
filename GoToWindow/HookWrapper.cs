using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace GoToWindow
{
    public class HookWrapper : IDisposable
    {
        #region User 32

        public enum HookType : int
        {
            WH_JOURNALRECORD = 0,
            WH_JOURNALPLAYBACK = 1,
            WH_KEYBOARD = 2,
            WH_GETMESSAGE = 3,
            WH_CALLWNDPROC = 4,
            WH_CBT = 5,
            WH_SYSMSGFILTER = 6,
            WH_MOUSE = 7,
            WH_HARDWARE = 8,
            WH_DEBUG = 9,
            WH_SHELL = 10,
            WH_FOREGROUNDIDLE = 11,
            WH_CALLWNDPROCRET = 12,
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14
        }

        public const int WM_KEYUP = 0x0101;

        [StructLayout(LayoutKind.Sequential)]
        public struct CWPSTRUCT
        {
            public IntPtr lparam;
            public IntPtr wparam;
            public int message;
            public IntPtr hwnd;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(HookType hookType, IntPtr pFunc, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr hHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool UnhookWindowsHookEx(IntPtr hHook);

        #endregion

        private delegate int HookProcDelegate(int code, IntPtr wParam, IntPtr lParam);
        public delegate void NotifyDelegate(int msg, IntPtr wParam, IntPtr lParam);

        public static int NoFilter = 0;

        private IntPtr m_hHook;
        private HookProcDelegate m_fnHookProc;
        private NotifyDelegate m_fnNotify;
        private bool _disposed = false;
        private int m_iMessageFilter;

        public HookWrapper(Window targetWindow, NotifyDelegate fnNotify, int iMessageFilter)
        {
            m_fnNotify = fnNotify;
            m_iMessageFilter = iMessageFilter;

#pragma warning disable 0618
            uint ThreadId = (uint)AppDomain.GetCurrentThreadId();
#pragma warning restore 0618

            m_fnHookProc = new HookProcDelegate(_localCallbackFunction);
            IntPtr pFunc = Marshal.GetFunctionPointerForDelegate(m_fnHookProc);

            m_hHook = SetWindowsHookEx(
                HookType.WH_CALLWNDPROC,
                pFunc,
                IntPtr.Zero,
                ThreadId
            );
        }

        ~HookWrapper()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                _disposed = true;

                if (m_hHook != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(m_hHook);
                    m_hHook = IntPtr.Zero;
                }
            }
        }

        private int _localCallbackFunction(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code < 0)
                return CallNextHookEx(m_hHook, code, wParam, lParam).ToInt32();

            CWPSTRUCT cwp = (CWPSTRUCT)Marshal.PtrToStructure(lParam, typeof(CWPSTRUCT));

            if ((m_iMessageFilter == NoFilter) || (m_iMessageFilter == cwp.message))
            {
                m_fnNotify(cwp.message, cwp.wparam, cwp.lparam);
            }

            return CallNextHookEx(m_hHook, code, wParam, lParam).ToInt32();
        }
    }
}
