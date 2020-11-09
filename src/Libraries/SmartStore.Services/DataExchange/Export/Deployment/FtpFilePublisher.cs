using System;
using System.IO;
using System.Net;
using SmartStore.Core.Domain;
using SmartStore.Core.Logging;

namespace SmartStore.Services.DataExchange.Export.Deployment
{
    public class FtpFilePublisher : IFilePublisher
    {
        private ExportDeploymentContext _context;
        private ExportDeployment _deployment;
        private int _succeededFiles;
        private string _ftpRootUrl;

        private string GetRelativePath(string path)
        {
            var relativePath = path.Substring(_context.FolderContent.Length).Replace("\\", "/");
            if (relativePath.StartsWith("/"))
            {
                return relativePath.Substring(1);
            }

            return relativePath;
        }

        private FtpWebRequest CreateRequest(string url, bool keepAlive = true, long? contentLength = null)
        {
            var request = (FtpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.KeepAlive = keepAlive;
            request.UseBinary = true;
            request.Proxy = null;
            request.UsePassive = _deployment.PassiveMode;
            request.EnableSsl = _deployment.UseSsl;

            if (_deployment.Username.HasValue())
            {
                request.Credentials = new NetworkCredential(_deployment.Username, _deployment.Password);
            }

            if (contentLength.HasValue)
            {
                request.ContentLength = contentLength.Value;
            }

            return request;
        }

        private bool UploadFile(string path, string fileUrl, bool keepAlive = true)
        {
            var succeeded = false;
            var bytesRead = 0;
            var buffLength = 32768;
            byte[] buff = new byte[buffLength];

            var request = CreateRequest(fileUrl, keepAlive, (new FileInfo(path)).Length);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            var requestStream = request.GetRequestStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                while ((bytesRead = stream.Read(buff, 0, buffLength)) != 0)
                {
                    requestStream.Write(buff, 0, bytesRead);
                }
            }

            requestStream.Close();

            using (var response = (FtpWebResponse)request.GetResponse())
            {
                var statusCode = (int)response.StatusCode;
                succeeded = statusCode >= 200 && statusCode <= 299;

                if (succeeded)
                {
                    ++_succeededFiles;
                }
                else
                {
                    _context.Result.LastError = _context.T("Admin.Common.FtpStatus", statusCode, response.StatusCode.ToString());
                    _context.Log.Error("The FTP transfer failed. FTP status {0} ({1}). File {3}".FormatInvariant(statusCode, response.StatusCode.ToString(), path));
                }
            }

            return succeeded;
        }

        private bool DirectoryExists(string directoryUrl)
        {
            var result = false;

            try
            {
                var request = CreateRequest(directoryUrl.EnsureEndsWith("/"));
                request.Method = WebRequestMethods.Ftp.ListDirectory;

                using (request.GetResponse())
                {
                    result = true;
                }
            }
            catch (WebException)
            {
                result = false;
            }

            return result;
        }

        private bool FtpCopyDirectory(DirectoryInfo source)
        {
            var files = source.GetFiles();
            var len = files.Length;

            for (var i = 0; i < len; ++i)
            {
                var path = files[i].FullName;
                var relativePath = GetRelativePath(path);

                UploadFile(path, _ftpRootUrl + relativePath, i != (len - 1));
            }

            foreach (var sourceSubDir in source.GetDirectories())
            {
                var relativePath = GetRelativePath(sourceSubDir.FullName);

                if (!DirectoryExists(_ftpRootUrl + relativePath))
                {
                    var request = CreateRequest(_ftpRootUrl + relativePath, true);
                    request.Method = WebRequestMethods.Ftp.MakeDirectory;
                    using (var response = (FtpWebResponse)request.GetResponse())
                    {
                        response.Close();
                    }
                }

                FtpCopyDirectory(sourceSubDir);
            }

            return true;
        }

        public virtual void Publish(ExportDeploymentContext context, ExportDeployment deployment)
        {
            _context = context;
            _deployment = deployment;
            _succeededFiles = 0;
            _ftpRootUrl = deployment.Url;

            if (!_ftpRootUrl.StartsWith("ftp://", StringComparison.InvariantCultureIgnoreCase))
            {
                _ftpRootUrl = "ftp://" + _ftpRootUrl;
            }

            _ftpRootUrl = _ftpRootUrl.EnsureEndsWith("/");

            if (context.CreateZipArchive)
            {
                if (File.Exists(context.ZipPath))
                {
                    var fileUrl = _ftpRootUrl + Path.GetFileName(context.ZipPath);
                    UploadFile(context.ZipPath, fileUrl, false);
                }
            }
            else
            {
                FtpCopyDirectory(new DirectoryInfo(context.FolderContent));
            }

            context.Log.Info("{0} file(s) successfully uploaded via FTP.".FormatInvariant(_succeededFiles));
        }
    }
}
