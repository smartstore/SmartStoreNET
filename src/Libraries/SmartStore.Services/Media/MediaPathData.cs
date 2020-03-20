using System;
using System.IO;
using SmartStore.Core.IO;

namespace SmartStore.Services.Media
{
	public class MediaPathData
	{
		private string _title;
		private string _ext;
		private string _mime;

		public MediaPathData(MediaFolderNode folder, string fileName)
		{
			Folder = folder;
			FileName = fileName;
		}

		public MediaPathData(MediaPathData pathData)
		{
			Folder = pathData.Folder;
			FileName = pathData.FileName;
			_title = pathData._title;
			_ext = pathData._ext;
			_mime = pathData._mime;
		}

		public MediaFolderNode Folder { get; }
		public string FileName { get; }

		public string FileTitle
		{
			get
			{
				return _title ?? (_title = Path.GetFileNameWithoutExtension(FileName));
			}
		}

		public string Extension
		{
			get
			{
				return _ext ?? (_ext = Path.GetExtension(FileName).EmptyNull().TrimStart('.'));
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
				return _mime ?? (_mime = MimeTypes.MapNameToMimeType(FileName));
			}
			set
			{
				_mime = value;
			}
		}
	}

}
