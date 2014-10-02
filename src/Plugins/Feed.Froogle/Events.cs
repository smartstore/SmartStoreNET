using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using SmartStore.Core.Events;
using SmartStore.Plugin.Feed.Froogle.Domain;
using SmartStore.Plugin.Feed.Froogle.Models;
using SmartStore.Plugin.Feed.Froogle.Services;
using SmartStore.Web.Framework.Events;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Plugin.Feed.Froogle
{
	public class Events : 
		IConsumer<TabStripCreated>,
		IConsumer<ModelBoundEvent>
	{
		private readonly IGoogleFeedService _googleService;

		public Events(IGoogleFeedService googleService)
		{
			this._googleService = googleService;
		}
		
		public void HandleEvent(TabStripCreated eventMessage)
		{
			if (eventMessage.TabStripName == "product-edit")
			{
				var productId = ((TabbableModel)eventMessage.Model).Id;
				eventMessage.ItemFactory.Add().Text("GMC")
					.Name("tab-gmc")
					.Icon("fa fa-google fa-lg fa-fw")
					.LinkHtmlAttributes(new { data_tab_name = "GMC" })
					.Route("Feed.Froogle", new { action = "ProductEditTab", productId = productId })
					.Ajax();
			}
		}

		public void HandleEvent(ModelBoundEvent eventMessage)
		{
			if (!eventMessage.BoundModel.CustomProperties.ContainsKey("GMC"))
				return;
			
			var model = eventMessage.BoundModel.CustomProperties["GMC"] as GoogleProductModel;
			if (model == null)
				return;

			var entity = _googleService.GetGoogleProductRecord(model.ProductId);
			var insert = (entity == null);
			var delete = model.Taxonomy.IsEmpty() && model.AgeGroup.IsEmpty() && model.Color.IsEmpty() && model.Gender.IsEmpty() && model.Size.IsEmpty() && model.Pattern.IsEmpty() && model.Material.IsEmpty();

			if (insert && delete)
			{
				// nothing to do
				return;
			}

			if (insert)
			{
				entity = new GoogleProductRecord()
				{
					ProductId = model.ProductId
				};
			}
			else
			{
				if (delete)
				{
					_googleService.DeleteGoogleProductRecord(entity);
					return;
				}
			}

			// map objects
			entity.AgeGroup = model.AgeGroup;
			entity.Color = model.Color;
			entity.Gender = model.Gender;
			entity.Size = model.Size;
			entity.Taxonomy = model.Taxonomy;
			entity.Material = model.Material;
			entity.Pattern = model.Pattern;

			if (insert)
			{
				_googleService.InsertGoogleProductRecord(entity);
			}
			else
			{
				_googleService.UpdateGoogleProductRecord(entity);
			}
		}
	}
}