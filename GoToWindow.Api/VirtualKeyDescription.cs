using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;

namespace GoToWindow.Api
{
	public class VirtualKeyDescription
	{
		[DllImport("user32.dll")]
		static extern int MapVirtualKey(uint uCode, uint uMapType);

		private const string UnnamedKey = "Unnamed Key";
		private const int F1VirtualKeyCode = 0x70;
		private const int FMaxVirtualKeyCode = 0x87;

		private static bool IsFKey(int value)
		{
			return value >= F1VirtualKeyCode && value <= FMaxVirtualKeyCode;
		}

		public static string GetDescription<TEnum>(TEnum key) where TEnum : struct
		{
			var value = (int) (object) key;

			if (IsFKey(value))
				return "F" + (value - F1VirtualKeyCode + 1);

			var description = GetDescriptionInternal(typeof(TEnum), key.ToString());
			if (description != null)
				return description;

			var nonVirtualKey = MapVirtualKey((uint)(int)value, 2);
			var mappedChar = Convert.ToChar(nonVirtualKey);
			return mappedChar.ToString();
		}

		private static string GetDescriptionInternal(Type enumType, string value)
		{
			if (enumType == null) throw new ArgumentNullException("enumType");

			var member = enumType.GetMember(value);
			var memberInfo = member.FirstOrDefault();
			if (memberInfo == null)
				return null;
			var attributes = memberInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
			var attribute = attributes.Cast<DescriptionAttribute>().FirstOrDefault();
			return attribute != null ? attribute.Description : UnnamedKey;
		}
	}
}