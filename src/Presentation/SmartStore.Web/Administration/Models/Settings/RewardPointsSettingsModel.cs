using FluentValidation.Attributes;
using SmartStore.Admin.Validators.Settings;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Configuration;
using SmartStore.Web.Framework;

namespace SmartStore.Admin.Models.Settings
{
    [Validator(typeof(RewardPointsSettingsValidator))]
    public class RewardPointsSettingsModel
    {
		public int ActiveStoreScopeConfiguration { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.RewardPoints.Enabled")]
        public StoreDependingSetting<bool> Enabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.RewardPoints.ExchangeRate")]
        public StoreDependingSetting<decimal> ExchangeRate { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.RewardPoints.PointsForRegistration")]
        public StoreDependingSetting<int> PointsForRegistration { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.RewardPoints.PointsForPurchases_Amount")]
        public StoreDependingSetting<decimal> PointsForPurchases_Amount { get; set; }
        public int PointsForPurchases_Points { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.RewardPoints.PointsForPurchases_Awarded")]
        public StoreDependingSetting<int> PointsForPurchases_Awarded { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.RewardPoints.PointsForPurchases_Canceled")]
		public StoreDependingSetting<int> PointsForPurchases_Canceled { get; set; }

        public string PrimaryStoreCurrencyCode { get; set; }
    }
}