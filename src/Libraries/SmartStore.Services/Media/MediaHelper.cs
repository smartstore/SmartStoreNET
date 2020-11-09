using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmartStore.Services.Media
{
    public partial class MediaHelper
    {
        #region Static

        private readonly static char[] _invalidFileNameChars = Path.GetInvalidFileNameChars().Concat(new[] { '&' }).ToArray();
        private readonly static char[] _invalidFolderNameChars = Path.GetInvalidPathChars().Concat(new[] { '&', '/', '\\' }).ToArray();

        public static string NormalizeFileName(string fileName)
        {
            return string.Join("-", fileName.ToSafe().Split(_invalidFileNameChars));
        }

        public static string NormalizeFolderName(string folderName)
        {
            return string.Join("-", folderName.ToSafe().Split(_invalidFolderNameChars));
        }

        #endregion

        private readonly IFolderService _folderService;

        public MediaHelper(IFolderService folderService)
        {
            _folderService = folderService;
        }

        public bool TokenizePath(string path, bool normalizeFileName, out MediaPathData data)
        {
            data = null;

            if (path.IsEmpty())
            {
                return false;
            }

            var dir = Path.GetDirectoryName(path);
            if (dir.HasValue())
            {
                var node = _folderService.GetNodeByPath(dir);
                if (node != null)
                {
                    data = new MediaPathData(node, path.Substring(dir.Length + 1), normalizeFileName);
                    return true;
                }
            }

            return false;
        }

        public bool CheckUniqueFileName(string title, string ext, string destFileName, out string uniqueName)
        {
            return CheckUniqueFileName(title, ext, new HashSet<string>(new[] { destFileName }, StringComparer.CurrentCultureIgnoreCase), out uniqueName);
        }

        public bool CheckUniqueFileName(string title, string ext, HashSet<string> destFileNames, out string uniqueName)
        {
            uniqueName = null;

            if (destFileNames.Count == 0)
            {
                return false;
            }

            int i = 1;
            while (true)
            {
                var test = string.Concat(title, "-", i, ".", ext.TrimStart('.'));
                if (!destFileNames.Contains(test))
                {
                    // Found our gap
                    uniqueName = test;
                    return true;
                }

                i++;
            }
        }
    }
}
