//// codehint: sm-add (file)

//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using SmartStore.Core.Domain;
//using SmartStore.Core.Configuration;
//using SmartStore.Services.Installation;
//using SmartStore.Core.Domain.Common;
//using SmartStore.Core.Domain.Directory;
//using SmartStore.Core.Domain.Tax;
//using SmartStore.Core.Domain.Topics;
//using SmartStore.Core.Domain.Seo;
//using SmartStore.Services.Media;       
//using SmartStore.Core.Infrastructure;  
//using SmartStore.Core;                 
//using SmartStore.Core.Domain.Cms;
//using System.IO;
//using SmartStore.Core.Data;
//using SmartStore.Services.Localization;   

//namespace SmartStore.Web.Infrastructure.Installation
//{

//	public class EnUSInstallationData : InvariantInstallationData
//	{
//		private readonly IPictureService _pictureService;
//		private readonly string _sampleImagesPath;

//		//public EnUSInstallationData(IRepository<Currency> currencyRepository)
//		public EnUSInstallationData()
//		{
//			//pictures
//			this._pictureService = EngineContext.Current.Resolve<IPictureService>();
//			this._sampleImagesPath = EngineContext.Current.Resolve<IWebHelper>().MapPath("~/content/samples/");
//		}


//		protected override void Alter(IList<ISettings> settings)
//		{
//			base.Alter(settings);

//			settings
//				.Alter<CommonSettings>(x =>
//				{
//					// [...]
//				})
//				.Alter<StoreInformationSettings>(x =>
//				{
//					// [...]
//				})
//				.Alter<ContentSliderSettings>(x =>
//				{

//					var slide1PicId = _pictureService.InsertPicture(File.ReadAllBytes(_sampleImagesPath + "iphone.png"), "image/png", "", true, false).Id;
//					var slide2PicId = _pictureService.InsertPicture(File.ReadAllBytes(_sampleImagesPath + "music.png"), "image/png", "", true, false).Id;
//					var slide3PicId = _pictureService.InsertPicture(File.ReadAllBytes(_sampleImagesPath + "packshot-net.png"), "image/png", "", true, false).Id;

//					//slide 1
//					x.Slides.Add(new ContentSliderSlideSettings
//					{
//						DisplayOrder = 1,
//						//LanguageName = "English",
//						Title = "The biggest thing that could ever happen to the iPhone",
//						Text = @"<ul>
//                                    <li>Thinner, slighter design</li>
//                                    <li>4"" retina display.</li>
//                                    <li>Ultrafast mobile data.</li>                       
//                                </ul>",
//						Published = true,
//						LanguageCulture = "en-US",
//						PictureId = slide1PicId,
//						//PictureUrl = _pictureService.GetPictureUrl(slide1PicId),
//						Button1 = new ContentSliderButtonSettings
//						{
//							Published = true,
//							Text = "more...",
//							Type = "btn-primary",
//							Url = "~/iphone-5"
//						},
//						Button2 = new ContentSliderButtonSettings
//						{
//							Published = true,
//							Text = "Buy now",
//							Type = "btn-danger",
//							Url = "~/iphone-5"
//						},
//						Button3 = new ContentSliderButtonSettings
//						{
//							Published = false
//						}
//					});
//					//slide 2
//					x.Slides.Add(new ContentSliderSlideSettings
//					{
//						DisplayOrder = 2,
//						//LanguageName = "English",
//						Title = "Buy music online!",
//						Text = @"<p>Buy here & download at once.</p>
//                                 <p>Best quality at 320 kbit/s.</p>
//                                 <p>Sample and download at light speed.</p>",
//						Published = true,
//						LanguageCulture = "en-US",
//						PictureId = slide2PicId,
//						//PictureUrl = _pictureService.GetPictureUrl(slide2PicId),
//						Button1 = new ContentSliderButtonSettings
//						{
//							Published = true,
//							Text = "more...",
//							Type = "btn-warning",
//							Url = "~/musik-kaufen-sofort-herunterladen"
//						},
//						Button2 = new ContentSliderButtonSettings
//						{
//							Published = false
//						},
//						Button3 = new ContentSliderButtonSettings
//						{
//							Published = false
//						}
//					});
//					//slide 3
//					x.Slides.Add(new ContentSliderSlideSettings
//					{
//						DisplayOrder = 3,
//						//LanguageName = "English",
//						Title = "Ready for the revolution?",
//						Text = @"<p>SmartStore.NET is the new dynamic E-Commerce solution from SmartStore.</p>
//                                 <ul>
//                                     <li>Order-, customer- and stock-management.</li>
//                                     <li>SEO optimized | 100% mobile optimized.</li>
//                                     <li>Reviews &amp; Ratings | SmartStore.biz Import.</li>
//                                 </ul>",
//						Published = true,
//						LanguageCulture = "en-US",
//						PictureId = slide3PicId,
//						//PictureUrl = _pictureService.GetPictureUrl(slide3PicId),
//						Button1 = new ContentSliderButtonSettings
//						{
//							Published = true,
//							Text = "and much more...",
//							Type = "btn-success",
//							Url = "http://net.smartstore.com"
//						},
//						Button2 = new ContentSliderButtonSettings
//						{
//							Published = false
//						},
//						Button3 = new ContentSliderButtonSettings
//						{
//							Published = false
//						}
//					});
//				});
//		}


//	}

//}
