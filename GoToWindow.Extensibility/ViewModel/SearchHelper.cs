using System;
using System.Globalization;
using System.Linq;

namespace GoToWindow.Extensibility.ViewModel
{
	public class SearchHelper
	{
		public static bool IsShown(string searchQuery, params string[] fields)
		{
			if (string.IsNullOrEmpty(searchQuery))
				return true;

			var text = String.Join(" ", fields);

			return searchQuery
				.Split(' ')
				.All(word => SearchWord(text, word));
		}

		private static bool SearchWord(string text, string word)
		{
			return CultureInfo.CurrentUICulture.CompareInfo.IndexOf(text, word,
				CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) > -1;
		}
	}
}