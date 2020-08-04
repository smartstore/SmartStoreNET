using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.OData;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Media;
using SmartStore.Services.Media;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.Configuration;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class MediaController : ODataController
    {
        private readonly IMediaService _mediaService;

        public MediaController(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }

        public static void Init(WebApiConfigurationBroadcaster configData)
        {
            var entityConfig = configData.ModelBuilder.Entity<MediaFileInfo>();

            entityConfig.Collection
                .Action("FileExists")
                .ReturnsFromEntitySet<MediaFileInfo>("Media")
                .Parameter<string>("Path");

            entityConfig.Collection
                .Action("CheckUniqueFileName")
                .ReturnsFromEntitySet<MediaFileInfo>("Media")
                .Parameter<string>("Path");

            entityConfig.Collection
                .Action("GetFileByPath")
                .ReturnsFromEntitySet<MediaFileInfo>("Media")
                .Parameter<string>("Path");

            //var getFileByName = entityConfig.Collection
            //    .Action("GetFileByName")
            //    .ReturnsFromEntitySet<MediaFileInfo>("Media");
            //getFileByName.Parameter<string>("FileName");
            //getFileByName.Parameter<int>("FolderId");

            entityConfig.Collection
                .Action("GetFilesByIds")
                .ReturnsFromEntitySet<MediaFileInfo>("Media")
                .CollectionParameter<int>("Ids");

            var countFiles = entityConfig.Collection
                .Action("CountFiles")
                .ReturnsFromEntitySet<MediaFileInfo>("Media");
            countFiles.Parameter<int?>("FolderId");
            countFiles.Parameter<bool?>("DeepSearch");
            countFiles.Parameter<bool?>("Hidden");
            countFiles.Parameter<bool?>("Deleted");
            countFiles.Parameter<string>("Term");
            countFiles.Parameter<bool?>("ExactMatch");
            countFiles.CollectionParameter<string>("MediaTypes");
            countFiles.CollectionParameter<string>("MimeTypes");
            countFiles.CollectionParameter<string>("Extensions");
        }

        // TODO: readonly properties of MediaFileInfo cannot be serialized!

        // GET /Media(123)
        [WebApiAuthenticate]
        public MediaFileInfo Get(int key)
        {
            MediaFileInfo file = null;

            this.ProcessEntity(() =>
            {
                file = _mediaService.GetFileById(key);
                if (file == null)
                {
                    throw this.ExceptionNotFound($"Cannot find file by ID {key}.");
                }
            });

            return file;
        }

        #region Actions

        /// POST /Media/FileExists {"Path":"content/my-file.jpg"}
        [HttpPost, WebApiAuthenticate]
        public bool FileExists(ODataActionParameters parameters)
        {
            var fileExists = false;

            this.ProcessEntity(() =>
            {
                var path = parameters.GetValueSafe<string>("Path");

                fileExists = _mediaService.FileExists(path);
            });

            return fileExists;
        }

        /// POST /Media/CheckUniqueFileName {"Path":"content/my-file.jpg"}
        [HttpPost, WebApiAuthenticate]
        public HttpResponseMessage CheckUniqueFileName(ODataActionParameters parameters)
        {
            string newPath = null;

            this.ProcessEntity(() =>
            {
                var path = parameters.GetValueSafe<string>("Path");

                if (!_mediaService.CheckUniqueFileName(path, out newPath))
                {
                    // Just to make sure the result is unmistakable ;-)
                    newPath = null;
                }
            });

            if (newPath == null)
            {
                return Request.CreateResponse(HttpStatusCode.NoContent);
            }

            return Request.CreateResponse(HttpStatusCode.OK, newPath);
        }

        /// POST /Media/GetFileByPath {"Path":"content/my-file.jpg"}
        [HttpPost, WebApiAuthenticate]
        public MediaFileInfo GetFileByPath(ODataActionParameters parameters)
        {
            MediaFileInfo file = null;

            this.ProcessEntity(() =>
            {
                var path = parameters.GetValueSafe<string>("Path");

                file = _mediaService.GetFileByPath(path);
                if (file == null)
                {
                    throw this.ExceptionNotFound($"The file with the path '{path ?? string.Empty}' does not exist.");
                }
            });

            return file;
        }

        // POST /Media/GetFileByName {"FolderId":2, "FileName":"my-file.jpg"}
        //[HttpPost, WebApiAuthenticate]
        //public MediaFileInfo GetFileByName(ODataActionParameters parameters)
        //{
        //    MediaFileInfo file = null;

        //    this.ProcessEntity(() =>
        //    {
        //        var folderId = parameters.GetValueSafe<int>("FolderId");
        //        var fileName = parameters.GetValueSafe<string>("FileName");

        //        file = _mediaService.GetFileByName(folderId, fileName);
        //        if (file == null)
        //        {
        //            throw this.ExceptionNotFound($"The file with the folder ID {folderId} and file name '{fileName ?? string.Empty}' does not exist.");
        //        }
        //    });

        //    return file;
        //}

        /// POST /Media/GetFilesByIds {"Ids":[1,2,3]}
        [HttpPost, WebApiAuthenticate]
        [WebApiQueryable(PagingOptional = true)]
        public IQueryable<MediaFileInfo> GetFilesByIds(ODataActionParameters parameters)
        {
            IList<MediaFileInfo> files = null;

            this.ProcessEntity(() =>
            {
                var ids = parameters.GetValueSafe<ICollection<int>>("Ids");
                if (ids?.Any() ?? false)
                {
                    files = _mediaService.GetFilesByIds(ids.ToArray());
                }
            });

            return (files ?? new List<MediaFileInfo>()).AsQueryable();
        }

        /// POST /Media/CountFiles {"FolderId":7, "Term":"xyz", "Extensions":["jpg"], ...}
        [HttpPost, WebApiAuthenticate]
        public async Task<int> CountFiles(ODataActionParameters parameters)
        {
            var count = 0;

            await this.ProcessEntityAsync(async () =>
            {
                var query = new MediaSearchQuery
                {
                    FolderId = parameters.GetValueSafe<int?>("FolderId"),
                    DeepSearch = parameters.GetValueSafe<bool?>("DeepSearch") ?? false,
                    Hidden = parameters.GetValueSafe<bool?>("Hidden") ?? false,
                    Deleted = parameters.GetValueSafe<bool?>("Deleted") ?? false,
                    Term = parameters.GetValueSafe<string>("Term"),
                    ExactMatch = parameters.GetValueSafe<bool?>("ExactMatch") ?? false,
                    MediaTypes = parameters.GetValueSafe<ICollection<string>>("MediaTypes")?.ToArray(),
                    MimeTypes = parameters.GetValueSafe<ICollection<string>>("MimeTypes")?.ToArray(),
                    Extensions = parameters.GetValueSafe<ICollection<string>>("Extensions")?.ToArray()
                };

                count = await _mediaService.CountFilesAsync(query);
            });

            return count;
        }

        #endregion
    }


    // TODO: do not support direct MediaFile entity editing through API.
    // Add MediaController for IMediaService methods instead.
    public class PicturesController : WebApiEntityController<MediaFile, IMediaService>
	{
        protected override IQueryable<MediaFile> GetEntitySet()
        {
            var query =
                from x in Repository.Table
                where !x.Deleted && !x.Hidden
                select x;

            return query;
        }

        //[WebApiAuthenticate(Permission = Permissions.Media.Upload)]
        protected override void Insert(MediaFile entity)
		{
			throw this.ExceptionNotImplemented();
		}

        //[WebApiAuthenticate(Permission = Permissions.Media.Update)]
        protected override void Update(MediaFile entity)
		{
			throw this.ExceptionNotImplemented();
		}

        //[WebApiAuthenticate(Permission = Permissions.Media.Delete)]
        protected override void Delete(MediaFile entity)
		{
            throw this.ExceptionNotImplemented();
        }

		[WebApiQueryable]
        public SingleResult<MediaFile> GetPicture(int key)
		{
			return GetSingleResult(key);
		}

		// Navigation properties.

		[WebApiQueryable]
        public IQueryable<ProductMediaFile> GetProductPictures(int key)
		{
			return GetRelatedCollection(key, x => x.ProductMediaFiles);
		}
	}
}
