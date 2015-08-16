using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace GoToWindow.Api
{
	public class VirtualKeyDescription
	{
		// http://blog.molecular-matters.com/2011/09/05/properly-handling-keyboard-input/

		// ReSharper disable InconsistentNaming
		private const uint MAPVK_VK_TO_VSC = 0x0;
		// ReSharper restore InconsistentNaming

		[DllImport("user32.dll")]
		internal static extern int MapVirtualKey(uint uCode, uint uMapType);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		internal static extern int GetKeyNameText(uint lParam, [MarshalAs(UnmanagedType.LPWStr), Out]StringBuilder str, int size);

		private const string UnnamedKey = "?";

		public static string GetModifierVirtualKeyDescription(int vkKey)
		{
			var member = typeof (ModifierVirtualKeys).GetMember(((ModifierVirtualKeys) vkKey).ToString());
			var memberInfo = member.FirstOrDefault();
			if (memberInfo == null)
				return UnnamedKey;
			var attributes = memberInfo.GetCustomAttributes(typeof (DescriptionAttribute), false);
			var attribute = attributes.Cast<DescriptionAttribute>().FirstOrDefault();
			return attribute != null ? attribute.Description : UnnamedKey;
		}

		public static string GetVirtualKeyDescription(int vkKey)
		{
			if (vkKey <= 0)
				return UnnamedKey;

			var scanCode = MapVirtualKey((uint)vkKey, MAPVK_VK_TO_VSC);

			if (scanCode == 0)
				return UnnamedKey;

			var sb = new StringBuilder();
			var length = GetKeyNameText(((uint)scanCode) << 16, sb, 256);
			if (length > 0)
				return sb.ToString();

			var mappedChar = Convert.ToChar(scanCode);
			return mappedChar.ToString();
		}
	}
}