using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;
using System.Web.Http.OData.Routing;
using Microsoft.Data.OData;
using Autofac;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Web.Framework.WebApi
{

    public abstract class GenericApiController<TEntity> : EntitySetController<TEntity, int>
        where TEntity : BaseEntity, new()
    {
        private IRepository<TEntity> _repository;

        protected internal IRepository<TEntity> Repository
        {
            get 
            {
                if (_repository == null)
                {
                    _repository = this.CreateRepository();
                }
                return _repository;
            }
        }

        protected virtual IRepository<TEntity> CreateRepository()
        {
            var repository = EngineContext.Current.Resolve<IRepository<TEntity>>();
            repository.Context.ProxyCreationEnabled = false;
            return repository;
        }

        protected override int GetKey(TEntity entity)
        {
            return entity.Id;
        }

        public override IQueryable<TEntity> Get()
        {
            return this.GetEntitySet();
        }

        protected internal virtual IQueryable<TEntity> GetEntitySet()
        {
            return this.Repository.Table;
        }

        protected override TEntity GetEntityByKey(int key)
        {
            return this.GetEntitySet().FirstOrDefault(x => x.Id == key);
        }

        protected override TEntity CreateEntity(TEntity entity)
        {
            this.Repository.Insert(entity);
            return entity;
        }

        public override void Delete(int key)
        {
            TEntity toDelete = this.GetEntityByKey(key);
            if (toDelete == null)
            {
                throw GetEntityNotFoundResponse(key);
            }
            this.Repository.Delete(toDelete);
        }

        protected override TEntity PatchEntity(int key, Delta<TEntity> patch)
        {
            TEntity toPatch = this.GetEntityByKey(key);
            if (toPatch == null)
            {
                throw GetEntityNotFoundResponse(key);
            }
            patch.Patch(toPatch);
            this.Repository.Update(toPatch);
            return toPatch;
        }

        protected override TEntity UpdateEntity(int key, TEntity update)
        {
            TEntity toUpdate = this.GetEntityByKey(key);
            if (toUpdate == null)
            {
                throw GetEntityNotFoundResponse(key);
            }
            toUpdate.Id = key; // ignore the ID in the entity use the ID in the URL.

            this.Repository.Update(toUpdate);
            return toUpdate;
        }

        protected internal HttpResponseException GetEntityNotFoundResponse<TKey>(TKey key)
        {
            return new HttpResponseException(
                base.Request.CreateResponse(
                    HttpStatusCode.NotFound,
                    new ODataError
                    {
                        Message = String.Format("Entity with key '{0}' could not be found.", key),
                        MessageLanguage = "en-US",
                        ErrorCode = "Entity not found."
                    }));
        }

    }

    #region First Try

    //public class GenericApiController<TEntity, TModel> : ApiController
    //    where TEntity : BaseEntity, new()
    //    where TModel : class, new()
    //{

    //    #region OData actions

    //    // GET api/entitysetname
    //    /// <summary>
    //    /// This method should be overridden to handle GET requests that attempt to retrieve entities from the entity set.
    //    /// </summary>
    //    /// <returns>The matching entities from the entity set.</returns>
    //    public virtual HttpResponseMessage Get()
    //    {
    //        var query = this.GetEntitySet();

    //        if (query == null)
    //        {
    //            // TODO
    //        }

    //        var queryOptions = GenericApiControllerHelpers.CreateQueryOptions<TEntity>(this);
    //        if (queryOptions != null)
    //        {
    //            query = this.ApplyQueryOptions(queryOptions, query);
    //        }

    //        //var model = query.ToArray().Select(x => this.ToModel(x));

    //        return this.Request.CreateResponse(HttpStatusCode.OK, query);
    //    }

    //    // GET api/entitysetname/1
    //    /// <summary>
    //    /// Handles GET requests that attempt to retrieve an individual entity by id from the entity set.
    //    /// </summary>
    //    /// <param name="key">The entity id of the entity to retrieve.</param>
    //    /// <returns>The response message to send back to the client.</returns>
    //    public virtual HttpResponseMessage Get([FromODataUri] int key)
    //    {
    //        TEntity entity = GetEntityById(key);
            
    //        //TModel model = null;
    //        //if (entity != null)
    //        //{
    //        //    model = this.ToModel(entity);
    //        //}

    //        return GenericApiControllerHelpers.GetByKeyResponse(Request, entity /* model */);
    //    }

    //    #endregion

    //    #region overridable core members

    //    protected internal virtual bool Authorize()
    //    {
    //        return true; // TODO: Implement
    //    }

    //    protected internal virtual IQueryable<TEntity> ApplyQueryOptions(ODataQueryOptions<TEntity> queryOptions, IQueryable<TEntity> entitySet) 
    //    {
    //        var query = queryOptions.ApplyTo(entitySet) as IQueryable<TEntity>;
    //        return query;
    //    }

    //    /// <summary>
    //    /// This method should be overridden to get the entity key of the specified entity.
    //    /// </summary>
    //    /// <param name="entity">The entity.</param>
    //    /// <returns>The entity key value</returns>
    //    protected internal virtual object GetKey(TEntity entity)
    //    {
    //        return entity.Id;
    //    }

    //    protected internal virtual TModel ToModel(TEntity entity) 
    //    {
    //        Guard.ArgumentNotNull(entity, "entity");

    //        return Mapper.Map(entity, typeof(TEntity), typeof(TModel)) as TModel;
    //    }

    //    protected internal virtual TEntity ToEntity(TModel model)
    //    {
    //        Guard.ArgumentNotNull(model, "model");

    //        return Mapper.Map(model, typeof(TModel), typeof(TEntity)) as TEntity;
    //    }

    //    /// <summary>
    //    /// This method should be overridden to retrieve the entity set.
    //    /// </summary>
    //    /// <param name="key">The entity key of the entity to retrieve.</param>
    //    /// <returns>The retrieved entity set.</returns>
    //    protected internal virtual IQueryable<TEntity> GetEntitySet()
    //    {
    //        throw GenericApiControllerHelpers.GetNotImplementedResponse(Request);
    //    }

    //    /// <summary>
    //    /// This method should be overridden to retrieve an entity by id from the entity set.
    //    /// </summary>
    //    /// <param name="id">The entity id of the entity to retrieve.</param>
    //    /// <returns>The retrieved entity, or <c>null</c> if an entity with the specified entity id cannot be found in the entity set.</returns>
    //    protected internal virtual TEntity GetEntityById(int id)
    //    {
    //        throw GenericApiControllerHelpers.GetEntityByKeyNotImplementedResponse(Request);
    //    }

    //    /// <summary>
    //    /// This method should be overridden to create a new entity in the entity set.
    //    /// </summary>
    //    /// <param name="entity">The entity to add to the entity set.</param>
    //    /// <returns>The created entity.</returns>
    //    protected internal virtual TEntity CreateEntity(TEntity entity)
    //    {
    //        throw GenericApiControllerHelpers.CreateEntityNotImplementedResponse(Request);
    //    }

    //    /// <summary>
    //    /// This method should be overridden to update an existing entity in the entity set.
    //    /// </summary>
    //    /// <param name="id">The entity id of the entity to update.</param>
    //    /// <param name="update">The updated entity.</param>
    //    /// <returns>The updated entity.</returns>
    //    protected internal virtual TEntity UpdateEntity(int id, TEntity update)
    //    {
    //        throw GenericApiControllerHelpers.UpdateEntityNotImplementedResponse(Request);
    //    }

    //    /// <summary>
    //    /// This method should be overridden to apply a partial update to an existing entity in the entity set.
    //    /// </summary>
    //    /// <param name="id">The entity id of the entity to update.</param>
    //    /// <param name="patch">The patch representing the partial update.</param>
    //    /// <returns>The updated entity.</returns>
    //    protected internal virtual TEntity PatchEntity(int id, Delta<TEntity> patch)
    //    {
    //        throw GenericApiControllerHelpers.PatchEntityNotImplementedResponse(Request);
    //    }

    //    /// <summary>
    //    /// This method should be overridden to handle all unmapped OData requests.
    //    /// </summary>
    //    /// <param name="odataPath">The OData path of the request.</param>
    //    /// <returns>The response message to send back to the client.</returns>
    //    [AcceptVerbs("GET", "POST", "PUT", "PATCH", "MERGE", "DELETE")]
    //    public virtual HttpResponseMessage HandleUnmappedRequest(ODataPath odataPath)
    //    {
    //        throw GenericApiControllerHelpers.UnmappedRequestResponse(Request, odataPath);
    //    }

    //    #endregion

    //    #region Utilities

    //    /// <summary>
    //    /// Gets the OData path of the current request.
    //    /// </summary>
    //    public ODataPath ODataPath
    //    {
    //        get
    //        {
    //            return GenericApiControllerHelpers.GetODataPath(this);
    //        }
    //    }

    //    /// <summary>
    //    /// Gets the OData query options of the current request.
    //    /// </summary>
    //    public ODataQueryOptions<TEntity> QueryOptions
    //    {
    //        get
    //        {
    //            return GenericApiControllerHelpers.CreateQueryOptions<TEntity>(this);
    //        }
    //    }

    //    #endregion

    //}

    #endregion

}
