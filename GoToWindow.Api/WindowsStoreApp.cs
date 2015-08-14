using System;
using System.Runtime.InteropServices;
using System.Text;

namespace GoToWindow.Api
{
	public class WindowsStoreApp
	{
		// http://stackoverflow.com/a/19670268/154480

		// ReSharper disable InconsistentNaming
		const int QueryLimitedInformation = 0x1000;
		const int ERROR_INSUFFICIENT_BUFFER = 0x7a;
		const int ERROR_SUCCESS = 0x0;
		const int APPMODEL_ERROR_NO_APPLICATION = 0x3d57;
		const int APPMODEL_ERROR_NO_PACKAGE = 0x3d54;
		const uint APPLICATION_USER_MODEL_ID_MAX_LENGTH = 130;
		const uint PACKAGE_FAMILY_NAME_MAX_LENGTH = 64;

		[StructLayout(LayoutKind.Sequential)]
		public struct PACKAGE_ID
		{
			public uint reserved;
			public uint processorArchitecture;
			public PACKAGE_VERSION version;
			public IntPtr name;
			public IntPtr publisher;
			public IntPtr resourceId;
			public IntPtr publisherId;
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct PACKAGE_VERSION
		{
			[FieldOffset(0)]
			public UInt64 Version;
			[FieldOffset(0)]
			public ushort Revision;
			[FieldOffset(2)]
			public ushort Build;
			[FieldOffset(4)]
			public ushort Minor;
			[FieldOffset(6)]
			public ushort Major;
		}
		// ReSharper restore InconsistentNaming

		[DllImport("kernel32.dll")]
		internal static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

		[DllImport("kernel32.dll")]
		static extern bool CloseHandle(IntPtr hHandle);

		[DllImport("kernel32.dll")]
		static extern Int32 GetApplicationUserModelId(IntPtr hProcess, ref UInt32 ppModelIdLength, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder sbAppUserModelId);

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern int GetPackageId(IntPtr hProcess, ref uint bufferLength, IntPtr pBuffer);

		public static string GetModelId(uint processId)
		{
			var hProcess = OpenProcess(QueryLimitedInformation, false, (int)processId);
			try
			{
				if (hProcess == IntPtr.Zero)
					return null;

				var cchLen = APPLICATION_USER_MODEL_ID_MAX_LENGTH;
				var sbName = new StringBuilder((int)APPLICATION_USER_MODEL_ID_MAX_LENGTH);
				var lResult = GetApplicationUserModelId(hProcess, ref cchLen, sbName);
				if (lResult == ERROR_INSUFFICIENT_BUFFER)
					lResult = GetApplicationUserModelId(hProcess, ref cchLen, sbName);

				if (lResult == ERROR_SUCCESS)
					return sbName.ToString();

				if (lResult == APPMODEL_ERROR_NO_APPLICATION)
					return "NOT A STORE APP";

				return "ERROR " + lResult.ToString("X");
			}
			finally
			{
				CloseHandle(hProcess);
			}
		}

		public static string GetPackageName(uint processId)
		{
			var hProcess = OpenProcess(QueryLimitedInformation, false, (int)processId);
			try
			{
				if (hProcess == IntPtr.Zero)
					return null;

				uint len = 0;
				var lResult = GetPackageId(hProcess, ref len, IntPtr.Zero);
				if (lResult != ERROR_INSUFFICIENT_BUFFER)
					return "ERROR " + lResult.ToString("X");

				if (lResult == APPMODEL_ERROR_NO_PACKAGE)
					return "NOT A STORE APP";

				var buffer = Marshal.AllocHGlobal((int)len);
				try
				{
					lResult = GetPackageId(hProcess, ref len, buffer);
					if (lResult != ERROR_SUCCESS)
						return "ERROR " + lResult.ToString("X");

					var packageId = (PACKAGE_ID)Marshal.PtrToStructure(buffer, typeof(PACKAGE_ID));
					var packageName = Marshal.PtrToStringUni(packageId.name);
					return packageName;
				}
				finally
				{
					Marshal.FreeHGlobal(buffer);
				}
			}
			finally
			{
				CloseHandle(hProcess);
			}
		}
	}
}