using System;
using System.Text.RegularExpressions;
namespace GoToWindow.Api
{
	public class KeyboardShortcut
	{
		public static Regex StringRepresentation = new Regex(@"^([0-9A-F]{2})\+([0-9A-F]{2}):([12])$", RegexOptions.Compiled);

		public static KeyboardShortcut FromString(string value)
		{
			if (string.IsNullOrEmpty(value))
				return new KeyboardShortcut();

			var matches = StringRepresentation.Match(value);

			if (!matches.Success)
				throw new ApplicationException(string.Format("Invalid keyboard shortcut: '{0}'", value));
			
			return new KeyboardShortcut{
				Modifier = Convert.ToInt32(matches.Groups[1].Value, 16),
				VirtualKeyCode = Convert.ToInt32(matches.Groups[2].Value, 16),
				ShortcutPressesBeforeOpen = Convert.ToInt32(matches.Groups[3].Value)
			};
		}

		public int VirtualKeyCode;
		public int Modifier;
		public int ShortcutPressesBeforeOpen;

		public int DownCounter;

		public bool Enabled { get { return VirtualKeyCode > 0 && ShortcutPressesBeforeOpen > 0 && Modifier > 0; } }

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

		public override string ToString()
		{
			return string.Format("{0:X2}+{1:X2}:{2}", Modifier, VirtualKeyCode, ShortcutPressesBeforeOpen);
		}
	}
}