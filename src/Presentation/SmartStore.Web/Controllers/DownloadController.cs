using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Html;
using SmartStore.Services.Catalog;
using SmartStore.Services.Media;
using SmartStore.Services.Orders;
using SmartStore.Services.Seo;
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
            _downloadService = downloadService;
            _productService = productService;
            _orderService = orderService;
            _workContext = workContext;
            _customerSettings = customerSettings;
        }

        private ActionResult GetFileContentResultFor(Download download, byte[] data)
        {
            if (data == null || data.LongLength == 0)
            {
                NotifyError(T("Common.Download.NoDataAvailable"));
                return new RedirectResult(Url.Action("Info", "Customer"));
            }

            var fileName = download.MediaFile.Name;
            var contentType = download.MediaFile.MimeType;

            return new FileContentResult(data, contentType)
            {
                FileDownloadName = fileName
            };
        }

        private ActionResult GetFileContentResultFor(Download download)
        {
            return GetFileContentResultFor(download, _downloadService.LoadDownloadBinary(download));
        }

        public ActionResult Sample(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                return HttpNotFound();

            if (!product.HasSampleDownload)
            {
                NotifyError(T("Common.Download.HasNoSample"));
                return RedirectToRoute("Product", new { SeName = product.GetSeName() });
            }

            var download = _downloadService.GetDownloadById(product.SampleDownloadId.GetValueOrDefault());
            if (download == null)
            {
                NotifyError(T("Common.Download.SampleNotAvailable"));
                return RedirectToRoute("Product", new { SeName = product.GetSeName() });
            }

            if (download.UseDownloadUrl)
                return new RedirectResult(download.DownloadUrl);

            return GetFileContentResultFor(download);
        }

        public ActionResult GetDownload(Guid id, bool agree = false, string fileVersion = "")
        {
            if (id == Guid.Empty)
                return HttpNotFound();

            var orderItem = _orderService.GetOrderItemByGuid(id);
            if (orderItem == null)
                return HttpNotFound();

            var order = orderItem.Order;
            var product = orderItem.Product;
            var hasNotification = false;

            if (!_downloadService.IsDownloadAllowed(orderItem))
            {
                hasNotification = true;
                NotifyError(T("Common.Download.NotAllowed"));
            }

            if (_customerSettings.DownloadableProductsValidateUser)
            {
                if (_workContext.CurrentCustomer == null)
                    return new HttpUnauthorizedResult();

                if (order.CustomerId != _workContext.CurrentCustomer.Id)
                {
                    hasNotification = true;
                    NotifyError(T("Account.CustomerOrders.NotYourOrder"));
                }
            }

            Download download;

            if (fileVersion.HasValue())
            {
                download = _downloadService.GetDownloadByVersion(product.Id, "Product", fileVersion);
            }
            else
            {
                download = _downloadService.GetDownloadsFor(product).FirstOrDefault();
            }

            if (download == null)
            {
                hasNotification = true;
                NotifyError(T("Common.Download.NoDataAvailable"));
            }

            if (product.HasUserAgreement && !agree)
            {
                hasNotification = true;
            }

            if (!product.UnlimitedDownloads && orderItem.DownloadCount >= product.MaxNumberOfDownloads)
            {
                hasNotification = true;
                NotifyError(T("Common.Download.MaxNumberReached", product.MaxNumberOfDownloads));
            }

            if (hasNotification)
            {
                return RedirectToAction("UserAgreement", "Customer", new { id, fileVersion });
            }

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
                {
                    NotifyError(T("Common.Download.NoDataAvailable"));
                    return RedirectToAction("UserAgreement", "Customer", new { id });
                }

                orderItem.DownloadCount++;
                _orderService.UpdateOrder(order);

                return GetFileContentResultFor(download, data);
            }
        }

        public ActionResult GetLicense(Guid id)
        {
            if (id == Guid.Empty)
                return HttpNotFound();

            var orderItem = _orderService.GetOrderItemByGuid(id);
            if (orderItem == null)
                return HttpNotFound();

            var order = orderItem.Order;
            var product = orderItem.Product;

            if (!_downloadService.IsLicenseDownloadAllowed(orderItem))
            {
                NotifyError(T("Common.Download.NotAllowed"));
                return RedirectToAction("DownloadableProducts", "Customer");
            }


            if (_customerSettings.DownloadableProductsValidateUser)
            {
                if (_workContext.CurrentCustomer == null)
                    return new HttpUnauthorizedResult();

                if (order.CustomerId != _workContext.CurrentCustomer.Id)
                {
                    NotifyError(T("Account.CustomerOrders.NotYourOrder"));
                    return RedirectToAction("DownloadableProducts", "Customer");
                }
            }

            var download = _downloadService.GetDownloadById(orderItem.LicenseDownloadId.HasValue ? orderItem.LicenseDownloadId.Value : 0);
            if (download == null)
            {
                NotifyError(T("Common.Download.NotAvailable"));
                return RedirectToAction("DownloadableProducts", "Customer");
            }

            if (download.UseDownloadUrl)
                return new RedirectResult(download.DownloadUrl);

            return GetFileContentResultFor(download);
        }

        public ActionResult GetFileUpload(Guid downloadId)
        {
            var download = _downloadService.GetDownloadByGuid(downloadId);
            if (download == null)
            {
                NotifyError(T("Common.Download.NotAvailable"));
                return RedirectToAction("DownloadableProducts", "Customer");
            }

            if (download.UseDownloadUrl)
            {
                return new RedirectResult(download.DownloadUrl);
            }

            return GetFileContentResultFor(download);
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
