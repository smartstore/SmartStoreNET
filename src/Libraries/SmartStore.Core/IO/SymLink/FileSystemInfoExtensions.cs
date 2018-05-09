using System;
using System.Collections.Generic;
using System.IO;
using SmartStore.Core.IO;

namespace SmartStore
{
	public static class FileSystemInfoExtensions
	{
		/// <summary>
		/// Creates a symbolic link to this directory at the specified path.
		/// </summary>
		/// <param name="directoryInfo">the source directory for the symbolic link.</param>
		/// <param name="path">the path of the symbolic link.</param>
		public static void CreateSymbolicLink(this DirectoryInfo directoryInfo, string path)
		{
			SymbolicLink.CreateDirectoryLink(path, directoryInfo.FullName);
		}

		/// <summary>
		/// Determines whether this directory is a symbolic link.
		/// </summary>
		/// <param name="directoryInfo">the directory in question.</param>
		/// <returns><code>true</code> if the directory is a symbolic link, <code>false</code> otherwise.</returns>
		public static bool IsSymbolicLink(this DirectoryInfo directoryInfo)
		{
			return SymbolicLink.GetTarget(directoryInfo.FullName) != null;
		}

		/// <summary>
		/// Determines whether the target of this symbolic link still exists.
		/// </summary>
		/// <param name="directoryInfo">The symbolic link in question.</param>
		/// <returns><code>true</code> if this symbolic link is valid, <code>false</code> otherwise.</returns>
		/// <exception cref="System.ArgumentException">If the directory is not a symbolic link.</exception>
		public static bool IsSymbolicLinkValid(this DirectoryInfo directoryInfo)
		{
			return File.Exists(directoryInfo.GetSymbolicLinkTarget());
		}

		/// <summary>
		/// Returns the full path to the target of this symbolic link.
		/// </summary>
		/// <param name="directoryInfo">The symbolic link in question.</param>
		/// <returns>The path to the target of the symbolic link.</returns>
		/// <exception cref="System.ArgumentException">If the directory in question is not a symbolic link.</exception>
		public static string GetSymbolicLinkTarget(this DirectoryInfo directoryInfo)
		{
			if (!directoryInfo.IsSymbolicLink())
				throw new ArgumentException("Specified directory is not a symbolic link.");

			return SymbolicLink.GetTarget(directoryInfo.FullName);
		}

		/// <summary>
		/// Creates a symbolic link to this file at the specified path.
		/// </summary>
		/// <param name="fileInfo">the source file for the symbolic link.</param>
		/// <param name="path">the path of the symbolic link.</param>
		public static void CreateSymbolicLink(this FileInfo fileInfo, string path)
		{
			SymbolicLink.CreateFileLink(path, fileInfo.FullName);
		}

		/// <summary>
		/// Determines whether this file is a symbolic link.
		/// </summary>
		/// <param name="fileInfo">the file in question.</param>
		/// <returns><code>true</code> if the file is a symbolic link, <code>false</code> otherwise.</returns>
		public static bool IsSymbolicLink(this FileInfo fileInfo)
		{
			return SymbolicLink.GetTarget(fileInfo.FullName) != null;
		}

		/// <summary>
		/// Determines whether the target of this symbolic link still exists.
		/// </summary>
		/// <param name="fileInfo">The symbolic link in question.</param>
		/// <returns><code>true</code> if this symbolic link is valid, <code>false</code> otherwise.</returns>
		/// <exception cref="System.ArgumentException">If the file in question is not a symbolic link.</exception>
		public static bool IsSymbolicLinkValid(this FileInfo fileInfo)
		{
			return File.Exists(fileInfo.GetSymbolicLinkTarget());
		}

		/// <summary>
		/// Returns the full path to the target of this symbolic link.
		/// </summary>
		/// <param name="fileInfo">The symbolic link in question.</param>
		/// <returns>The path to the target of the symbolic link.</returns>
		/// <exception cref="System.ArgumentException">If the file in question is not a symbolic link.</exception>
		public static string GetSymbolicLinkTarget(this FileInfo fileInfo)
		{
			if (!fileInfo.IsSymbolicLink())
				throw new ArgumentException("file specified is not a symbolic link.");
			return SymbolicLink.GetTarget(fileInfo.FullName);
		}
	}
}
