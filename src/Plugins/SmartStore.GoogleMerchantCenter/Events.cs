using System;
using SmartStore.Core.Events;
using SmartStore.GoogleMerchantCenter.Domain;
using SmartStore.GoogleMerchantCenter.Models;
using SmartStore.GoogleMerchantCenter.Services;
using SmartStore.Web.Framework.Events;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.GoogleMerchantCenter
{
	public class Events : 
		IConsumer<TabStripCreated>,
		IConsumer<ModelBoundEvent>
	{
		private readonly IGoogleFeedService _googleService;

		public Events(IGoogleFeedService googleService)
		{
			_googleService = googleService;
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
                    .Route("SmartStore.GoogleMerchantCenter", new { action = "ProductEditTab", productId = productId })
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

			var utcNow = DateTime.UtcNow;
			var entity = _googleService.GetGoogleProductRecord(model.ProductId);
			var insert = (entity == null);

			if (entity == null)
			{
				entity = new GoogleProductRecord
				{
					ProductId = model.ProductId,
					CreatedOnUtc = utcNow
				};
			}

			entity.AgeGroup = model.AgeGroup;
			entity.Color = model.Color;
			entity.Gender = model.Gender;
			entity.Size = model.Size;
			entity.Taxonomy = model.Taxonomy;
			entity.Material = model.Material;
			entity.Pattern = model.Pattern;
			entity.Export = model.Export2;
			entity.UpdatedOnUtc = utcNow;
			entity.Multipack = model.Multipack2 ?? 0;
			entity.IsBundle = model.IsBundle;
			entity.IsAdult = model.IsAdult;
			entity.EnergyEfficiencyClass = model.EnergyEfficiencyClass;
			entity.CustomLabel0 = model.CustomLabel0;
			entity.CustomLabel1 = model.CustomLabel1;
			entity.CustomLabel2 = model.CustomLabel2;
			entity.CustomLabel3 = model.CustomLabel3;
			entity.CustomLabel4 = model.CustomLabel4;

			entity.IsTouched = entity.IsTouched();

			if (!insert && !entity.IsTouched)
			{
				_googleService.DeleteGoogleProductRecord(entity);
				return;
			}

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