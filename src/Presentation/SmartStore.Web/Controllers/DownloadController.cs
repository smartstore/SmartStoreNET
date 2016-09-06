using System;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Html;
using SmartStore.Services.Catalog;
using SmartStore.Services.Media;
using SmartStore.Services.Orders;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Web.Controllers
{
	public partial class DownloadController : PublicControllerBase
	{
		private readonly IDownloadService _downloadService;
		private readonly IProductService _productService;
		private readonly IOrderService _orderService;
		private readonly IWorkContext _workContext;

		private readonly CustomerSettings _customerSettings;

		public DownloadController(
			IDownloadService downloadService,
			IProductService productService,
			IOrderService orderService,
			IWorkContext workContext,
			CustomerSettings customerSettings)
		{
			this._downloadService = downloadService;
			this._productService = productService;
			this._orderService = orderService;
			this._workContext = workContext;
			this._customerSettings = customerSettings;
		}

		private ActionResult GetFileContentResultFor(Download download, Product product, byte[] data)
		{
			if (data == null || data.LongLength == 0)
				return Content(T("Common.Download.NoDataAvailable"));

			var fileName = (download.Filename.HasValue() ? download.Filename : download.Id.ToString());
			var contentType = (download.ContentType.HasValue() ? download.ContentType : "application/octet-stream");

			return new FileContentResult(data, contentType)
			{
				FileDownloadName = fileName + download.Extension
			};
		}

		private ActionResult GetFileContentResultFor(Download download, Product product)
		{
			return GetFileContentResultFor(download, product, _downloadService.LoadDownloadBinary(download));
		}


		public ActionResult Sample(int id /* productId */)
        {
            var product = _productService.GetProductById(id);
            if (product == null)
				return HttpNotFound();

			if (!product.HasSampleDownload)
				return Content(T("Common.Download.HasNoSample"));

			var download = _downloadService.GetDownloadById(product.SampleDownloadId.GetValueOrDefault());
            if (download == null)
                return Content(T("Common.Download.SampleNotAvailable"));

            if (download.UseDownloadUrl)
                return new RedirectResult(download.DownloadUrl);

			return GetFileContentResultFor(download, product);
        }

		public ActionResult GetDownload(Guid id /* orderItemId */, bool agree = false)
        {
			if (id == Guid.Empty)
				return HttpNotFound();

			var orderItem = _orderService.GetOrderItemByGuid(id);
            if (orderItem == null)
				return HttpNotFound();

            var order = orderItem.Order;
            var product = orderItem.Product;
            if (!_downloadService.IsDownloadAllowed(orderItem))
                return Content(T("Common.Download.NotAllowed"));

            if (_customerSettings.DownloadableProductsValidateUser)
            {
                if (_workContext.CurrentCustomer == null)
                    return new HttpUnauthorizedResult();

                if (order.CustomerId != _workContext.CurrentCustomer.Id)
                    return Content(T("Account.CustomerOrders.NotYourOrder"));
            }

            var download = _downloadService.GetDownloadById(product.DownloadId);
            if (download == null)
				return Content(T("Common.Download.NoDataAvailable"));

			if (product.HasUserAgreement && !agree)
				return RedirectToAction("UserAgreement", "Customer", new { id = id });

            if (!product.UnlimitedDownloads && orderItem.DownloadCount >= product.MaxNumberOfDownloads)
                return Content(T("Common.Download.MaxNumberReached", product.MaxNumberOfDownloads));
            
            if (download.UseDownloadUrl)
            {
                orderItem.DownloadCount++;
                _orderService.UpdateOrder(order);

                return new RedirectResult(download.DownloadUrl);
            }
            else
            {
				var data = _downloadService.LoadDownloadBinary(download);

				if (data == null || data.LongLength == 0)
                    return Content(T("Common.Download.NoDataAvailable"));

                orderItem.DownloadCount++;
                _orderService.UpdateOrder(order);

				return GetFileContentResultFor(download, product, data);
            }
        }

		public ActionResult GetLicense(Guid id /* orderItemId */)
        {
			if (id == Guid.Empty)
				return HttpNotFound();
			
			var orderItem = _orderService.GetOrderItemByGuid(id);
            if (orderItem == null)
				return HttpNotFound();

            var order = orderItem.Order;
            var product = orderItem.Product;

            if (!_downloadService.IsLicenseDownloadAllowed(orderItem))
                return Content(T("Common.Download.NotAllowed"));

            if (_customerSettings.DownloadableProductsValidateUser)
            {
                if (_workContext.CurrentCustomer == null)
                    return new HttpUnauthorizedResult();

                if (order.CustomerId != _workContext.CurrentCustomer.Id)
                    return Content(T("Account.CustomerOrders.NotYourOrder"));
            }

            var download = _downloadService.GetDownloadById(orderItem.LicenseDownloadId.HasValue ? orderItem.LicenseDownloadId.Value : 0);
            if (download == null)
                return Content(T("Common.Download.NotAvailable"));
            
            if (download.UseDownloadUrl)
                return new RedirectResult(download.DownloadUrl);

			return GetFileContentResultFor(download, product);
		}

        public ActionResult GetFileUpload(Guid downloadId)
        {
            var download = _downloadService.GetDownloadByGuid(downloadId);
            if (download == null)
                return Content(T("Common.Download.NotAvailable"));

            if (download.UseDownloadUrl)
                return new RedirectResult(download.DownloadUrl);

			return GetFileContentResultFor(download, null);
		}

		public ActionResult GetUserAgreement(int productId, bool? asPlainText)
		{
			var product = _productService.GetProductById(productId);
			if (product == null)
				return Content(T("Products.NotFound", productId));

			if (!product.IsDownload || !product.HasUserAgreement || product.UserAgreementText.IsEmpty())
				return Content(T("DownloadableProducts.HasNoUserAgreement"));

			if (asPlainText ?? false)
			{
				var agreement = HtmlUtils.ConvertHtmlToPlainText(product.UserAgreementText);
				agreement = HtmlUtils.StripTags(HttpUtility.HtmlDecode(agreement));

				return Content(agreement);
			}

			return Content(product.UserAgreementText);
		}
	}
}
