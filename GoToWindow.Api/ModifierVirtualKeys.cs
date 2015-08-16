using System.ComponentModel;

namespace GoToWindow.Api
{
	public enum ModifierVirtualKeys
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
}