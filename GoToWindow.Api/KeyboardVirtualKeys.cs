using System;

namespace GoToWindow.Api
{
	public enum KeyboardControlKeys
	{
		Undefined = 0,
		Ctrl = 0xA2, //VK_LCONTROL
		Alt = 0xA4, //VK_LMENU,
		Win = 0x5B //VK_LWIN
	}

	public enum KeyboardVirtualKeys
	{
		Undefined = 0,
		Tab = 0x09, //VK_TAB
		Console = 0xC0, //~
		Escape = 0x1B //VK_ESCAPE
	}
}