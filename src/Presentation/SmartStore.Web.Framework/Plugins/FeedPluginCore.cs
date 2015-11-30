using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Logging;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Web.Framework.Plugins
{
	public class PromotionFeedSettings
	{
		public PromotionFeedSettings()
		{
			PageSize = 100;
		}

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
		public int StoreId { get; set; }
		public string ExportFormat { get; set; }
		public bool ConvertNetToGrossPrices { get; set; }
		public int LanguageId { get; set; }
		public int PageSize { get; set; }
		public decimal? FreeShippingThreshold { get; set; }
	}


	public class PromotionFeedConfigModel
	{
		public PromotionFeedConfigModel()
		{
			GeneratedFiles = new List<FeedFileData>();
		}

		public FeedPluginHelper Helper { get; set; }
		public bool IsRunning { get; set; }
		public string ProcessInfo { get; set; }
		public string GenerateFeedUrl { get; set; }
		public string GenerateFeedProgressUrl { get; set; }
		public string DeleteFilesUrl { get; set; }

		[SmartResourceDisplayName("Admin.PromotionFeeds")]
		public List<FeedFileData> GeneratedFiles { get; set; }
		public List<SelectListItem> AvailableStores { get; set; }
		public List<SelectListItem> AvailableLanguages { get; set; }
	}


	public class FeedFileData : ModelBase
	{
		public int StoreId { get; set; }
		public string StoreName { get; set; }
		public string FileTempPath { get; set; }
		public string FilePath { get; set; }
		public string FileUrl { get; set; }
		public string LogPath { get; set; }
		public string LogUrl { get; set; }
		public string LastWriteTime { get; set; }
	}


	public class FeedFileCreationContext
	{
		public FileStream Stream { get; set; }
		public TraceLogger Logger { get; set; }
		public Store Store { get; set; }
		public int StoreCount { get; set; }
		public int TotalRecords { get; set; }
		public int TotalProcessed { get; set; }
		public string FeedFileUrl { get; set; }
		public string SecondFilePath { get; set; }
		public string ErrorMessage { get; set; }
		public IProgress<FeedFileCreationProgress> Progress { get; set; }

		public void Report()
		{
			if (Progress != null)
			{
				Progress.Report(new FeedFileCreationProgress
				{
					TotalRecords = TotalRecords,
					TotalProcessed = ++TotalProcessed
				});
			}
		}
	}


	public class FeedFileCreationProgress
	{
		public int TotalRecords { get; set; }
		public int TotalProcessed { get; set; }
		public double ProcessedPercent
		{
			get
			{
				if (TotalRecords == 0)
					return 0;

				return ((double)TotalProcessed / (double)TotalRecords) * 100;
			}
		}
	}
}
