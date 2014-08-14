using System;
using System.Runtime.InteropServices;

namespace GoToWindow.Api
{
    public static class KeyboardSend
    {
        // http://msdn.microsoft.com/en-ca/library/windows/desktop/dd375731(v=vs.85).aspx
        public static byte LCtrl = 0xA2; //VK_LCONTROL
        public static byte LWin = 0x5B; //VK_LWIN
        public static byte LAlt = 0xA4; //VK_LMENU
        public static byte Tab = 0x09; //VK_TAB

        public static byte GetNumber(int number)
        {
            if (number < 0 || number > 9)
                throw new ApplicationException("Invalid numbre for key press. Must be between 0 and 9:" + number);

            return (byte)(0x30 + number);
        }

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        // ReSharper disable InconsistentNaming
        private const int KEYEVENTF_EXTENDEDKEY = 1;
        private const int KEYEVENTF_KEYUP = 2;
        // ReSharper restore InconsistentNaming

        public static void KeyPress(byte vKey)
        {
            KeyDown(vKey);
            KeyUp(vKey);
        }

        public static void KeyDown(byte vKey)
        {
            keybd_event(vKey, 0, KEYEVENTF_EXTENDEDKEY, 0);
        }

        public static void KeyUp(byte vKey)
        {
            keybd_event(vKey, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
        }
    }
}
