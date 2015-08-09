using System;
using System.ComponentModel;
using System.Linq;

namespace GoToWindow.Api
{
	public class VirtualKeyDescription
	{
		private const string UnnamedKey = "Unnamed Key";

		public static string GetDescription<TEnum>(TEnum key) where TEnum : struct
		{
			return GetDescriptionInternal(typeof(TEnum), key.ToString());
		}

		private static string GetDescriptionInternal(Type enumType, string value)
		{
			if (enumType == null) throw new ArgumentNullException("enumType");

			var member = enumType.GetMember(value);
			var attributes = member[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
			var attribute = attributes.Cast<DescriptionAttribute>().FirstOrDefault();
			return attribute != null ? attribute.Description : UnnamedKey;
		}
	}
}