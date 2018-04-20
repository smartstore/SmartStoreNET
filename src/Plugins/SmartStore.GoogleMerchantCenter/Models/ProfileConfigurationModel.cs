using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Xml.Serialization;
using FluentValidation.Attributes;
using SmartStore.GoogleMerchantCenter.Validators;
using SmartStore.Web.Framework;

namespace SmartStore.GoogleMerchantCenter.Models
{
	[Serializable]
	[Validator(typeof(ProfileConfigurationValidator))]
	public class ProfileConfigurationModel
	{
		public ProfileConfigurationModel()
		{
			Condition = "new";
			AdditionalImages = true;
			SpecialPrice = true;
		}

		[XmlIgnore]
		public string LanguageSeoCode { get; set; }

		[XmlIgnore]
		public List<SelectListItem> AvailableCategories { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.DefaultGoogleCategory")]
		public string DefaultGoogleCategory { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.AdditionalImages")]
		public bool AdditionalImages { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Condition")]
		public string Condition { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Availability")]
		public string Availability { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.SpecialPrice")]
		public bool SpecialPrice { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Gender")]
		public string Gender { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.AgeGroup")]
		public string AgeGroup { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Color")]
		public string Color { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Size")]
		public string Size { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Material")]
		public string Material { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Pattern")]
		public string Pattern { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.ExpirationDays")]
		public int ExpirationDays { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.ExportShipping")]
		public bool ExportShipping { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.ExportBasePrice")]
		public bool ExportBasePrice { get; set; }
	}
}