using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using SmartStore.Core.IO;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Security;
using SmartStore.Services.Media;
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

        private string _fileRoot = null;
        private Dictionary<string, string> _lang = null;
        private Dictionary<string, string> _roxySettings = null;

        private readonly IMediaTypeResolver _mediaTypeResolver;
        private readonly MediaHelper _mediaHelper;
        private readonly IMediaFileSystem _fileSystem;
        private readonly ILocalizationFileResolver _locFileResolver;

        private readonly AlbumInfo _album;

        public RoxyFileManagerController(
            IMediaService mediaService,
            IFolderService folderService,
            IAlbumRegistry albumRegistry,
            IMediaTypeResolver mediaTypeResolver,
            MediaHelper mediaHelper,
            MediaExceptionFactory exceptionFactory,
            //IMediaFileSystem fileSystem,
            ILocalizationFileResolver locFileResolver)
        {
            _mediaTypeResolver = mediaTypeResolver;
            _mediaHelper = mediaHelper;
            _locFileResolver = locFileResolver;

            _album = albumRegistry.GetAlbumByName(SystemAlbumProvider.Files);
            _fileSystem = new MediaServiceFileSystemAdapter(mediaService, folderService, _mediaHelper, exceptionFactory);
            _fileRoot = _album.Name;

            //_fileSystem = fileSystem;
            //_fileRoot = "Uploaded";
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
            if (_roxySettings == null)
            {
                _roxySettings = ParseJson(CommonHelper.MapPath(CONFIG_FILE));
            }

            if (_roxySettings.TryGetValue(name, out var result))
            {
                return result;
            }

            return result.EmptyNull();
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

        private string GetResultString(string message = null, string type = "ok")
        {
            var result = new
            {
                res = type,
                msg = message.EmptyNull().Replace("\"", "\\\"")
            };

            return JsonConvert.SerializeObject(result);
            //return "{\"res\":\"" + type + "\",\"msg\":\"" + message.EmptyNull().Replace("\"", "\\\"") + "\"}";
        }

        private bool IsAjaxUpload()
        {
            return Request.IsAjaxRequest() || (Request["method"] != null && Request["method"].ToString() == "ajax");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetMimeType(IFile file)
        {
            return (file as MediaFileInfo)?.MimeType ?? MimeTypes.MapNameToMimeType(file.Name);
        }

        internal class RoxyFolder
        {
            public IFolder Folder { get; set; }
            public string DisplayName { get; set; }
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

                    var sessionPathKey = GetSetting("SESSION_PATH_KEY");

                    if (sessionPathKey.HasValue() && Session[sessionPathKey] != null)
                        _fileRoot = (string)Session[GetSetting("SESSION_PATH_KEY")];

                    if (_fileRoot.IsEmpty())
                        _fileRoot = _album.Name;
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

            return (_fileSystem.GetStoragePath(path) ?? path).TrimStart('/', '\\');
        }

        private IEnumerable<IFile> GetFiles(string path, string type)
        {
            var files = _fileSystem.ListFiles(GetRelativePath(path));

            if (type.IsEmpty() || type == "#")
            {
                return files;
            }

            type = type.ToLowerInvariant();
            bool predicate(IFile x)
            {
                return _mediaTypeResolver.Resolve(x.Extension) == type;
            }

            return files.Where(predicate);
        }

        private long CountFiles(string path, string type)
        {
            if (_fileSystem.IsCloudStorage)
            {
                // Dont't count, it's expensive!
                return 0;
            }

            Func<string, bool> predicate = null;

            if (type.HasValue() && type != "#")
            {
                type = type.ToLowerInvariant();
                predicate = x => _mediaTypeResolver.Resolve(Path.GetExtension(x)) == type;
            }

            return _fileSystem.CountFiles(path, "*", predicate, false);
        }

        private void ListDirTree(string type)
        {
            var root = _fileSystem.GetFolder(FileRoot);
            var folders = ListDirs(FileRoot);
            var rootName = FileRoot;

            if (root is MediaFolderInfo fi && fi.Node.Value.ResKey.HasValue())
            {
                rootName = T(fi.Node.Value.ResKey);
            }

            folders.Insert(0, new RoxyFolder
            {
                Folder = root,
                DisplayName = rootName,
                SubFolders = folders.Count
            });

            var result = folders
                .Select(x =>
                {
                    var numFiles = CountFiles(x.Folder.Path, type);
                    return new
                    {
                        p = x.Folder.Path.Replace('\\', '/'),
                        n = x.DisplayName,
                        f = numFiles,
                        d = x.SubFolders
                    };
                })
                .ToArray();

            Write(result);
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

        private void ListFiles(string path, string type)
        {
            var files = GetFiles(GetRelativePath(path), type);

            var result = files.Select(x => new
            {
                p = _fileSystem.GetPublicUrl(x),
                t = x.LastUpdated.ToUnixTime().ToString(),
                m = GetMimeType(x),
                s = x.Size.ToString(),
                w = x.Dimensions.Width.ToString(),
                h = x.Dimensions.Height.ToString()
            }).ToArray();

            Write(result);
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
                    Response.ContentType = GetMimeType(file);

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

            var folder = _fileSystem.GetFolder(path);
            if (!folder.Exists)
            {
                throw new DirectoryNotFoundException($"Directory '{path}' does not exist.");
            }


            var files = GetFiles(path, null).DistinctBy(x => x.Name).ToList();

            // Create zip file
            var zipFilePath = Path.Combine(FileSystemHelper.TempDirTenant(), folder.Name + "-media.zip");
            FileSystemHelper.DeleteFile(zipFilePath);

            using (var zipFile = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            {
                foreach (var file in files)
                {
                    using (var fileStream = file.OpenRead())
                    {
                        var entry = zipFile.CreateEntry(file.Name, CompressionLevel.Fastest);

                        DateTime lastWriteTime = file.LastUpdated;
                        if ((lastWriteTime.Year < 1980) || (lastWriteTime.Year > 2107))
                        {
                            lastWriteTime = new DateTime(1980, 1, 1, 0, 0, 0);
                        }
                        entry.LastWriteTime = lastWriteTime;

                        using (var entryStream = entry.Open())
                        {
                            fileStream.CopyTo(entryStream);
                        }
                    }
                }
            }

            Response.Clear();
            Response.Headers.Add("Content-Disposition", "attachment; filename=\"" + folder.Name + "-media.zip\"");
            Response.ContentType = "application/zip";
            Response.TransmitFile(zipFilePath);
            Response.Flush();

            FileSystemHelper.DeleteFile(zipFilePath);

            Response.End();
        }

        private void RenameDir(string path, string name)
        {
            try
            {
                path = GetRelativePath(path);
                var newPath = _fileSystem.Combine(Path.GetDirectoryName(path), name);
                _fileSystem.RenameFolder(path, newPath);
                Response.Write(GetResultString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void MoveFile(string path, string newPath)
        {
            try
            {
                _fileSystem.RenameFile(GetRelativePath(path), GetRelativePath(newPath));
                Response.Write(GetResultString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void RenameFile(string path, string newName)
        {
            path = GetRelativePath(path);

            var fileType = Path.GetExtension(newName);
            if (!IsAllowedFileType(fileType))
            {
                throw new Exception(LangRes("E_FileExtensionForbidden"));
            }

            try
            {
                var folder = _fileSystem.GetFolderForFile(path);
                var newPath = _fileSystem.Combine(folder.Path, newName);

                _fileSystem.RenameFile(path, newPath);
                Response.Write(GetResultString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void MoveDir(string path, string newPath)
        {
            try
            {
                newPath = _fileSystem.Combine(GetRelativePath(newPath), Path.GetFileName(path));
                _fileSystem.RenameFolder(GetRelativePath(path), newPath);
                Response.Write(GetResultString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void CopyFile(string path, string newPath)
        {
            path = GetRelativePath(path);

            var file = _fileSystem.GetFile(path);
            if (!file.Exists)
            {
                throw new FileNotFoundException($"File {path} does not exist.", path);
            }

            try
            {
                newPath = _fileSystem.Combine(GetRelativePath(newPath), file.Name);

                if (_fileSystem.CheckUniqueFileName(newPath, out var uniquePath))
                {
                    newPath = uniquePath;
                }

                _fileSystem.CopyFile(path, newPath, false);
                Response.Write(GetResultString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void DeleteFile(string path)
        {
            path = GetRelativePath(path);
            var file = _fileSystem.GetFile(path);
            if (!file.Exists)
            {
                throw new FileNotFoundException($"File {path} does not exist.", path);
            }

            try
            {
                _fileSystem.DeleteFile(path);
                Response.Write(GetResultString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void DeleteDir(string path)
        {
            path = GetRelativePath(path);

            if (path == FileRoot)
            {
                throw new Exception(LangRes("E_CannotDeleteRoot"));
            }

            try
            {
                _fileSystem.DeleteFolder(path);
                Response.Write(GetResultString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void CreateDir(string path, string name)
        {
            path = GetRelativePath(path);

            try
            {
                path = _fileSystem.Combine(path, name);
                _fileSystem.CreateFolder(path);
                Response.Write(GetResultString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void CopyDir(string path, string targetPath)
        {
            path = GetRelativePath(path);
            targetPath = GetRelativePath(targetPath);

            try
            {
                _fileSystem.CopyFolder(path, targetPath, false);
                Response.Write(GetResultString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task UploadAsync(string destinationPath, bool external = false)
        {
            destinationPath = GetRelativePath(destinationPath);

            string message = null;
            var hasError = false;

            try
            {
                // Copy uploaded files to file storage
                for (var i = 0; i < Request.Files.Count; ++i)
                {
                    var uploadedFile = Request.Files[i];
                    var extension = Path.GetExtension(uploadedFile.FileName);

                    if (IsAllowedFileType(extension))
                    {
                        var path = _fileSystem.Combine(destinationPath, uploadedFile.FileName);
                        if (_fileSystem.CheckUniqueFileName(path, out var uniquePath))
                        {
                            path = uniquePath;
                        }

                        await _fileSystem.SaveStreamAsync(path, uploadedFile.InputStream);
                    }
                    else
                    {
                        message = LangRes("E_UploadNotAll");
                    }
                }
            }
            catch (Exception ex)
            {
                hasError = true;
                message = ex.Message;
            }

            if (IsAjaxUpload())
            {
                if (external)
                {
                    var result = new
                    {
                        Success = !hasError,
                        Message = message
                    };
                    Response.ContentType = "text/json";
                    Response.Write(JsonConvert.SerializeObject(result));
                }
                else
                {
                    if (message.HasValue())
                    {
                        Response.Write(GetResultString(message, hasError ? "error" : "ok"));
                    }
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