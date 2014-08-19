namespace GoToWindow.Api
{
	public static class KeyboardVirtualCodes
	{
		public const int Tab = 0x09; //VK_TAB
		public const int LAlt = 0xA4; //VK_LMENU

		public static class Modifiers
		{
			public const int Alt = 0x20;
			public const int Released = 0x80; // LLKHF_UP (KF_UP >> 8)
		}
	}
}