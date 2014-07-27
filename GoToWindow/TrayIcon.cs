using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Interop;
using System.Diagnostics;
using System.ComponentModel;

namespace GoToWindow
{
    public class TrayIcon : IDisposable
    {
        #region User 32

        const int NIM_ADD = 0x00;
        const int NIM_MODIFY = 0x01;
        const int NIM_DELETE = 0x02;

        const int NIF_MESSAGE = 0x01;
        const int NIF_ICON = 0x02;
        const int NIF_TIP = 0x04;

        const int ID_TRAY_APP_ICON = 5000;
        const int ID_TRAY_EXIT_CONTEXT_MENU_ITEM = 3000;
        const int WM_USER = 0x0400;
        const int WM_TRAYICON = (WM_USER + 1);

        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_LBUTTONUP = 0x0202;
        const int WM_LBUTTONDBLCLK = 0x0203;

        [StructLayout(LayoutKind.Sequential)]
        public struct NOTIFYICONDATA
        {
            public System.Int32 cbSize; // DWORD
            public System.IntPtr hWnd; // HWND
            public System.Int32 uID; // UINT
            public UInt32 uFlags; // UINT
            public System.Int32 uCallbackMessage; // UINT
            public System.IntPtr hIcon; // HICON
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public System.String szTip; // char[128]
            public System.Int32 dwState; // DWORD
            public System.Int32 dwStateMask; // DWORD
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public System.String szInfo; // char[256]
            public System.Int32 uTimeoutOrVersion; // UINT
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public System.String szInfoTitle; // char[64]
            public System.Int32 dwInfoFlags; // DWORD
        }

        [DllImport("shell32.dll")]
        static extern bool Shell_NotifyIcon(uint dwMessage, [In] ref NOTIFYICONDATA pnid);

        #endregion

        public event EventHandler LeftDoubleClick;

        private NOTIFYICONDATA m_notifyIconData;
        private Icon m_currentTrayIcon;
        private Window m_mainWindow;
        private HookWrapper m_hook;
        private bool m_bIsIconVisible = false;
        private bool _disposed = false;

        public bool IsIconVisible
        {
            get { return m_bIsIconVisible; }
        }

        public TrayIcon(Window mainWindow)
        {
            m_mainWindow = mainWindow;
            WindowInteropHelper helper = new WindowInteropHelper(mainWindow);
            IntPtr hMainWindow = helper.Handle;

            m_notifyIconData = new NOTIFYICONDATA();
            m_notifyIconData.cbSize = Marshal.SizeOf(m_notifyIconData);
            m_notifyIconData.hWnd = hMainWindow;
            m_notifyIconData.uID = ID_TRAY_APP_ICON;
            m_notifyIconData.uFlags = NIF_MESSAGE;
            m_notifyIconData.uCallbackMessage = WM_TRAYICON;
        }

        ~TrayIcon()
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

                if (disposing)
                {
                    m_hook.Dispose();
                }

                if (m_bIsIconVisible)
                    this.Hide();
            }
        }

        public void Show(Icon trayIcon, string strTip)
        {
            if (m_bIsIconVisible)
                return;

            m_currentTrayIcon = trayIcon;

            _prepStructure_Icon(trayIcon);
            _prepStructure_Tip(strTip);

            if (!Shell_NotifyIcon(NIM_ADD, ref m_notifyIconData))
                throw new Win32Exception();

            m_bIsIconVisible = true;

            m_hook = new HookWrapper(m_mainWindow, _fnHookCallback, WM_TRAYICON);
        }

        public void SetTip(string strTip)
        {
            if (!m_bIsIconVisible)
                return;

            _prepStructure_Icon(m_currentTrayIcon);
            _prepStructure_Tip(strTip);

            if (!Shell_NotifyIcon(NIM_MODIFY, ref m_notifyIconData))
                throw new Win32Exception();
        }

        public void Hide()
        {
            if (!m_bIsIconVisible)
                return;

            _prepStructure_Icon(null);
            _prepStructure_Tip(null);

            // Remove the icon with a system-level call
            if (!Shell_NotifyIcon(NIM_DELETE, ref m_notifyIconData))
                throw new Win32Exception();

            m_bIsIconVisible = false;

            if (m_hook != null)
            {
                m_hook.Dispose();
                m_hook = null;
            }
        }

        private void _prepStructure_Icon(Icon trayIcon)
        {
            if (trayIcon != null)
            {
                m_notifyIconData.uFlags |= NIF_ICON;
                m_notifyIconData.hIcon = trayIcon.Handle;
            }
            else
            {
                if ((m_notifyIconData.uFlags & NIF_ICON) == NIF_ICON)
                    m_notifyIconData.uFlags ^= NIF_ICON;

                m_notifyIconData.hIcon = IntPtr.Zero;
            }
        }

        private void _prepStructure_Tip(string strTip)
        {
            if (!string.IsNullOrEmpty(strTip))
            {
                m_notifyIconData.uFlags |= NIF_TIP;
                m_notifyIconData.szTip = strTip.Substring(0, Math.Min(127, strTip.Length));
            }
            else
            {
                if ((m_notifyIconData.uFlags & NIF_TIP) == NIF_TIP)
                    m_notifyIconData.uFlags ^= NIF_TIP;

                m_notifyIconData.szTip = null;
            }
        }

        private void _fnHookCallback(int msg, IntPtr wParam, IntPtr lParam)
        {
            int iEvent = lParam.ToInt32() & 0xFFFF;
            if (iEvent == WM_LBUTTONDBLCLK)
            {
                if (LeftDoubleClick != null)
                    LeftDoubleClick(this, new EventArgs());
            }
        }
    }
}
