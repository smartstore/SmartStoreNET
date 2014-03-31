using System.Web.Mvc;
using SmartStore.Web.Framework.Security;
using SmartStore.Core.Domain.Cms;
using SmartStore.Services.Configuration;
using System.Linq;
using System;
using SmartStore.Core;
using SmartStore.Services.Media;
using SmartStore.Core.Infrastructure;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Web.Controllers
{
    public partial class HomeController : PublicControllerBase
    {

        #region Constructors

        public HomeController()
        {
        }
        
        #endregion

        [RequireHttpsByConfigAttribute(SslRequirement.No)]
        public ActionResult Index()
        {
            return View();
        }


        [ChildActionOnly]
        public ActionResult ContentSlider()
        {
            var workContext = EngineContext.Current.Resolve<IWorkContext>();
			var storeContext = EngineContext.Current.Resolve<IStoreContext>();
            var pictureService = EngineContext.Current.Resolve<IPictureService>();
            var model = EngineContext.Current.Resolve<ContentSliderSettings>();

            model.BackgroundPictureUrl = pictureService.GetPictureUrl(model.BackgroundPictureId, 0, false);

            var slides = model.Slides
				.Where(s => 
					s.LanguageCulture == workContext.WorkingLanguage.LanguageCulture && 
					(!s.LimitedToStores || (s.SelectedStoreIds != null && s.SelectedStoreIds.Contains(storeContext.CurrentStore.Id)))
				)
				.OrderBy(s => s.DisplayOrder);
            
            foreach (ContentSliderSlideSettings slide in slides)
            {
                slide.PictureUrl = pictureService.GetPictureUrl(slide.PictureId, 0, false);
                slide.Button1.Url = CheckButtonUrl(slide.Button1.Url);
                slide.Button2.Url = CheckButtonUrl(slide.Button2.Url);
                slide.Button3.Url = CheckButtonUrl(slide.Button3.Url);
            }

            model.Slides = slides.ToList();

            return PartialView(model);
        }

        #region helper functions
        
        private string CheckButtonUrl(string url) 
        {
            if (!String.IsNullOrEmpty(url))
            {
                if (url.StartsWith("//") || url.StartsWith("http://"))
                {
                    //  //www.domain.de/dir
                    //  http://www.domain.de/dir
                    // nothing needs to be done
                }
                else if (url.StartsWith("~/"))
                {
                    //  ~/directory
                    url = Url.Content(url);
                }
                else if (url.StartsWith("/"))
                {
                    //  /directory
                    url = Url.Content("~" + url);
                }
                else
                {
                    //  directory
                    url = Url.Content("~/" + url);
                }
            }
            return url;
        }
        
        #endregion helper functions

    }
}
