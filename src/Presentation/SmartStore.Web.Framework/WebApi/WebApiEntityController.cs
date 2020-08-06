using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Formatter;
//using System.Web.Http.OData;
//using System.Web.Http.OData.Extensions;
//using System.Web.Http.OData.Formatter;
//using System.Web.Http.OData.Routing;
using Autofac;
using SmartStore.ComponentModel;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Services;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;

namespace SmartStore.Web.Framework.WebApi
{
    public abstract class WebApiEntityController<TEntity, TService> : ODataController
        where TEntity : BaseEntity, new()
    {
        // TODO: Implement custom routing convention(s):
        // public override HttpResponseMessage HandleUnmappedRequest(ODataPath odataPath)

        // TODO: Needs to be handled by the respective controller:
        // protected override int GetKey(TEntity entity)
        // protected override TEntity GetEntityByKey(int key)

        // public override HttpResponseMessage Post(TEntity entity)
        // protected override TEntity CreateEntity(TEntity entity)
        // public override HttpResponseMessage Put(int key, TEntity update)
        // protected override TEntity UpdateEntity(int key, TEntity update)
        // public override HttpResponseMessage Patch(int key, Delta<TEntity> patch)
        // protected override TEntity PatchEntity(int key, Delta<TEntity> patch)
        // public override void Delete(int key)

        // TODO:
        // XML serialization issues.

        /// <summary>
        /// Auto injected by Autofac.
        /// </summary>
        public virtual IRepository<TEntity> Repository { get; set; }

        /// <summary>
        /// Auto injected by Autofac.
        /// </summary>
        public virtual TService Service { get; set; }

        /// <summary>
        /// Auto injected by Autofac.
        /// </summary>
        public virtual ICommonServices Services { get; set; }

        protected internal HttpResponseException ExceptionEntityNotFound<TKey>(TKey key)
        {
            var response = Request.CreateErrorResponse(HttpStatusCode.NotFound, WebApiGlobal.Error.EntityNotFound.FormatInvariant(key));

            return new HttpResponseException(response);
        }

        protected internal HttpResponseException ExceptionNotExpanded<TProperty>(Expression<Func<TEntity, TProperty>> path)
        {
            // NotFound cause of nullable properties.
            var response = Request.CreateErrorResponse(HttpStatusCode.NotFound, WebApiGlobal.Error.PropertyNotExpanded.FormatInvariant(path.ToString()));

            return new HttpResponseException(response);
        }

        protected internal virtual IQueryable<TEntity> GetEntitySet()
        {
            return this.Repository.Table;
        }

        protected internal virtual IQueryable<TEntity> GetExpandedEntitySet<TProperty>(Expression<Func<TEntity, TProperty>> path)
        {
            var query = GetEntitySet().Expand(path);
            return query;
        }

        protected internal virtual IQueryable<TEntity> GetExpandedEntitySet(string properties)
        {
            var query = GetEntitySet();

            foreach (var property in properties.SplitSafe(","))
            {
                query = query.Expand(property.Trim());
            }

            return query;
        }

        protected internal virtual TEntity GetEntityByKeyNotNull(int key)
        {
            if (!ModelState.IsValid)
                throw this.ExceptionInvalidModelState();

            var entity = this.Repository.GetById(key);

            if (entity == null)
                throw ExceptionEntityNotFound(key);

            return entity;
        }

        protected internal virtual SingleResult<TEntity> GetSingleResult(int key)
        {
            if (!ModelState.IsValid)
            {
                throw this.ExceptionInvalidModelState();
            }

            var entity = GetEntitySet().FirstOrDefault(x => x.Id == key);

            return GetSingleResult(entity);
            //return SingleResult.Create(GetEntitySet().Where(x => x.Id == key));
        }

        protected internal virtual SingleResult<TEntity> GetSingleResult(TEntity entity)
        {
            return SingleResult.Create(new[] { entity }.AsQueryable());
        }

        protected internal virtual TEntity GetExpandedEntity<TProperty>(int key, Expression<Func<TEntity, TProperty>> path)
        {
            if (!ModelState.IsValid)
                throw this.ExceptionInvalidModelState();

            var query = GetExpandedEntitySet<TProperty>(path);

            var entity = query.FirstOrDefault(x => x.Id == key);

            if (entity == null)
                throw ExceptionEntityNotFound(key);

            return entity;
        }

        protected internal virtual TEntity GetExpandedEntity(int key, string properties)
        {
            if (!ModelState.IsValid)
                throw this.ExceptionInvalidModelState();

            var query = GetExpandedEntitySet(properties);

            var entity = query.FirstOrDefault(x => x.Id == key);

            if (entity == null)
                throw ExceptionEntityNotFound(key);

            return entity;
        }

        protected internal virtual TEntity GetExpandedEntity(int key, SingleResult<TEntity> result, string path)
        {
            var query = result.Queryable;

            foreach (var property in path.SplitSafe(","))
            {
                query = query.Expand(property.Trim());
            }

            var entity = query.FirstOrDefault(x => x.Id == key);

            if (entity == null)
                throw ExceptionEntityNotFound(key);

            return entity;
        }

        protected internal virtual TProperty GetExpandedProperty<TProperty>(int key, Expression<Func<TEntity, TProperty>> path)
        {
            var entity = GetExpandedEntity<TProperty>(key, path);

            var expression = path.CompileFast(PropertyCachingStrategy.EagerCached);
            var property = expression.Invoke(entity);

            if (property == null)
                throw ExceptionNotExpanded<TProperty>(path);

            return property;
        }

        protected internal virtual IQueryable<TCollection> GetRelatedCollection<TCollection>(
            int key,
            Expression<Func<TEntity, IEnumerable<TCollection>>> navigationProperty)
        {
            Guard.NotNull(navigationProperty, nameof(navigationProperty));

            var query = GetEntitySet().Where(x => x.Id.Equals(key));
            return query.SelectMany(navigationProperty);
        }

        protected internal virtual IQueryable<TCollection> GetRelatedCollection<TCollection>(
            int key,
            string navigationProperty)
        {
            Guard.NotEmpty(navigationProperty, nameof(navigationProperty));

            if (!ModelState.IsValid)
                throw this.ExceptionInvalidModelState();


            var ctx = (DbContext)Repository.Context;
            var product = this.Repository.GetById(key);
            var entry = ctx.Entry(product);
            var query = entry.Collection(navigationProperty).Query();

            return query.Cast<TCollection>();
        }

        protected internal virtual SingleResult<TElement> GetRelatedEntity<TElement>(
            int key,
            Expression<Func<TEntity, TElement>> navigationProperty)
        {
            Guard.NotNull(navigationProperty, nameof(navigationProperty));

            var query = GetEntitySet().Where(x => x.Id.Equals(key)).Select(navigationProperty);
            return SingleResult.Create(query);
        }

        protected internal virtual void Insert(TEntity entity)
        {
            Repository.Insert(entity);
        }

        protected internal virtual void Update(TEntity entity)
        {
            Repository.Update(entity);
        }

        protected internal virtual void Delete(TEntity entity)
        {
            Repository.Delete(entity);
        }

        protected internal virtual object FulfillPropertyOn(TEntity entity, string propertyName, string queryValue)
        {
            var container = Services.Container;

            if (propertyName.IsCaseInsensitiveEqual("Country"))
            {
                return container.Resolve<ICountryService>().GetCountryByTwoOrThreeLetterIsoCode(queryValue);
            }
            else if (propertyName.IsCaseInsensitiveEqual("StateProvince"))
            {
                return container.Resolve<IStateProvinceService>().GetStateProvinceByAbbreviation(queryValue);
            }
            else if (propertyName.IsCaseInsensitiveEqual("Language"))
            {
                return container.Resolve<ILanguageService>().GetLanguageByCulture(queryValue);
            }
            else if (propertyName.IsCaseInsensitiveEqual("Currency"))
            {
                return container.Resolve<ICurrencyService>().GetCurrencyByCode(queryValue);
            }

            return null;
        }

        protected internal virtual TEntity FulfillPropertiesOn(TEntity entity)
        {
            try
            {
                if (entity == null)
                    return entity;

                var queries = Request.RequestUri.ParseQueryString();

                if (queries == null || queries.Count <= 0)
                    return entity;

                foreach (string key in queries.AllKeys.Where(x => x.StartsWith(WebApiGlobal.QueryOption.Fulfill)))
                {
                    string propertyName = key.Substring(WebApiGlobal.QueryOption.Fulfill.Length);
                    string queryValue = queries.Get(key);

                    if (propertyName.HasValue() && queryValue.HasValue())
                    {
                        var prop = FastProperty.GetProperty(entity.GetType(), propertyName);
                        if (prop != null)
                        {
                            var propertyValue = prop.GetValue(entity);
                            if (propertyValue == null)
                            {
                                object value = FulfillPropertyOn(entity, propertyName, queryValue);

                                // there's no requirement to set a property value of null
                                if (value != null)
                                {
                                    prop.SetValue(entity, value);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw this.ExceptionUnprocessableEntity(ex.Message);
            }

            return entity;
        }

        protected internal virtual T ReadContent<T>()
        {
            var formatters = ODataMediaTypeFormatters
                .Create()
                .Select(formatter => formatter.GetPerRequestFormatterInstance(typeof(T), Request, Request.Content.Headers.ContentType));

            return Request.Content.ReadAsAsync<T>(formatters).Result;
        }
    }

    #region Old WebApiEntityController

    //public abstract class WebApiEntityController<TEntity, TService> : EntitySetController<TEntity, int>
    //    where TEntity : BaseEntity, new()
    //{

    //    protected internal HttpResponseException ExceptionEntityNotFound<TKey>(TKey key)
    //    {
    //        var response = Request.CreateErrorResponse(HttpStatusCode.NotFound,
    //            WebApiGlobal.Error.EntityNotFound.FormatInvariant(key));

    //        return new HttpResponseException(response);
    //    }

    //    protected internal HttpResponseException ExceptionNotExpanded<TProperty>(Expression<Func<TEntity, TProperty>> path)
    //    {
    //        // NotFound cause of nullable properties
    //        var response = Request.CreateErrorResponse(HttpStatusCode.NotFound,
    //            WebApiGlobal.Error.PropertyNotExpanded.FormatInvariant(path.ToString()));

    //        return new HttpResponseException(response);
    //    }

    //    public override HttpResponseMessage HandleUnmappedRequest(ODataPath odataPath)
    //    {
    //        if (odataPath.PathTemplate.IsCaseInsensitiveEqual("~/entityset/key/property") ||
    //            odataPath.PathTemplate.IsCaseInsensitiveEqual("~/entityset/key/cast/property") ||
    //            odataPath.PathTemplate.IsCaseInsensitiveEqual("~/entityset/key/unresolved"))
    //        {
    //            if (Request.Method == HttpMethod.Get || Request.Method == HttpMethod.Post)
    //            {
    //                return UnmappedGetProperty(odataPath);
    //            }
    //        }
    //        else if (odataPath.PathTemplate.IsCaseInsensitiveEqual("~/entityset/key/navigation/key"))
    //        {
    //            if (Request.Method == HttpMethod.Get || Request.Method == HttpMethod.Post || Request.Method == HttpMethod.Delete)
    //            {
    //                // we ignore standard odata path cause they differ:
    //                // ~/entityset/key/$links/navigation (odata 3 "link"), ~/entityset/key/navigation/$ref (odata 4 "reference")

    //                return UnmappedGetNavigation(odataPath);
    //            }
    //        }
    //        else if (odataPath.PathTemplate.IsCaseInsensitiveEqual("~/entityset/key/navigation"))
    //        {
    //            if (Request.Method == HttpMethod.Delete)
    //            {
    //                return UnmappedGetNavigation(odataPath);
    //            }
    //        }

    //        return base.HandleUnmappedRequest(odataPath);
    //    }

    //    protected virtual internal HttpResponseMessage UnmappedGetProperty(ODataPath odataPath)
    //    {
    //        int key;

    //        if (!odataPath.GetNormalizedKey(1, out key))
    //            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, WebApiGlobal.Error.NoKeyFromPath);

    //        var entity = GetEntityByKey(key);

    //        if (entity == null)
    //            return Request.CreateErrorResponse(HttpStatusCode.NotFound, WebApiGlobal.Error.EntityNotFound.FormatInvariant(key));

    //        FastProperty prop = null;
    //        string propertyName = null;
    //        var lastSegment = odataPath.Segments.Last();
    //        var propertySegment = (lastSegment as PropertyAccessPathSegment);

    //        if (propertySegment == null)
    //            propertyName = lastSegment.ToString();
    //        else
    //            propertyName = propertySegment.PropertyName;

    //        if (propertyName.HasValue())
    //            prop = FastProperty.GetProperty(entity.GetType(), propertyName);

    //        if (prop == null)
    //            return UnmappedGetProperty(entity, propertyName ?? "");

    //        var propertyValue = prop.GetValue(entity);

    //        return Request.CreateResponse(HttpStatusCode.OK, prop.Property.PropertyType, propertyValue);
    //    }

    //    protected virtual internal HttpResponseMessage UnmappedGetProperty(TEntity entity, string propertyName)
    //    {
    //        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, WebApiGlobal.Error.PropertyNotFound.FormatInvariant(propertyName));
    //    }

    //    protected virtual internal HttpResponseMessage UnmappedGetNavigation(ODataPath odataPath)
    //    {
    //        int key, relatedKey;

    //        if (!odataPath.GetNormalizedKey(1, out key))
    //            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, WebApiGlobal.Error.NoKeyFromPath);

    //        var navigationProperty = odataPath.GetNavigation(2);

    //        if (navigationProperty.IsEmpty())
    //            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, WebApiGlobal.Error.NoNavigationFromPath);

    //        if (!odataPath.GetNormalizedKey(3, out relatedKey) && Request.Method != HttpMethod.Delete)
    //            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, WebApiGlobal.Error.NoRelatedKeyFromPath);

    //        var methodName = string.Concat("Navigation", navigationProperty);
    //        var methodInfo = GetType().GetMethod(methodName);

    //        if (methodInfo != null)
    //        {
    //            HttpResponseMessage response = null;

    //            this.ProcessEntity(() =>
    //            {
    //                response = (HttpResponseMessage)methodInfo.Invoke(this, new object[] { key, relatedKey });
    //            });

    //            return response;
    //        }
    //        return base.HandleUnmappedRequest(odataPath);
    //    }

    //    /// <summary>
    //    /// Auto injected by Autofac
    //    /// </summary>
    //    public virtual IRepository<TEntity> Repository
    //    {
    //        get;
    //        set;
    //    }

    //    /// <summary>
    //    /// Auto injected by Autofac
    //    /// </summary>
    //    public virtual TService Service
    //    {
    //        get;
    //        set;
    //    }

    //    /// <summary>
    //    /// Auto injected by Autofac
    //    /// </summary>
    //    public virtual ICommonServices Services
    //    {
    //        get;
    //        set;
    //    }

    //    public override IQueryable<TEntity> Get()
    //    {
    //        if (!ModelState.IsValid)
    //            throw this.ExceptionInvalidModelState();

    //        return this.GetEntitySet();
    //    }

    //    protected internal virtual IQueryable<TEntity> GetEntitySet()
    //    {
    //        return this.Repository.Table;
    //    }

    //    protected internal virtual IQueryable<TEntity> GetExpandedEntitySet<TProperty>(Expression<Func<TEntity, TProperty>> path)
    //    {
    //        var query = GetEntitySet().Expand(path);
    //        return query;
    //    }

    //    protected internal virtual IQueryable<TEntity> GetExpandedEntitySet(string properties)
    //    {
    //        var query = GetEntitySet();

    //        foreach (var property in properties.SplitSafe(","))
    //        {
    //            query = query.Expand(property.Trim());
    //        }

    //        return query;
    //    }

    //    protected override int GetKey(TEntity entity)
    //    {
    //        if (!ModelState.IsValid)
    //            throw this.ExceptionInvalidModelState();

    //        return entity.Id;
    //    }

    //    protected override TEntity GetEntityByKey(int key)
    //    {
    //        if (!ModelState.IsValid)
    //            throw this.ExceptionInvalidModelState();

    //        return this.Repository.GetById(key);
    //    }

    //    protected internal virtual TEntity GetEntityByKeyNotNull(int key)
    //    {
    //        var entity = GetEntityByKey(key);

    //        if (entity == null)
    //            throw ExceptionEntityNotFound(key);

    //        return entity;
    //    }

    //    protected internal virtual SingleResult<TEntity> GetSingleResult(int key)
    //    {
    //        if (!ModelState.IsValid)
    //            throw this.ExceptionInvalidModelState();

    //        return SingleResult.Create(GetEntitySet().Where(x => x.Id == key));
    //    }

    //    protected internal virtual TEntity GetExpandedEntity<TProperty>(int key, Expression<Func<TEntity, TProperty>> path)
    //    {
    //        if (!ModelState.IsValid)
    //            throw this.ExceptionInvalidModelState();

    //        var query = GetExpandedEntitySet<TProperty>(path);

    //        var entity = query.FirstOrDefault(x => x.Id == key);

    //        if (entity == null)
    //            throw ExceptionEntityNotFound(key);

    //        return entity;
    //    }
    //    protected internal virtual TEntity GetExpandedEntity(int key, string properties)
    //    {
    //        if (!ModelState.IsValid)
    //            throw this.ExceptionInvalidModelState();

    //        var query = GetExpandedEntitySet(properties);

    //        var entity = query.FirstOrDefault(x => x.Id == key);

    //        if (entity == null)
    //            throw ExceptionEntityNotFound(key);

    //        return entity;
    //    }

    //    protected internal virtual TEntity GetExpandedEntity(int key, SingleResult<TEntity> result, string path)
    //    {
    //        var query = result.Queryable;

    //        foreach (var property in path.SplitSafe(","))
    //        {
    //            query = query.Expand(property.Trim());
    //        }

    //        var entity = query.FirstOrDefault(x => x.Id == key);

    //        if (entity == null)
    //            throw ExceptionEntityNotFound(key);

    //        return entity;
    //    }

    //    protected internal virtual TProperty GetExpandedProperty<TProperty>(int key, Expression<Func<TEntity, TProperty>> path)
    //    {
    //        var entity = GetExpandedEntity<TProperty>(key, path);

    //        var expression = path.CompileFast(PropertyCachingStrategy.EagerCached);
    //        var property = expression.Invoke(entity);

    //        if (property == null)
    //            throw ExceptionNotExpanded<TProperty>(path);

    //        return property;
    //    }

    //    protected internal virtual IQueryable<TCollection> GetRelatedCollection<TCollection>(
    //        int key,
    //        Expression<Func<TEntity, IEnumerable<TCollection>>> navigationProperty)
    //    {
    //        Guard.NotNull(navigationProperty, nameof(navigationProperty));

    //        var query = GetEntitySet().Where(x => x.Id.Equals(key));
    //        return query.SelectMany(navigationProperty);
    //    }

    //    protected internal virtual IQueryable<TCollection> GetRelatedCollection<TCollection>(
    //        int key,
    //        string navigationProperty)
    //    {
    //        Guard.NotEmpty(navigationProperty, nameof(navigationProperty));

    //        var ctx = (DbContext)Repository.Context;
    //        var product = GetEntityByKey(key);
    //        var entry = ctx.Entry(product);
    //        var query = entry.Collection(navigationProperty).Query();

    //        return query.Cast<TCollection>();
    //    }

    //    protected internal virtual SingleResult<TElement> GetRelatedEntity<TElement>(
    //        int key,
    //        Expression<Func<TEntity, TElement>> navigationProperty)
    //    {
    //        Guard.NotNull(navigationProperty, nameof(navigationProperty));

    //        var query = GetEntitySet().Where(x => x.Id.Equals(key)).Select(navigationProperty);
    //        return SingleResult.Create(query);
    //    }

    //    protected internal virtual SingleResult<TElement> GetRelatedEntity<TElement>(
    //        int key,
    //        string navigationProperty)
    //    {
    //        Guard.NotEmpty(navigationProperty, nameof(navigationProperty));

    //        var ctx = (DbContext)Repository.Context;
    //        var product = GetEntityByKey(key);
    //        var entry = ctx.Entry(product);
    //        var query = entry.Reference(navigationProperty).Query().Cast<TElement>();

    //        return SingleResult.Create(query);
    //    }

    //    public override HttpResponseMessage Post(TEntity entity)
    //    {
    //        var response = Request.CreateResponse(HttpStatusCode.OK, CreateEntity(entity));

    //        try
    //        {
    //            var entityUrl = Url.CreateODataLink(
    //                new EntitySetPathSegment(entity.GetType().Name.EnsureEndsWith("s")),
    //                new KeyValuePathSegment(entity.Id.ToString())
    //            );

    //            response.Headers.Location = new Uri(entityUrl);
    //        }
    //        catch { }

    //        return response;
    //    }

    //    protected override TEntity CreateEntity(TEntity entity)
    //    {
    //        if (!ModelState.IsValid)
    //            throw this.ExceptionInvalidModelState();

    //        if (entity == null)
    //            throw this.ExceptionBadRequest(WebApiGlobal.Error.NoDataToInsert);

    //        Insert(FulfillPropertiesOn(entity));

    //        return entity;
    //    }

    //    protected internal virtual void Insert(TEntity entity)
    //    {
    //        Repository.Insert(entity);
    //    }

    //    public override HttpResponseMessage Put(int key, TEntity update)
    //    {
    //        return Request.CreateResponse(HttpStatusCode.OK, UpdateEntity(key, update));
    //    }

    //    protected override TEntity UpdateEntity(int key, TEntity update)
    //    {
    //        if (!ModelState.IsValid)
    //            throw this.ExceptionInvalidModelState();

    //        var originalEntity = GetEntityByKeyNotNull(key);

    //        var context = ((IObjectContextAdapter)this.Repository.Context).ObjectContext;
    //        var container = context.MetadataWorkspace.GetEntityContainer(context.DefaultContainerName, DataSpace.CSpace);

    //        string entityName = typeof(TEntity).Name;
    //        string entitySetName = container.BaseEntitySets.First(x => x.ElementType.Name == entityName).Name;

    //        update.Id = key;
    //        var entity = context.ApplyCurrentValues(entitySetName, update);

    //        Update(FulfillPropertiesOn(entity));

    //        return entity;
    //    }

    //    protected internal virtual void Update(TEntity entity)
    //    {
    //        Repository.Update(entity);
    //    }

    //    public override HttpResponseMessage Patch(int key, Delta<TEntity> patch)
    //    {
    //        return Request.CreateResponse(HttpStatusCode.OK, PatchEntity(key, patch));
    //    }

    //    protected override TEntity PatchEntity(int key, Delta<TEntity> patch)
    //    {
    //        if (!ModelState.IsValid)
    //            throw this.ExceptionInvalidModelState();

    //        var entity = GetEntityByKeyNotNull(key);

    //        if (patch != null)
    //            patch.Patch(entity);

    //        Update(FulfillPropertiesOn(entity));

    //        return entity;
    //    }

    //    public override void Delete(int key)
    //    {
    //        if (!ModelState.IsValid)
    //            throw this.ExceptionInvalidModelState();

    //        var entity = GetEntityByKeyNotNull(key);

    //        Delete(entity);
    //    }

    //    protected internal virtual void Delete(TEntity entity)
    //    {
    //        Repository.Delete(entity);
    //    }

    //    protected internal virtual object FulfillPropertyOn(TEntity entity, string propertyName, string queryValue)
    //    {
    //        var container = Services.Container;

    //        if (propertyName.IsCaseInsensitiveEqual("Country"))
    //        {
    //            return container.Resolve<ICountryService>().GetCountryByTwoOrThreeLetterIsoCode(queryValue);
    //        }
    //        else if (propertyName.IsCaseInsensitiveEqual("StateProvince"))
    //        {
    //            return container.Resolve<IStateProvinceService>().GetStateProvinceByAbbreviation(queryValue);
    //        }
    //        else if (propertyName.IsCaseInsensitiveEqual("Language"))
    //        {
    //            return container.Resolve<ILanguageService>().GetLanguageByCulture(queryValue);
    //        }
    //        else if (propertyName.IsCaseInsensitiveEqual("Currency"))
    //        {
    //            return container.Resolve<ICurrencyService>().GetCurrencyByCode(queryValue);
    //        }

    //        return null;
    //    }

    //    protected internal virtual TEntity FulfillPropertiesOn(TEntity entity)
    //    {
    //        try
    //        {
    //            if (entity == null)
    //                return entity;

    //            var queries = Request.RequestUri.ParseQueryString();

    //            if (queries == null || queries.Count <= 0)
    //                return entity;

    //            foreach (string key in queries.AllKeys.Where(x => x.StartsWith(WebApiGlobal.QueryOption.Fulfill)))
    //            {
    //                string propertyName = key.Substring(WebApiGlobal.QueryOption.Fulfill.Length);
    //                string queryValue = queries.Get(key);

    //                if (propertyName.HasValue() && queryValue.HasValue())
    //                {
    //                    var prop = FastProperty.GetProperty(entity.GetType(), propertyName);
    //                    if (prop != null)
    //                    {
    //                        var propertyValue = prop.GetValue(entity);
    //                        if (propertyValue == null)
    //                        {
    //                            object value = FulfillPropertyOn(entity, propertyName, queryValue);

    //                            // there's no requirement to set a property value of null
    //                            if (value != null)
    //                            {
    //                                prop.SetValue(entity, value);
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            throw this.ExceptionUnprocessableEntity(ex.Message);
    //        }

    //        return entity;
    //    }

    //    protected internal virtual T ReadContent<T>()
    //    {
    //        var formatters = ODataMediaTypeFormatters.Create()
    //            .Select(formatter => formatter.GetPerRequestFormatterInstance(typeof(T), Request, Request.Content.Headers.ContentType));

    //        return Request.Content.ReadAsAsync<T>(formatters).Result;
    //    }
    //}

    #endregion

    #region Old OData3 EntitySetController

    //[CLSCompliant(false)]
    //[ODataNullValue]
    //public abstract class EntitySetController<TEntity, TKey> : ODataController
    //where TEntity : class
    //{
    //    /// <summary>Gets the OData path of the current request.</summary>
    //    public ODataPath ODataPath
    //    {
    //        get
    //        {
    //            return EntitySetControllerHelpers.GetODataPath(this);
    //        }
    //    }

    //    /// <summary>Gets the OData query options of the current request.</summary>
    //    public ODataQueryOptions<TEntity> QueryOptions
    //    {
    //        get
    //        {
    //            return EntitySetControllerHelpers.CreateQueryOptions<TEntity>(this);
    //        }
    //    }

    //    protected EntitySetController()
    //    {
    //    }

    //    /// <summary>This method should be overridden to create a new entity in the entity set.</summary>
    //    /// <returns>The created entity.</returns>
    //    /// <param name="entity">The entity to add to the entity set.</param>
    //    protected internal virtual TEntity CreateEntity(TEntity entity)
    //    {
    //        throw EntitySetControllerHelpers.CreateEntityNotImplementedResponse(base.get_Request());
    //    }

    //    /// <summary>This method should be overridden to handle POST and PUT requests that attempt to create a link between two entities.</summary>
    //    /// <param name="key">The key of the entity with the navigation property.</param>
    //    /// <param name="navigationProperty">The name of the navigation property.</param>
    //    /// <param name="link">The URI of the entity to link.</param>
    //    [AcceptVerbs(new string[] { "POST", "PUT" })]
    //    public virtual void CreateLink([FromODataUri] TKey key, string navigationProperty, [FromBody] Uri link)
    //    {
    //        throw EntitySetControllerHelpers.CreateLinkNotImplementedResponse(base.get_Request(), navigationProperty);
    //    }

    //    /// <summary>This method should be overriden to handle DELETE requests for deleting existing entities from the entity set.</summary>
    //    /// <param name="key">The entity key of the entity to delete.</param>
    //    public virtual void Delete([FromODataUri] TKey key)
    //    {
    //        throw EntitySetControllerHelpers.DeleteEntityNotImplementedResponse(base.get_Request());
    //    }

    //    /// <summary>This method should be overridden to handle DELETE requests that attempt to break a relationship between two entities.</summary>
    //    /// <param name="key">The key of the entity with the navigation property.</param>
    //    /// <param name="navigationProperty">The name of the navigation property.</param>
    //    /// <param name="link">The URI of the entity to remove from the navigation property.</param>
    //    public virtual void DeleteLink([FromODataUri] TKey key, string navigationProperty, [FromBody] Uri link)
    //    {
    //        throw EntitySetControllerHelpers.DeleteLinkNotImplementedResponse(base.get_Request(), navigationProperty);
    //    }

    //    /// <summary>This method should be overridden to handle DELETE requests that attempt to break a relationship between two entities.</summary>
    //    /// <param name="key">The key of the entity with the navigation property.</param>
    //    /// <param name="relatedKey">The key of the related entity.</param>
    //    /// <param name="navigationProperty">The name of the navigation property.</param>
    //    public virtual void DeleteLink([FromODataUri] TKey key, string relatedKey, string navigationProperty)
    //    {
    //        throw EntitySetControllerHelpers.DeleteLinkNotImplementedResponse(base.get_Request(), navigationProperty);
    //    }

    //    /// <summary>This method should be overridden to handle GET requests that attempt to retrieve entities from the entity set.</summary>
    //    /// <returns>The matching entities from the entity set.</returns>
    //    public virtual IQueryable<TEntity> Get()
    //    {
    //        throw EntitySetControllerHelpers.GetNotImplementedResponse(base.get_Request());
    //    }

    //    /// <summary>Handles GET requests that attempt to retrieve an individual entity by key from the entity set.</summary>
    //    /// <returns>The response message to send back to the client.</returns>
    //    /// <param name="key">The entity key of the entity to retrieve.</param>
    //    public virtual HttpResponseMessage Get([FromODataUri] TKey key)
    //    {
    //        TEntity entityByKey = this.GetEntityByKey(key);
    //        return EntitySetControllerHelpers.GetByKeyResponse<TEntity>(base.get_Request(), entityByKey);
    //    }

    //    /// <summary>This method should be overridden to retrieve an entity by key from the entity set.</summary>
    //    /// <returns>The retrieved entity, or null if an entity with the specified entity key cannot be found in the entity set.</returns>
    //    /// <param name="key">The entity key of the entity to retrieve.</param>
    //    protected internal virtual TEntity GetEntityByKey(TKey key)
    //    {
    //        throw EntitySetControllerHelpers.GetEntityByKeyNotImplementedResponse(base.get_Request());
    //    }

    //    /// <summary>This method should be overridden to get the entity key of the specified entity.</summary>
    //    /// <returns>The entity key value</returns>
    //    /// <param name="entity">The entity.</param>
    //    protected internal virtual TKey GetKey(TEntity entity)
    //    {
    //        throw EntitySetControllerHelpers.GetKeyNotImplementedResponse(base.get_Request());
    //    }

    //    /// <summary>This method should be overridden to handle all unmapped OData requests.</summary>
    //    /// <returns>The response message to send back to the client.</returns>
    //    /// <param name="odataPath">The OData path of the request.</param>
    //    [AcceptVerbs(new string[] { "GET", "POST", "PUT", "PATCH", "MERGE", "DELETE" })]
    //    public virtual HttpResponseMessage HandleUnmappedRequest(ODataPath odataPath)
    //    {
    //        throw EntitySetControllerHelpers.UnmappedRequestResponse(base.get_Request(), odataPath);
    //    }

    //    /// <summary>Handles PATCH and MERGE requests to partially update a single entity in the entity set.</summary>
    //    /// <returns>The response message to send back to the client.</returns>
    //    /// <param name="key">The entity key of the entity to update.</param>
    //    /// <param name="patch">The patch representing the partial update.</param>
    //    [AcceptVerbs(new string[] { "PATCH", "MERGE" })]
    //    public virtual HttpResponseMessage Patch([FromODataUri] TKey key, Delta<TEntity> patch)
    //    {
    //        TEntity tEntity = this.PatchEntity(key, patch);
    //        return EntitySetControllerHelpers.PatchResponse<TEntity>(base.get_Request(), tEntity);
    //    }

    //    /// <summary>This method should be overridden to apply a partial update to an existing entity in the entity set.</summary>
    //    /// <returns>The updated entity.</returns>
    //    /// <param name="key">The entity key of the entity to update.</param>
    //    /// <param name="patch">The patch representing the partial update.</param>
    //    protected internal virtual TEntity PatchEntity(TKey key, Delta<TEntity> patch)
    //    {
    //        throw EntitySetControllerHelpers.PatchEntityNotImplementedResponse(base.get_Request());
    //    }

    //    /// <summary>Handles POST requests that create new entities in the entity set.</summary>
    //    /// <returns>The response message to send back to the client.</returns>
    //    /// <param name="entity">The entity to insert into the entity set.</param>
    //    public virtual HttpResponseMessage Post([FromBody] TEntity entity)
    //    {
    //        TEntity tEntity = this.CreateEntity(entity);
    //        return EntitySetControllerHelpers.PostResponse<TEntity, TKey>(this, tEntity, this.GetKey(entity));
    //    }

    //    /// <summary>Handles PUT requests that attempt to replace a single entity in the entity set.</summary>
    //    /// <returns>The response message to send back to the client.</returns>
    //    /// <param name="key">The entity key of the entity to replace.</param>
    //    /// <param name="update">The updated entity.</param>
    //    public virtual HttpResponseMessage Put([FromODataUri] TKey key, [FromBody] TEntity update)
    //    {
    //        TEntity tEntity = this.UpdateEntity(key, update);
    //        return EntitySetControllerHelpers.PutResponse<TEntity>(base.get_Request(), tEntity);
    //    }

    //    /// <summary>This method should be overridden to update an existing entity in the entity set.</summary>
    //    /// <returns>The updated entity.</returns>
    //    /// <param name="key">The entity key of the entity to update.</param>
    //    /// <param name="update">The updated entity.</param>
    //    protected internal virtual TEntity UpdateEntity(TKey key, TEntity update)
    //    {
    //        throw EntitySetControllerHelpers.UpdateEntityNotImplementedResponse(base.get_Request());
    //    }
    //}


    //internal static class EntitySetControllerHelpers
    //{
    //    private const string PreferHeaderName = "Prefer";

    //    private const string PreferenceAppliedHeaderName = "Preference-Applied";

    //    private const string ReturnContentHeaderValue = "return-content";

    //    private const string ReturnNoContentHeaderValue = "return-no-content";

    //    public static HttpResponseException CreateEntityNotImplementedResponse(HttpRequestMessage request)
    //    {
    //        ODataError oDataError = new ODataError();
    //        oDataError.set_Message(SRResources.EntitySetControllerUnsupportedCreate);
    //        oDataError.set_MessageLanguage(SRResources.EntitySetControllerErrorMessageLanguage);
    //        string entitySetControllerUnsupportedMethodErrorCode = SRResources.EntitySetControllerUnsupportedMethodErrorCode;
    //        object[] objArray = new object[] { "POST" };
    //        oDataError.set_ErrorCode(Error.Format(entitySetControllerUnsupportedMethodErrorCode, objArray));
    //        return new HttpResponseException(HttpRequestMessageExtensions.CreateResponse<ODataError>(request, HttpStatusCode.NotImplemented, oDataError));
    //    }

    //    public static HttpResponseException CreateLinkNotImplementedResponse(HttpRequestMessage request, string navigationProperty)
    //    {
    //        ODataError oDataError = new ODataError();
    //        string entitySetControllerUnsupportedCreateLink = SRResources.EntitySetControllerUnsupportedCreateLink;
    //        object[] objArray = new object[] { navigationProperty };
    //        oDataError.set_Message(Error.Format(entitySetControllerUnsupportedCreateLink, objArray));
    //        oDataError.set_MessageLanguage(SRResources.EntitySetControllerErrorMessageLanguage);
    //        oDataError.set_ErrorCode(SRResources.EntitySetControllerUnsupportedCreateLinkErrorCode);
    //        return new HttpResponseException(HttpRequestMessageExtensions.CreateResponse<ODataError>(request, HttpStatusCode.NotImplemented, oDataError));
    //    }

    //    public static ODataQueryOptions<TEntity> CreateQueryOptions<TEntity>(ApiController controller)
    //    {
    //        ODataQueryContext oDataQueryContext = new ODataQueryContext(controller.get_Request().ODataProperties().Model, typeof(TEntity));
    //        return new ODataQueryOptions<TEntity>(oDataQueryContext, controller.get_Request());
    //    }

    //    public static HttpResponseException DeleteEntityNotImplementedResponse(HttpRequestMessage request)
    //    {
    //        ODataError oDataError = new ODataError();
    //        oDataError.set_Message(SRResources.EntitySetControllerUnsupportedDelete);
    //        oDataError.set_MessageLanguage(SRResources.EntitySetControllerErrorMessageLanguage);
    //        string entitySetControllerUnsupportedMethodErrorCode = SRResources.EntitySetControllerUnsupportedMethodErrorCode;
    //        object[] objArray = new object[] { "DELETE" };
    //        oDataError.set_ErrorCode(Error.Format(entitySetControllerUnsupportedMethodErrorCode, objArray));
    //        return new HttpResponseException(HttpRequestMessageExtensions.CreateResponse<ODataError>(request, HttpStatusCode.NotImplemented, oDataError));
    //    }

    //    public static HttpResponseException DeleteLinkNotImplementedResponse(HttpRequestMessage request, string navigationProperty)
    //    {
    //        ODataError oDataError = new ODataError();
    //        string entitySetControllerUnsupportedDeleteLink = SRResources.EntitySetControllerUnsupportedDeleteLink;
    //        object[] objArray = new object[] { navigationProperty };
    //        oDataError.set_Message(Error.Format(entitySetControllerUnsupportedDeleteLink, objArray));
    //        oDataError.set_MessageLanguage(SRResources.EntitySetControllerErrorMessageLanguage);
    //        oDataError.set_ErrorCode(SRResources.EntitySetControllerUnsupportedDeleteLinkErrorCode);
    //        return new HttpResponseException(HttpRequestMessageExtensions.CreateResponse<ODataError>(request, HttpStatusCode.NotImplemented, oDataError));
    //    }

    //    public static HttpResponseMessage GetByKeyResponse<TEntity>(HttpRequestMessage request, TEntity entity)
    //    {
    //        if (entity == null)
    //        {
    //            return HttpRequestMessageExtensions.CreateResponse(request, HttpStatusCode.NotFound);
    //        }
    //        return HttpRequestMessageExtensions.CreateResponse<TEntity>(request, HttpStatusCode.OK, entity);
    //    }

    //    public static HttpResponseException GetEntityByKeyNotImplementedResponse(HttpRequestMessage request)
    //    {
    //        ODataError oDataError = new ODataError();
    //        oDataError.set_Message(SRResources.EntitySetControllerUnsupportedGetByKey);
    //        oDataError.set_MessageLanguage(SRResources.EntitySetControllerErrorMessageLanguage);
    //        oDataError.set_ErrorCode(SRResources.EntitySetControllerUnsupportedGetByKeyErrorCode);
    //        return new HttpResponseException(HttpRequestMessageExtensions.CreateResponse<ODataError>(request, HttpStatusCode.NotImplemented, oDataError));
    //    }

    //    public static HttpResponseException GetKeyNotImplementedResponse(HttpRequestMessage request)
    //    {
    //        ODataError oDataError = new ODataError();
    //        oDataError.set_Message(SRResources.EntitySetControllerUnsupportedGetKey);
    //        oDataError.set_MessageLanguage(SRResources.EntitySetControllerErrorMessageLanguage);
    //        string entitySetControllerUnsupportedMethodErrorCode = SRResources.EntitySetControllerUnsupportedMethodErrorCode;
    //        object[] objArray = new object[] { "POST" };
    //        oDataError.set_ErrorCode(Error.Format(entitySetControllerUnsupportedMethodErrorCode, objArray));
    //        return new HttpResponseException(HttpRequestMessageExtensions.CreateResponse<ODataError>(request, HttpStatusCode.NotImplemented, oDataError));
    //    }

    //    public static HttpResponseException GetNotImplementedResponse(HttpRequestMessage request)
    //    {
    //        ODataError oDataError = new ODataError();
    //        oDataError.set_Message(SRResources.EntitySetControllerUnsupportedGet);
    //        oDataError.set_MessageLanguage(SRResources.EntitySetControllerErrorMessageLanguage);
    //        string entitySetControllerUnsupportedMethodErrorCode = SRResources.EntitySetControllerUnsupportedMethodErrorCode;
    //        object[] objArray = new object[] { "GET" };
    //        oDataError.set_ErrorCode(Error.Format(entitySetControllerUnsupportedMethodErrorCode, objArray));
    //        return new HttpResponseException(HttpRequestMessageExtensions.CreateResponse<ODataError>(request, HttpStatusCode.NotImplemented, oDataError));
    //    }

    //    public static ODataPath GetODataPath(ApiController controller)
    //    {
    //        return controller.get_Request().ODataProperties().Path;
    //    }

    //    public static HttpResponseException PatchEntityNotImplementedResponse(HttpRequestMessage request)
    //    {
    //        ODataError oDataError = new ODataError();
    //        oDataError.set_Message(SRResources.EntitySetControllerUnsupportedPatch);
    //        oDataError.set_MessageLanguage(SRResources.EntitySetControllerErrorMessageLanguage);
    //        string entitySetControllerUnsupportedMethodErrorCode = SRResources.EntitySetControllerUnsupportedMethodErrorCode;
    //        object[] objArray = new object[] { "PATCH" };
    //        oDataError.set_ErrorCode(Error.Format(entitySetControllerUnsupportedMethodErrorCode, objArray));
    //        return new HttpResponseException(HttpRequestMessageExtensions.CreateResponse<ODataError>(request, HttpStatusCode.NotImplemented, oDataError));
    //    }

    //    public static HttpResponseMessage PatchResponse<TEntity>(HttpRequestMessage request, TEntity patchedEntity)
    //    {
    //        if (!EntitySetControllerHelpers.RequestPrefersReturnContent(request))
    //        {
    //            return HttpRequestMessageExtensions.CreateResponse(request, HttpStatusCode.NoContent);
    //        }
    //        HttpResponseMessage httpResponseMessage = HttpRequestMessageExtensions.CreateResponse<TEntity>(request, HttpStatusCode.OK, patchedEntity);
    //        httpResponseMessage.Headers.Add("Preference-Applied", "return-content");
    //        return httpResponseMessage;
    //    }

    //    public static HttpResponseMessage PostResponse<TEntity, TKey>(ApiController controller, TEntity createdEntity, TKey entityKey)
    //    {
    //        HttpResponseMessage httpResponseMessage = null;
    //        HttpRequestMessage request = controller.get_Request();
    //        if (!EntitySetControllerHelpers.RequestPrefersReturnNoContent(request))
    //        {
    //            httpResponseMessage = HttpRequestMessageExtensions.CreateResponse<TEntity>(request, HttpStatusCode.Created, createdEntity);
    //        }
    //        else
    //        {
    //            httpResponseMessage = HttpRequestMessageExtensions.CreateResponse(request, HttpStatusCode.NoContent);
    //            httpResponseMessage.Headers.Add("Preference-Applied", "return-no-content");
    //        }
    //        ODataPath path = request.ODataProperties().Path;
    //        if (path == null)
    //        {
    //            throw Error.InvalidOperation(SRResources.LocationHeaderMissingODataPath, new object[0]);
    //        }
    //        EntitySetPathSegment entitySetPathSegment = path.Segments.FirstOrDefault<ODataPathSegment>() as EntitySetPathSegment;
    //        if (entitySetPathSegment == null)
    //        {
    //            throw Error.InvalidOperation(SRResources.LocationHeaderDoesNotStartWithEntitySet, new object[0]);
    //        }
    //        UrlHelper url = controller.get_Url() ?? new UrlHelper(request);
    //        HttpResponseHeaders headers = httpResponseMessage.Headers;
    //        ODataPathSegment[] keyValuePathSegment = new ODataPathSegment[] { entitySetPathSegment, new KeyValuePathSegment(ODataUriUtils.ConvertToUriLiteral(entityKey, 2)) };
    //        headers.Location = new Uri(url.CreateODataLink(keyValuePathSegment));
    //        return httpResponseMessage;
    //    }

    //    public static HttpResponseMessage PutResponse<TEntity>(HttpRequestMessage request, TEntity updatedEntity)
    //    {
    //        if (!EntitySetControllerHelpers.RequestPrefersReturnContent(request))
    //        {
    //            return HttpRequestMessageExtensions.CreateResponse(request, HttpStatusCode.NoContent);
    //        }
    //        HttpResponseMessage httpResponseMessage = HttpRequestMessageExtensions.CreateResponse<TEntity>(request, HttpStatusCode.OK, updatedEntity);
    //        httpResponseMessage.Headers.Add("Preference-Applied", "return-content");
    //        return httpResponseMessage;
    //    }

    //    internal static bool RequestPrefersReturnContent(HttpRequestMessage request)
    //    {
    //        IEnumerable<string> strs = null;
    //        if (!request.Headers.TryGetValues("Prefer", out strs))
    //        {
    //            return false;
    //        }
    //        return strs.Contains<string>("return-content");
    //    }

    //    internal static bool RequestPrefersReturnNoContent(HttpRequestMessage request)
    //    {
    //        IEnumerable<string> strs = null;
    //        if (!request.Headers.TryGetValues("Prefer", out strs))
    //        {
    //            return false;
    //        }
    //        return strs.Contains<string>("return-no-content");
    //    }

    //    public static HttpResponseException UnmappedRequestResponse(HttpRequestMessage request, ODataPath odataPath)
    //    {
    //        ODataError oDataError = new ODataError();
    //        string entitySetControllerUnmappedRequest = SRResources.EntitySetControllerUnmappedRequest;
    //        object[] pathTemplate = new object[] { odataPath.PathTemplate };
    //        oDataError.set_Message(Error.Format(entitySetControllerUnmappedRequest, pathTemplate));
    //        oDataError.set_MessageLanguage(SRResources.EntitySetControllerErrorMessageLanguage);
    //        oDataError.set_ErrorCode(SRResources.EntitySetControllerUnmappedRequestErrorCode);
    //        return new HttpResponseException(HttpRequestMessageExtensions.CreateResponse<ODataError>(request, HttpStatusCode.NotImplemented, oDataError));
    //    }

    //    public static HttpResponseException UpdateEntityNotImplementedResponse(HttpRequestMessage request)
    //    {
    //        ODataError oDataError = new ODataError();
    //        oDataError.set_Message(SRResources.EntitySetControllerUnsupportedUpdate);
    //        oDataError.set_MessageLanguage(SRResources.EntitySetControllerErrorMessageLanguage);
    //        string entitySetControllerUnsupportedMethodErrorCode = SRResources.EntitySetControllerUnsupportedMethodErrorCode;
    //        object[] objArray = new object[] { "PUT" };
    //        oDataError.set_ErrorCode(Error.Format(entitySetControllerUnsupportedMethodErrorCode, objArray));
    //        return new HttpResponseException(HttpRequestMessageExtensions.CreateResponse<ODataError>(request, HttpStatusCode.NotImplemented, oDataError));
    //    }
    //}

    #endregion
}
