namespace GoToWindow.Api
{
	public class KeyboardShortcut
	{
		public int DownCounter;
		public int ShortcutPressesBeforeOpen;

		public int VirtualKeyCode;
		public int Modifier;

		public bool IsDown(int vkCode, int flags)
		{
			return vkCode == KeyboardVirtualCodes.Tab && HasModifier(flags);
		}

		public bool IsControlKeyReleased(int vkCode, int flags)
		{
			return vkCode == KeyboardVirtualCodes.LAlt && (flags & KeyboardVirtualCodes.Modifiers.Released) == KeyboardVirtualCodes.Modifiers.Released;
		}

		private bool HasModifier(int flags)
		{
			return (flags & Modifier) == Modifier;
		}
	}
}