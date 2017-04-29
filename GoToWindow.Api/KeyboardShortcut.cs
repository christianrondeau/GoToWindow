using System;
using System.Linq;
using System.Text.RegularExpressions;
namespace GoToWindow.Api
{
	public class KeyboardShortcut
	{
		public static Regex StringRepresentation = new Regex(@"^([0-9A-F]{2})\+([0-9A-F]{2}):([12])$", RegexOptions.Compiled);

		public static KeyboardShortcut FromString(string value)
		{
			if (string.IsNullOrEmpty(value))
				return new KeyboardShortcut("Empty shortcut");

			var matches = StringRepresentation.Match(value);

			if (!matches.Success)
				return new KeyboardShortcut("Configured value does not match the required format");

			// ReSharper disable RedundantArgumentName
			return new KeyboardShortcut(
				controlVirtualKeyCode: Convert.ToInt32(matches.Groups[1].Value, 16),
				virtualKeyCode: Convert.ToInt32(matches.Groups[2].Value, 16),
				shortcutPressesBeforeOpen: Convert.ToInt32(matches.Groups[3].Value)
				);
			// ReSharper restore RedundantArgumentName
		}

		public bool IsValid;
		public string InvalidReason { get; set; }
		public int ControlVirtualKeyCode;
		public int VirtualKeyCode;
		public int ShortcutPressesBeforeOpen;

		public bool Enabled => ControlVirtualKeyCode > 0 && VirtualKeyCode > 0 && ShortcutPressesBeforeOpen > 0;

		public KeyboardShortcut(string invalidReason)
		{
			InvalidReason = invalidReason;
			IsValid = false;
		}

		public KeyboardShortcut(int controlVirtualKeyCode, int virtualKeyCode, int shortcutPressesBeforeOpen)
		{
			if (!Validate(controlVirtualKeyCode, virtualKeyCode, shortcutPressesBeforeOpen))
			{
				IsValid = false;
				return;
			}

			ControlVirtualKeyCode = controlVirtualKeyCode;
			VirtualKeyCode = virtualKeyCode;
			ShortcutPressesBeforeOpen = shortcutPressesBeforeOpen;
			IsValid = true;
		}

		private static bool Validate(int controlVirtualKeyCode, int virtualKeyCode, int shortcutPressesBeforeOpen)
		{
			if (controlVirtualKeyCode <= 0 || virtualKeyCode <= 0 || shortcutPressesBeforeOpen <= 0)
				return false;

			if (Enum.IsDefined(typeof (ModifierVirtualKeys), (ModifierVirtualKeys) virtualKeyCode))
				return false;

			if (controlVirtualKeyCode == (int) ModifierVirtualKeys.LWin && virtualKeyCode == 0x53) // WIN + S
				return false;

			return true;
		}

		public string ToHumanReadableString()
		{
			var virtualKeyString = " + " + VirtualKeyDescription.GetVirtualKeyDescription(VirtualKeyCode);
			virtualKeyString = string.Concat(Enumerable.Repeat(virtualKeyString, ShortcutPressesBeforeOpen));
			return VirtualKeyDescription.GetModifierVirtualKeyDescription(ControlVirtualKeyCode) + virtualKeyString;
		}

		public override string ToString()
		{
			return $"{ControlVirtualKeyCode:X2}+{VirtualKeyCode:X2}:{ShortcutPressesBeforeOpen}";
		}
	}
}