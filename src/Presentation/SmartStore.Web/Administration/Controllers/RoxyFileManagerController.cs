using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using SmartStore.Core.IO;
using SmartStore.Core.Logging;
using SmartStore.Services.Media;
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

		private readonly Lazy<IImageResizerService> _imageResizer;
		private readonly Lazy<IPictureService> _pictureService;
		private readonly IMediaFileSystem _fileSystem;

		public RoxyFileManagerController(
			Lazy<IImageResizerService> imageResizer,
			Lazy<IPictureService> pictureService,
			IMediaFileSystem fileSystem)
		{
			_imageResizer = imageResizer;
			_pictureService = pictureService;
			_fileSystem = fileSystem;
		}

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
			var result = false;
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

		private void GetImageSize(string path, out int width, out int height)
		{
			width = height = 0;

			var size = _pictureService.Value.GetPictureSize(_fileSystem.ReadAllBytes(path));
			width = size.Width;
			height = size.Height;
		}

		private void ImageResize(string path, string dest, int width, int height)
		{
			if (dest.IsEmpty() || (width == 0 && height == 0))
				return;

			using (var stream = new FileStream(path, FileMode.Open))
			{
				using (var resultStream = _imageResizer.Value.ResizeImage(stream, width, height))
				{
					var result = resultStream.GetBuffer();
					System.IO.File.WriteAllBytes(dest, result);
				}
			}
		}

		private string GetResultString(string message = null, string type = "ok")
		{
			return "{\"res\":\"" + type + "\",\"msg\":\"" + message.EmptyNull().Replace("\"", "\\\"") + "\"}";
		}

		private bool IsAjaxUpload()
		{
			return (Request["method"] != null && Request["method"].ToString() == "ajax");
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
					
					if (GetSetting("SESSION_PATH_KEY") != "" && Session[GetSetting("SESSION_PATH_KEY")] != null)
						_fileRoot = (string)Session[GetSetting("SESSION_PATH_KEY")];

					if (_fileRoot.IsEmpty())
						_fileRoot = "Uploaded";
				}

				return _fileRoot;
			}
		}

		private string GetRelativePath(string path)
		{
			if (path.IsWebUrl())
			{
				var uri = new Uri(path);
				path = uri.PathAndQuery;
			}

			var root = HttpContext.GetContentUrl(_fileSystem.Root);

			if (path.StartsWith(root, StringComparison.OrdinalIgnoreCase))
			{
				path = path.Substring(root.Length);
			}

			return path.TrimStart('/', '\\');
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

			return files
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

			Response.Write("[");

			foreach (var folder in folders)
			{
				if (isFirstItem)
					isFirstItem = false;
				else
					Response.Write(",");

				var fileCount = GetFiles(folder.Folder.Path, type).Count;

				Response.Write(
					"{\"p\":\"/" + folder.Folder.Path.Replace("\\", "/")
					+ "\",\"f\":\"" + fileCount.ToString()
					+ "\",\"d\":\"" + folder.SubFolders.ToString()
					+ "\"}"
				);
			}

			Response.Write("]");
		}

		private void ListFiles(string path, string type)
		{
			var isFirstItem = true;
			var width = 0;
			var height = 0;
			var files = GetFiles(GetRelativePath(path), type);

			Response.Write("[");

			foreach (var file in files)
			{
				try
				{
					GetImageSize(file.Path, out width, out height);
				}
				catch {	}

				if (isFirstItem)
					isFirstItem = false;
				else
					Response.Write(",");

				var url = _fileSystem.GetPublicUrl(file.Path);

				Response.Write("{");
				Response.Write("\"p\":\"" + url + "\"");
				Response.Write(",\"t\":\"" + file.LastUpdated.ToUnixTime().ToString() + "\"");
				Response.Write(",\"s\":\"" + file.Size.ToString() + "\"");
				Response.Write(",\"w\":\"" + width.ToString() + "\"");
				Response.Write(",\"h\":\"" + height.ToString() + "\"");
				Response.Write("}");
			}

			Response.Write("]");
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
					Response.Clear();
					Response.Headers.Add("Content-Disposition", "attachment; filename=\"" + file.Name + "\"");
					Response.ContentType = MimeTypes.MapNameToMimeType(file.Name);

					while (Response.IsClientConnected && (len = stream.Read(buffer, 0, BUFFER_SIZE)) > 0)
					{
						Response.OutputStream.Write(buffer, 0, len);
						Response.Flush();

						Array.Clear(buffer, 0, BUFFER_SIZE);
					}

					Response.End();
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
			var tempDir = FileSystemHelper.TempDirTenant("roxy " + folder.Name);	
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
			var tempZip = Path.Combine(FileSystemHelper.TempDirTenant(), folder.Name + ".zip");
			FileSystemHelper.Delete(tempZip);

			ZipFile.CreateFromDirectory(tempDir, tempZip, CompressionLevel.Fastest, false);

			Response.Clear();
			Response.Headers.Add("Content-Disposition", "attachment; filename=\"" + folder.Name + ".zip\"");
			Response.ContentType = "application/zip";
			Response.TransmitFile(tempZip);
			Response.Flush();

			FileSystemHelper.Delete(tempZip);
			FileSystemHelper.ClearDirectory(tempDir, true);

			Response.End();
		}

		private void ShowThumbnail(string path, int width, int height)
		{
			path = GetRelativePath(path);
			if (!_fileSystem.FileExists(path))
				return;

			Bitmap image = null;
			var file = _fileSystem.GetFile(path);
			using (var stream = file.OpenRead())
			{
				try
				{
					image = new Bitmap(Image.FromStream(stream));
				}
				catch {	}

				stream.Close();
			}

			if (image == null)
				return;

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

				Response.AddHeader("Content-Type", "image/png");

				cropImg
					.GetThumbnailImage(width, height, imgCallback, IntPtr.Zero)
					.Save(Response.OutputStream, ImageFormat.Png);

				Response.OutputStream.Close();
			}
		}

		private void RenameFile(string oldPath, string name)
		{
			oldPath = GetRelativePath(oldPath);

			if (!_fileSystem.FileExists(oldPath))
			{
				throw new Exception(LangRes("E_RenameFileInvalidPath"));
			}

			var fileType = Path.GetExtension(name);

			if (!IsAllowedFileType(fileType))
			{
				throw new Exception(LangRes("E_FileExtensionForbidden"));
			}

			try
			{
				var folder = _fileSystem.GetFolderForFile(oldPath);
				var newPath = _fileSystem.Combine(folder.Path, name);

				_fileSystem.RenameFile(oldPath, newPath);

				Response.Write(GetResultString());
			}
			catch (Exception exception)
			{
				throw new Exception(exception.Message + "; " + LangRes("E_RenameFile") + " \"" + oldPath + "\"");
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

				Response.Write(GetResultString());
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

				Response.Write(GetResultString());
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

				Response.Write(GetResultString());
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

				Response.Write(GetResultString());
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

				Response.Write(GetResultString());
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

				Response.Write(GetResultString());
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

				Response.Write(GetResultString());
			}
			catch
			{
				throw new Exception(LangRes("E_CreateDirFailed"));
			}
		}

		private void CopyDirCore(string path, string targetPath)
		{
			if (!_fileSystem.FolderExists(targetPath))
			{
				_fileSystem.CreateFolder(targetPath);
			}

			foreach (var file in _fileSystem.ListFiles(path))
			{
				var newPath = _fileSystem.Combine(targetPath, file.Name);

				if (!_fileSystem.FileExists(newPath))
				{
					_fileSystem.CopyFile(file.Path, newPath);
				}
			}

			foreach (var folder in _fileSystem.ListFolders(path))
			{
				var newPath = _fileSystem.Combine(targetPath, folder.Name);

				CopyDirCore(folder.Path, newPath);
			}
		}

		private void CopyDir(string path, string targetPath)
		{
			path = GetRelativePath(path);
			targetPath = GetRelativePath(targetPath);

			if (!_fileSystem.FolderExists(path))
			{
				throw new Exception(LangRes("E_CopyDirInvalidPath"));
			}

			var folder = _fileSystem.GetFolder(path);

			targetPath = _fileSystem.Combine(targetPath, folder.Name);

			if (_fileSystem.FolderExists(targetPath))
			{
				throw new Exception(LangRes("E_DirAlreadyExists"));
			}

			if (targetPath.Contains(path))
			{
				throw new Exception(T("Common.CannotCopyFolderIntoItself"));
			}

			CopyDirCore(path, targetPath);

			Response.Write(GetResultString());
		}

		private void Upload(string path)
		{
			path = GetRelativePath(path);

			string message = null;
			var hasError = false;
			var width = 0;
			var height = 0;

			int.TryParse(GetSetting("MAX_IMAGE_WIDTH"), out width);
			int.TryParse(GetSetting("MAX_IMAGE_HEIGHT"), out height);

			var tempDir = FileSystemHelper.TempDirTenant("roxy " + CommonHelper.GenerateRandomInteger().ToString());

			try
			{
				// copy uploaded files to temp folder and resize them
				for (var i = 0; i < Request.Files.Count; ++i)
				{
					var file = Request.Files[i];
					var extension = Path.GetExtension(file.FileName);

					if (IsAllowedFileType(extension))
					{
						var dest = Path.Combine(tempDir, file.FileName);
						file.SaveAs(dest);

						if (GetFileContentType(extension).IsCaseInsensitiveEqual("image"))
						{
							ImageResize(dest, dest, width, height);
						}
					}
					else
					{
						message = LangRes("E_UploadNotAll");
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
				hasError = true;
				message = exception.Message;
			}
			finally
			{
				FileSystemHelper.ClearDirectory(tempDir, true);
			}

			if (IsAjaxUpload())
			{
				if (message.HasValue())
				{
					Response.Write(GetResultString(message, hasError ? "error" : "ok"));
				}
			}
			else
			{
				Response.Write("<script>");
				Response.Write("parent.fileUploaded(" + GetResultString(message, hasError ? "error" : "ok") + ");");
				Response.Write("</script>");
			}
		}

		#endregion

		public void ProcessRequest()
		{
			if (!Services.Permissions.Authorize(StandardPermissionProvider.UploadPictures))
			{
				Response.Write(T("Admin.AccessDenied.Description"));
				return;
			}

			var action = "DIRLIST";

			try
			{
				if (Request["a"] != null)
				{
					action = Request["a"];
				}

				switch (action.ToUpper())
				{
					case "DIRLIST":
						ListDirTree(Request["type"]);
						break;
					case "FILESLIST":
						ListFiles(Request["d"], Request["type"]);
						break;
					case "COPYDIR":
						CopyDir(Request["d"], Request["n"]);
						break;
					case "COPYFILE":
						CopyFile(Request["f"], Request["n"]);
						break;
					case "CREATEDIR":
						CreateDir(Request["d"], Request["n"]);
						break;
					case "DELETEDIR":
						DeleteDir(Request["d"]);
						break;
					case "DELETEFILE":
						DeleteFile(Request["f"]);
						break;
					case "DOWNLOAD":
						DownloadFile(Request["f"]);
						break;
					case "DOWNLOADDIR":
						DownloadDir(Request["d"]);
						break;
					case "MOVEDIR":
						MoveDir(Request["d"], Request["n"]);
						break;
					case "MOVEFILE":
						MoveFile(Request["f"], Request["n"]);
						break;
					case "RENAMEDIR":
						RenameDir(Request["d"], Request["n"]);
						break;
					case "RENAMEFILE":
						RenameFile(Request["f"], Request["n"]);
						break;
					case "GENERATETHUMB":
						int w = 140, h = 0;
						int.TryParse(Request["width"].Replace("px", ""), out w);
						int.TryParse(Request["height"].Replace("px", ""), out h);
						ShowThumbnail(Request["f"], w, h);
						break;
					case "UPLOAD":
						Upload(Request["d"]);
						break;
					default:
						Response.Write(GetResultString("This action is not implemented.", "error"));
						break;
				}
			}
			catch (Exception exception)
			{
				if (action == "UPLOAD")
				{
					if (IsAjaxUpload())
					{
						Response.Write(GetResultString(LangRes("E_UploadNoFiles"), "error"));
					}
					else
					{
						Response.Write("<script>");
						Response.Write("parent.fileUploaded(" + GetResultString(LangRes("E_UploadNoFiles"), "error") + ");");
						Response.Write("</script>");
					}
				}
				else
				{
					Response.Write(GetResultString(exception.Message, "error"));
				}

				Logger.ErrorsAll(exception);
			}
		}
	}
}