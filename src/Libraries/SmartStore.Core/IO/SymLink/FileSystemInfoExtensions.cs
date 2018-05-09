using System;
using System.IO;
using SmartStore.Core.IO;

namespace SmartStore
{
	public static class FileSystemInfoExtensions
	{
		/// <summary>
		/// Determines whether this file system entry is a symbolic link.
		/// </summary>
		/// <param name="fsi">The directory or file in question.</param>
		/// <returns><code>true</code> if the entry is a symbolic link, <code>false</code> otherwise.</returns>
		public static bool IsSymbolicLink(this FileSystemInfo fsi)
		{
			return SymbolicLink.IsSymbolicLink(fsi);
		}

		/// <summary>
		/// Returns the full path to the target of a symbolic link or mount.
		/// </summary>
		/// <param name="fsi">The symbolic link in question.</param>
		/// <returns>The path to the target.</returns>
		public static string GetFinalPathName(this FileSystemInfo fsi)
		{
			return SymbolicLink.GetFinalPathName(fsi.FullName);
		}
	}
}
