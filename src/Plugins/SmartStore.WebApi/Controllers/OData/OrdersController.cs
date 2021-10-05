using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Security;
using SmartStore.Services.Orders;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.Configuration;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;
using SmartStore.WebApi.Models.OData;
using SmartStore.WebApi.Services;

namespace SmartStore.WebApi.Controllers.OData
{
    [IEEE754Compatible]
    public class OrdersController : WebApiEntityController<Order, IOrderService>
    {
        private readonly Lazy<IOrderProcessingService> _orderProcessingService;
        private readonly Lazy<WebApiPdfHelper> _apiPdfHelper;

        public OrdersController(
            Lazy<IOrderProcessingService> orderProcessingService,
            Lazy<WebApiPdfHelper> apiPdfHelper)
        {
            _orderProcessingService = orderProcessingService;
            _apiPdfHelper = apiPdfHelper;
        }

        protected override IQueryable<Order> GetEntitySet()
        {
            var query =
                from x in Repository.Table
                where !x.Deleted
                select x;

            return query;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.Read)]
        public IHttpActionResult Get()
        {
            return Ok(GetEntitySet());
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.Read)]
        public IHttpActionResult Get(int key)
        {
            return Ok(GetByKey(key));
        }

        [WebApiAuthenticate(Permission = Permissions.Order.Read)]
        public IHttpActionResult GetProperty(int key, string propertyName)
        {
            return GetPropertyValue(key, propertyName);
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.Create)]
        public IHttpActionResult Post(Order entity)
        {
            var result = Insert(entity, () => Service.InsertOrder(entity));
            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.Update)]
        public async Task<IHttpActionResult> Put(int key, Order entity)
        {
            var result = await UpdateAsync(entity, key, () => Service.UpdateOrder(entity));
            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.Update)]
        public async Task<IHttpActionResult> Patch(int key, Delta<Order> model)
        {
            var result = await PartiallyUpdateAsync(key, model, entity => Service.UpdateOrder(entity));
            return result;
        }

        [WebApiAuthenticate(Permission = Permissions.Order.Delete)]
        public async Task<IHttpActionResult> Delete(int key)
        {
            var result = await DeleteAsync(key, entity => Service.DeleteOrder(entity));
            return result;
        }

        #region Navigation properties

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Read)]
        public IHttpActionResult GetCustomer(int key)
        {
            return Ok(GetRelatedEntity(key, x => x.Customer));
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.Read)]
        public IHttpActionResult GetBillingAddress(int key)
        {
            return Ok(GetRelatedEntity(key, x => x.BillingAddress));
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.Read)]
        public IHttpActionResult GetShippingAddress(int key)
        {
            return Ok(GetRelatedEntity(key, x => x.ShippingAddress));
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.Read)]
        public IHttpActionResult GetOrderNotes(int key)
        {
            return Ok(GetRelatedCollection(key, x => x.OrderNotes));
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.Read)]
        public IHttpActionResult GetShipments(int key)
        {
            return Ok(GetRelatedCollection(key, x => x.Shipments));
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.Read)]
        public IHttpActionResult GetOrderItems(int key)
        {
            return Ok(GetRelatedCollection(key, x => x.OrderItems));
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.Read)]
        public IHttpActionResult GetRedeemedRewardPointsEntry(int key)
        {
            return Ok(GetRelatedEntity(key, x => x.RedeemedRewardPointsEntry));
        }

        #endregion

        #region Actions

        public static void Init(WebApiConfigurationBroadcaster configData)
        {
            var entityConfig = configData.ModelBuilder.EntityType<Order>();

            entityConfig
                .Action("Infos")
                .Returns<OrderInfo>();

            entityConfig.Action("Pdf");

            entityConfig
                .Action("PaymentPending")
                .ReturnsFromEntitySet<Order>("Orders");

            entityConfig
                .Action("PaymentPaid")
                .ReturnsFromEntitySet<Order>("Orders")
                .Parameter<string>("PaymentMethodName");

            entityConfig
                .Action("PaymentRefund")
                .ReturnsFromEntitySet<Order>("Orders")
                .Parameter<bool>("Online");

            entityConfig
                .Action("Cancel")
                .ReturnsFromEntitySet<Order>("Orders");

            var addShipment = entityConfig
                .Action("AddShipment")
                .ReturnsFromEntitySet<Order>("Orders");
            addShipment.Parameter<string>("TrackingNumber");
            addShipment.Parameter<bool?>("SetAsShipped");

            entityConfig
                .Action("CompleteOrder")
                .ReturnsFromEntitySet<Order>("Orders");
        }

        [HttpPost]
        [WebApiAuthenticate(Permission = Permissions.Order.Read)]
        public IHttpActionResult Infos(int key)
        {
            var result = new OrderInfo();
            var entity = GetByKeyNotNull(key);

            this.ProcessEntity(() =>
            {
                result.HasItemsToDispatch = entity.HasItemsToDispatch();
                result.HasItemsToDeliver = entity.HasItemsToDeliver();
                result.CanAddItemsToShipment = entity.CanAddItemsToShipment();
            });

            return Ok(result);
        }

        [HttpPost]
        [WebApiAuthenticate(Permission = Permissions.Order.Read)]
        public IHttpActionResult Pdf(int key)
        {
            HttpResponseMessage response = null;

            this.ProcessEntity(() =>
            {
                var order = GetByKeyNotNull(key);
                var pdfData = _apiPdfHelper.Value.OrderToPdf(order);

                var fileName = Services.Localization.GetResource("Order.PdfInvoiceFileName").FormatInvariant(order.Id);
                response = _apiPdfHelper.Value.CreateResponse(Request, pdfData, fileName);
            });

            return ResponseMessage(response);
        }

        [HttpPost, WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.Update)]
        public IHttpActionResult PaymentPending(int key)
        {
            var order = GetByKeyNotNull(key);

            this.ProcessEntity(() =>
            {
                order.PaymentStatus = PaymentStatus.Pending;
                Service.UpdateOrder(order);
            });

            return Ok(order);
        }

        [HttpPost, WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.Update)]
        public IHttpActionResult PaymentPaid(int key, ODataActionParameters parameters)
        {
            var order = GetByKeyNotNull(key);

            this.ProcessEntity(() =>
            {
                var paymentMethodName = parameters.GetValueSafe<string>("PaymentMethodName");

                if (paymentMethodName != null)
                {
                    order.PaymentMethodSystemName = paymentMethodName;
                    Service.UpdateOrder(order);
                }

                _orderProcessingService.Value.MarkOrderAsPaid(order);
            });

            return Ok(order);
        }

        [HttpPost, WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.Update)]
        public IHttpActionResult PaymentRefund(int key, ODataActionParameters parameters)
        {
            var order = GetByKeyNotNull(key);

            this.ProcessEntity(() =>
            {
                var online = parameters.GetValueSafe<bool>("Online");
                if (online)
                {
                    var errors = _orderProcessingService.Value.Refund(order);

                    if (errors.Count > 0)
                        throw new SmartException(errors[0]);
                }
                else
                {
                    _orderProcessingService.Value.RefundOffline(order);
                }
            });

            return Ok(order);
        }

        [HttpPost, WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.Update)]
        public IHttpActionResult Cancel(int key)
        {
            var order = GetByKeyNotNull(key);

            this.ProcessEntity(() =>
            {
                _orderProcessingService.Value.CancelOrder(order, true);
            });

            return Ok(order);
        }

        [HttpPost, WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.EditShipment)]
        public IHttpActionResult AddShipment(int key, ODataActionParameters parameters)
        {
            var order = GetByKeyNotNull(key);

            this.ProcessEntity(() =>
            {
                if (order.CanAddItemsToShipment())
                {
                    var trackingNumber = parameters.GetValueSafe<string>("TrackingNumber");
                    var trackingUrl = parameters.GetValueSafe<string>("TrackingUrl");
                    var shipment = _orderProcessingService.Value.AddShipment(order, trackingNumber, trackingUrl, null);

                    if (shipment != null)
                    {
                        if (parameters.GetValueSafe<bool>("SetAsShipped"))
                        {
                            _orderProcessingService.Value.Ship(shipment, true);
                        }
                    }
                }
            });

            return Ok(order);
        }

        [HttpPost, WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.Update)]
        public IHttpActionResult CompleteOrder(int key)
        {
            var order = GetByKeyNotNull(key);

            this.ProcessEntity(() =>
            {
                _orderProcessingService.Value.CompleteOrder(order);
            });

            return Ok(order);
        }

        #endregion
    }
}
