using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace SmartStore.Core.IO
{
	internal static class SymbolicLink
	{
		private const uint genericReadAccess = 0x80000000;
		private const uint fileFlagsForOpenReparsePointAndBackupSemantics = 0x02200000;
		private const int ioctlCommandGetReparsePoint = 0x000900A8;
		private const uint openExisting = 0x3;
		private const uint pathNotAReparsePointError = 0x80071126;
		private const uint shareModeAll = 0x7; // Read, Write, Delete
		private const uint symLinkTag = 0xA000000C;
		private const int targetIsAFile = 0;
		private const int targetIsADirectory = 1;

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern SafeFileHandle CreateFile(
			string lpFileName,
			uint dwDesiredAccess,
			uint dwShareMode,
			IntPtr lpSecurityAttributes,
			uint dwCreationDisposition,
			uint dwFlagsAndAttributes,
			IntPtr hTemplateFile);

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern bool DeviceIoControl(
			IntPtr hDevice,
			uint dwIoControlCode,
			IntPtr lpInBuffer,
			int nInBufferSize,
			IntPtr lpOutBuffer,
			int nOutBufferSize,
			out int lpBytesReturned,
			IntPtr lpOverlapped);

		public static void CreateDirectoryLink(string linkPath, string targetPath)
		{
			if (!CreateSymbolicLink(linkPath, targetPath, targetIsADirectory) || Marshal.GetLastWin32Error() != 0)
			{
				try
				{
					Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
				}
				catch (COMException exception)
				{
					throw new IOException(exception.Message, exception);
				}
			}
		}

		public static void CreateFileLink(string linkPath, string targetPath)
		{
			if (!CreateSymbolicLink(linkPath, targetPath, targetIsAFile))
			{
				Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
			}
		}

		public static bool Exists(string path)
		{
			if (!Directory.Exists(path) && !File.Exists(path))
			{
				return false;
			}
			string target = GetTarget(path);
			return target != null;
		}

		private static SafeFileHandle GetFileHandle(string path)
		{
			return CreateFile(path, genericReadAccess, shareModeAll, IntPtr.Zero, openExisting,
				fileFlagsForOpenReparsePointAndBackupSemantics, IntPtr.Zero);
		}

		public static string GetTarget(string path)
		{
			SymbolicLinkReparseData reparseDataBuffer;

			using (SafeFileHandle fileHandle = GetFileHandle(path))
			{
				if (fileHandle.IsInvalid)
				{
					Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
				}

				int outBufferSize = Marshal.SizeOf<SymbolicLinkReparseData>();

				IntPtr outBuffer = IntPtr.Zero;
				try
				{
					outBuffer = Marshal.AllocHGlobal(outBufferSize);
					bool success = DeviceIoControl(
						fileHandle.DangerousGetHandle(), ioctlCommandGetReparsePoint, IntPtr.Zero, 0,
						outBuffer, outBufferSize, out int bytesReturned, IntPtr.Zero);

					fileHandle.Dispose();

					if (!success)
					{
						if (((uint)Marshal.GetHRForLastWin32Error()) == pathNotAReparsePointError)
						{
							return null;
						}
						Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
					}

					reparseDataBuffer = Marshal.PtrToStructure<SymbolicLinkReparseData>(outBuffer);
				}
				finally
				{
					Marshal.FreeHGlobal(outBuffer);
				}
			}
			if (reparseDataBuffer.ReparseTag != symLinkTag)
			{
				return null;
			}

			string target = Encoding.Unicode.GetString(reparseDataBuffer.PathBuffer,
				reparseDataBuffer.PrintNameOffset, reparseDataBuffer.PrintNameLength);

			return target;
		}
	}

	/// <remarks>
	/// Refer to http://msdn.microsoft.com/en-us/library/windows/hardware/ff552012%28v=vs.85%29.aspx
	/// </remarks>
	[StructLayout(LayoutKind.Sequential)]
	internal struct SymbolicLinkReparseData
	{
		// Not certain about this!
		private const int maxUnicodePathLength = 260 * 2;

		public uint ReparseTag;
		public ushort ReparseDataLength;
		public ushort Reserved;
		public ushort SubstituteNameOffset;
		public ushort SubstituteNameLength;
		public ushort PrintNameOffset;
		public ushort PrintNameLength;
		public uint Flags;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = maxUnicodePathLength)]
		public byte[] PathBuffer;
	}
}