using System.Web.Http;
using SmartStore.Core.Domain.Common;
using SmartStore.Services.Common;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageMaintenance")]
	public class GenericAttributesController : WebApiEntityController<GenericAttribute, IGenericAttributeService>
	{
		protected override void Insert(GenericAttribute entity)
		{
			Service.InsertAttribute(entity);
		}
		protected override void Update(GenericAttribute entity)
		{
			Service.UpdateAttribute(entity);
		}
		protected override void Delete(GenericAttribute entity)
		{
			Service.DeleteAttribute(entity);
		}

		[WebApiQueryable]
		public SingleResult<GenericAttribute> GetGenericAttribute(int key)
		{
			return GetSingleResult(key);
		}
	}
}
