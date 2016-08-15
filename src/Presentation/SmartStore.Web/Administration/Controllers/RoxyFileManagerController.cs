using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using SmartStore.Core.IO;
using SmartStore.Core.Logging;
using SmartStore.Services.Security;
using SmartStore.Utilities;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
	public class RoxyFileManagerController : AdminControllerBase
	{
		private const int BUFFER_SIZE = 32768;
		private const string CONFIG_FILE = "~/Administration/Content/filemanager/conf.json";
		private const string LANGUAGE_FILE = "~/Administration/Content/filemanager/lang/{0}.json";

		private string _fileRoot = null;
		private Dictionary<string, string> _lang = null;
		private Dictionary<string, string> _settings = null;

		private readonly IFileSystem _fileSystem;
		private readonly HttpContextBase _context;
		private readonly HttpResponseBase _response;

		public RoxyFileManagerController(
			IFileSystem fileSystem,
			HttpContextBase context)
		{
			_fileSystem = fileSystem;
			_context = context;
			_response = _context.Response;

			Logger = NullLogger.Instance;
		}

		public ILogger Logger { get; set; }

		#region Utilities

		private Dictionary<string, string> ParseJson(string path)
		{
			var result = new Dictionary<string, string>();
			var json = "";

			try
			{
				json = System.IO.File.ReadAllText(path, System.Text.Encoding.UTF8);
			}
			catch (Exception exception)
			{
				exception.Dump();
			}

			json = json.Trim();
			if (json != "")
			{
				if (json.StartsWith("{"))
				{
					json = json.Substring(1, json.Length - 2);
				}

				json = json.Trim();
				json = json.Substring(1, json.Length - 2);

				var lines = Regex.Split(json, "\"\\s*,\\s*\"");

				foreach (var line in lines)
				{
					var tmp = Regex.Split(line, "\"\\s*:\\s*\"");
					try
					{
						if (tmp[0] != "" && !result.ContainsKey(tmp[0]))
						{
							result.Add(tmp[0], tmp[1]);
						}
					}
					catch { }
				}
			}
			return result;
		}

		private string LangRes(string name)
		{
			var result = name;

			if (_lang == null)
			{
				var lang = GetSetting("LANG");

				if (lang.IsCaseInsensitiveEqual("auto"))
				{
					lang = Services.WorkContext.WorkingLanguage.UniqueSeoCode.EmptyNull().ToLower();
				}

				var filename = CommonHelper.MapPath(LANGUAGE_FILE.FormatInvariant(lang));

				if (!System.IO.File.Exists(filename))
				{
					filename = CommonHelper.MapPath(LANGUAGE_FILE.FormatInvariant("en"));
				}

				_lang = ParseJson(filename);
			}

			if (_lang.ContainsKey(name))
			{
				result = _lang[name];
			}

			return result;
		}

		private string GetSetting(string name)
		{
			var result = "";

			if (_settings == null)
				_settings = ParseJson(CommonHelper.MapPath(CONFIG_FILE));

			if (_settings.ContainsKey(name))
				result = _settings[name];

			return result;
		}

		private string GetFileContentType(string extension)
		{
			extension = extension.EmptyNull().ToLower();

			if (extension == ".swf" || extension == ".flv")
				return "flash";

			if (MimeTypes.MapNameToMimeType(extension).EmptyNull().StartsWith("image"))
				return "image";

			return "file";
		}

		private bool IsAllowedFileType(string extension)
		{
			var result = true;
			extension = extension.EmptyNull().ToLower().Replace(".", "");

			var setting = GetSetting("FORBIDDEN_UPLOADS").EmptyNull().Trim().ToLower();
			if (setting.HasValue())
			{
				var tmp = new ArrayList();
				tmp.AddRange(Regex.Split(setting, "\\s+"));
				if (!tmp.Contains(extension))
					result = true;
			}

			setting = GetSetting("ALLOWED_UPLOADS").EmptyNull().Trim().ToLower();
			if (setting.HasValue())
			{
				var tmp = new ArrayList();
				tmp.AddRange(Regex.Split(setting, "\\s+"));
				if (!tmp.Contains(extension))
					result = false;
			}

			return result;
		}

		private void GetImageSize(IFile file, out int width, out int height)
		{
			width = height = 0;

			try
			{
				if (GetFileContentType(file.FileType).IsCaseInsensitiveEqual("image"))
				{
					using (var stream = file.OpenRead())
					using (var image = Image.FromStream(stream))
					{
						width = image.Width;
						height = image.Height;
					}
				}
			}
			catch { }
		}

		private void ImageResize(string path, string dest, int width, int height)
		{
			if (dest.IsEmpty())
				return;

			var stream = new FileStream(path, FileMode.Open);
			var image = Image.FromStream(stream);
			var imageFormat = ImageFormat.Jpeg;

			switch (Path.GetExtension(path).EmptyNull().ToLower())
			{
				case ".png":
					imageFormat = ImageFormat.Png;
					break;
				case ".gif":
					imageFormat = ImageFormat.Gif;
					break;
			}

			stream.Close();
			stream.Dispose();

			float ratio = (float)image.Width / (float)image.Height;
			if ((image.Width <= width && image.Height <= height) || (width == 0 && height == 0))
				return;

			int newWidth = width;
			int newHeight = Convert.ToInt16(Math.Floor((float)newWidth / ratio));
			if ((height > 0 && newHeight > height) || (width == 0))
			{
				newHeight = height;
				newWidth = Convert.ToInt16(Math.Floor((float)newHeight * ratio));
			}

			using (var newImage = new Bitmap(newWidth, newHeight))
			using (var graph = Graphics.FromImage(newImage))
			{
				graph.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				graph.DrawImage(image, 0, 0, newWidth, newHeight);

				image.Dispose();

				newImage.Save(dest, imageFormat);
			}
		}

		private string GetResultString(string message = null, string type = "ok")
		{
			return "{\"res\":\"" + type + "\",\"msg\":\"" + message.EmptyNull().Replace("\"", "\\\"") + "\"}";
		}


		internal class RoxyFolder
		{
			public IFolder Folder { get; set; }
			public int SubFolders { get; set; }
		}

		#endregion

		#region File system

		private string FileRoot
		{
			get
			{
				if (_fileRoot == null)
				{
					_fileRoot = GetSetting("FILES_ROOT");

					if (GetSetting("SESSION_PATH_KEY") != "" && _context.Session[GetSetting("SESSION_PATH_KEY")] != null)
						_fileRoot = (string)_context.Session[GetSetting("SESSION_PATH_KEY")];

					if (_fileRoot.IsEmpty())
						_fileRoot = "Media/Uploaded";
				}

				return _fileRoot;
			}
		}

		private string GetRelativePath(string path)
		{
			if (path.IsEmpty())
				return path;

			if (path.StartsWith("~/"))
				return path.Substring(2);

			if (path.StartsWith("/"))
				return path.Substring(1);

			if (path.IsWebUrl())
			{
				var uri = new Uri(path);

				return _fileSystem.GetStoragePath(uri.PathAndQuery);
			}

			return path;
		}

		private string GetUniqueFileName(string folder, string fileName)
		{
			var result = fileName;
			var copy = T("Admin.Common.Copy");
			string name = null;
			string extension = null;

			for (var i = 1; i < 999999; ++i)
			{
				var path = _fileSystem.Combine(folder, result);

				if (!_fileSystem.FileExists(path))
				{
					return result;
				}

				if (name == null || extension == null)
				{
					var file = _fileSystem.GetFile(path);
					extension = file.FileType;
					// this assumes that a storage file name always ends with its file type
					name = file.Name.EmptyNull().Substring(0, file.Name.EmptyNull().Length - extension.Length);
				}				

				result = "{0} - {1} {2}{3}".FormatInvariant(name, copy, i, extension);
			}

			return result;
		}

		private List<IFile> GetFiles(string path, string type)
		{
			var files = _fileSystem.ListFiles(path);

			if (type.IsEmpty() || type == "#")
				return files.ToList();

			return _fileSystem.ListFiles(path)
				.Where(x => GetFileContentType(x.FileType).IsCaseInsensitiveEqual(type))
				.ToList();
		}

		private List<RoxyFolder> ListDirs(string path)
		{
			var result = new List<RoxyFolder>();

			_fileSystem.ListFolders(path).Each(x =>
			{
				var subFolders = ListDirs(x.Path);

				result.Add(new RoxyFolder
				{
					Folder = x,
					SubFolders = subFolders.Count
				});

				result.AddRange(subFolders);
			});

			return result;
		}

		private void ListDirTree(string type)
		{
			var isFirstItem = true;
			var folders = ListDirs(FileRoot);

			folders.Insert(0, new RoxyFolder
			{
				Folder = _fileSystem.GetFolder(FileRoot),
				SubFolders = folders.Count
			});

			_response.Write("[");

			foreach (var folder in folders)
			{
				if (isFirstItem)
					isFirstItem = false;
				else
					_response.Write(",");

				var fileCount = GetFiles(folder.Folder.Path, type).Count;

				_response.Write(
					"{\"p\":\"/" + folder.Folder.Path.Replace(FileRoot, "").Replace("\\", "/")
					+ "\",\"f\":\"" + fileCount.ToString()
					+ "\",\"d\":\"" + folder.SubFolders.ToString()
					+ "\"}"
				);
			}

			_response.Write("]");
		}

		private void ListFiles(string path, string type)
		{
			var isFirstItem = true;
			var width = 0;
			var height = 0;
			var files = GetFiles(GetRelativePath(path), type);

			_response.Write("[");

			foreach (var file in files)
			{
				if (isFirstItem)
					isFirstItem = false;
				else
					_response.Write(",");

				GetImageSize(file, out width, out height);

				_response.Write("{");
				_response.Write("\"p\":\"" + _context.GetContentUrl(file.Path) + "\"");
				_response.Write(",\"t\":\"" + file.LastUpdated.ToUnixTime().ToString() + "\"");
				_response.Write(",\"s\":\"" + file.Size.ToString() + "\"");
				_response.Write(",\"w\":\"" + width.ToString() + "\"");
				_response.Write(",\"h\":\"" + height.ToString() + "\"");
				_response.Write("}");
			}

			_response.Write("]");
		}

		private void DownloadFile(string path)
		{
			path = GetRelativePath(path);
			if (!_fileSystem.FileExists(path))
				return;

			var len = 0;
			var buffer = new byte[BUFFER_SIZE];
			var file = _fileSystem.GetFile(path);

			try
			{
				using (var stream = file.OpenRead())
				{
					_response.Clear();
					_response.Headers.Add("Content-Disposition", "attachment; filename=\"" + file.Name + "\"");
					_response.ContentType = MimeTypes.MapNameToMimeType(file.Name);

					while (_response.IsClientConnected && (len = stream.Read(buffer, 0, BUFFER_SIZE)) > 0)
					{
						_response.OutputStream.Write(buffer, 0, len);
						_response.Flush();

						Array.Clear(buffer, 0, BUFFER_SIZE);
					}

					_response.End();
				}
			}
			catch (IOException)
			{
				throw new Exception(T("Admin.Common.FileInUse"));
			}
		}

		private void DownloadDir(string path)
		{
			path = GetRelativePath(path);
			if (!_fileSystem.FolderExists(path))
			{
				throw new Exception(LangRes("E_CreateArchive"));
			}

			var folder = _fileSystem.GetFolder(path);

			// copy files from file storage to temp folder
			var tempDir = FileSystemHelper.TempDir("roxy " + folder.Name);	
			FileSystemHelper.ClearDirectory(tempDir, false);
			var files = GetFiles(path, null);

			foreach (var file in files)
			{
				var bytes = _fileSystem.ReadAllBytes(file.Path);
				if (bytes != null && bytes.Length > 0)
				{
					System.IO.File.WriteAllBytes(Path.Combine(tempDir, file.Name), bytes);
				}
			}

			// create zip from temp folder
			var tempZip = Path.Combine(FileSystemHelper.TempDir(), folder.Name + ".zip");
			FileSystemHelper.Delete(tempZip);

			ZipFile.CreateFromDirectory(tempDir, tempZip, CompressionLevel.Fastest, false);

			_response.Clear();
			_response.Headers.Add("Content-Disposition", "attachment; filename=\"" + folder.Name + ".zip\"");
			_response.ContentType = "application/zip";
			_response.TransmitFile(tempZip);
			_response.Flush();

			FileSystemHelper.Delete(tempZip);
			FileSystemHelper.ClearDirectory(tempDir, true);

			_response.End();
		}

		private void ShowThumbnail(string path, int width, int height)
		{
			path = GetRelativePath(path);
			if (!_fileSystem.FileExists(path))
				return;

			var file = _fileSystem.GetFile(path);
			var stream = file.OpenRead();
			var image = new Bitmap(Image.FromStream(stream));

			stream.Close();
			stream.Dispose();

			int cropWidth = image.Width, cropHeight = image.Height;
			int cropX = 0, cropY = 0;
			double imgRatio = (double)image.Width / (double)image.Height;

			if (height == 0)
				height = Convert.ToInt32(Math.Floor((double)width / imgRatio));

			if (width > image.Width)
				width = image.Width;
			if (height > image.Height)
				height = image.Height;

			double cropRatio = (double)width / (double)height;

			cropWidth = Convert.ToInt32(Math.Floor((double)image.Height * cropRatio));
			cropHeight = Convert.ToInt32(Math.Floor((double)cropWidth / cropRatio));
			if (cropWidth > image.Width)
			{
				cropWidth = image.Width;
				cropHeight = Convert.ToInt32(Math.Floor((double)cropWidth / cropRatio));
			}
			if (cropHeight > image.Height)
			{
				cropHeight = image.Height;
				cropWidth = Convert.ToInt32(Math.Floor((double)cropHeight * cropRatio));
			}
			if (cropWidth < image.Width)
			{
				cropX = Convert.ToInt32(Math.Floor((double)(image.Width - cropWidth) / 2));
			}
			if (cropHeight < image.Height)
			{
				cropY = Convert.ToInt32(Math.Floor((double)(image.Height - cropHeight) / 2));
			}

			var area = new Rectangle(cropX, cropY, cropWidth, cropHeight);

			using (var cropImg = image.Clone(area, PixelFormat.DontCare))
			{
				image.Dispose();
				var imgCallback = new Image.GetThumbnailImageAbort(() => false);

				_response.AddHeader("Content-Type", "image/png");

				cropImg
					.GetThumbnailImage(width, height, imgCallback, IntPtr.Zero)
					.Save(_response.OutputStream, ImageFormat.Png);

				_response.OutputStream.Close();
			}
		}

		private void RenameFile(string path, string name)
		{
			path = GetRelativePath(path);

			if (!_fileSystem.FileExists(path))
			{
				throw new Exception(LangRes("E_RenameFileInvalidPath"));
			}

			var fileType = _fileSystem.GetFile(path).FileType;

			if (!IsAllowedFileType(fileType))
			{
				throw new Exception(LangRes("E_FileExtensionForbidden"));
			}

			try
			{
				var folder = _fileSystem.GetFolderForFile(path);

				_fileSystem.RenameFile(path, _fileSystem.Combine(folder.Path, name));

				_response.Write(GetResultString());
			}
			catch (Exception exception)
			{
				throw new Exception(exception.Message + "; " + LangRes("E_RenameFile") + " \"" + path + "\"");
			}
		}

		private void RenameDir(string path, string name)
		{
			path = GetRelativePath(path);

			if (!_fileSystem.FolderExists(path))
			{
				throw new Exception(LangRes("E_RenameDirInvalidPath"));
			}

			if (path == FileRoot)
			{
				throw new Exception(LangRes("E_CannotRenameRoot"));
			}

			try
			{
				var folder = _fileSystem.GetFolder(path);
				var newPath = _fileSystem.Combine(folder.Parent.Path, name);

				_fileSystem.RenameFolder(path, newPath);

				_response.Write(GetResultString());
			}
			catch
			{
				throw new Exception(LangRes("E_RenameDir") + " \"" + path + "\"");
			}
		}

		private void MoveFile(string path, string newPath)
		{
			path = GetRelativePath(path);
			newPath = GetRelativePath(newPath);

			if (!_fileSystem.FileExists(path))
			{
				throw new Exception(LangRes("E_MoveFileInvalisPath"));
			}

			if (_fileSystem.FileExists(newPath))
			{
				throw new Exception(LangRes("E_MoveFileAlreadyExists"));
			}

			try
			{
				_fileSystem.RenameFile(path, newPath);

				_response.Write(GetResultString());
			}
			catch
			{
				throw new Exception(LangRes("E_MoveFile") + " \"" + path + "\"");
			}
		}

		private void MoveDir(string path, string newPath)
		{
			path = GetRelativePath(path);
			
			if (!_fileSystem.FolderExists(path))
			{
				throw new Exception(LangRes("E_MoveDirInvalisPath"));
			}

			var folder = _fileSystem.GetFolder(path);
			newPath = _fileSystem.Combine(GetRelativePath(newPath), folder.Name).Replace("\\", "/");

			if (_fileSystem.FolderExists(newPath))
			{
				throw new Exception(LangRes("E_DirAlreadyExists"));
			}

			if (newPath.IndexOf(path) == 0)
			{
				throw new Exception(LangRes("E_CannotMoveDirToChild"));
			}

			try
			{
				_fileSystem.RenameFolder(path, newPath);

				_response.Write(GetResultString());
			}
			catch
			{
				throw new Exception(LangRes("E_MoveDir") + " \"" + path + "\"");
			}
		}

		private void CopyFile(string path, string newPath)
		{
			path = GetRelativePath(path);
			newPath = GetRelativePath(newPath);

			if (!_fileSystem.FileExists(path))
			{
				throw new Exception(LangRes("E_CopyFileInvalisPath"));
			}

			try
			{
				var file = _fileSystem.GetFile(path);
				var newName = GetUniqueFileName(newPath, file.Name);

				_fileSystem.CopyFile(path, _fileSystem.Combine(newPath, newName));

				_response.Write(GetResultString());
			}
			catch
			{
				throw new Exception(LangRes("E_CopyFile"));
			}
		}

		private void DeleteFile(string path)
		{
			path = GetRelativePath(path);

			if (!_fileSystem.FileExists(path))
			{
				throw new Exception(LangRes("E_DeleteFileInvalidPath"));
			}

			try
			{
				_fileSystem.DeleteFile(path);
				
				_response.Write(GetResultString());
			}
			catch
			{
				throw new Exception(LangRes("E_DeletеFile"));
			}
		}

		private void DeleteDir(string path)
		{
			path = GetRelativePath(path);

			if (!_fileSystem.FolderExists(path))
			{
				throw new Exception(LangRes("E_DeleteDirInvalidPath"));
			}

			if (path == FileRoot)
			{
				throw new Exception(LangRes("E_CannotDeleteRoot"));
			}

			//throw new Exception(LangRes("E_DeleteNonEmpty"));

			try
			{
				_fileSystem.DeleteFolder(path);

				_response.Write(GetResultString());
			}
			catch
			{
				throw new Exception(LangRes("E_CannotDeleteDir"));
			}
		}

		private void CreateDir(string path, string name)
		{
			path = GetRelativePath(path);

			if (!_fileSystem.FolderExists(path))
			{
				throw new Exception(LangRes("E_CreateDirInvalidPath"));
			}

			try
			{
				path = _fileSystem.Combine(path, name);

				if (!_fileSystem.FolderExists(path))
					_fileSystem.CreateFolder(path);

				_response.Write(GetResultString());
			}
			catch
			{
				throw new Exception(LangRes("E_CreateDirFailed"));
			}
		}

		private void CopyDirCore(string path, string dest)
		{
			if (!_fileSystem.FolderExists(dest))
			{
				_fileSystem.CreateFolder(dest);
			}

			foreach (var file in _fileSystem.ListFiles(path))
			{
				var newPath = _fileSystem.Combine(dest, file.Name);

				if (!_fileSystem.FileExists(newPath))
				{
					_fileSystem.CopyFile(file.Path, newPath);
				}
			}

			foreach (var folder in _fileSystem.ListFolders(path))
			{
				var newPath = _fileSystem.Combine(dest, folder.Name);

				CopyDirCore(folder.Path, newPath);
			}
		}

		private void CopyDir(string path, string newPath)
		{
			path = GetRelativePath(path);
			newPath = GetRelativePath(newPath);

			if (!_fileSystem.FolderExists(path))
			{
				throw new Exception(LangRes("E_CopyDirInvalidPath"));
			}

			var folder = _fileSystem.GetFolder(path);

			newPath = _fileSystem.Combine(newPath, folder.Name);

			if (_fileSystem.FolderExists(newPath))
			{
				throw new Exception(LangRes("E_DirAlreadyExists"));
			}

			CopyDirCore(path, newPath);

			_response.Write(GetResultString());
		}

		private void Upload(string path)
		{
			path = GetRelativePath(path);

			string result = null;
			var width = 0;
			var height = 0;

			int.TryParse(GetSetting("MAX_IMAGE_WIDTH"), out width);
			int.TryParse(GetSetting("MAX_IMAGE_HEIGHT"), out height);

			var tempDir = FileSystemHelper.TempDir("roxy " + CommonHelper.GenerateRandomInteger().ToString());

			try
			{
				// copy uploaded files to temp folder and resize them
				for (var i = 0; i < Request.Files.Count; ++i)
				{
					var file = Request.Files[i];
					file.FileName.Dump();
					var extension = Path.GetExtension(file.FileName);

					if (GetFileContentType(extension).IsCaseInsensitiveEqual("image") && IsAllowedFileType(extension))
					{
						var dest = Path.Combine(tempDir, file.FileName);
						file.SaveAs(dest);

						ImageResize(dest, dest, width, height);
					}
					else
					{
						result = GetResultString(LangRes("E_UploadNotAll"));
					}
				}

				// copy files to file storage
				foreach (var tempPath in Directory.EnumerateFiles(tempDir, "*", SearchOption.TopDirectoryOnly))
				{
					using (var stream = new FileStream(tempPath, FileMode.Open, FileAccess.Read))
					{
						var name = GetUniqueFileName(path, Path.GetFileName(tempPath));
						var newPath = _fileSystem.Combine(path, name);

						_fileSystem.SaveStream(newPath, stream);
					}
				}
			}
			catch (Exception exception)
			{
				result = GetResultString(exception.Message, "error");
			}
			finally
			{
				FileSystemHelper.ClearDirectory(tempDir, true);
			}

			_response.Write("<script>");
			_response.Write("parent.fileUploaded(" + (result ?? GetResultString()) + ");");
			_response.Write("</script>");
		}

		#endregion

		public void ProcessRequest()
		{
			if (!Services.Permissions.Authorize(StandardPermissionProvider.UploadPictures))
			{
				_response.Write(T("Admin.AccessDenied.Description"));
				return;
			}

			var action = "DIRLIST";

			try
			{
				if (_context.Request["a"] != null)
				{
					action = _context.Request["a"];
				}

				switch (action.ToUpper())
				{
					case "DIRLIST":
						ListDirTree(_context.Request["type"]);
						break;
					case "FILESLIST":
						ListFiles(_context.Request["d"], _context.Request["type"]);
						break;
					case "COPYDIR":
						CopyDir(_context.Request["d"], _context.Request["n"]);
						break;
					case "COPYFILE":
						CopyFile(_context.Request["f"], _context.Request["n"]);
						break;
					case "CREATEDIR":
						CreateDir(_context.Request["d"], _context.Request["n"]);
						break;
					case "DELETEDIR":
						DeleteDir(_context.Request["d"]);
						break;
					case "DELETEFILE":
						DeleteFile(_context.Request["f"]);
						break;
					case "DOWNLOAD":
						DownloadFile(_context.Request["f"]);
						break;
					case "DOWNLOADDIR":
						DownloadDir(_context.Request["d"]);
						break;
					case "MOVEDIR":
						MoveDir(_context.Request["d"], _context.Request["n"]);
						break;
					case "MOVEFILE":
						MoveFile(_context.Request["f"], _context.Request["n"]);
						break;
					case "RENAMEDIR":
						RenameDir(_context.Request["d"], _context.Request["n"]);
						break;
					case "RENAMEFILE":
						RenameFile(_context.Request["f"], _context.Request["n"]);
						break;
					case "GENERATETHUMB":
						int w = 140, h = 0;
						int.TryParse(_context.Request["width"].Replace("px", ""), out w);
						int.TryParse(_context.Request["height"].Replace("px", ""), out h);
						ShowThumbnail(_context.Request["f"], w, h);
						break;
					case "UPLOAD":
						Upload(_context.Request["d"]);
						break;
					default:
						_response.Write(GetResultString("This action is not implemented.", "error"));
						break;
				}
			}
			catch (Exception exception)
			{
				if (action == "UPLOAD")
				{
					_response.Write("<script>");
					_response.Write("parent.fileUploaded(" + GetResultString(LangRes("E_UploadNoFiles"), "error") + ");");
					_response.Write("</script>");
				}
				else
				{
					_response.Write(GetResultString(exception.Message, "error"));
				}

				Logger.ErrorsAll(exception);
			}
		}
	}
}