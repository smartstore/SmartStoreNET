using SmartStore.Services;
using SmartStore.Services.ContentSlider;
using SmartStore.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartStore.Web.Controllers
{
    public class ContentSliderController : PublicControllerBase
    {
        private readonly ICommonServices _services;
        private readonly IContentSliderService _contentSliderService;
        
        public ContentSliderController(ICommonServices services,
            IContentSliderService contentSliderService)
        {
            _services = services;
            _contentSliderService = contentSliderService;
        }

        [ChildActionOnly]
        public ActionResult HomepageContentSlider()
        {
            var contentSliders = _contentSliderService.GetAllContentSliders();

            return PartialView(contentSliders[0]);
        }
    }
}