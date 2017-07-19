using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.IO
{
	/// <summary>
	/// Abstraction over the virtual files/directories of a web site.
	/// </summary>
	public interface IVirtualFolder
	{
		IVirtualPathProvider VirtualPathProvider { get; }

		/// <summary>
		/// Virtual root path, e.g. ~/App_Data or ~/Themes
		/// </summary>
		string RootPath { get; }

		string GetVirtualPath(string relativePath);

		string MapPath(string relativePath);
		string Combine(params string[] paths);

		bool DirectoryExists(string relativePath);
		bool FileExists(string relativePath);

		IEnumerable<string> ListDirectories(string relativePath);
		IEnumerable<string> ListFiles(string relativePath, bool deep = false);
		string GetDirectoryName(string relativePath);

		Stream OpenFile(string relativePath);
		void CreateTextFile(string relativePath, string content);
		Stream CreateFile(string relativePath);
		void CreateDirectory(string relativePath);

		void DeleteFile(string relativePath);
		void DeleteDirectory(string relativePath);

		string ReadFile(string relativePath);
		void CopyFile(string relativePath, Stream destination);

		DateTime GetFileLastWriteTimeUtc(string relativePath);
	}

	public static class IVirtualFolderExtensions
	{
		public static bool TryDeleteDirectory(this IVirtualFolder folder, string relativePath)
		{
			try
			{
				folder.DeleteDirectory(relativePath);
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
