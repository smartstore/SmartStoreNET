using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Plugin.Feed.ElmarShopinfo.Models;
using SmartStore.Web.Framework.Plugins;

namespace SmartStore.Plugin.Feed.ElmarShopinfo.Services
{
	public partial interface IElmarShopinfoCoreService
    {
		ElmarShopinfoSettings Settings { get; set; }
		PluginHelperFeed Helper { get; }

		void CreateFeed(Store store, GeneratedFeedFile feedFile, Stream streamCsv, Stream streamXml);
		void CreateFeed();
		void SetupModel(FeedElmarShopinfoModel model, ScheduleTask task = null);
	}
}
