using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using SmartStore.Core.IO;

namespace SmartStore
{
	public class PostedFileResult
	{
		private static readonly Regex s_ImageTypes = new Regex(@"(.*?)\.(gif|jpg|jpeg|jpe|jfif|pjpeg|pjp|png|tiff|tif|bmp|ico|svg)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private readonly HttpPostedFileBase _httpFile;

		private string _contentType;
		private string _fileName;
		private string _fileTitle;
		private string _fileExt;
		private bool? _isImage;
		private byte[] _buffer;

		public PostedFileResult(HttpPostedFileBase httpFile)
		{
			Guard.NotNull(httpFile, nameof(httpFile));

			this._httpFile = httpFile;

			this.TimeStamp = DateTime.UtcNow;
			this.BatchId = Guid.NewGuid();
		}

		public HttpPostedFileBase File
		{
			get { return _httpFile; }
		}

		public DateTime TimeStamp 
		{ 
			get; 
			private set; 
		}

		public Guid BatchId
		{
			get;
			internal set;
		}

		public string FileName
		{
			get 
			{
				if (_fileName == null)
				{
					_fileName = Path.GetFileName(_httpFile.FileName);
				}
				return _fileName; 
			}
		}

		public string FileTitle
		{
			get 
			{
				if (_fileTitle == null)
				{
					_fileTitle = Path.GetFileNameWithoutExtension(this.FileName);
				}
				return _fileTitle;
			}
		}

		public string FileExtension
		{
			get
			{
				if (_fileExt == null)
				{
					_fileExt = Path.GetExtension(this.FileName).EmptyNull();
				}
				return _fileExt;
			}
		}

		public string ContentType
		{
			get
			{
				if (_contentType == null)
				{
					var contentType = _httpFile.ContentType;

					if (contentType == null && this.FileExtension.HasValue())
					{
						contentType = MimeTypes.MapNameToMimeType(this.FileExtension);

						// ContentType is not always available.
						// That's why we manually update it here
						if (contentType.IsEmpty())
						{
							switch (this.FileExtension.ToLowerInvariant())
							{
								case ".bmp":
									contentType = "image/bmp";
									break;
								case ".gif":
									contentType = "image/gif";
									break;
								case ".jpeg":
								case ".jpg":
								case ".jpe":
								case ".jfif":
								case ".pjpeg":
								case ".pjp":
									contentType = "image/jpeg";
									break;
								case ".png":
									contentType = "image/png";
									break;
								case ".tiff":
								case ".tif":
									contentType = "image/tiff";
									break;
								case ".svg":
									contentType = "image/svg+xml";
									break;
								case ".ico":
									contentType = "image/x-icon";
									break;
								default:
									break;
							}
						}
					}

					_contentType = contentType.EmptyNull();
				}
				return _contentType;
			}
		}

		public bool IsImage
		{
			get
			{
				if (!_isImage.HasValue)
				{
					_isImage = s_ImageTypes.IsMatch(FileExtension);
				}
				return _isImage.Value;
			}
		}

		public int Size
		{
			get { return _httpFile.ContentLength; }
		}

		public Stream Stream
		{
			get { return _httpFile.InputStream; }
		}

		public byte[] Buffer
		{
			get
			{
				if (_buffer == null)
				{
					_buffer = _httpFile.InputStream.ToByteArray();
				}

				return _buffer;
			}
		}

		public bool FileNameMatches(string pattern)
		{
			return Regex.IsMatch(_httpFile.FileName, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
		}
	}
}
