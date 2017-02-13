using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Routing;
using Autofac;
using SmartStore.ComponentModel;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Services;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;

namespace SmartStore.Web.Framework.WebApi
{
	public abstract class WebApiEntityController<TEntity, TService> : EntitySetController<TEntity, int>
		where TEntity : BaseEntity, new()
	{

		protected internal HttpResponseException ExceptionEntityNotFound<TKey>(TKey key)
		{
			var response = Request.CreateErrorResponse(HttpStatusCode.NotFound,
				WebApiGlobal.Error.EntityNotFound.FormatInvariant(key));

			return new HttpResponseException(response);
		}

		protected internal HttpResponseException ExceptionNotExpanded<TProperty>(Expression<Func<TEntity, TProperty>> path)
		{
			// NotFound cause of nullable properties
			var response = Request.CreateErrorResponse(HttpStatusCode.NotFound,
				WebApiGlobal.Error.PropertyNotExpanded.FormatInvariant(path.ToString()));

			return new HttpResponseException(response);
		}

		public override HttpResponseMessage HandleUnmappedRequest(ODataPath odataPath)
		{
			if (odataPath.PathTemplate.IsCaseInsensitiveEqual("~/entityset/key/property") ||
				odataPath.PathTemplate.IsCaseInsensitiveEqual("~/entityset/key/cast/property") ||
				odataPath.PathTemplate.IsCaseInsensitiveEqual("~/entityset/key/unresolved"))
			{
				if (Request.Method == HttpMethod.Get || Request.Method == HttpMethod.Post)
				{
					return UnmappedGetProperty(odataPath);
				}
			}
			else if (odataPath.PathTemplate.IsCaseInsensitiveEqual("~/entityset/key/navigation/key"))
			{
				if (Request.Method == HttpMethod.Get || Request.Method == HttpMethod.Post || Request.Method == HttpMethod.Delete)
				{
					// we ignore standard odata path cause they differ:
					// ~/entityset/key/$links/navigation (odata 3 "link"), ~/entityset/key/navigation/$ref (odata 4 "reference")

					return UnmappedGetNavigation(odataPath);
				}
			}
			else if (odataPath.PathTemplate.IsCaseInsensitiveEqual("~/entityset/key/navigation"))
			{
				if (Request.Method == HttpMethod.Delete)
				{
					return UnmappedGetNavigation(odataPath);
				}
			}

			return base.HandleUnmappedRequest(odataPath);
		}

		protected virtual internal HttpResponseMessage UnmappedGetProperty(ODataPath odataPath)
		{
			int key;

			if (!odataPath.GetNormalizedKey(1, out key))
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, WebApiGlobal.Error.NoKeyFromPath);

			var entity = GetEntityByKey(key);

			if (entity == null)
				return Request.CreateErrorResponse(HttpStatusCode.NotFound, WebApiGlobal.Error.EntityNotFound.FormatInvariant(key));

			FastProperty prop = null;
			string propertyName = null;
			var lastSegment = odataPath.Segments.Last();
			var propertySegment = (lastSegment as PropertyAccessPathSegment);

			if (propertySegment == null)
				propertyName = lastSegment.ToString();
			else
				propertyName = propertySegment.PropertyName;

			if (propertyName.HasValue())
				prop = FastProperty.GetProperty(entity.GetType(), propertyName);

			if (prop == null)
				return UnmappedGetProperty(entity, propertyName ?? "");

			var propertyValue = prop.GetValue(entity);

			return Request.CreateResponse(HttpStatusCode.OK, prop.Property.PropertyType, propertyValue);
		}

		protected virtual internal HttpResponseMessage UnmappedGetProperty(TEntity entity, string propertyName)
		{
			return Request.CreateErrorResponse(HttpStatusCode.BadRequest, WebApiGlobal.Error.PropertyNotFound.FormatInvariant(propertyName));
		}

		protected virtual internal HttpResponseMessage UnmappedGetNavigation(ODataPath odataPath)
		{
			int key, relatedKey;

			if (!odataPath.GetNormalizedKey(1, out key))
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, WebApiGlobal.Error.NoKeyFromPath);

			var navigationProperty = odataPath.GetNavigation(2);

			if (navigationProperty.IsEmpty())
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, WebApiGlobal.Error.NoNavigationFromPath);

			if (!odataPath.GetNormalizedKey(3, out relatedKey) && Request.Method != HttpMethod.Delete)
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, WebApiGlobal.Error.NoRelatedKeyFromPath);

			var methodName = string.Concat("Navigation", navigationProperty);
			var methodInfo = GetType().GetMethod(methodName);

			if (methodInfo != null)
			{
				HttpResponseMessage response = null;

				this.ProcessEntity(() =>
				{
					response = (HttpResponseMessage)methodInfo.Invoke(this, new object[] { key, relatedKey });
				});

				return response;
			}
			return base.HandleUnmappedRequest(odataPath);
		}

		/// <summary>
		/// Auto injected by Autofac
		/// </summary>
		public virtual IRepository<TEntity> Repository
		{
			get;
			set;
		}

		/// <summary>
		/// Auto injected by Autofac
		/// </summary>
		public virtual TService Service
		{
			get;
			set;
		}

		/// <summary>
		/// Auto injected by Autofac
		/// </summary>
		public virtual ICommonServices Services
		{
			get;
			set;
		}

		public override IQueryable<TEntity> Get()
		{
			if (!ModelState.IsValid)
				throw this.ExceptionInvalidModelState();
			
			return this.GetEntitySet();
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

		protected override int GetKey(TEntity entity)
		{
			if (!ModelState.IsValid)
				throw this.ExceptionInvalidModelState();

			return entity.Id;
		}

		protected override TEntity GetEntityByKey(int key)
		{
			if (!ModelState.IsValid)
				throw this.ExceptionInvalidModelState();

			return this.Repository.GetById(key);
		}

		protected internal virtual TEntity GetEntityByKeyNotNull(int key)
		{
			var entity = GetEntityByKey(key);

			if (entity == null)
				throw ExceptionEntityNotFound(key);

			return entity;
		}

		protected internal virtual SingleResult<TEntity> GetSingleResult(int key)
		{
			if (!ModelState.IsValid)
				throw this.ExceptionInvalidModelState();

			return SingleResult.Create(GetEntitySet().Where(x => x.Id == key));
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

			var expression = path.Compile();
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

			var ctx = (DbContext)Repository.Context;
			var product = GetEntityByKey(key);
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

		protected internal virtual SingleResult<TElement> GetRelatedEntity<TElement>(
			int key,
			string navigationProperty)
		{
			Guard.NotEmpty(navigationProperty, nameof(navigationProperty));

			var ctx = (DbContext)Repository.Context;
			var product = GetEntityByKey(key);
			var entry = ctx.Entry(product);
			var query = entry.Reference(navigationProperty).Query().Cast<TElement>();

			return SingleResult.Create(query);
		}

		public override HttpResponseMessage Post(TEntity entity)
		{
			var response = Request.CreateResponse(HttpStatusCode.OK, CreateEntity(entity));

			try
			{
				var entityUrl = Url.CreateODataLink(
					new EntitySetPathSegment(entity.GetType().Name.EnsureEndsWith("s")),
					new KeyValuePathSegment(entity.Id.ToString())
				);

				response.Headers.Location = new Uri(entityUrl);
			}
			catch { }

			return response;
		}

		protected override TEntity CreateEntity(TEntity entity)
		{
			if (!ModelState.IsValid)
				throw this.ExceptionInvalidModelState();

			if (entity == null)
				throw this.ExceptionBadRequest(WebApiGlobal.Error.NoDataToInsert);

			Insert(FulfillPropertiesOn(entity));

			return entity;
		}

		protected internal virtual void Insert(TEntity entity)
		{
			Repository.Insert(entity);
		}

		public override HttpResponseMessage Put(int key, TEntity update)
		{
			return Request.CreateResponse(HttpStatusCode.OK, UpdateEntity(key, update));
		}

		protected override TEntity UpdateEntity(int key, TEntity update)
		{
			if (!ModelState.IsValid)
				throw this.ExceptionInvalidModelState();

			var originalEntity = GetEntityByKeyNotNull(key);
		
			var context = ((IObjectContextAdapter)this.Repository.Context).ObjectContext;
			var container = context.MetadataWorkspace.GetEntityContainer(context.DefaultContainerName, DataSpace.CSpace);

			string entityName = typeof(TEntity).Name;
			string entitySetName = container.BaseEntitySets.First(x => x.ElementType.Name == entityName).Name;
			
			update.Id = key;
			var entity = context.ApplyCurrentValues(entitySetName, update);

			Update(FulfillPropertiesOn(entity));

			return entity;
		}

		protected internal virtual void Update(TEntity entity)
		{
			Repository.Update(entity);
		}

		public override HttpResponseMessage Patch(int key, Delta<TEntity> patch)
		{
			return Request.CreateResponse(HttpStatusCode.OK, PatchEntity(key, patch));
		}

		protected override TEntity PatchEntity(int key, Delta<TEntity> patch)
		{
			if (!ModelState.IsValid)
				throw this.ExceptionInvalidModelState();

			var entity = GetEntityByKeyNotNull(key);

			if (patch != null)
				patch.Patch(entity);

			Update(FulfillPropertiesOn(entity));

			return entity;
		}

		public override void Delete(int key)
		{
			if (!ModelState.IsValid)
				throw this.ExceptionInvalidModelState();

			var entity = GetEntityByKeyNotNull(key);

			Delete(entity);
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
			var formatters = ODataMediaTypeFormatters.Create()
				.Select(formatter => formatter.GetPerRequestFormatterInstance(typeof(T), Request, Request.Content.Headers.ContentType));

			return Request.Content.ReadAsAsync<T>(formatters).Result;
		}
	}
}
