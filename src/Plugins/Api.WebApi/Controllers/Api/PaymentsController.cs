using SmartStore.Core.Infrastructure;
using SmartStore.Core.Plugins;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;
using System.Linq;
using System.Web.Http;

namespace SmartStore.Plugin.Api.WebApi.Controllers.Api
{
	[WebApiAuthenticate(Permission = "ManagePaymentMethods")]
	public class PaymentsController : ApiController
	{
		private readonly IPluginFinder _pluginFinder;

		public PaymentsController()
		{
			_pluginFinder = EngineContext.Current.Resolve<IPluginFinder>();
		}

		[WebApiQueryable(PagingOptional = true)]
		public IQueryable<PluginDescriptor> GetMethods()
		{
			if (!ModelState.IsValid)
				throw this.ExceptionInvalidModelState();

			var query = _pluginFinder
				.GetPlugins<IPaymentMethod>(false)
				.Select(x => x.PluginDescriptor)
				.AsQueryable();

			return query;
		}
	}
}
