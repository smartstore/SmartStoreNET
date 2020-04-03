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

		public MediaPathData(TreeNode<MediaFolderNode> node, string fileName)
		{
			Guard.NotNull(node, nameof(node));
			Guard.NotEmpty(fileName, nameof(fileName));

			Node = node;
			_name = fileName;
		}

		public MediaPathData(string path)
		{
			Guard.NotEmpty(path, nameof(path));

			_name = Path.GetFileName(path);
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
		public MediaFolderNode Folder 
		{
			get => Node.Value;
		}

		public string FileName 
		{
			get
			{
				return _name;
			}
			set
			{
				Guard.NotEmpty(value, nameof(value));

				_name = value;
				_title = null;
				_ext = null;
				_mime = null;
			}
		}

		public string FullPath 
		{
			get => Folder.Path + "/" + _name;
		}

		public string FileTitle
		{
			get
			{
				return _title ?? (_title = Path.GetFileNameWithoutExtension(_name));
			}
		}

		public string Extension
		{
			get
			{
				return _ext ?? (_ext = Path.GetExtension(_name).EmptyNull().TrimStart('.'));
			}
			set
			{
				_ext = value;
			}
		}

		public string MimeType
		{
			get
			{
				return _mime ?? (_mime = MimeTypes.MapNameToMimeType(_name));
			}
			set
			{
				_mime = value;
			}
		}
	}

}
