using System;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
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

        public DownloadController(IDownloadService downloadService, IProductService productService,
            IOrderService orderService, IWorkContext workContext, CustomerSettings customerSettings)
        {
            this._downloadService = downloadService;
            this._productService = productService;
            this._orderService = orderService;
            this._workContext = workContext;
            this._customerSettings = customerSettings;
        }

		public ActionResult Sample(int id /* productId */)
        {
            var product = _productService.GetProductById(id);
            if (product == null)
				return HttpNotFound();

            if (!product.HasSampleDownload)
                return Content("Product variant doesn't have a sample download.");

			var download = _downloadService.GetDownloadById(product.SampleDownloadId.GetValueOrDefault());
            if (download == null)
                return Content("Sample download is not available any more.");

            if (download.UseDownloadUrl)
            {
                return new RedirectResult(download.DownloadUrl);
            }
            else
            {
                if (download.DownloadBinary == null)
                    return Content("Download data is not available any more.");

                string fileName = !String.IsNullOrWhiteSpace(download.Filename) ? download.Filename : product.Id.ToString();
                string contentType = !String.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : "application/octet-stream";
                return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
            }
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
                return Content("Downloads are not allowed");

            if (_customerSettings.DownloadableProductsValidateUser)
            {
                if (_workContext.CurrentCustomer == null)
                    return new HttpUnauthorizedResult();

                if (order.CustomerId != _workContext.CurrentCustomer.Id)
                    return Content("This is not your order");
            }

            var download = _downloadService.GetDownloadById(product.DownloadId);
            if (download == null)
                return Content("Download is not available any more.");

            if (product.HasUserAgreement)
            {
                if (!agree)
					return RedirectToAction("UserAgreement", "Customer", new { id = id });
            }


            if (!product.UnlimitedDownloads && orderItem.DownloadCount >= product.MaxNumberOfDownloads)
                return Content(string.Format("You have reached maximum number of downloads {0}", product.MaxNumberOfDownloads));
            

            if (download.UseDownloadUrl)
            {
                //increase download
                orderItem.DownloadCount++;
                _orderService.UpdateOrder(order);

                //return result
                return new RedirectResult(download.DownloadUrl);
            }
            else
            {
                if (download.DownloadBinary == null)
                    return Content("Download data is not available any more.");

                //increase download
                orderItem.DownloadCount++;
                _orderService.UpdateOrder(order);

                //return result
                string fileName = !String.IsNullOrWhiteSpace(download.Filename) ? download.Filename : product.Id.ToString();
                string contentType = !String.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : "application/octet-stream";
                return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
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
                return Content("Downloads are not allowed");

            if (_customerSettings.DownloadableProductsValidateUser)
            {
                if (_workContext.CurrentCustomer == null)
                    return new HttpUnauthorizedResult();

                if (order.CustomerId != _workContext.CurrentCustomer.Id)
                    return Content("This is not your order");
            }

            var download = _downloadService.GetDownloadById(orderItem.LicenseDownloadId.HasValue ? orderItem.LicenseDownloadId.Value : 0);
            if (download == null)
                return Content("Download is not available any more.");
            
            if (download.UseDownloadUrl)
            {
                //return result
                return new RedirectResult(download.DownloadUrl);
            }
            else
            {
                if (download.DownloadBinary == null)
                    return Content("Download data is not available any more.");
                
                //return result
                string fileName = !String.IsNullOrWhiteSpace(download.Filename) ? download.Filename : product.Id.ToString();
                string contentType = !String.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : "application/octet-stream";
                return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
            }
        }

        public ActionResult GetFileUpload(Guid downloadId)
        {
            var download = _downloadService.GetDownloadByGuid(downloadId);
            if (download == null)
                return Content("Download is not available any more.");

            if (download.UseDownloadUrl)
            {
                //return result
                return new RedirectResult(download.DownloadUrl);
            }
            else
            {
                if (download.DownloadBinary == null)
                    return Content("Download data is not available any more.");

                //return result
                string fileName = !String.IsNullOrWhiteSpace(download.Filename) ? download.Filename : downloadId.ToString();
                string contentType = !String.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : "application/octet-stream";
                return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
            }
        }

    }
}
