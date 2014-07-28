using System.Runtime.InteropServices;

namespace GoToWindow.Api
{
    public static class KeyboardSend
    {
        public static byte LWin = 0x5B;

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        private const int KEYEVENTF_EXTENDEDKEY = 1;
        private const int KEYEVENTF_KEYUP = 2;

        public static void PressKey(byte vKey)
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
