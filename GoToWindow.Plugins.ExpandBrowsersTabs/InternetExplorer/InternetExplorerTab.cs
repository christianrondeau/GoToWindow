using System;
using System.Collections.Generic;
using GoToWindow.Plugins.ExpandBrowsersTabs.Contracts;
using System.Runtime.InteropServices;
using Accessibility;

namespace GoToWindow.Plugins.ExpandBrowsersTabs.InternetExplorer
{
	/// <remarks>
	/// Thanks to http://stackoverflow.com/questions/3924812/how-can-i-select-a-ie-tab-from-its-handle
	/// </remarks>
	public class InternetExplorerTab : TabBase, ITab
	{
		private string p;
		private SHDocVw.InternetExplorer _ie;

		public InternetExplorerTab(string title, SHDocVw.InternetExplorer ie)
			: base(title)
		{
			_ie = ie;
		}

		public void Select()
		{
			string urlOfTabToActivate = _ie.LocationURL;
			IntPtr hwnd = (IntPtr)_ie.HWND;
			var directUi = GetDirectUIHWND(hwnd);
			var iacc = AccessibleObjectFromWindow(directUi);
			var tabRow = FindAccessibleDescendant(iacc, "Tab Row");

			var tabs = AccChildren(tabRow);
			int tc = tabs.Count;
			int k = 0;

			// walk through the tabs and tick the chosen one
			foreach (var candidateTab in tabs)
			{
				k++;
				if (k == tc) continue; // the last tab is "New Tab", which we don't want

				string localUrl = UrlForTab(candidateTab); // // the URL on *this* tab

				if (urlOfTabToActivate != null && localUrl.Equals(urlOfTabToActivate))
				{
					candidateTab.accDoDefaultAction(0);
					break;
				}
			}
		}

		private static IntPtr GetDirectUIHWND(IntPtr ieFrame)
		{
			// try IE 9 first:
			IntPtr intptr = FindWindowEx(ieFrame, IntPtr.Zero, "WorkerW", null);
			if (intptr == IntPtr.Zero)
			{
				// IE8 and IE7
				intptr = FindWindowEx(ieFrame, IntPtr.Zero, "CommandBarClass", null);
			}
			intptr = FindWindowEx(intptr, IntPtr.Zero, "ReBarWindow32", null);
			intptr = FindWindowEx(intptr, IntPtr.Zero, "TabBandClass", null);
			intptr = FindWindowEx(intptr, IntPtr.Zero, "DirectUIHWND", null);
			return intptr;
		}

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr FindWindowEx(
			IntPtr hwndParent,
			IntPtr hwndChildAfter,
			string lpszClass,
			string lpszWindow);

		[DllImport("oleacc.dll")]
		internal static extern int AccessibleObjectFromWindow(
			IntPtr hwnd,
			uint id,
			ref Guid iid,
			[In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object ppvObject);

		[DllImport("oleacc.dll")]
		private static extern int AccessibleChildren(
			IAccessible paccContainer,
			int iChildStart,
			int cChildren,
			[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] object[] rgvarChildren,
			out int pcObtained);

		private static IAccessible AccessibleObjectFromWindow(IntPtr hwnd)
		{
			Guid guid = new Guid("{618736e0-3c3d-11cf-810c-00aa00389b71}"); // IAccessible
			object obj = null;
			uint id = 0U;
			int num = AccessibleObjectFromWindow(hwnd, id, ref guid, ref obj);
			var acc = obj as IAccessible;
			return acc;
		}

		private static IAccessible FindAccessibleDescendant(IAccessible parent, String strName)
		{
			int c = parent.accChildCount;
			if (c == 0)
				return null;

			var children = AccChildren(parent);

			foreach (var child in children)
			{
				if (child == null) continue;
				if (strName.Equals(child.get_accName(0)))
					return child;

				var x = FindAccessibleDescendant(child, strName);
				if (x != null) return x;
			}

			return null;
		}

		private static List<IAccessible> AccChildren(IAccessible accessible)
		{
			int childs;
			object[] res = GetAccessibleChildren(accessible, out childs);
			var list = new List<IAccessible>();
			if (res == null) return list;

			foreach (object obj in res)
			{
				IAccessible acc = obj as IAccessible;
				if (acc != null) list.Add(acc);
			}
			return list;
		}

		private static object[] GetAccessibleChildren(IAccessible ao, out int childs)
		{
			childs = 0;
			object[] ret = null;
			var count = ao.accChildCount;

			if (count > 0)
			{
				ret = new object[count];
				AccessibleChildren(ao, 0, count, ret, out childs);
			}

			return ret;
		}

		private static string UrlForTab(IAccessible tab)
		{
			try
			{
				var desc = tab.get_accDescription(0);
				if (desc != null)
				{
					if (desc.Contains("\n"))
					{
						string url = desc.Substring(desc.IndexOf("\n")).Trim();
						return url;
					}
					else
					{
						return desc;
					}
				}
			}
			catch { }
			return "??";
		}
	}
}
