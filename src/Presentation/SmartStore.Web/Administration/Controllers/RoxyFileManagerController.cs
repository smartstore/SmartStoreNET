using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Events;
using SmartStore.Core.IO;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Security;
using SmartStore.Services.Media;
using SmartStore.Utilities;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Services.Media.Storage;
using SmartStore.Collections;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
	public class RoxyFileManagerController : AdminControllerBase
	{
		private const int BUFFER_SIZE = 32768;
		private const string CONFIG_FILE = "~/Administration/Content/filemanager/conf.json";

		private string _fileRoot = null;
		private Dictionary<string, string> _lang = null;
		private Dictionary<string, string> _roxySettings = null;

		private readonly IMediaService _mediaService;
		private readonly IFolderService _folderService;
		private readonly IAlbumRegistry _albumRegistry;
		private readonly Lazy<IImageProcessor> _imageProcessor;
		private readonly Lazy<IImageCache> _imageCache;
		//private readonly IMediaFileSystem _fileSystem;
		private readonly IEventPublisher _eventPublisher;
		private readonly ILocalizationFileResolver _locFileResolver;
		private readonly Lazy<MediaSettings> _mediaSettings;

		private readonly AlbumInfo _album;
		private readonly TreeNode<MediaFolderNode> _rootNode;

		public RoxyFileManagerController(
			IMediaService mediaService,
			IFolderService folderService,
			IAlbumRegistry albumRegistry,
			Lazy<IImageProcessor> imageProcessor,
			Lazy<IImageCache> imageCache,
			//IMediaFileSystem fileSystem,
			IEventPublisher eventPublisher,
			ILocalizationFileResolver locFileResolver,
			Lazy<MediaSettings> mediaSettings)
		{
			_mediaService = mediaService;
			_folderService = folderService;
			_albumRegistry = albumRegistry;
			_imageProcessor = imageProcessor;
			_imageCache = imageCache;
			//_fileSystem = fileSystem;
			_eventPublisher = eventPublisher;
			_locFileResolver = locFileResolver;
			_mediaSettings = mediaSettings;

			_album = _albumRegistry.GetAlbumByName(SystemAlbumProvider.Files);
			_rootNode = _folderService.GetNodeById(_album.Id);

			//var albumInfo = albumRegistry.GetAlbumByName(SystemAlbumProvider.Files);
			//_fileSystem = new AlbumFileSystemAdapter(albumInfo, mediaService, folderService);
		}

		#region Utilities

		private void Write(object obj)
		{
			var json = JsonConvert.SerializeObject(obj);
			Response.Write(json);
		}

		private Dictionary<string, string> ParseJson(string path)
		{
			try
			{
				var json = System.IO.File.ReadAllText(path, System.Text.Encoding.UTF8);
				return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
			}
			catch (Exception ex)
			{
				ex.Dump();
				return new Dictionary<string, string>();
			}
		}

		private string LangRes(string name)
		{
			var result = name;

			if (_lang == null)
			{
				var locFile = _locFileResolver.Resolve(Services.WorkContext.WorkingLanguage.UniqueSeoCode, "~/Administration/Content/filemanager/lang/", "*.js");

				if (locFile == null)
				{
					return name;
				}

				var js = System.IO.File.ReadAllText(CommonHelper.MapPath(locFile.VirtualPath));
				var objStart = js.IndexOf("{");
				var json = js.Substring(objStart);

				_lang = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
			}

			return _lang.Get(name) ?? name;
		}

		private string GetSetting(string name)
		{
			var result = "";

			if (_roxySettings == null)
				_roxySettings = ParseJson(CommonHelper.MapPath(CONFIG_FILE));

			if (_roxySettings.ContainsKey(name))
				result = _roxySettings[name];

			return result;
		}

		private string GetFileContentType(string extension)
		{
			extension = extension.EmptyNull().ToLower();

			var mimeType = MimeTypes.MapNameToMimeType(extension).EmptyNull();
			var slashIndex = mimeType.IndexOf('/');
			if (slashIndex < 0)
				return "file";

			var mediaType = mimeType.Substring(0, slashIndex);
			var subType = mimeType.Substring(slashIndex + 1);

			switch (mediaType)
			{
				case "image":
				case "audio":
				case "video":
					return mediaType;
				case "application":
					if (extension == ".pdf")
					{
						return "pdf";
					}
					else if (extension == ".swf")
					{
						return "flash";
					}
					else
					{
						return "file";
					}
				default:
					return "file";
			}
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

		private void ImageResize(string path, string dest, int maxWidth, int maxHeight, bool notify = true)
		{
			if (dest.IsEmpty())
				return;

			if (maxWidth == 0 && maxHeight == 0)
			{
				maxWidth = _mediaSettings.Value.MaximumImageSize;
				maxHeight = _mediaSettings.Value.MaximumImageSize;
			}

			var buffer = System.IO.File.ReadAllBytes(path);

			var query = new ProcessImageQuery(buffer)
			{
				Quality = _mediaSettings.Value.DefaultImageQuality,
				Format = Path.GetExtension(path).Trim('.').ToLower(),
				IsValidationMode = true,
				Notify = notify
			};

			var originalSize = ImageHeader.GetDimensions(buffer, MimeTypes.MapNameToMimeType(path));

			if (originalSize.IsEmpty || (originalSize.Height <= maxHeight && originalSize.Width <= maxWidth))
			{
				// Give subscribers the chance to (pre)-process
				var evt = new ImageUploadValidatedEvent(query, originalSize);
				_eventPublisher.Publish(evt);

				if (evt.ResultBuffer != null)
				{
					System.IO.File.WriteAllBytes(dest, evt.ResultBuffer);
				}

				return;
			}

			if (maxWidth > 0) query.MaxWidth = maxWidth;
			if (maxHeight > 0) query.MaxHeight = maxHeight;

			using (var result = _imageProcessor.Value.ProcessImage(query))
			{
				buffer = result.OutputStream.ToArray();
				System.IO.File.WriteAllBytes(dest, buffer);
			}
		}

		private string GetResultString(string message = null, string type = "ok")
		{
			var result = new
			{
				res = type,
				msg = message
			};

			return JsonConvert.SerializeObject(result);
			//return "{\"res\":\"" + type + "\",\"msg\":\"" + message.EmptyNull().Replace("\"", "\\\"") + "\"}";
		}

		private bool IsAjaxUpload()
		{
			return Request.IsAjaxRequest() || (Request["method"] != null && Request["method"].ToString() == "ajax");
		}

		internal class RoxyFolder
		{
			public IFolder Folder { get; set; }
			public int SubFolders { get; set; }
		}

		#endregion

		#region Views

		public ActionResult Index(string type)
		{
			return View();
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
						_fileRoot = SystemAlbumProvider.Files;
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

			return path;

			//return (_fileSystem.GetStoragePath(path) ?? path).TrimStart('/', '\\');
		}

		private MediaFolderNode GetNodeByPath(string path)
		{
			return _folderService.GetNodeByPath(path).Value;
		}

		private MediaSearchQuery CreateSearchQuery(int folderId, string type)
		{
			var query = new MediaSearchQuery
			{
				FolderId = folderId,
				Deleted = false
			};

			if (type.HasValue() && type != "#")
			{
				query.MediaTypes = new[] { type };
			}

			return query;
		}

		private IEnumerable<MediaFileInfo> GetFiles(int folderId, string type)
		{
			var query = CreateSearchQuery(folderId, type);
			var files = _mediaService.SearchFiles(query);
			return files;
		}

		private long CountFiles(int folderId, string type)
		{
			var query = CreateSearchQuery(folderId, type);
			return _mediaService.CountFiles(query);
		}

		private void ListDirTree(string type)
		{
			var result = _rootNode.FlattenNodes(true)
				.Select(x => 
				{
					var numFiles = CountFiles(x.Value.Id, type);
					return new 
					{
						p = x.Value.Path,
						f = numFiles.ToString(),
						d = x.Children.Count
					};
				})
				.ToArray();

			Write(result);
		}

		private void ListFiles(string path, string type)
		{
			var files = GetFiles(GetNodeByPath(path).Id, type);

			var result = files.Select(x => new 
			{ 
				p = _mediaService.GetUrl(x, null, string.Empty),
				t = x.LastUpdated.ToUnixTime().ToString(),
				m = x.MimeType,
				s = x.Size.ToString(),
				w = x.Dimensions.Width.ToString(),
				h = x.Dimensions.Height.ToString()
			}).ToArray();

			Write(result);
		}

		private void DownloadFile(string path)
		{
			//path = GetRelativePath(path);
			//if (!_fileSystem.FileExists(path))
			//	return;

			//var len = 0;
			//var buffer = new byte[BUFFER_SIZE];
			//var file = _fileSystem.GetFile(path);

			//try
			//{
			//	using (var stream = file.OpenRead())
			//	{
			//		Response.Clear();
			//		Response.Headers.Add("Content-Disposition", "attachment; filename=\"" + file.Name + "\"");
			//		Response.ContentType = MimeTypes.MapNameToMimeType(file.Name);

			//		while (Response.IsClientConnected && (len = stream.Read(buffer, 0, BUFFER_SIZE)) > 0)
			//		{
			//			Response.OutputStream.Write(buffer, 0, len);
			//			Response.Flush();

			//			Array.Clear(buffer, 0, BUFFER_SIZE);
			//		}

			//		Response.End();
			//	}
			//}
			//catch (IOException)
			//{
			//	throw new Exception(T("Admin.Common.FileInUse"));
			//}
		}

		private void DownloadDir(string path)
		{
			//path = GetRelativePath(path);
			//if (!_fileSystem.FolderExists(path))
			//{
			//	throw new Exception(LangRes("E_CreateArchive"));
			//}

			//var folder = _fileSystem.GetFolder(path);

			//// copy files from file storage to temp folder
			//var tempDir = FileSystemHelper.TempDirTenant("roxy " + folder.Name);	
			//FileSystemHelper.ClearDirectory(tempDir, false);
			//var files = GetFiles(path, null);

			//foreach (var file in files)
			//{
			//	var bytes = _fileSystem.ReadAllBytes(file.Path);
			//	if (bytes != null && bytes.Length > 0)
			//	{
			//		System.IO.File.WriteAllBytes(Path.Combine(tempDir, file.Name), bytes);
			//	}
			//}

			//// create zip from temp folder
			//var tempZip = Path.Combine(FileSystemHelper.TempDirTenant(), folder.Name + ".zip");
			//FileSystemHelper.DeleteFile(tempZip);

			//ZipFile.CreateFromDirectory(tempDir, tempZip, CompressionLevel.Fastest, false);

			//Response.Clear();
			//Response.Headers.Add("Content-Disposition", "attachment; filename=\"" + folder.Name + ".zip\"");
			//Response.ContentType = "application/zip";
			//Response.TransmitFile(tempZip);
			//Response.Flush();

			//FileSystemHelper.DeleteFile(tempZip);
			//FileSystemHelper.ClearDirectory(tempDir, true);

			//Response.End();
		}

		private void RenameFile(string oldPath, string name)
		{
			//oldPath = GetRelativePath(oldPath);

			//if (!_fileSystem.FileExists(oldPath))
			//{
			//	throw new Exception(LangRes("E_RenameFileInvalidPath"));
			//}

			//var fileType = Path.GetExtension(name);

			//if (!IsAllowedFileType(fileType))
			//{
			//	throw new Exception(LangRes("E_FileExtensionForbidden"));
			//}

			//try
			//{
			//	var folder = _fileSystem.GetFolderForFile(oldPath);
			//	var newPath = _fileSystem.Combine(folder.Path, name);

			//	_fileSystem.RenameFile(oldPath, newPath);

			//	Response.Write(GetResultString());
			//}
			//catch (Exception ex)
			//{
			//	throw new Exception(ex.Message + "; " + LangRes("E_RenameFile") + " \"" + oldPath + "\"");
			//}
		}

		private void RenameDir(string path, string name)
		{
			//path = GetRelativePath(path);

			//if (!_fileSystem.FolderExists(path))
			//{
			//	throw new Exception(LangRes("E_RenameDirInvalidPath"));
			//}

			//if (path == FileRoot)
			//{
			//	throw new Exception(LangRes("E_CannotRenameRoot"));
			//}

			//try
			//{
			//	var folder = _fileSystem.GetFolder(path);
			//	var newPath = _fileSystem.Combine(folder.Parent.Path, name);

			//	_fileSystem.RenameFolder(path, newPath);

			//	Response.Write(GetResultString());
			//}
			//catch
			//{
			//	throw new Exception(LangRes("E_RenameDir") + " \"" + path + "\"");
			//}
		}

		private void MoveFile(string path, string newPath)
		{
			//path = GetRelativePath(path);
			//newPath = GetRelativePath(newPath);

			//if (!_fileSystem.FileExists(path))
			//{
			//	throw new Exception(LangRes("E_MoveFileInvalisPath"));
			//}

			//if (_fileSystem.FileExists(newPath))
			//{
			//	throw new Exception(LangRes("E_MoveFileAlreadyExists"));
			//}

			//try
			//{
			//	_fileSystem.RenameFile(path, newPath);

			//	Response.Write(GetResultString());
			//}
			//catch
			//{
			//	throw new Exception(LangRes("E_MoveFile") + " \"" + path + "\"");
			//}
		}

		private void MoveDir(string path, string newPath)
		{
			//path = GetRelativePath(path);
			
			//if (!_fileSystem.FolderExists(path))
			//{
			//	throw new Exception(LangRes("E_MoveDirInvalisPath"));
			//}

			//var folder = _fileSystem.GetFolder(path);
			//newPath = _fileSystem.Combine(GetRelativePath(newPath), folder.Name).Replace("\\", "/");

			//if (_fileSystem.FolderExists(newPath))
			//{
			//	throw new Exception(LangRes("E_DirAlreadyExists"));
			//}

			//if (newPath.IndexOf(path) == 0)
			//{
			//	throw new Exception(LangRes("E_CannotMoveDirToChild"));
			//}

			//try
			//{
			//	_fileSystem.RenameFolder(path, newPath);

			//	Response.Write(GetResultString());
			//}
			//catch (Exception ex)
			//{
			//	throw new Exception(LangRes("E_MoveDir") + " \"" + path + "\"" + ": (" + ex.Message + ")");
			//}
		}

		private void CopyFile(string path, string newPath)
		{
			//path = GetRelativePath(path);

			//var file = _fileSystem.GetFile(path);

			//if (!file.Exists)
			//{
			//	throw new Exception(LangRes("E_CopyFileInvalisPath"));
			//}

			//try
			//{
			//	newPath = _fileSystem.Combine(GetRelativePath(newPath), file.Name);
			//	if (_fileSystem.CheckFileUniqueness(newPath, out var newFile))
			//	{
			//		newPath = newFile.Path;
			//	}

			//	_fileSystem.CopyFile(path, newPath);

			//	Response.Write(GetResultString());
			//}
			//catch (Exception ex)
			//{
			//	throw new Exception(LangRes("E_CopyFile") + ": " + ex.Message);
			//}
		}

		private void DeleteFile(string path)
		{
			//path = GetRelativePath(path);
			//var file = _fileSystem.GetFile(path);
			//if (!file.Exists)
			//{
			//	throw new Exception(LangRes("E_DeleteFileInvalidPath"));
			//}
			
			//try
			//{
			//	_fileSystem.DeleteFile(path);
			//	_imageCache.Value.Delete(file);

			//	Response.Write(GetResultString());
			//}
			//catch (Exception ex)
			//{
			//	throw new Exception(LangRes("E_DeletеFile") + ": " + ex.Message);
			//}
		}

		private void DeleteDir(string path)
		{
			//path = GetRelativePath(path);

			//if (!_fileSystem.FolderExists(path))
			//{
			//	throw new Exception(LangRes("E_DeleteDirInvalidPath"));
			//}

			//if (path == FileRoot)
			//{
			//	throw new Exception(LangRes("E_CannotDeleteRoot"));
			//}

			////throw new Exception(LangRes("E_DeleteNonEmpty"));

			//try
			//{
			//	_fileSystem.DeleteFolder(path);

			//	Response.Write(GetResultString());
			//}
			//catch (Exception ex)
			//{
			//	throw new Exception(LangRes("E_CannotDeleteDir") + ": " + ex.Message);
			//}
		}

		private void CreateDir(string path, string name)
		{
			//path = GetRelativePath(path);

			//if (!_fileSystem.FolderExists(path))
			//{
			//	throw new Exception(LangRes("E_CreateDirInvalidPath"));
			//}

			//try
			//{
			//	path = _fileSystem.Combine(path, name);

			//	if (!_fileSystem.FolderExists(path))
			//		_fileSystem.CreateFolder(path);

			//	Response.Write(GetResultString());
			//}
			//catch (Exception ex)
			//{
			//	throw new Exception(LangRes("E_CreateDirFailed") + ": " + ex.Message);
			//}
		}

		private void CopyDirCore(string path, string targetPath)
		{
			//if (!_fileSystem.FolderExists(targetPath))
			//{
			//	_fileSystem.CreateFolder(targetPath);
			//}

			//foreach (var file in _fileSystem.ListFiles(path))
			//{
			//	var newPath = _fileSystem.Combine(targetPath, file.Name);

			//	if (!_fileSystem.FileExists(newPath))
			//	{
			//		_fileSystem.CopyFile(file.Path, newPath);
			//	}
			//}

			//foreach (var folder in _fileSystem.ListFolders(path))
			//{
			//	var newPath = _fileSystem.Combine(targetPath, folder.Name);

			//	CopyDirCore(folder.Path, newPath);
			//}
		}

		private void CopyDir(string path, string targetPath)
		{
			//path = GetRelativePath(path);
			//targetPath = GetRelativePath(targetPath);

			//if (!_fileSystem.FolderExists(path))
			//{
			//	throw new Exception(LangRes("E_CopyDirInvalidPath"));
			//}

			//var folder = _fileSystem.GetFolder(path);

			//targetPath = _fileSystem.Combine(targetPath, folder.Name);

			//if (_fileSystem.FolderExists(targetPath))
			//{
			//	throw new Exception(LangRes("E_DirAlreadyExists"));
			//}

			//if (targetPath.Contains(path))
			//{
			//	throw new Exception(T("Common.CannotCopyFolderIntoItself"));
			//}

			//CopyDirCore(path, targetPath);

			//Response.Write(GetResultString());
		}

		private async Task UploadAsync(string path, bool external = false)
		{
			await Task.FromResult(0);
			
			//path = GetRelativePath(path);

			//string message = null;
			//var hasError = false;
			//string url = null;

			//int.TryParse(GetSetting("MAX_IMAGE_WIDTH"), out var width);
			//int.TryParse(GetSetting("MAX_IMAGE_HEIGHT"), out var height);

			//var tempDir = FileSystemHelper.TempDirTenant("roxy " + CommonHelper.GenerateRandomInteger().ToString());

			//try
			//{
			//	var notify = Request.Files.Count < 4;

			//	// copy uploaded files to temp folder and resize them
			//	for (var i = 0; i < Request.Files.Count; ++i)
			//	{
			//		var file = Request.Files[i];
			//		var extension = Path.GetExtension(file.FileName);

			//		if (IsAllowedFileType(extension))
			//		{
			//			var dest = Path.Combine(tempDir, file.FileName);
			//			file.SaveAs(dest);

			//			if (GetFileContentType(extension) == "image" && extension != ".svg")
			//			{
			//				ImageResize(dest, dest, width, height, notify);
			//			}
			//		}
			//		else
			//		{
			//			message = LangRes("E_UploadNotAll");
			//		}
			//	}

			//	// Copy files to file storage
			//	foreach (var tempPath in Directory.EnumerateFiles(tempDir, "*", SearchOption.TopDirectoryOnly))
			//	{
			//		using (var stream = new FileStream(tempPath, FileMode.Open, FileAccess.Read))
			//		{
			//			var name = Path.GetFileName(tempPath);
			//			var newPath = _fileSystem.Combine(path, name);
			//			if (_fileSystem.CheckFileUniqueness(newPath, out var file))
			//			{
			//				newPath = file.Path;
			//			}

			//			await _fileSystem.SaveStreamAsync(newPath, stream);
			//			url = _fileSystem.GetPublicUrl(newPath);
			//		}
			//	}
			//}
			//catch (Exception ex)
			//{
			//	hasError = true;
			//	message = ex.Message;
			//}
			//finally
			//{
			//	FileSystemHelper.ClearDirectory(tempDir, true);
			//}

			//if (IsAjaxUpload())
			//{
			//	if (external)
			//	{
			//		var result = new
			//		{
			//			Success = !hasError,
			//			Url = url,
			//			Message = message
			//		};
			//		Response.ContentType = "text/json";
			//		Response.Write(JsonConvert.SerializeObject(result));
			//	}
			//	else
			//	{
			//		if (message.HasValue())
			//		{
			//			Response.Write(GetResultString(message, hasError ? "error" : "ok"));
			//		}
			//	}
			//}
			//else
			//{
			//	Response.Write("<script>");
			//	Response.Write("parent.fileUploaded(" + GetResultString(message, hasError ? "error" : "ok") + ");");
			//	Response.Write("</script>");
			//}
		}

		#endregion

		public async Task ProcessRequest(string a = null, string d = null)
		{
			if (!Services.Permissions.Authorize(Permissions.Media.Upload))
			{
				Response.Write(T("Admin.AccessDenied.Description"));
				return;
			}

			var action = Request["a"] ?? a ?? "DIRLIST";

			try
			{
				if (a == null && Request["a"] != null)
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
					case "UPLOAD":
						await UploadAsync(Request["d"] ?? d, Request["ext"].ToBool());
						break;
					default:
						Response.Write(GetResultString("This action is not implemented.", "error"));
						break;
				}
			}
			catch (Exception ex)
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
					Response.Write(GetResultString(ex.Message, "error"));
				}

				Logger.ErrorsAll(ex);
			}
		}
	}
}