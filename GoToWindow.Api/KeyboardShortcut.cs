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
				ControlVirtualKeyCode = Convert.ToInt32(matches.Groups[1].Value, 16),
				VirtualKeyCode = Convert.ToInt32(matches.Groups[2].Value, 16),
				ShortcutPressesBeforeOpen = Convert.ToInt32(matches.Groups[3].Value)
			};
		}

		public int ControlVirtualKeyCode;
		public int VirtualKeyCode;
		public int ShortcutPressesBeforeOpen;

		public bool Enabled { get { return ControlVirtualKeyCode > 0 && VirtualKeyCode > 0 && ShortcutPressesBeforeOpen > 0; } }

		public override string ToString()
		{
			return string.Format("{0:X2}+{1:X2}:{2}", ControlVirtualKeyCode, VirtualKeyCode, ShortcutPressesBeforeOpen);
		}

		#region Stateful

		private int _downCounter;
		private bool _isControlKeyDown;
		private bool _isActive;

		public void ControlKeyDown()
		{
			_isControlKeyDown = true;
			_downCounter = 0;
		}

		public bool ControlKeyUp()
		{
			_isControlKeyDown = false;

			if (_isActive)
			{
				_isActive = false;
				return true;
			}

			return false;
		}

		public bool ShortcutKeyDown()
		{
			if (!_isControlKeyDown)
				return false;

			if (_isActive)
				return true;

			return _isActive = ++_downCounter >= ShortcutPressesBeforeOpen;
		}

		public bool ShortcutKeyUp()
		{
			return _isControlKeyDown && _isActive;
		}

		#endregion

	}
}