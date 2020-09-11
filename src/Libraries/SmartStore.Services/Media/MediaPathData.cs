using System;
using System.IO;
using SmartStore.Collections;
using SmartStore.Core.IO;

namespace SmartStore.Services.Media
{
    public class MediaPathData
    {
        private string _name;
        private string _title;
        private string _ext;
        private string _mime;

        public MediaPathData(TreeNode<MediaFolderNode> node, string fileName, bool normalizeFileName = false)
        {
            Guard.NotNull(node, nameof(node));
            Guard.NotEmpty(fileName, nameof(fileName));

            Node = node;
            _name = normalizeFileName
                ? MediaHelper.NormalizeFileName(fileName)
                : fileName;
        }

        public MediaPathData(string path, bool normalizeFileName = false)
        {
            Guard.NotEmpty(path, nameof(path));

            _name = normalizeFileName
                ? MediaHelper.NormalizeFileName(Path.GetFileName(path))
                : Path.GetFileName(path);
        }

        public MediaPathData(MediaPathData pathData)
        {
            Node = pathData.Node;
            _name = pathData.FileName;
            _title = pathData._title;
            _ext = pathData._ext;
            _mime = pathData._mime;
        }

        public TreeNode<MediaFolderNode> Node { get; }
        public MediaFolderNode Folder => Node.Value;

        public string FileName
        {
            get => _name;
            set
            {
                Guard.NotEmpty(value, nameof(value));

                _name = value;
                _title = null;
                _ext = null;
                _mime = null;
            }
        }

        public string FullPath => Folder.Path + "/" + _name;

        public string FileTitle => _title ?? (_title = Path.GetFileNameWithoutExtension(_name));

        public string Extension
        {
            get => _ext ?? (_ext = Path.GetExtension(_name).EmptyNull().TrimStart('.'));
            set => _ext = value?.TrimStart('.');
        }

        public string MimeType
        {
            get => _mime ?? (_mime = MimeTypes.MapNameToMimeType(_name));
            set => _mime = value;
        }
    }

}
