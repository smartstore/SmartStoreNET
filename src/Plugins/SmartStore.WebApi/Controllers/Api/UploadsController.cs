using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Media;
using SmartStore.Services.Catalog;
using SmartStore.Services.Media;
using SmartStore.Utilities;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;
using SmartStore.WebApi.Models.Api;

namespace SmartStore.WebApi.Controllers.Api
{
	public class UploadsController : ApiController
	{
		private readonly Lazy<IProductService> _productService;
		private readonly Lazy<IPictureService> _pictureService;
		private readonly Lazy<IStoreContext> _storeContext;
		private readonly Lazy<MediaSettings> _mediaSettings;

		public UploadsController(
			Lazy<IProductService> productService,
			Lazy<IPictureService> pictureService,
			Lazy<IStoreContext> storeContext,
			Lazy<MediaSettings> mediaSettings)
		{
			_productService = productService;
			_pictureService = pictureService;
			_storeContext = storeContext;
			_mediaSettings = mediaSettings;
		}

		/// <see cref="http://www.asp.net/web-api/overview/advanced/sending-html-form-data,-part-2"/>
		[WebApiAuthenticate(Permission = "ManageCatalog")]
		[WebApiQueryable(PagingOptional = true)]
		public async Task<IQueryable<UploadImage>> PostProductImages()
		{
			if (!Request.Content.IsMimeMultipartContent())
			{
				throw this.ExceptionUnsupportedMediaType();
			}

			Product entity = null;
			string identifier = null;
			string tempDir = FileSystemHelper.TempDir();
			var provider = new MultipartFormDataStreamProvider(tempDir);

			try
			{
				await Request.Content.ReadAsMultipartAsync(provider);
			}
			catch (Exception exc)
			{
				provider.DeleteLocalFiles();
				throw this.ExceptionInternalServerError(exc);
			}

			// find product entity
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
				throw this.ExceptionNotFound(WebApiGlobal.Error.EntityNotFound.FormatWith(identifier.NaIfEmpty()));
			}

			// process images
			var equalPictureId = 0;
			var displayOrder = 0;
			var result = new List<UploadImage>();
			var storeUrl = _storeContext.Value.CurrentStore.Url;
			var pictures = entity.ProductPictures.Select(x => x.Picture);

			if (entity.ProductPictures.Count > 0)
				displayOrder = entity.ProductPictures.Max(x => x.DisplayOrder);

			foreach (var file in provider.FileData)
			{
				var image = new UploadImage
				{
					FileName = file.Headers.ContentDisposition.FileName.ToUnquoted(),
					Name = file.Headers.ContentDisposition.Name.ToUnquoted(),
					MediaType = file.Headers.ContentType.MediaType.ToUnquoted(),
					ContentDisposition = file.Headers.ContentDisposition.Parameters
				};

				if (image.FileName.IsEmpty())
					image.FileName = entity.Name;

				var pictureBinary = File.ReadAllBytes(file.LocalFileName);

				if (pictureBinary != null && pictureBinary.Length > 0)
				{
					pictureBinary = _pictureService.Value.ValidatePicture(pictureBinary);

					pictureBinary = _pictureService.Value.FindEqualPicture(pictureBinary, pictures, out equalPictureId);

					if (pictureBinary != null)
					{
						var seoName = _pictureService.Value.GetPictureSeName(Path.GetFileNameWithoutExtension(image.FileName));

						var newPicture = _pictureService.Value.InsertPicture(pictureBinary, image.MediaType, seoName, true, false, false);

						if (newPicture != null)
						{
							_productService.Value.InsertProductPicture(new ProductPicture
							{
								PictureId = newPicture.Id,
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

					if (image.Picture != null)
					{
						image.ImageUrl = _pictureService.Value.GetPictureUrl(image.Picture, _mediaSettings.Value.ProductDetailsPictureSize, false, storeUrl);
						image.ThumbImageUrl = _pictureService.Value.GetPictureUrl(image.Picture, _mediaSettings.Value.ProductThumbPictureSize, false, storeUrl);
						image.FullSizeImageUrl = _pictureService.Value.GetPictureUrl(image.Picture, 0, false, storeUrl);
					}
				}

				result.Add(image);
			}

			provider.DeleteLocalFiles();
			return result.AsQueryable();
		}
	}
}