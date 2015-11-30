using SmartStore.Core.Plugins;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;
using System.Linq;
using System.Web.Http;
using System;

namespace SmartStore.WebApi.Controllers.Api
{
	[WebApiAuthenticate(Permission = "ManagePaymentMethods")]
	public class PaymentsController : ApiController
	{
		private readonly Lazy<IProviderManager> _providerManager;

		public PaymentsController(Lazy<IProviderManager> providerManager)
		{
			this._providerManager = providerManager;
		}

		[WebApiQueryable(PagingOptional = true)]
		public IQueryable<ProviderMetadata> GetMethods()
		{
			if (!ModelState.IsValid)
				throw this.ExceptionInvalidModelState();

			var query = _providerManager.Value
				.GetAllProviders<IPaymentMethod>()
				.Select(x => x.Metadata)
				.AsQueryable();

			return query;
		}
	}
}
