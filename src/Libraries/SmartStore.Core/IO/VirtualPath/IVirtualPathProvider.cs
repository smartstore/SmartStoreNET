using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace SmartStore.Core.IO.VirtualPath
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

}
