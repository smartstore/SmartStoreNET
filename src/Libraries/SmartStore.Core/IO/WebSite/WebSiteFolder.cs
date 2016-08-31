using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace SmartStore.Core.IO
{
    public class WebSiteFolder : IWebSiteFolder
    {
        private readonly IVirtualPathProvider _virtualPathProvider;

        public WebSiteFolder(IVirtualPathProvider virtualPathProvider)
        {
            _virtualPathProvider = virtualPathProvider;
        }

		public string Combine(params string[] paths)
		{
			return _virtualPathProvider.Combine(paths);
		}

		public IEnumerable<string> ListDirectories(string virtualPath)
        {
            if (!_virtualPathProvider.DirectoryExists(virtualPath))
            {
                return Enumerable.Empty<string>();
            }

            return _virtualPathProvider.ListDirectories(virtualPath);
        }

        private IEnumerable<string> ListFiles(IEnumerable<string> directories)
        {
            return directories.SelectMany(d => ListFiles(d, true));
        }

        public IEnumerable<string> ListFiles(string virtualPath, bool recursive)
        {
            if (!recursive)
            {
                return _virtualPathProvider.ListFiles(virtualPath);
            }
            return _virtualPathProvider.ListFiles(virtualPath).Concat(ListFiles(ListDirectories(virtualPath)));
        }

        public bool FileExists(string virtualPath)
        {
            return _virtualPathProvider.FileExists(virtualPath);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public string ReadFile(string virtualPath)
        {
			return _virtualPathProvider.ReadFile(virtualPath);
        }

        public void CopyFileTo(string virtualPath, Stream destination)
        {
            using (var stream = _virtualPathProvider.OpenFile(Normalize(virtualPath)))
            {
                stream.CopyTo(destination);
            }
        }

        private string Normalize(string virtualPath)
        {
			return _virtualPathProvider.Normalize(virtualPath);
        }
    }
}
