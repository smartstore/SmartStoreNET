using System;
using System.Net;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using System.Xml;
using SmartStore.Core;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Common;
using SmartStore.Services.Configuration;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class HomeController : AdminControllerBase
    {
        #region Fields

		private readonly IStoreContext _storeContext;
        private readonly CommonSettings _commonSettings;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

		public HomeController(IStoreContext storeContext, 
            CommonSettings commonSettings, ISettingService settingService)
        {
			this._storeContext = storeContext;
            this._commonSettings = commonSettings;
            this._settingService = settingService;
        }

        #endregion

        #region Methods

        public ActionResult Index()
        {
            return View();
        }

		public ActionResult About()
		{
			return View();
		}

        [ChildActionOnly]
        public ActionResult SmartStoreNews()
        {
            try
            {
                string feedUrl = string.Format("http://www.smartstore.com/NewsRSS.aspx?Version={0}&Localhost={1}&HideAdvertisements={2}&StoreURL={3}",
                    SmartStoreVersion.CurrentVersion,
                    Request.Url.IsLoopback,
                    _commonSettings.HideAdvertisementsOnAdminArea,
					_storeContext.CurrentStore.Url);

                //specify timeout (5 secs)
                var request = WebRequest.Create(feedUrl);
                request.Timeout = 5000;
                using (WebResponse response = request.GetResponse())
                using (var reader = XmlReader.Create(response.GetResponseStream()))
                {
                    var rssData = SyndicationFeed.Load(reader);
                    return PartialView(rssData);
                }
            }
            catch (Exception)
            {
                return Content("");
            }
        }

        [HttpPost]
        public ActionResult SmartStoreNewsHideAdv()
        {
            _commonSettings.HideAdvertisementsOnAdminArea = !_commonSettings.HideAdvertisementsOnAdminArea;
            _settingService.SaveSetting(_commonSettings);
            return Content("Setting changed");
        }

        #endregion
    }
}
