using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Formatter;
using Autofac;
using SmartStore.ComponentModel;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Services;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;

namespace SmartStore.Web.Framework.WebApi.OData
{
    /// <summary>
    /// Base class for API OData controller.
    /// </summary>
    /// <typeparam name="TEntity">The entity of the endpoint.</typeparam>
    /// <typeparam name="TService">The service interface belonging to the entity.</typeparam>
    public abstract class WebApiEntityController<TEntity, TService> : ODataController
        where TEntity : BaseEntity, new()
    {
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

        protected internal virtual IQueryable<TEntity> GetEntitySet()
        {
            return Repository.Table;
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

        protected internal virtual TEntity GetByKey(int key)
        {
            if (key == 0)
            {
                return null;
            }

            return GetEntitySet().FirstOrDefault(x => x.Id == key);
        }

        protected internal virtual TEntity GetByKeyNotNull(int key)
        {
            if (!ModelState.IsValid)
            {
                throw this.InvalidModelStateException();
            }

            var entity = GetByKey(key);
            if (entity == null)
            {
                throw Request.NotFoundException(WebApiGlobal.Error.EntityNotFound.FormatInvariant(key));
            }

            return entity;
        }

        protected internal virtual TEntity GetExpandedEntity<TProperty>(int key, Expression<Func<TEntity, TProperty>> path)
        {
            if (!ModelState.IsValid)
            {
                throw this.InvalidModelStateException();
            }

            var query = GetExpandedEntitySet<TProperty>(path);
            var entity = query.FirstOrDefault(x => x.Id == key);

            if (entity == null)
            {
                throw Request.NotFoundException(WebApiGlobal.Error.EntityNotFound.FormatInvariant(key));
            }

            return entity;
        }

        protected internal virtual TEntity GetExpandedEntity(int key, string properties)
        {
            if (!ModelState.IsValid)
            {
                throw this.InvalidModelStateException();
            }

            var query = GetExpandedEntitySet(properties);
            var entity = query.FirstOrDefault(x => x.Id == key);

            if (entity == null)
            {
                throw Request.NotFoundException(WebApiGlobal.Error.EntityNotFound.FormatInvariant(key));
            }

            return entity;
        }

        protected internal virtual IQueryable<TCollection> GetRelatedCollection<TCollection>(
            int key,
            Expression<Func<TEntity, IEnumerable<TCollection>>> navigationProperty)
        {
            Guard.NotNull(navigationProperty, nameof(navigationProperty));

            var query = GetEntitySet().Where(x => x.Id.Equals(key));

            return query.SelectMany(navigationProperty);
        }

        protected internal virtual TElement GetRelatedEntity<TElement>(
            int key,
            Expression<Func<TEntity, TElement>> navigationProperty)
        {
            Guard.NotNull(navigationProperty, nameof(navigationProperty));

            var query = GetEntitySet().Where(x => x.Id.Equals(key)).Select(navigationProperty);

            return query.FirstOrDefault();
        }

        protected internal virtual IHttpActionResult GetPropertyValue(int key, string propertyName)
        {
            var entity = GetByKey(key);
            if (entity == null)
            {
                return NotFound();
            }

            var prop = FastProperty.GetProperty(entity.GetType(), propertyName);
            if (prop == null)
            {
                return BadRequest(WebApiGlobal.Error.PropertyNotFound.FormatInvariant(propertyName.EmptyNull()));
            }

            var propertyValue = prop.GetValue(entity);

            var response = Request.CreateResponse(HttpStatusCode.OK, prop.Property.PropertyType, propertyValue);
            return ResponseMessage(response);
        }

        protected internal virtual new IHttpActionResult Ok<T>(T content)
        {
            if (content == null && !typeof(T).IsNullable(out _))
            {
                return NotFound();
            }

            return base.Ok(content);
        }

        protected internal virtual IHttpActionResult Insert(TEntity entity, Action insert)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            entity = FulfillPropertiesOn(entity);
            insert();

            return Created(entity);
        }

        /// <summary>
        /// Fully updates an entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="key">Key (ID) of the entity.</param>
        /// <param name="update">Update action.</param>
        /// <returns>
        /// Depends on consumers request "Prefer" header. HTTP 200 OK including updated entity by default.
        /// <see cref="https://docs.microsoft.com/en-us/rest/api/storageservices/setting-the-prefer-header-to-manage-response-echo-on-insert-operations"/>
        /// </returns>
        protected internal virtual async Task<IHttpActionResult> UpdateAsync(TEntity entity, int key, Action update)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (key != entity.Id)
            {
                return BadRequest($"Missing or differing key {key}.");
            }

            entity = FulfillPropertiesOn(entity);

            try
            {
                update();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await Repository.GetByIdAsync(key) == null)
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            if (Request?.Headers?.Contains("Prefer") ?? false)
            {
                return Updated(entity);
            }

            // OData V3 backward compatibility.
            // Avoid HTTP 204 No Content by default.
            return Ok(entity);
        }

        /// <summary>
        /// Partially updates an entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="key">Key (ID) of the entity.</param>
        /// <param name="update">Update action.</param>
        /// <returns>
        /// Depends on consumers request "Prefer" header. HTTP 200 OK including updated entity by default.
        /// <see cref="https://docs.microsoft.com/en-us/rest/api/storageservices/setting-the-prefer-header-to-manage-response-echo-on-insert-operations"/>
        /// </returns>
        protected internal virtual async Task<IHttpActionResult> PartiallyUpdateAsync(int key, Delta<TEntity> model, Action<TEntity> update)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var entity = await Repository.GetByIdAsync(key);
            if (entity == null)
            {
                return NotFound();
            }

            model?.Patch(entity);
            entity = FulfillPropertiesOn(entity);

            try
            {
                update(entity);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await Repository.GetByIdAsync(key) == null)
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            if (Request?.Headers?.Contains("Prefer") ?? false)
            {
                return Updated(entity);
            }

            // OData V3 backward compatibility.
            // Avoid HTTP 204 No Content by default.
            return Ok(entity);
        }

        /// <summary>
        /// Deletes an entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="delete">Delete action.</param>
        /// <returns>HTTP 204 No Content.</returns>
        protected internal virtual async Task<IHttpActionResult> DeleteAsync(int key, Action<TEntity> delete)
        {
            var entity = await Repository.GetByIdAsync(key);
            if (entity == null)
            {
                return NotFound();
            }

            delete(entity);

            return StatusCode(HttpStatusCode.NoContent);
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
                throw Request.UnprocessableEntityException(ex.Message);
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
}
