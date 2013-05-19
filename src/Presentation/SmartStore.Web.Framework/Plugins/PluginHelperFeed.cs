using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Html;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Catalog;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Tasks;
using SmartStore.Services.Seo;

namespace SmartStore.Web.Framework.Plugins
{
	/// <remarks>codehint: sm-add</remarks>
	public partial class PluginHelperFeed : PluginHelperBase
	{
		private readonly string _namespace;
		private ScheduleTask _scheduleTask;
		private Func<PromotionFeedSettings> _settingsFunc;

		public PluginHelperFeed(string systemName, string rootNamespace, Func<PromotionFeedSettings> settings) : 
			base(systemName) {

			_namespace = rootNamespace;
			_settingsFunc = settings;
		}

		private PromotionFeedSettings BaseSettings { get { return _settingsFunc(); } }

		public string FeedFilePath {
			get {
                string path = Path.Combine(HttpRuntime.AppDomainAppPath, "Content\\files\\exportimport");

				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);

				return Path.Combine(path, BaseSettings.StaticFileName);
			}
		}
		public string FeedFileUrl {
			get {
				if (File.Exists(FeedFilePath)) {
                    return "{0}Content/files/exportimport/{1}".FormatWith(StoreLocation, BaseSettings.StaticFileName);
				}
				return null;
			}
		}
		public string GeneratedFeedResult {
			get {
                string clickHereStr = string.Format("<a href=\"{0}Content/files/exportimport/{1}\" target=\"_blank\">{2}</a>",
					StoreLocation, BaseSettings.StaticFileName, Resource("ClickHere"));

				return string.Format(Resource("SuccessResult"), clickHereStr);
			}
		}
		private string ScheduleTaskType {
			get {
				return "{0}.StaticFileGenerationTask, {0}".FormatWith(_namespace);
			}
		}
		public ScheduleTask ScheduledTask {
			get {
				if (_scheduleTask == null) {
					_scheduleTask = EngineContext.Current.Resolve<IScheduleTaskService>().GetTaskByType(ScheduleTaskType);
				}
				return _scheduleTask;
			}
		}

		public string RemoveInvalidFeedChars(string input, bool isHtmlEncoded) {
			if (String.IsNullOrWhiteSpace(input))
				return input;

			//Microsoft uses a proprietary encoding (called CP-1252) for the bullet symbol and some other special characters, 
			//whereas most websites and data feeds use UTF-8. When you copy-paste from a Microsoft product into a website, 
			//some characters may appear as junk. Our system generates data feeds in the UTF-8 character encoding, 
			//which many shopping engines now require.

			//http://www.atensoftware.com/p90.php?q=182

			if (isHtmlEncoded)
				input = HttpUtility.HtmlDecode(input);

			input = input.Replace("¼", "");
			input = input.Replace("½", "");
			input = input.Replace("¾", "");
			//input = input.Replace("•", "");
			//input = input.Replace("”", "");
			//input = input.Replace("“", "");
			//input = input.Replace("’", "");
			//input = input.Replace("‘", "");
			//input = input.Replace("™", "");
			//input = input.Replace("®", "");
			//input = input.Replace("°", "");

			if (isHtmlEncoded)
				input = HttpUtility.HtmlEncode(input);

			return input;
		}
		public string ReplaceCsvChars(string value) {
			if (value.HasValue()) {
				value = value.Replace(';', ',');
				value = value.Replace('\r', ' ');
				value = value.Replace('\n', ' ');
				return value.Replace("'", "");
			}
			return "";
		}
		public string DecimalUsFormat(decimal value) {
			return Math.Round(value, 2).ToString(new CultureInfo("en-US", false).NumberFormat);
		}
		public string CategoryName(Product product) {
			var productCategory = EngineContext.Current.Resolve<ICategoryService>().GetProductCategoriesByProductId(product.Id).FirstOrDefault();
			if (productCategory != null) {
				var category = productCategory.Category;
				if (category != null)
					return category.Name;
			}
			return "";
		}
		private IList<Category> CategoryBreadCrumb(Product product) {
			var lst = new List<Category>();
			var categoryService = EngineContext.Current.Resolve<ICategoryService>();
			var productCategory = categoryService.GetProductCategoriesByProductId(product.Id).FirstOrDefault();

			if (productCategory != null) {
				Category category = productCategory.Category;

				while (category != null && !category.Deleted && category.Published) {
					lst.Add(category);
					category = categoryService.GetCategoryById(category.ParentCategoryId);
				}
				lst.Reverse();
			}
			return lst;
		}
		public string CategoryBreadCrumbJoined(Product product) {
			string result = "";
			var categories = CategoryBreadCrumb(product);

			for (int i = 0; i < categories.Count; i++) {
				var cat = categories[i];
				result = result + cat.Name;
				if (i != categories.Count - 1)
					result = result + " > ";
			}
			return result;
		}
		public string BuildProductDescription(Product product, ProductVariant variant, ProductManufacturer manu, Func<string, string> updateResult = null) {
			if (BaseSettings.BuildDescription.IsCaseInsensitiveEqual(NotSpecified))
				return "";

			string description = "";
			string fullName = variant.FullProductName;
			string manuName = (manu == null ? "" : manu.Manufacturer.Name);

			if (BaseSettings.BuildDescription.IsNullOrEmpty()) {
				description = variant.Description;

				if (description.IsNullOrEmpty())
					description = product.FullDescription;
				if (description.IsNullOrEmpty())
					description = product.ShortDescription;
				if (description.IsNullOrEmpty())
					description = fullName;
			}
			else if (BaseSettings.BuildDescription.IsCaseInsensitiveEqual("short")) {
				description = product.ShortDescription;
			}
			else if (BaseSettings.BuildDescription.IsCaseInsensitiveEqual("long")) {
				description = product.FullDescription;
			}
			else if (BaseSettings.BuildDescription.IsCaseInsensitiveEqual("titleAndShort")) {
				description = fullName.Grow(product.ShortDescription, " ");
			}
			else if (BaseSettings.BuildDescription.IsCaseInsensitiveEqual("titleAndLong")) {
				description = fullName.Grow(product.FullDescription, " ");
			}
			else if (BaseSettings.BuildDescription.IsCaseInsensitiveEqual("manuAndTitleAndShort")) {
				description = manuName.Grow(fullName, " ");
				description = description.Grow(product.ShortDescription, " ");
			}
			else if (BaseSettings.BuildDescription.IsCaseInsensitiveEqual("manuAndTitleAndLong")) {
				description = manuName.Grow(fullName, " ");
				description = description.Grow(product.FullDescription, " ");
			}

			if (updateResult != null) {
				description = updateResult(description);
			}

			try {
				if (BaseSettings.DescriptionToPlainText) {
					//Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
					//description = HttpUtility.HtmlDecode(reg.Replace(description, ""));

					description = HtmlUtils.ConvertHtmlToPlainText(description);
					description = HtmlUtils.StripTags(HttpUtility.HtmlDecode(description));

					description = RemoveInvalidFeedChars(description, false);
				}
				else {
					description = RemoveInvalidFeedChars(description, true);
				}
			}
			catch (Exception exc) {
				exc.Dump();
			}

			return description;
		}
		public string ManufacturerPartNumber(ProductVariant variant) {
			if (variant.ManufacturerPartNumber.HasValue())
				return variant.ManufacturerPartNumber;

			if (BaseSettings.UseOwnProductNo)
				return variant.Sku;

			return "";
		}
		public string ShippingCost(ProductVariant variant, decimal? shippingCost = null) {
			if (variant.IsFreeShipping)
				return "0";

			decimal cost = shippingCost ?? BaseSettings.ShippingCost;

			if (decimal.Compare(cost, decimal.Zero) == 0)
				return "";

			return DecimalUsFormat(cost);
		}
		public string BasePrice(ProductVariant variant) {
			if (variant.BasePrice.BaseAmount.HasValue && variant.BasePrice.MeasureUnit.HasValue()) {
				decimal price = Convert.ToDecimal(variant.Price / (variant.BasePrice.Amount * variant.BasePrice.BaseAmount));

				string priceFormatted = EngineContext.Current.Resolve<IPriceFormatter>().FormatPrice(price, false, false);

				return "{0} / {1} {2}".FormatWith(priceFormatted, variant.BasePrice.BaseAmount, variant.BasePrice.MeasureUnit);
			}
			return "";
		}
		public string SpecialPrice(ProductVariant variant, bool checkDate) {
			if (variant.SpecialPrice.HasValue && variant.SpecialPriceStartDateTimeUtc.HasValue && variant.SpecialPriceEndDateTimeUtc.HasValue) {
				
				if (checkDate && !(DateTime.UtcNow >= variant.SpecialPriceStartDateTimeUtc && DateTime.UtcNow <= variant.SpecialPriceEndDateTimeUtc))
					return "";

				return DecimalUsFormat(variant.SpecialPrice.Value);
			}
			return "";
		}
		public string ProductDetailLink(Product product) {
			return "{0}{1}".FormatWith(StoreLocation, product.GetSeName(Language.Id));
		}

		public string MainImageUrl(Product product, ProductVariant variant) {
			string url;
			var pictureService = EngineContext.Current.Resolve<IPictureService>();
			var picture = pictureService.GetPictureById(variant.PictureId);

			if (picture == null)
				picture = pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();

			//always use HTTP when getting image URL
			if (picture != null)
				url = pictureService.GetPictureUrl(picture, BaseSettings.ProductPictureSize);
			else
				url = pictureService.GetDefaultPictureUrl(BaseSettings.ProductPictureSize);

			return url;
		}
		public List<string> AdditionalImages(Product product, string mainImageUrl, int maxImages = 10) {
			List<string> urls = new List<string>();

			if (BaseSettings.AdditionalImages) {
				var pictureService = EngineContext.Current.Resolve<IPictureService>();
				var pics = pictureService.GetPicturesByProductId(product.Id, 0);

				foreach (var pic in pics) {
					if (pic != null) {
						string url = pictureService.GetPictureUrl(pic, BaseSettings.ProductPictureSize);
						if (url.HasValue() && (mainImageUrl.IsNullOrEmpty() || !mainImageUrl.IsCaseInsensitiveEqual(url))) {
							urls.Add(url);
							if (urls.Count >= maxImages)
								break;
						}
					}
				}
			}
			return urls;
		}
		public void ScheduleTaskUpdate(bool enabled, int seconds) {
			var task = ScheduledTask;
			if (task != null) {
				task.Enabled = enabled;
				task.Seconds = seconds;

				EngineContext.Current.Resolve<IScheduleTaskService>().UpdateTask(task);
			}
		}
		public void ScheduleTaskInsert(int minutes = 360) {
			var task = ScheduledTask;
			if (task == null) {
				EngineContext.Current.Resolve<IScheduleTaskService>().InsertTask(new ScheduleTask {
					Name = "{0} feed file generation".FormatWith(SystemName),
					Seconds = minutes * 60,
					Type = ScheduleTaskType,
					Enabled = false,
					StopOnError = false,
				});
			}
		}
		public void ScheduleTaskDelete() {
			var task = ScheduledTask;
			if (task != null)
				EngineContext.Current.Resolve<IScheduleTaskService>().DeleteTask(task);
		}

	}	// class


	public class PromotionFeedSettings
	{
		public int ProductPictureSize { get; set; }
		public int CurrencyId { get; set; }
		public string StaticFileName { get; set; }
		public string BuildDescription { get; set; }
		public bool DescriptionToPlainText { get; set; }
		public bool AdditionalImages { get; set; }
		public string Availability { get; set; }
		public decimal ShippingCost { get; set; }
		public string ShippingTime { get; set; }
		public string Brand { get; set; }
		public bool UseOwnProductNo { get; set; }
	}	// class
}
