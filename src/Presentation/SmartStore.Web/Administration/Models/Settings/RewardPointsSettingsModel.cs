using FluentValidation.Attributes;
using SmartStore.Admin.Validators.Settings;
using SmartStore.Core.Domain.Orders;
using SmartStore.Web.Framework;

namespace SmartStore.Admin.Models.Settings
{
    [Validator(typeof(RewardPointsSettingsValidator))]
	public class RewardPointsSettingsModel
    {
        [SmartResourceDisplayName("Admin.Configuration.Settings.RewardPoints.Enabled")]
        public bool Enabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.RewardPoints.ExchangeRate")]
        public decimal ExchangeRate { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.RewardPoints.RoundDownRewardPoints")]
		public bool RoundDownRewardPoints { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.RewardPoints.PointsForRegistration")]
        public int PointsForRegistration { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.RewardPoints.PointsForProductReview")]
		public int PointsForProductReview { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.RewardPoints.PointsForPurchases_Amount")]
        public decimal PointsForPurchases_Amount { get; set; }
        public int PointsForPurchases_Points { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.RewardPoints.PointsForPurchases_Awarded")]
        public OrderStatus PointsForPurchases_Awarded { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.RewardPoints.PointsForPurchases_Canceled")]
		public OrderStatus PointsForPurchases_Canceled { get; set; }

        public string PrimaryStoreCurrencyCode { get; set; }
    }
}