using System;
using System.Linq;
using System.Web.Http;
using SmartStore.Core.Plugins;
using SmartStore.Core.Security;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.Api
{
    public class PaymentsController : ApiController
    {
        private readonly Lazy<IProviderManager> _providerManager;

        public PaymentsController(Lazy<IProviderManager> providerManager)
        {
            _providerManager = providerManager;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.PaymentMethod.Read)]
        public IQueryable<ProviderMetadata> GetMethods()
        {
            if (!ModelState.IsValid)
                throw this.InvalidModelStateException();

            var query = _providerManager.Value
                .GetAllProviders<IPaymentMethod>()
                .Select(x => x.Metadata)
                .AsQueryable();

            return query;
        }
    }
}
