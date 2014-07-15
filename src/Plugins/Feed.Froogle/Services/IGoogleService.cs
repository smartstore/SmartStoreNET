using System.IO;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Plugin.Feed.Froogle.Domain;
using SmartStore.Plugin.Feed.Froogle.Models;
using SmartStore.Web.Framework.Plugins;
using Telerik.Web.Mvc;

namespace SmartStore.Plugin.Feed.Froogle.Services
{
    public partial interface IGoogleService
    {
		FroogleSettings Settings { get; set; }
		FeedPluginHelper Helper { get; }

		GoogleProductRecord GetGoogleProductRecord(int productId);
		void InsertGoogleProductRecord(GoogleProductRecord record);
		void UpdateGoogleProductRecord(GoogleProductRecord record);
		void DeleteGoogleProductRecord(GoogleProductRecord record);

		string[] GetTaxonomyList();
		void UpdateInsert(int pk, string name, string value);
		GridModel<GoogleProductModel> GetGridModel(GridCommand command, string searchProductName = null);
		void CreateFeed(FeedFileCreationContext context);
		void CreateFeed();
		void SetupModel(FeedFroogleModel model);
    }
}
