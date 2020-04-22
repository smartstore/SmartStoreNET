using System.Web.Http;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Security;
using SmartStore.Services.Messages;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class NewsLetterSubscriptionsController : WebApiEntityController<NewsLetterSubscription, INewsLetterSubscriptionService>
    {
        // There is no insert permission because a subscription is always created by customer.
        [WebApiAuthenticate(Permission = Permissions.Promotion.Newsletter.Update)]
        protected override void Insert(NewsLetterSubscription entity)
        {
            var publishEvent = this.GetQueryStringValue("publishevent", true);

            Service.InsertNewsLetterSubscription(entity, publishEvent);
        }

        [WebApiAuthenticate(Permission = Permissions.Promotion.Newsletter.Update)]
        protected override void Update(NewsLetterSubscription entity)
        {
            var publishEvent = this.GetQueryStringValue("publishevent", true);

            Service.UpdateNewsLetterSubscription(entity, publishEvent);
        }

        [WebApiAuthenticate(Permission = Permissions.Promotion.Newsletter.Delete)]
        protected override void Delete(NewsLetterSubscription entity)
        {
            Service.DeleteNewsLetterSubscription(entity);
        }

        [WebApiQueryable]
        public SingleResult<NewsLetterSubscription> GetNewsLetterSubscription(int key)
        {
            return GetSingleResult(key);
        }
    }
}