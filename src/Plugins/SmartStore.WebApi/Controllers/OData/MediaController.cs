using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Media;
using SmartStore.Services.Media;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.Configuration;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;
using SmartStore.WebApi.Models.OData;

namespace SmartStore.WebApi.Controllers.OData
{
    /// <summary>
    /// Is intended to make methods of the IMediaService accessible. Direct access to the MediaFile entity is not intended.
    /// </summary>
    /// <remarks>
    /// Functions like GET /Media/FileExists(Path='content/my-file.jpg') would never work (404).
    /// That's why some endpoints are implemented as Actions (POST).
    /// </remarks>
    public class MediaController : WebApiEntityController<MediaFile, IMediaService>
    {
        // GET /Media(123)
        [WebApiQueryable]
        [WebApiAuthenticate]
        public SingleResult<MediaItemInfo> Get(int key)
        {
            var file = Service.GetFileById(key);
            if (file == null)
            {
                throw Request.NotFoundException(WebApiGlobal.Error.EntityNotFound.FormatInvariant(key));
            }

            return SingleResult.Create(new[] { Convert(file) }.AsQueryable());
        }

        // GET /Media
        [WebApiQueryable]
        [WebApiAuthenticate]
        public IQueryable<MediaItemInfo> Get(/*ODataQueryOptions<MediaFile> queryOptions*/)
        {
            throw new HttpResponseException(HttpStatusCode.NotImplemented);

            // TODO or not TODO :)
            //var maxTop = WebApiCachingControllingData.Data().MaxTop;
            //var top = Math.Min(this.GetQueryStringValue("$top", maxTop), maxTop);

            //var query = queryOptions.ApplyTo(GetEntitySet(), new ODataQuerySettings { PageSize = top }) as IQueryable<MediaFile>;
            //var files = query.ToList();
            //var result = files.Select(x => Convert(Service.ConvertMediaFile(x)));

            //return result.AsQueryable();
        }

        // GET /Media(123)/ThumbUrl
        [WebApiAuthenticate]
        public HttpResponseMessage GetProperty(int key, string propertyName)
        {
            var file = Service.GetFileById(key);
            if (file == null)
            {
                throw Request.NotFoundException(WebApiGlobal.Error.EntityNotFound.FormatInvariant(key));
            }

            var item = Convert(file);

            var prop = FastProperty.GetProperty(item.GetType(), propertyName);
            if (prop == null)
            {
                throw Request.BadRequestException(WebApiGlobal.Error.PropertyNotFound.FormatInvariant(propertyName.EmptyNull()));
            }

            var propertyValue = prop.GetValue(item);

            return Request.CreateResponse(HttpStatusCode.OK, prop.Property.PropertyType, propertyValue);
        }

        public IHttpActionResult Post(MediaFile entity)
        {
            throw new HttpResponseException(HttpStatusCode.Forbidden);
        }

        public IHttpActionResult Put(int key, MediaFile entity)
        {
            throw new HttpResponseException(HttpStatusCode.Forbidden);
        }

        public IHttpActionResult Patch(int key, Delta<MediaFile> model)
        {
            throw new HttpResponseException(HttpStatusCode.Forbidden);
        }

        public IHttpActionResult Delete(int key)
        {
            throw new HttpResponseException(HttpStatusCode.Forbidden);
        }

        #region Actions and functions

        public static void Init(WebApiConfigurationBroadcaster configData)
        {
            var entityConfig = configData.ModelBuilder.EntityType<MediaItemInfo>();

            entityConfig.Collection
                .Action("GetFileByPath")
                .ReturnsFromEntitySet<MediaItemInfo>("Media")
                .Parameter<string>("Path");

            //var getFileByName = entityConfig.Collection
            //    .Action("GetFileByName")
            //    .ReturnsFromEntitySet<MediaItemInfo>("Media");
            //getFileByName.Parameter<string>("FileName");
            //getFileByName.Parameter<int>("FolderId");

            entityConfig.Collection
                .Function("GetFilesByIds")
                .ReturnsFromEntitySet<MediaItemInfo>("Media")
                .CollectionParameter<int>("Ids");

            entityConfig.Collection
                .Action("FileExists")
                .Returns<bool>()
                .Parameter<string>("Path");

            entityConfig.Collection
                .Action("CheckUniqueFileName")
                .Returns<CheckUniqueFileName>()
                .Parameter<string>("Path");

            entityConfig.Collection
                .Action("CountFiles")
                .Returns<int>()
                .Parameter<MediaSearchQuery>("Query");
        }

        /// POST /Media/GetFileByPath {"Path":"content/my-file.jpg"}
        [HttpPost]
        [WebApiAuthenticate]
        public MediaItemInfo GetFileByPath(ODataActionParameters parameters)
        {
            var path = parameters.GetValueSafe<string>("Path");
            var file = Service.GetFileByPath(path);

            if (file == null)
            {
                throw Request.NotFoundException($"The file with the path '{path ?? string.Empty}' does not exist.");
            }

            return Convert(file);
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

        /// GET /Media/GetFilesByIds(Ids=[1,2,3])
        [HttpGet, WebApiQueryable]
        [WebApiAuthenticate]
        public IQueryable<MediaItemInfo> GetFilesByIds([FromODataUri] int[] ids)
        {
            if (ids?.Any() ?? false)
            {
                var files = Service.GetFilesByIds(ids.ToArray());

                return files.Select(x => Convert(x)).AsQueryable();
            }

            return new List<MediaItemInfo>().AsQueryable();
        }

        /// POST /Media/FileExists {"Path":"content/my-file.jpg"}
        [HttpPost]
        [WebApiAuthenticate]
        public bool FileExists(ODataActionParameters parameters)
        {
            var path = parameters.GetValueSafe<string>("Path");
            var fileExists = Service.FileExists(path);
            return fileExists;
        }

        /// POST /Media/CheckUniqueFileName {"Path":"content/my-file.jpg"}
        [HttpPost]
        [WebApiAuthenticate]
        public CheckUniqueFileName CheckUniqueFileName(ODataActionParameters parameters)
        {
            var result = new CheckUniqueFileName();
            var path = parameters.GetValueSafe<string>("Path");

            result.Result = Service.CheckUniqueFileName(path, out string newPath);
            result.NewPath = newPath;            

            return result;
        }

        /// POST /Media/CountFiles {"Query":{"FolderId":7,"Extensions":["jpg"], ...}}
        [HttpPost]
        [WebApiAuthenticate]
        public async Task<int> CountFiles(ODataActionParameters parameters)
        {
            var query = parameters.GetValueSafe<MediaSearchQuery>("Query");
            var count = await Service.CountFilesAsync(query ?? new MediaSearchQuery());
            return count;
        }

        #endregion

        #region Utilities

        private MediaItemInfo Convert(MediaFileInfo file)
        {
            var item = MiniMapper.Map<MediaFileInfo, MediaItemInfo>(file);
            return item;
        }

        #endregion
    }


    //   public class PicturesController : WebApiEntityController<MediaFile, IMediaService>
    //{
    //       protected override IQueryable<MediaFile> GetEntitySet()
    //       {
    //           var query =
    //               from x in Repository.Table
    //               where !x.Deleted && !x.Hidden
    //               select x;

    //           return query;
    //       }

    //	[WebApiQueryable]
    //       public SingleResult<MediaFile> GetPicture(int key)
    //	{
    //		return GetSingleResult(key);
    //	}

    //	[WebApiQueryable]
    //       public IQueryable<ProductMediaFile> GetProductPictures(int key)
    //	{
    //		return GetRelatedCollection(key, x => x.ProductMediaFiles);
    //	}
    //}
}
