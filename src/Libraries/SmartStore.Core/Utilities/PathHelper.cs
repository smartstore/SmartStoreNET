using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SmartStore.Utilities
{
    public static class PathHelper
    {
        private static readonly char[] _invalidPathChars;
        private static readonly char[] _invalidFileNameChars;
        private static readonly Regex _invalidCharsPattern;

        static PathHelper()
        {
            _invalidPathChars = Path.GetInvalidPathChars();
            _invalidFileNameChars = Path.GetInvalidFileNameChars();

            var invalidChars = Regex.Escape(new string(_invalidPathChars) + new string(_invalidFileNameChars));
            _invalidCharsPattern = new Regex(string.Format(@"[{0}]+", invalidChars));
        }

        /// <summary>
        /// Checks whether path is empty or starts with '/' or '\'
        /// </summary>
        public static bool IsRootedPath(string basepath)
        {
            return (string.IsNullOrEmpty(basepath) || basepath[0] == '/' || basepath[0] == '\\');
        }

        /// <summary>
        /// Checks whether path starts with '~/'
        /// </summary>
        public static bool IsAppRelativePath(string path)
        {
            if (path == null)
                return false;

            int len = path.Length;

            // Empty string case
            if (len == 0) return false;

            // It must start with ~
            if (path[0] != '~')
                return false;

            // Single character case: "~"
            if (len == 1)
                return true;

            // If it's longer, checks if it starts with "~/" or "~\"
            return path[1] == '\\' || path[1] == '/';
        }

        /// <summary>
        /// Determines the relative path from <paramref name="fromPath"/> to <paramref name="toPath"/>
        /// </summary>
        /// <param name="fromPath">From path</param>
        /// <param name="toPath">To path</param>
        /// <param name="sep">Directory separator</param>
        /// <returns>The relative path</returns>
        public static string MakeRelativePath(string fromPath, string toPath, string sep = "\\")
        {
            var fromParts = fromPath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            var toParts = toPath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

            var matchedParts = fromParts
                .Zip(toParts, (x, y) => string.Compare(x, y, true) == 0)
                .TakeWhile(x => x).Count();

            return string.Join("", Enumerable.Range(0, fromParts.Length - matchedParts)
                .Select(x => ".." + sep)) +
                    string.Join(sep, toParts.Skip(matchedParts));
        }

        /// <summary>
        /// Checks if virtual path contains a protocol, which is referred to as a scheme in the
        /// URI spec.
        /// </summary>
        public static bool HasScheme(string virtualPath)
        {
            // URIs have the format <scheme>:<scheme-specific-path>, e.g. mailto:user@ms.com,
            // http://server/, nettcp://server/, etc.  The <scheme> cannot contain slashes.
            // The virtualPath passed to this method may be absolute or relative. Although
            // ':' is only allowed in the <scheme-specific-path> if it is encoded, the 
            // virtual path that we're receiving here may be decoded, so it is impossible
            // for us to determine if virtualPath has a scheme.  We will be conservative
            // and err on the side of assuming it has a scheme when we cannot tell for certain.
            // To do this, we first check for ':'.  If not found, then it doesn't have a scheme.
            // If ':' is found, then as long as we find a '/' before the ':', it cannot be
            // a scheme because schemes don't contain '/'.  Otherwise, we will assume it has a 
            // scheme.
            int indexOfColon = virtualPath.IndexOf(':');
            if (indexOfColon == -1)
                return false;
            int indexOfSlash = virtualPath.IndexOf('/');
            return (indexOfSlash == -1 || indexOfColon < indexOfSlash);
        }

        /// <summary>
        /// Ensures that a path is a valid app root path by checking whether it starts
        /// with '~/' and prepending it if needed and by replacing '\' with '/'
        /// </summary>
        /// <param name="path">Relative path</param>
        /// <returns>Normalized root path</returns>
        public static string NormalizeAppRelativePath(string path)
        {
            if (path.IsEmpty())
                return path;

            path = path.Replace('\\', '/');

            if (!path.StartsWith("~/"))
            {
                if (path.StartsWith("~"))
                    path = path.Substring(1);

                path = (path.StartsWith("/") ? "~" : "~/") + path;
            }

            return path;
        }

        /// <summary>
        /// Checks whether a path is a safe app root path.
        /// </summary>
        /// <param name="path">Relative path</param>
        public static bool IsSafeAppRootPath(string path)
        {
            if (path.EmptyNull().Length > 2 && !path.IsCaseInsensitiveEqual("con") && !HasInvalidPathChars(path))
            {
                try
                {
                    var mappedPath = CommonHelper.MapPath(path);
                    var appPath = CommonHelper.MapPath("~/");
                    return !mappedPath.IsCaseInsensitiveEqual(appPath);
                }
                catch { }
            }

            return false;
        }

        /// <summary>
        /// Replaces all occurences of any illegal path or file name char by '-'
        /// </summary>
        /// <param name="name">Path/File name</param>
        /// <returns>Sanitized path/file name</returns>
        public static string SanitizeFileName(string name)
        {
            if (name.IsEmpty())
                return name;

            return _invalidCharsPattern.Replace(name, "-");
        }

        public static bool HasInvalidPathChars(string path, bool checkWildcardChars = false)
        {
            if (path == null)
                return false;

            return path.IndexOfAny(_invalidPathChars) >= 0
                || (checkWildcardChars && ContainsWildcardChars(path, 0));
        }

        public static bool HasInvalidFileNameChars(string fileName, bool checkWildcardChars = false)
        {
            if (fileName == null)
                return false;

            return fileName.IndexOfAny(_invalidFileNameChars) >= 0
                || (checkWildcardChars && ContainsWildcardChars(fileName, 0));
        }

        private static bool ContainsWildcardChars(string path, int startIndex = 0)
        {
            for (int i = startIndex; i < path.Length; i++)
            {
                switch (path[i])
                {
                    case '*':
                    case '?':
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks whether the given path is a fully qualified absolute path (either UNC or rooted with drive letter)
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns><c>true</c> if path is fully qualified</returns>
        public static bool IsAbsolutePhysicalPath(string path)
        {
            if ((path == null) || (path.Length < 3))
            {
                return false;
            }

            return (((path[1] == ':') && IsDirectorySeparatorChar(path[2])) || IsUncSharePath(path));
        }

        internal static bool IsUncSharePath(string path)
        {
            return (((path.Length > 2) && IsDirectorySeparatorChar(path[0])) && IsDirectorySeparatorChar(path[1]));
        }

        private static bool IsDirectorySeparatorChar(char ch)
        {
            if (ch != '\\')
            {
                return (ch == '/');
            }

            return true;
        }
    }
}
