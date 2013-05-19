using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Plugin.Feed.Froogle.Domain;
using SmartStore.Plugin.Feed.Froogle.Models;
using SmartStore.Services.Media;
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
		void CreateFeed(Stream stream);
		void CreateFeed();
		void SetupModel(FeedFroogleModel model, ScheduleTask task = null);
    }
}
