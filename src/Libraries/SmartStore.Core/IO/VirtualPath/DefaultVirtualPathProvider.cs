﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using SmartStore.Core.Logging;

namespace SmartStore.Core.IO.VirtualPath
{

    public class DefaultVirtualPathProvider : IVirtualPathProvider
    {
        private readonly ILogger _logger;
        
        public DefaultVirtualPathProvider(ILogger logger)
        {
            this._logger = logger;
        }

        public virtual string GetDirectoryName(string virtualPath)
        {
            return Path.GetDirectoryName(virtualPath).Replace(Path.DirectorySeparatorChar, '/');
        }

        public virtual IEnumerable<string> ListFiles(string path)
        {
            return HostingEnvironment
                .VirtualPathProvider
                .GetDirectory(path)
                .Files
                .OfType<VirtualFile>()
                .Select(f => VirtualPathUtility.ToAppRelative(f.VirtualPath));
        }

        public virtual IEnumerable<string> ListDirectories(string path)
        {
            return HostingEnvironment
                .VirtualPathProvider
                .GetDirectory(path)
                .Directories
                .OfType<VirtualDirectory>()
                .Select(d => VirtualPathUtility.ToAppRelative(d.VirtualPath));
        }

        public virtual string Combine(params string[] paths)
        {
            return Path.Combine(paths).Replace(Path.DirectorySeparatorChar, '/');
        }

        public virtual string ToAppRelative(string virtualPath)
        {
            if (IsMalformedVirtualPath(virtualPath))
                return null;

            try
            {
                string result = VirtualPathUtility.ToAppRelative(virtualPath);

                // In some cases, ToAppRelative doesn't normalize the path. In those cases,
                // the path is invalid.
                // Example:
                //   ApplicationPath: /Foo
                //   VirtualPath    : ~/Bar/../Blah/Blah2
                //   Result         : /Blah/Blah2  <= that is not an app relative path!
                if (!result.StartsWith("~/"))
                {
                    _logger.Information("Path '{0}' cannot be made app relative: Path returned ('{1}') is not app relative.".FormatCurrent(virtualPath, result));
                    return null;
                }
                return result;
            }
            catch (Exception e)
            {
                // The initial path might have been invalid (e.g. path indicates a path outside the application root)
                _logger.Information("Path '{0}' cannot be made app relative".FormatCurrent(virtualPath), e);
                return null;
            }
        }

        /// <summary>
        /// We want to reject path that contains ".." going outside of the application root.
        /// ToAppRelative does that already, but we want to do the same while avoiding exceptions.
        /// 
        /// Note: This method doesn't detect all cases of malformed paths, it merely checks
        ///       for *some* cases of malformed paths, so this is not a replacement for full virtual path
        ///       verification through VirtualPathUtilty methods.
        ///       In other words, !IsMalformed does *not* imply "IsWellformed".
        /// </summary>
        public bool IsMalformedVirtualPath(string virtualPath)
        {
            if (string.IsNullOrEmpty(virtualPath))
                return true;

            if (virtualPath.IndexOf("..", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                virtualPath = virtualPath.Replace(Path.DirectorySeparatorChar, '/');
                string rootPrefix = virtualPath.StartsWith("~/") ? "~/" : virtualPath.StartsWith("/") ? "/" : "";
                if (!string.IsNullOrEmpty(rootPrefix))
                {
                    string[] terms = virtualPath.Substring(rootPrefix.Length).Split('/');
                    int depth = 0;
                    foreach (var term in terms)
                    {
                        if (term == "..")
                        {
                            if (depth == 0)
                            {
                                _logger.Information("Path '{0}' cannot be made app relative: Too many '..'".FormatCurrent(virtualPath));
                                return true;
                            }
                            depth--;
                        }
                        else
                        {
                            depth++;
                        }
                    }
                }
            }

            return false;
        }

        public virtual Stream OpenFile(string virtualPath)
        {
            return HostingEnvironment.VirtualPathProvider.GetFile(virtualPath).Open();
        }

        public virtual StreamWriter CreateText(string virtualPath)
        {
            return File.CreateText(MapPath(virtualPath));
        }

        public virtual Stream CreateFile(string virtualPath)
        {
            return File.Create(MapPath(virtualPath));
        }

        public virtual DateTime GetFileLastWriteTimeUtc(string virtualPath)
        {
            return File.GetLastWriteTimeUtc(MapPath(virtualPath));
        }

        public string GetFileHash(string virtualPath)
        {
            return GetFileHash(virtualPath, new[] { virtualPath });
        }

        public string GetFileHash(string virtualPath, IEnumerable<string> dependencies)
        {
            return HostingEnvironment.VirtualPathProvider.GetFileHash(virtualPath, dependencies);
        }

        public virtual void DeleteFile(string virtualPath)
        {
            File.Delete(MapPath(virtualPath));
        }

        public virtual string MapPath(string virtualPath)
        {
            return HostingEnvironment.MapPath(virtualPath);
        }

		public virtual string Normalize(string virtualPath)
		{
			return HostingEnvironment.VirtualPathProvider.GetFile(virtualPath).VirtualPath;
		}

        public virtual bool FileExists(string virtualPath)
        {
            return HostingEnvironment.VirtualPathProvider.FileExists(virtualPath);
        }

        public virtual bool TryFileExists(string virtualPath)
        {
            if (IsMalformedVirtualPath(virtualPath))
                return false;

            try
            {
                return FileExists(virtualPath);
            }
            catch (Exception e)
            {
                _logger.Information("File '{0}' can not be checked for existence. Assuming doesn't exist.".FormatCurrent(virtualPath), e);
                return false;
            }
        }

        public virtual bool DirectoryExists(string virtualPath)
        {
            return HostingEnvironment.VirtualPathProvider.DirectoryExists(virtualPath);
        }

        public virtual void CreateDirectory(string virtualPath)
        {
            Directory.CreateDirectory(MapPath(virtualPath));
        }

        public virtual void DeleteDirectory(string virtualPath)
        {
            Directory.Delete(MapPath(virtualPath));
        }

	}

}
