using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using SmartStore.Core;
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

        private readonly Lazy<IProductService> _productService;
		private readonly Lazy<IPictureService> _pictureService;
		private readonly Lazy<IImportProfileService> _importProfileService;
        private readonly Lazy<IStoreService> _storeService;
        private readonly Lazy<IPermissionService> _permissionService;
        private readonly Lazy<ITaskScheduler> _taskScheduler;
        private readonly Lazy<IWorkContext> _workContext;
        private readonly Lazy<IStoreContext> _storeContext;
		private readonly Lazy<MediaSettings> _mediaSettings;

		public UploadsController(
			Lazy<IProductService> productService,
			Lazy<IPictureService> pictureService,
			Lazy<IImportProfileService> importProfileService,
            Lazy<IStoreService> storeService,
            Lazy<IPermissionService> permissionService,
            Lazy<ITaskScheduler> taskScheduler,
            Lazy<IWorkContext> workContext,
            Lazy<IStoreContext> storeContext,
			Lazy<MediaSettings> mediaSettings)
		{
			_productService = productService;
			_pictureService = pictureService;
			_importProfileService = importProfileService;
            _storeService = storeService;
            _permissionService = permissionService;
            _taskScheduler = taskScheduler;
            _workContext = workContext;
            _storeContext = storeContext;
			_mediaSettings = mediaSettings;
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

		[HttpPost, WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditPicture)]
		[WebApiQueryable(PagingOptional = true)]
		public async Task<IQueryable<UploadImage>> ProductImages()
		{
			if (!Request.Content.IsMimeMultipartContent())
			{
				throw this.ExceptionUnsupportedMediaType();
			}

			Product entity = null;
			string identifier = null;
			string tempDir = FileSystemHelper.TempDirTenant();
			var provider = new MultipartFormDataStreamProvider(tempDir);

			try
			{
				await Request.Content.ReadAsMultipartAsync(provider);
			}
			catch (Exception ex)
			{
				provider.DeleteLocalFiles();
				throw this.ExceptionInternalServerError(ex);
			}

			// Find product entity.
			if (provider.FormData.AllKeys.Contains("Id"))
			{
				identifier = provider.FormData.GetValues("Id").FirstOrDefault();
				entity = _productService.Value.GetProductById(identifier.ToInt());
			}
			else if (provider.FormData.AllKeys.Contains("Sku"))
			{
				identifier = provider.FormData.GetValues("Sku").FirstOrDefault();
				entity = _productService.Value.GetProductBySku(identifier);
			}
			else if (provider.FormData.AllKeys.Contains("Gtin"))
			{
				identifier = provider.FormData.GetValues("Gtin").FirstOrDefault();
				entity = _productService.Value.GetProductByGtin(identifier);
			}

			if (entity == null)
			{
				provider.DeleteLocalFiles();
				throw this.ExceptionNotFound(WebApiGlobal.Error.EntityNotFound.FormatInvariant(identifier.NaIfEmpty()));
			}

			// Process images.
			var equalPictureId = 0;
			var displayOrder = 0;
			var result = new List<UploadImage>();
			var storeUrl = _storeService.Value.GetHost(_storeContext.Value.CurrentStore);
			var pictures = entity.ProductPictures.Select(x => x.MediaFile);

			if (entity.ProductPictures.Count > 0)
				displayOrder = entity.ProductPictures.Max(x => x.DisplayOrder);

			foreach (var file in provider.FileData)
			{
				var image = new UploadImage(file.Headers);

				if (image.FileName.IsEmpty())
					image.FileName = entity.Name;

				var pictureBinary = File.ReadAllBytes(file.LocalFileName);

				if (pictureBinary != null && pictureBinary.Length > 0)
				{
					if (image.PictureId != 0 && (image.Picture = pictures.FirstOrDefault(x => x.Id == image.PictureId)) != null)
					{
						image.Exists = true;

						var seoName = _pictureService.Value.GetPictureSeName(Path.GetFileNameWithoutExtension(image.FileName));
						_pictureService.Value.UpdatePicture(image.Picture, pictureBinary, image.MediaType, seoName, false);
					}
					else
					{
						pictureBinary = _pictureService.Value.FindEqualPicture(pictureBinary, pictures, out equalPictureId);
						if (pictureBinary != null)
						{
							var seoName = _pictureService.Value.GetPictureSeName(Path.GetFileNameWithoutExtension(image.FileName));
                            var newPicture = _pictureService.Value.InsertPicture(pictureBinary, image.MediaType, seoName, false, false, "product");
                            
							if (newPicture != null)
							{
								_productService.Value.InsertProductPicture(new ProductMediaFile
								{
									MediaFileId = newPicture.Id,
									ProductId = entity.Id,
									DisplayOrder = ++displayOrder
								});

								image.Inserted = true;
								image.Picture = newPicture;
							}
						}
						else
						{
							image.Exists = true;
							image.Picture = pictures.FirstOrDefault(x => x.Id == equalPictureId);
						}
					}

					if (image.Picture != null)
					{
						image.PictureId = image.Picture.Id;
						image.ImageUrl = _pictureService.Value.GetUrl(image.Picture, _mediaSettings.Value.ProductDetailsPictureSize, false, storeUrl);
						image.ThumbImageUrl = _pictureService.Value.GetUrl(image.Picture, _mediaSettings.Value.ProductThumbPictureSize, false, storeUrl);
						image.FullSizeImageUrl = _pictureService.Value.GetUrl(image.Picture, 0, false, storeUrl);
					}
				}

				result.Add(image);
			}

			provider.DeleteLocalFiles();
			return result.AsQueryable();
		}

		[HttpPost, WebApiAuthenticate(Permission = Permissions.Configuration.Import.Execute)]
		[WebApiQueryable(PagingOptional = true)]
		public async Task<IQueryable<UploadImportFile>> ImportFiles()
		{
			if (!Request.Content.IsMimeMultipartContent())
			{
				throw this.ExceptionUnsupportedMediaType();
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
				throw this.ExceptionInternalServerError(ex);
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
				throw this.ExceptionNotFound(WebApiGlobal.Error.EntityNotFound.FormatInvariant(identifier.NaIfEmpty()));
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

			return result.AsQueryable();
		}
	}
}