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
using SmartStore.Services;
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

		private Dictionary<string, string> _lang = null;
		private Dictionary<string, string> _settings = null;
		private string _fileRoot = null;

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
		}

		#region Old Utilities

		private string FixPath(string path)
		{
			if (path == null)
			{
				path = "";
			}

			if (!path.StartsWith("~"))
			{
				if (!path.StartsWith("/"))
					path = "/" + path;
				path = "~" + path;
			}

			//var filesRoot = GetSetting("FILES_ROOT");
			//if (!path.ToLowerInvariant().Contains(filesRoot.ToLowerInvariant()))
			//{
			//	path = filesRoot;
			//}

			return _context.Server.MapPath(path);
		}

		protected bool CanHandleFile(string filename)
		{
			bool ret = false;
			FileInfo file = new FileInfo(filename);
			string ext = file.Extension.Replace(".", "").ToLower();
			string setting = GetSetting("FORBIDDEN_UPLOADS").Trim().ToLower();
			if (setting != "")
			{
				ArrayList tmp = new ArrayList();
				tmp.AddRange(Regex.Split(setting, "\\s+"));
				if (!tmp.Contains(ext))
					ret = true;
			}
			setting = GetSetting("ALLOWED_UPLOADS").Trim().ToLower();
			if (setting != "")
			{
				ArrayList tmp = new ArrayList();
				tmp.AddRange(Regex.Split(setting, "\\s+"));
				if (!tmp.Contains(ext))
					ret = false;
			}

			return ret;
		}

		protected string GetFilesRoot()
		{
			string ret = GetSetting("FILES_ROOT");
			if (GetSetting("SESSION_PATH_KEY") != "" && _context.Session[GetSetting("SESSION_PATH_KEY")] != null)
				ret = (string)_context.Session[GetSetting("SESSION_PATH_KEY")];

			if (ret == "")
				ret = _context.Server.MapPath("~/Media/Uploaded");  // ../Uploads
			else
				ret = FixPath(ret);
			return ret;
		}

		protected void CheckPath(string path)
		{
			if (FixPath(path).IndexOf(GetFilesRoot()) != 0)
			{
				throw new Exception("Access to " + path + " is denied");
			}
		}

		private void _copyDir(string path, string dest)
		{
			if (!Directory.Exists(dest))
				Directory.CreateDirectory(dest);
			foreach (string f in Directory.GetFiles(path))
			{
				FileInfo file = new FileInfo(f);
				if (!System.IO.File.Exists(Path.Combine(dest, file.Name)))
				{
					System.IO.File.Copy(f, Path.Combine(dest, file.Name));
				}
			}
			foreach (string d in Directory.GetDirectories(path))
			{
				DirectoryInfo dir = new DirectoryInfo(d);
				_copyDir(d, Path.Combine(dest, dir.Name));
			}
		}

		protected void CopyDir(string path, string newPath)
		{
			CheckPath(path);
			CheckPath(newPath);
			DirectoryInfo dir = new DirectoryInfo(FixPath(path));
			DirectoryInfo newDir = new DirectoryInfo(FixPath(newPath + "/" + dir.Name));

			if (!dir.Exists)
			{
				throw new Exception(LangRes("E_CopyDirInvalidPath"));
			}
			else if (newDir.Exists)
			{
				throw new Exception(LangRes("E_DirAlreadyExists"));
			}
			else
			{
				_copyDir(dir.FullName, newDir.FullName);
			}
			_response.Write(GetResultString());
		}

		protected void CreateDir(string path, string name)
		{
			CheckPath(path);
			path = FixPath(path);
			if (!Directory.Exists(path))
				throw new Exception(LangRes("E_CreateDirInvalidPath"));
			else
			{
				try
				{
					path = Path.Combine(path, name);
					if (!Directory.Exists(path))
						Directory.CreateDirectory(path);
					_response.Write(GetResultString());
				}
				catch
				{
					throw new Exception(LangRes("E_CreateDirFailed"));
				}
			}
		}

		private ImageFormat GetImageFormat(string filename)
		{
			ImageFormat ret = ImageFormat.Jpeg;
			switch (new FileInfo(filename).Extension.ToLower())
			{
				case ".png":
					ret = ImageFormat.Png;
					break;
				case ".gif":
					ret = ImageFormat.Gif;
					break;
			}
			return ret;
		}

		protected void ImageResize(string path, string dest, int width, int height)
		{
			FileStream fs = new FileStream(path, FileMode.Open);
			Image img = Image.FromStream(fs);
			fs.Close();
			fs.Dispose();
			float ratio = (float)img.Width / (float)img.Height;
			if ((img.Width <= width && img.Height <= height) || (width == 0 && height == 0))
				return;

			int newWidth = width;
			int newHeight = Convert.ToInt16(Math.Floor((float)newWidth / ratio));
			if ((height > 0 && newHeight > height) || (width == 0))
			{
				newHeight = height;
				newWidth = Convert.ToInt16(Math.Floor((float)newHeight * ratio));
			}
			Bitmap newImg = new Bitmap(newWidth, newHeight);
			Graphics g = Graphics.FromImage((Image)newImg);
			g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
			g.DrawImage(img, 0, 0, newWidth, newHeight);
			img.Dispose();
			g.Dispose();
			if (dest != "")
			{
				newImg.Save(dest, GetImageFormat(dest));
			}
			newImg.Dispose();
		}

		protected void Upload(string path)
		{
			CheckPath(path);
			path = FixPath(path);
			string res = GetResultString();
			try
			{
				for (int i = 0; i < Request.Files.Count; i++)
				{
					if (CanHandleFile(Request.Files[i].FileName))
					{
						string filename = GetUniqueFileName(path, Request.Files[i].FileName);
						string dest = Path.Combine(path, filename);
						Request.Files[i].SaveAs(dest);
						if (GetFileType(new FileInfo(filename).Extension) == "image")
						{
							int w = 0;
							int h = 0;
							int.TryParse(GetSetting("MAX_IMAGE_WIDTH"), out w);
							int.TryParse(GetSetting("MAX_IMAGE_HEIGHT"), out h);
							ImageResize(dest, dest, w, h);
						}
					}
					else
					{
						res = GetResultString(LangRes("E_UploadNotAll"));
					}
				}
			}
			catch (Exception exception)
			{
				res = GetResultString(exception.Message, "error");
			}
			_response.Write("<script>");
			_response.Write("parent.fileUploaded(" + res + ");");
			_response.Write("</script>");
		}

		#endregion

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

				var filename = _context.Server.MapPath(LANGUAGE_FILE.FormatInvariant(lang));

				if (!System.IO.File.Exists(filename))
				{
					filename = _context.Server.MapPath(LANGUAGE_FILE.FormatInvariant("en"));
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
				_settings = ParseJson(_context.Server.MapPath(CONFIG_FILE));

			if (_settings.ContainsKey(name))
				result = _settings[name];

			return result;
		}

		private string GetFileType(string extension)
		{
			extension = extension.EmptyNull().ToLower();

			if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".gif")
				return "image";

			if (extension == ".swf" || extension == ".flv")
				return "flash";

			return "file";
		}

		private void GetImageSize(IFile file, out int width, out int height)
		{
			width = height = 0;

			try
			{
				if (GetFileType(file.FileType).IsCaseInsensitiveEqual("image"))
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

		private string GetUniqueFileName(string dir, string filename)
		{
			var result = filename;
			var copy = T("Admin.Common.Copy");
			string name = null;
			string extension = null;

			for (var i = 1; i < 999999; ++i)
			{
				var path = _fileSystem.Combine(dir, result);

				if (!_fileSystem.FileExists(path))
				{
					return result;
				}

				if (name == null || extension == null)
				{
					var file = _fileSystem.GetFile(path);

					extension = file.FileType;
					name = file.Name.EmptyNull().Substring(0, file.Name.EmptyNull().Length - extension.Length);
				}				

				result = "{0} - {1} {2}{3}".FormatInvariant(name, copy, i, extension);
			}

			return result;
		}

		private List<IFile> GetFiles2(string path, string type)
		{
			var files = _fileSystem.ListFiles(path);

			if (type.IsEmpty() || type == "#")
				return files.ToList();

			return _fileSystem.ListFiles(path)
				.Where(x => GetFileType(x.FileType).IsCaseInsensitiveEqual(type))
				.ToList();
		}

		private List<RoxyFolder> ListDirs2(string path)
		{
			var result = new List<RoxyFolder>();

			_fileSystem.ListFolders(path).Each(x =>
			{
				var subFolders = ListDirs2(x.Path);

				result.Add(new RoxyFolder
				{
					Folder = x,
					SubFolders = subFolders.Count
				});

				result.AddRange(subFolders);
			});

			return result;
		}

		private void ListDirTree2(string type)
		{
			var isFirstItem = true;
			var folders = ListDirs2(FileRoot);

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

				var fileCount = GetFiles2(folder.Folder.Path, type).Count;

				_response.Write(
					"{\"p\":\"/" + folder.Folder.Path.Replace(FileRoot, "").Replace("\\", "/")
					+ "\",\"f\":\"" + fileCount.ToString()
					+ "\",\"d\":\"" + folder.SubFolders.ToString()
					+ "\"}"
				);
			}

			_response.Write("]");
		}

		private void ListFiles2(string path, string type)
		{
			var isFirstItem = true;
			var width = 0;
			var height = 0;
			var files = GetFiles2(GetRelativePath(path), type);

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

		private void DownloadFile2(string path)
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

		private void DownloadDir2(string path)
		{
			path = GetRelativePath(path);
			if (!_fileSystem.FolderExists(path))
			{
				throw new Exception(LangRes("E_CreateArchive"));
			}

			var folder = _fileSystem.GetFolder(path);

			// copy files to temp folder
			var tempDir = FileSystemHelper.TempDir("roxy " + folder.Name);	
			FileSystemHelper.ClearDirectory(tempDir, false);
			var files = GetFiles2(path, null);

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

		private void ShowThumbnail2(string path, int width, int height)
		{
			path = GetRelativePath(path);
			if (!_fileSystem.FileExists(path))
				return;

			var file = _fileSystem.GetFile(path);
			var stream = file.OpenRead();
			var img = new Bitmap(Image.FromStream(stream));

			stream.Close();
			stream.Dispose();

			int cropWidth = img.Width, cropHeight = img.Height;
			int cropX = 0, cropY = 0;
			double imgRatio = (double)img.Width / (double)img.Height;

			if (height == 0)
				height = Convert.ToInt32(Math.Floor((double)width / imgRatio));

			if (width > img.Width)
				width = img.Width;
			if (height > img.Height)
				height = img.Height;

			double cropRatio = (double)width / (double)height;

			cropWidth = Convert.ToInt32(Math.Floor((double)img.Height * cropRatio));
			cropHeight = Convert.ToInt32(Math.Floor((double)cropWidth / cropRatio));
			if (cropWidth > img.Width)
			{
				cropWidth = img.Width;
				cropHeight = Convert.ToInt32(Math.Floor((double)cropWidth / cropRatio));
			}
			if (cropHeight > img.Height)
			{
				cropHeight = img.Height;
				cropWidth = Convert.ToInt32(Math.Floor((double)cropHeight * cropRatio));
			}
			if (cropWidth < img.Width)
			{
				cropX = Convert.ToInt32(Math.Floor((double)(img.Width - cropWidth) / 2));
			}
			if (cropHeight < img.Height)
			{
				cropY = Convert.ToInt32(Math.Floor((double)(img.Height - cropHeight) / 2));
			}

			var area = new Rectangle(cropX, cropY, cropWidth, cropHeight);
			var cropImg = img.Clone(area, PixelFormat.DontCare);
			img.Dispose();
			var imgCallback = new Image.GetThumbnailImageAbort(() => false);

			_response.AddHeader("Content-Type", "image/png");
			cropImg
				.GetThumbnailImage(width, height, imgCallback, IntPtr.Zero)
				.Save(_response.OutputStream, ImageFormat.Png);

			_response.OutputStream.Close();
			cropImg.Dispose();
		}

		private bool CanHandleFileType(string str)
		{
			var result = true;
			var extension = Path.GetExtension(str.EmptyNull()).Replace(".", "").ToLower();

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

		private void RenameFile2(string path, string name)
		{
			path = GetRelativePath(path);
			if (!_fileSystem.FileExists(path))
			{
				throw new Exception(LangRes("E_RenameFileInvalidPath"));
			}

			if (!CanHandleFileType(name))
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

		private void RenameDir2(string path, string name)
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

		private void MoveFile2(string path, string newPath)
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

		private void MoveDir2(string path, string newPath)
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

		private void CopyFile2(string path, string newPath)
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

		private void DeleteFile2(string path)
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

		private void DeleteDir2(string path)
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
						ListDirTree2(_context.Request["type"]);
						break;
					case "FILESLIST":
						ListFiles2(_context.Request["d"], _context.Request["type"]);
						break;
					case "COPYDIR":
						CopyDir(_context.Request["d"], _context.Request["n"]);
						break;
					case "COPYFILE":
						CopyFile2(_context.Request["f"], _context.Request["n"]);
						break;
					case "CREATEDIR":
						CreateDir(_context.Request["d"], _context.Request["n"]);
						break;
					case "DELETEDIR":
						DeleteDir2(_context.Request["d"]);
						break;
					case "DELETEFILE":
						DeleteFile2(_context.Request["f"]);
						break;
					case "DOWNLOAD":
						DownloadFile2(_context.Request["f"]);
						break;
					case "DOWNLOADDIR":
						DownloadDir2(_context.Request["d"]);
						break;
					case "MOVEDIR":
						MoveDir2(_context.Request["d"], _context.Request["n"]);
						break;
					case "MOVEFILE":
						MoveFile2(_context.Request["f"], _context.Request["n"]);
						break;
					case "RENAMEDIR":
						RenameDir2(_context.Request["d"], _context.Request["n"]);
						break;
					case "RENAMEFILE":
						RenameFile2(_context.Request["f"], _context.Request["n"]);
						break;
					case "GENERATETHUMB":
						int w = 140, h = 0;
						int.TryParse(_context.Request["width"].Replace("px", ""), out w);
						int.TryParse(_context.Request["height"].Replace("px", ""), out h);
						ShowThumbnail2(_context.Request["f"], w, h);
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
			}
		}
	}
}