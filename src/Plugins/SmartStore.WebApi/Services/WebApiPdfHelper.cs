using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Fakes;
using SmartStore.Services;
using SmartStore.Services.Pdf;
using SmartStore.Utilities;
using SmartStore.Web.Controllers;
using SmartStore.Web.Framework.Pdf;
using SmartStore.Web.Models.Order;

namespace SmartStore.WebApi.Services
{
    public class WebApiPdfHelper
    {
        private readonly ICommonServices _services;
        private readonly IPdfConverter _pdfConverter;
        private readonly OrderHelper _orderHelper;

        public WebApiPdfHelper(
            ICommonServices services,
            IPdfConverter pdfConverter,
            OrderHelper orderHelper)
        {
            _services = services;
            _pdfConverter = pdfConverter;
            _orderHelper = orderHelper;
        }

        private ControllerContext CreateControllerContext()
        {
            var context = new HttpContextWrapper(HttpContext.Current);
            var fakeController = new FakeController();

            var routeData = new RouteData();
            routeData.Values.Add("controller", "Home");
            routeData.Values.Add("action", "Index");
            routeData.Values.Add("area", "");

            var controllerContext = new ControllerContext(new RequestContext(context, routeData), fakeController);
            fakeController.ControllerContext = controllerContext;

            return controllerContext;
        }

        //private string Render(ControllerContext controllerContext, RazorView view, object model)
        //{
        //    //http://forums.asp.net/t/1888849.aspx?Render+PartialView+without+ControllerContext
        //    //https://weblog.west-wind.com/posts/2012/May/30/Rendering-ASPNET-MVC-Views-to-String

        //    using (var writer = new StringWriter())
        //    {
        //        view.Render(new ViewContext(controllerContext, view, new ViewDataDictionary(model), new TempDataDictionary(), writer), writer);
        //        return writer.ToString();
        //    }
        //}

        public byte[] OrderToPdf(Order order)
        {
            Guard.NotNull(order, nameof(order));

            var controllerContext = CreateControllerContext();
            var pdfSettings = _services.Settings.LoadSetting<PdfSettings>(order.StoreId);
            var routeValues = new RouteValueDictionary(new { storeId = order.StoreId, lid = _services.WorkContext.WorkingLanguage.Id, area = "" });

            var model = _orderHelper.PrepareOrderDetailsModel(order);
            var models = new List<OrderDetailsModel> { model };

            var settings = new PdfConvertSettings();
            settings.Size = pdfSettings.LetterPageSizeEnabled ? PdfPageSize.Letter : PdfPageSize.A4;
            settings.Margins = new PdfPageMargins { Top = 35, Bottom = 35 };
            settings.Page = new PdfViewContent(OrderHelper.OrderDetailsPrintViewPath, models, controllerContext);
            settings.Header = new PdfRouteContent("PdfReceiptHeader", "Common", routeValues, controllerContext);
            settings.Footer = new PdfRouteContent("PdfReceiptFooter", "Common", routeValues, controllerContext);

            var pdfData = _pdfConverter.Convert(settings);
            return pdfData;
        }

        public HttpResponseMessage CreateResponse(HttpRequestMessage request, byte[] pdfData, string fileName)
        {
            Guard.NotNull(pdfData, nameof(pdfData));

            var response = request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StreamContent(new MemoryStream(pdfData));
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            response.Content.Headers.ContentLength = pdfData.LongLength;

            if (fileName.HasValue())
            {
                if (ContentDispositionHeaderValue.TryParse($"inline; filename=\"{PathHelper.SanitizeFileName(fileName)}\"", out ContentDispositionHeaderValue contentDisposition))
                {
                    response.Content.Headers.ContentDisposition = contentDisposition;
                }
            }

            return response;
        }
    }
}