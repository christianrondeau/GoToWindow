using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Accessibility;
using GoToWindow.Api;
using GoToWindow.Plugins.ExpandBrowsersTabs.Contracts;
using log4net;

namespace GoToWindow.Plugins.ExpandBrowsersTabs.InternetExplorer
{
	/// <remarks>
	/// Thanks to http://stackoverflow.com/questions/3924812/how-can-i-select-a-ie-tab-from-its-handle
	/// </remarks>
	public class InternetExplorerTab : TabBase, ITab
	{
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProcessExtensions).Assembly, "GoToWindow");

		private readonly SHDocVw.InternetExplorer _ie;

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

		public InternetExplorerTab(string title, SHDocVw.InternetExplorer ie)
			: base(title)
		{
			_ie = ie;
		}

		public void Select()
		{
			var urlOfTabToActivate = _ie.LocationURL;
			var hwnd = (IntPtr)_ie.HWND;
			var directUi = GetDirectUiHWnd(hwnd);
			var iacc = AccessibleObjectFromWindow(directUi);
			var tabRow = FindAccessibleDescendant(iacc, "Tab Row");

			var tabs = AccChildren(tabRow);
			var tc = tabs.Count;
			var k = 0;

			// walk through the tabs and tick the chosen one
			foreach (var candidateTab in tabs)
			{
				k++;
				if (k == tc) continue; // the last tab is "New Tab", which we don't want

				var localUrl = UrlForTab(candidateTab); // // the URL on *this* tab

				if (urlOfTabToActivate == null || !localUrl.Equals(urlOfTabToActivate)) continue;
				
				candidateTab.accDoDefaultAction(0);
				
				break;
			}
		}

		private static IntPtr GetDirectUiHWnd(IntPtr ieFrame)
		{
			// try IE 9 first:
			var intptr = FindWindowEx(ieFrame, IntPtr.Zero, "WorkerW", null);
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
		
		private static IAccessible AccessibleObjectFromWindow(IntPtr hwnd)
		{
			var guid = new Guid("{618736e0-3c3d-11cf-810c-00aa00389b71}"); // IAccessible
			object obj = null;
			const uint id = 0U;
			AccessibleObjectFromWindow(hwnd, id, ref guid, ref obj);
			var acc = obj as IAccessible;
			return acc;
		}

		private static IAccessible FindAccessibleDescendant(IAccessible parent, string strName)
		{
			var accessibleChildCount = parent.accChildCount;
			if (accessibleChildCount == 0)
				return null;

			var children = AccChildren(parent);

			foreach (var child in children.Where(child => child != null))
			{
				if (strName.Equals(child.accName[0]))
					return child;

				var accessibleDescendant = FindAccessibleDescendant(child, strName);
				
				if (accessibleDescendant != null) return accessibleDescendant;
			}

			return null;
		}

		private static List<IAccessible> AccChildren(IAccessible accessible)
		{
			var res = GetAccessibleChildren(accessible, out int _);
			var list = new List<IAccessible>();
			if (res == null) return list;
			list.AddRange(res.OfType<IAccessible>());
			return list;
		}

		private static IEnumerable<object> GetAccessibleChildren(IAccessible ao, out int childs)
		{
			childs = 0;
			var count = ao.accChildCount;

			if (count <= 0) return null;
			var ret = new object[count];
			AccessibleChildren(ao, 0, count, ret, out childs);
			return ret;
		}

		private static string UrlForTab(IAccessible tab)
		{
		    try
		    {
		        var desc = tab.accDescription[0];
		        if (desc != null)
		        {
		            return desc.Contains("\n")
		                ? desc.Substring(desc.IndexOf("\n", StringComparison.Ordinal)).Trim()
		                : desc;
		        }
		    }
		    catch (Exception exc)
		    {
		        Log.Warn("Error trying to get URL for Internet Explorer tab", exc);
		    }
			return "empty://";
		}
	}
}
