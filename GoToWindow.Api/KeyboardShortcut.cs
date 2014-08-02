namespace GoToWindow.Api
{
    public class KeyboardShortcut
    {
        public int VirtualKeyCode;
        public int Modifier;

        public bool IsDown(int vkCode, int flags)
        {
            return vkCode == KeyboardVirtualCodes.Tab && HasModifier(flags);
        }

        private bool HasModifier(int flags)
        {
            return (flags & Modifier) == Modifier;
        }
    }
}