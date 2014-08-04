using SmartStore.Plugin.Feed.Froogle.Models;
using SmartStore.Services.Tasks;
using SmartStore.Web.Framework.Plugins;
using Telerik.Web.Mvc;

namespace SmartStore.Plugin.Feed.Froogle.Services
{
    public partial interface IGoogleFeedService
    {
		FroogleSettings Settings { get; set; }
		FeedPluginHelper Helper { get; }

		string[] GetTaxonomyList();

		void UpdateInsert(int pk, string name, string value);

		GridModel<GoogleProductModel> GetGridModel(GridCommand command, string searchProductName = null, string touched = null);

		void CreateFeed(TaskExecutionContext context);

		void SetupModel(FeedFroogleModel model);
    }
}
