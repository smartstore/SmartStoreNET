using System;
using System.Net;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartStore.Core;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Common;
using SmartStore.Services.Configuration;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Admin.Models.Common;

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

		[ChildActionOnly]
		public ActionResult MarketplaceFeed()
		{
			try
			{
				string url = "http://community.smartstore.com/index.php?/rss/downloads/";

				var request = WebRequest.Create(url);
				request.Timeout = 3000;
				
				using (WebResponse response = request.GetResponse())
				{
					using (var reader = XmlReader.Create(response.GetResponseStream()))
					{
						var feed = SyndicationFeed.Load(reader);
						var model = new List<FeedItemModel>();
						foreach (var item in feed.Items)
						{
							var modelItem = new FeedItemModel();
							modelItem.Title = item.Title.Text;
							modelItem.Summary = item.Summary.Text.RemoveHtml().Truncate(150, "...");
							modelItem.PublishDate = item.PublishDate.LocalDateTime.RelativeFormat();

							var link = item.Links.FirstOrDefault();
							if (link != null)
							{
								modelItem.Link = link.Uri.ToString();
							}

							model.Add(modelItem);
						}

						return PartialView(model);
					}
				}
			}
			catch (Exception ex)
			{
				return Content("<div class='alert alert-error'>{0}</div>".FormatCurrent(ex.Message));
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
