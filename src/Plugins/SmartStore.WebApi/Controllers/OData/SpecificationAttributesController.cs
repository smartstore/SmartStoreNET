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
	public class SpecificationAttributesController : WebApiEntityController<SpecificationAttribute, ISpecificationAttributeService>
	{
		protected override void Insert(SpecificationAttribute entity)
		{
			Service.InsertSpecificationAttribute(entity);
		}
		protected override void Update(SpecificationAttribute entity)
		{
			Service.UpdateSpecificationAttribute(entity);
		}
		protected override void Delete(SpecificationAttribute entity)
		{
			Service.DeleteSpecificationAttribute(entity);
		}

		[WebApiQueryable]
		public SingleResult<SpecificationAttribute> GetSpecificationAttribute(int key)
		{
			return GetSingleResult(key);
		}

		// navigation properties

		[WebApiQueryable]
		public IQueryable<SpecificationAttributeOption> GetSpecificationAttributeOptions(int key)
		{
			return GetRelatedCollection(key, x => x.SpecificationAttributeOptions);
		}
	}
}
