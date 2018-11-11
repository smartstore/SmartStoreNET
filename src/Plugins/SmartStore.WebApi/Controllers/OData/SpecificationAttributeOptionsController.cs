using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Catalog;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageCatalog")]
	public class SpecificationAttributeOptionsController : WebApiEntityController<SpecificationAttributeOption, ISpecificationAttributeService>
	{
		protected override void Insert(SpecificationAttributeOption entity)
		{
			Service.InsertSpecificationAttributeOption(entity);
		}
		protected override void Update(SpecificationAttributeOption entity)
		{
			Service.UpdateSpecificationAttributeOption(entity);
		}
		protected override void Delete(SpecificationAttributeOption entity)
		{
			Service.DeleteSpecificationAttributeOption(entity);
		}

		[WebApiQueryable]
		public SingleResult<SpecificationAttributeOption> GetSpecificationAttributeOption(int key)
		{
			return GetSingleResult(key);
		}

		// navigation properties

		[WebApiQueryable]
		public SingleResult<SpecificationAttribute> GetSpecificationAttribute(int key)
		{
			return GetRelatedEntity(key, x => x.SpecificationAttribute);
		}

		[WebApiQueryable]
		public IQueryable<ProductSpecificationAttribute> GetProductSpecificationAttributes(int key)
		{
			return GetRelatedCollection(key, x => x.ProductSpecificationAttributes);
		}
	}
}
