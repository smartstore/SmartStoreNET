using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace SmartStore.Core.IO
{   
    public interface IVirtualPathProvider
    {
        string Combine(params string[] paths);
        string ToAppRelative(string virtualPath);
        string MapPath(string virtualPath);
		string Normalize(string virtualPath);

        bool FileExists(string virtualPath);
        bool TryFileExists(string virtualPath);
        Stream OpenFile(string virtualPath);
        StreamWriter CreateText(string virtualPath);
        Stream CreateFile(string virtualPath);
        DateTime GetFileLastWriteTimeUtc(string virtualPath);
        string GetFileHash(string virtualPath);
        string GetFileHash(string virtualPath, IEnumerable<string> dependencies);
        void DeleteFile(string virtualPath);

        bool DirectoryExists(string virtualPath);
        void CreateDirectory(string virtualPath);
        string GetDirectoryName(string virtualPath);
        void DeleteDirectory(string virtualPath);

        IEnumerable<string> ListFiles(string path);
        IEnumerable<string> ListDirectories(string path);
    }

	public static class IVirtualPathProviderExtensions
	{
		public static string ReadFile(this IVirtualPathProvider vpp, string virtualPath)
		{
			if (!vpp.FileExists(virtualPath))
			{
				return null;
			}

			using (var stream = vpp.OpenFile(vpp.Normalize(virtualPath)))
			{
				using (var reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}

		public static void CreateFile(this IVirtualPathProvider vpp, string path, string content)
		{
			using (var stream = vpp.CreateFile(path))
			{
				using (var tw = new StreamWriter(stream))
				{
					tw.Write(content);
				}
			}
		}
	}
}
