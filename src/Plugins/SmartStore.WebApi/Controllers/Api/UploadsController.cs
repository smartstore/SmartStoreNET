using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.IO;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Services.DataExchange.Import;
using SmartStore.Services.Media;
using SmartStore.Services.Stores;
using SmartStore.Services.Tasks;
using SmartStore.Utilities;
using SmartStore.Utilities.Threading;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;
using SmartStore.WebApi.Models.Api;

namespace SmartStore.WebApi.Controllers.Api
{
    /// <see cref="http://www.asp.net/web-api/overview/advanced/sending-html-form-data,-part-2"/>
    public class UploadsController : ApiController
    {
        private static readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

        private readonly IDbContext _dbContext;
        private readonly Lazy<IProductService> _productService;
        private readonly Lazy<IMediaService> _mediaService;
        private readonly Lazy<IFolderService> _folderService;
        private readonly Lazy<IImportProfileService> _importProfileService;
        private readonly Lazy<IStoreService> _storeService;
        private readonly Lazy<IPermissionService> _permissionService;
        private readonly Lazy<ITaskScheduler> _taskScheduler;
        private readonly Lazy<IWorkContext> _workContext;
        private readonly Lazy<IStoreContext> _storeContext;
        private readonly Lazy<MediaSettings> _mediaSettings;

        public UploadsController(
            IDbContext dbContext,
            Lazy<IProductService> productService,
            Lazy<IMediaService> mediaService,
            Lazy<IFolderService> folderService,
            Lazy<IImportProfileService> importProfileService,
            Lazy<IStoreService> storeService,
            Lazy<IPermissionService> permissionService,
            Lazy<ITaskScheduler> taskScheduler,
            Lazy<IWorkContext> workContext,
            Lazy<IStoreContext> storeContext,
            Lazy<MediaSettings> mediaSettings)
        {
            _dbContext = dbContext;
            _productService = productService;
            _mediaService = mediaService;
            _folderService = folderService;
            _importProfileService = importProfileService;
            _storeService = storeService;
            _permissionService = permissionService;
            _taskScheduler = taskScheduler;
            _workContext = workContext;
            _storeContext = storeContext;
            _mediaSettings = mediaSettings;
        }

        [HttpPost, WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditPicture)]
        [WebApiQueryable]
        public async Task<IHttpActionResult> ProductImages()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return StatusCode(HttpStatusCode.UnsupportedMediaType);
            }

            Product entity = null;
            string identifier = null;
            var identifiers = new[] { "Id", "Sku", "Gtin", "Mpn" };
            var result = new List<UploadImage>();
            var provider = new MultipartMemoryStreamProvider();

            try
            {
                await Request.Content.ReadAsMultipartAsync(provider);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            // Find product entity.
            foreach (var content in provider.Contents)
            {
                if (!content.IsFileContent())
                {
                    var p = content.Headers?.ContentDisposition?.Parameters;
                    var hv = p.FirstOrDefault(x => identifiers.Contains(x.Value.ToUnquoted()));

                    if (hv != null)
                    {
                        identifier = await content.ReadAsStringAsync();
                        switch (hv.Value.ToUnquoted())
                        {
                            case "Id":
                                entity = _productService.Value.GetProductById(identifier.ToInt());
                                break;
                            case "Sku":
                                entity = _productService.Value.GetProductBySku(identifier);
                                break;
                            case "Gtin":
                                entity = _productService.Value.GetProductByGtin(identifier);
                                break;
                            case "Mpn":
                                entity = _productService.Value.GetProductByManufacturerPartNumber(identifier);
                                break;
                        }
                    }
                }
                if (entity != null)
                {
                    break;
                }
            }

            if (entity == null)
            {
                throw Request.NotFoundException(WebApiGlobal.Error.EntityNotFound.FormatInvariant(identifier.NaIfEmpty()));
            }

            // Process files.
            await this.ProcessEntityAsync(async () =>
            {
                var storeUrl = _storeService.Value.GetHost(_storeContext.Value.CurrentStore);
                var displayOrder = entity.ProductPictures.Any()
                    ? entity.ProductPictures.Max(x => x.DisplayOrder)
                    : 0;

                var files = entity.ProductPictures.Select(x => x.MediaFile);
                var catalogAlbumId = _folderService.Value.GetNodeByPath(SystemAlbumProvider.Catalog).Value.Id;

                foreach (var content in provider.Contents)
                {
                    if (content.IsFileContent())
                    {
                        var image = new UploadImage(content.Headers);
                        if (image.FileName.IsEmpty())
                        {
                            image.FileName = Path.GetRandomFileName();
                        }

                        using (var stream = await content.ReadAsStreamAsync())
                        {
                            if (image.PictureId != 0 && (image.Picture = files.FirstOrDefault(x => x.Id == image.PictureId)) != null)
                            {
                                image.Exists = true;

                                var fileInfo = _mediaService.Value.ConvertMediaFile(image.Picture);
                                var path = fileInfo.Path;
                                var existingFile = await _mediaService.Value.SaveFileAsync(path, stream, false, DuplicateFileHandling.Overwrite);

                                if (existingFile == null || existingFile.Id != image.Picture.Id)
                                {
                                    throw Request.InternalServerErrorException(new Exception($"Failed to update existing product image: id {image.Picture.Id}, path '{path.NaIfEmpty()}'."));
                                }
                            }
                            else
                            {
                                // Important notes to avoid image duplicates:
                                // a) if TinyImage compresses the image, FindEqualFile will never find an equal image here.
                                // We have to live with that. There is no ad-hoc solution for it.
                                // b) MediaSettings.MaximumImageSize (default is 2048 pixel) must be large enough to prevent the image processor from resizing it.

                                MediaFile sourceFile = null;

                                if (_mediaService.Value.FindEqualFile(stream, files, true, out var assignedFile))
                                {
                                    image.Exists = true;
                                    image.Picture = assignedFile;
                                }
                                else if (_mediaService.Value.FindEqualFile(stream, image.FileName, catalogAlbumId, true, out sourceFile))
                                {
                                    image.Exists = true;
                                    image.Picture = sourceFile;
                                }
                                else
                                {
                                    var path = _mediaService.Value.CombinePaths(SystemAlbumProvider.Catalog, image.FileName);
                                    var newFile = await _mediaService.Value.SaveFileAsync(path, stream, false, DuplicateFileHandling.Rename);
                                    sourceFile = newFile?.File;

                                    image.Inserted = sourceFile != null;
                                    image.Picture = sourceFile;
                                }

                                if (sourceFile?.Id > 0)
                                {
                                    _productService.Value.InsertProductPicture(new ProductMediaFile
                                    {
                                        MediaFileId = sourceFile.Id,
                                        ProductId = entity.Id,
                                        DisplayOrder = ++displayOrder
                                    });
                                }
                            }

                            if (image.Picture != null)
                            {
                                // Prevent "failed to serialize the response body".
                                _dbContext.LoadCollection(image.Picture, (MediaFile x) => x.ProductMediaFiles);

                                image.PictureId = image.Picture.Id;
                                image.ImageUrl = _mediaService.Value.GetUrl(image.Picture, _mediaSettings.Value.ProductDetailsPictureSize, storeUrl, false);
                                image.ThumbImageUrl = _mediaService.Value.GetUrl(image.Picture, _mediaSettings.Value.ProductThumbPictureSize, storeUrl, false);
                                image.FullSizeImageUrl = _mediaService.Value.GetUrl(image.Picture, 0, storeUrl, false);
                            }
                        }

                        result.Add(image);
                    }
                }
            });

            return Ok(result.AsQueryable());
        }

        [HttpPost, WebApiAuthenticate(Permission = Permissions.Configuration.Import.Execute)]
        [WebApiQueryable]
        public async Task<IHttpActionResult> ImportFiles()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return StatusCode(HttpStatusCode.UnsupportedMediaType);
            }

            ImportProfile profile = null;
            string identifier = null;
            var tempDir = FileSystemHelper.TempDirTenant(Guid.NewGuid().ToString());
            var provider = new MultipartFormDataStreamProvider(tempDir);

            try
            {
                await Request.Content.ReadAsMultipartAsync(provider);
            }
            catch (Exception ex)
            {
                FileSystemHelper.ClearDirectory(tempDir, true);
                return InternalServerError(ex);
            }

            // Find import profile.
            if (provider.FormData.AllKeys.Contains("Id"))
            {
                identifier = provider.FormData.GetValues("Id").FirstOrDefault();
                profile = _importProfileService.Value.GetImportProfileById(identifier.ToInt());
            }
            else if (provider.FormData.AllKeys.Contains("Name"))
            {
                identifier = provider.FormData.GetValues("Name").FirstOrDefault();
                profile = _importProfileService.Value.GetImportProfileByName(identifier);
            }

            if (profile == null)
            {
                FileSystemHelper.ClearDirectory(tempDir, true);
                throw Request.NotFoundException(WebApiGlobal.Error.EntityNotFound.FormatInvariant(identifier.NaIfEmpty()));
            }

            var startImport = false;
            var deleteExisting = false;
            var result = new List<UploadImportFile>();
            var unzippedFiles = new List<MultipartFileData>();
            var importFolder = profile.GetImportFolder(true, true);
            var csvTypes = new string[] { ".csv", ".txt", ".tab" };

            if (provider.FormData.AllKeys.Contains("deleteExisting"))
            {
                var strDeleteExisting = provider.FormData.GetValues("deleteExisting").FirstOrDefault();
                deleteExisting = strDeleteExisting.HasValue() && strDeleteExisting.ToBool();
            }

            if (provider.FormData.AllKeys.Contains("startImport"))
            {
                var strStartImport = provider.FormData.GetValues("startImport").FirstOrDefault();
                startImport = strStartImport.HasValue() && strStartImport.ToBool();
            }

            // Unzip files.
            foreach (var file in provider.FileData)
            {
                var import = new UploadImportFile(file.Headers);

                if (import.FileExtension.IsCaseInsensitiveEqual(".zip"))
                {
                    var subDir = Path.Combine(tempDir, Guid.NewGuid().ToString());
                    ZipFile.ExtractToDirectory(file.LocalFileName, subDir);
                    FileSystemHelper.DeleteFile(file.LocalFileName);

                    foreach (var unzippedFile in Directory.GetFiles(subDir, "*.*"))
                    {
                        var content = CloneHeaderContent(unzippedFile, file);
                        unzippedFiles.Add(new MultipartFileData(content.Headers, unzippedFile));
                    }
                }
                else
                {
                    unzippedFiles.Add(new MultipartFileData(file.Headers, file.LocalFileName));
                }
            }

            // Copy files to import folder.
            if (unzippedFiles.Any())
            {
                using (_rwLock.GetWriteLock())
                {
                    if (deleteExisting)
                    {
                        FileSystemHelper.ClearDirectory(importFolder, false);
                    }

                    foreach (var file in unzippedFiles)
                    {
                        var import = new UploadImportFile(file.Headers);
                        var destPath = Path.Combine(importFolder, import.FileName);

                        import.Exists = File.Exists(destPath);

                        switch (profile.FileType)
                        {
                            case ImportFileType.XLSX:
                                import.IsSupportedByProfile = import.FileExtension.IsCaseInsensitiveEqual(".xlsx");
                                break;
                            case ImportFileType.CSV:
                                import.IsSupportedByProfile = csvTypes.Contains(import.FileExtension, StringComparer.OrdinalIgnoreCase);
                                break;
                        }

                        import.Inserted = FileSystemHelper.CopyFile(file.LocalFileName, destPath);

                        result.Add(import);
                    }
                }
            }

            FileSystemHelper.ClearDirectory(tempDir, true);

            if (startImport)
            {
                var customer = _workContext.Value.CurrentCustomer;

                if (_permissionService.Value.Authorize(Permissions.System.ScheduleTask.Execute, customer))
                {
                    _taskScheduler.Value.RunSingleTask(profile.SchedulingTaskId, new Dictionary<string, string>
                    {
                        { TaskExecutor.CurrentCustomerIdParamName, customer.Id.ToString() },
                        { TaskExecutor.CurrentStoreIdParamName, _storeContext.Value.CurrentStore.Id.ToString() }
                    });
                }
            }

            return Ok(result.AsQueryable());
        }

        #region Utilities

        private StringContent CloneHeaderContent(string path, MultipartFileData origin)
        {
            var content = new StringContent(path);

            ContentDispositionHeaderValue disposition;
            ContentDispositionHeaderValue.TryParse(origin.Headers.ContentDisposition.ToString(), out disposition);

            content.Headers.ContentDisposition = disposition;

            content.Headers.ContentDisposition.Name = origin.Headers.ContentDisposition.Name.ToUnquoted();
            content.Headers.ContentDisposition.FileName = Path.GetFileName(path);

            content.Headers.ContentType.MediaType = MimeTypes.MapNameToMimeType(path);

            return content;
        }

        #endregion
    }
}