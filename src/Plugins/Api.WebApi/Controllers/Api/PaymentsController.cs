using SmartStore.Core.Plugins;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;
using System.Linq;
using System.Web.Http;
using System;

namespace SmartStore.Plugin.Api.WebApi.Controllers.Api
{
	[WebApiAuthenticate(Permission = "ManagePaymentMethods")]
	public class PaymentsController : ApiController
	{
		private readonly Lazy<IPluginFinder> _pluginFinder;

		public PaymentsController(Lazy<IPluginFinder> pluginFinder)
		{
			_pluginFinder = pluginFinder;
		}

		[WebApiQueryable(PagingOptional = true)]
		public IQueryable<PluginDescriptor> GetMethods()
		{
			if (!ModelState.IsValid)
				throw this.ExceptionInvalidModelState();

			var query = _pluginFinder.Value
				.GetPlugins<IPaymentMethod>(false)
				.Select(x => x.PluginDescriptor)
				.AsQueryable();

			return query;
		}
	}
}
