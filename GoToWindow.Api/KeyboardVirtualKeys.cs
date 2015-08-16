using System.ComponentModel;

namespace GoToWindow.Api
{
	public enum KeyboardControlKeys
	{
		Undefined = 0,
		[Description("Left Control")]
		LCtrl = 0xA2, //VK_LCONTROL
		[Description("Left Alt")]
		LAlt = 0xA4, //VK_LMENU,
		[Description("Left Win")]
		LWin = 0x5B, //VK_LWIN
		[Description("Left Shift")]
		LShift = 0x10 // VK_SHIFT
	}

	public enum KeyboardVirtualKeys
	{
		[Description("Custom")]
		Custom = 0,
		[Description("Tab")]
		Tab = 0x09, //VK_TAB
		[Description("Escape")]
		Escape = 0x1B //VK_ESCAPE
	}
}