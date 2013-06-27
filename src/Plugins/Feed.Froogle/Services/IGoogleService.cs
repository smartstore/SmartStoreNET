using System.IO;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Plugin.Feed.Froogle.Models;
using SmartStore.Web.Framework.Plugins;
using Telerik.Web.Mvc;

namespace SmartStore.Plugin.Feed.Froogle.Services
{
    public partial interface IGoogleService
    {
		FroogleSettings Settings { get; set; }
		PluginHelperFeed Helper { get; }

		string[] GetTaxonomyList();
		void UpdateInsert(int pk, string name, string value);
		GridModel<GoogleProductModel> GetGridModel(GridCommand command, string searchProductName = null);
		void CreateFeed(Stream stream, Store store);
		void CreateFeed();
		void SetupModel(FeedFroogleModel model, ScheduleTask task = null);
    }
}
