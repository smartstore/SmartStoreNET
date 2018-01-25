using SmartStore.GoogleMerchantCenter.Domain;

namespace SmartStore.GoogleMerchantCenter
{
	public static class MiscExtensions
	{
		public static bool IsTouched(this GoogleProductRecord p)
		{
			if (p != null)
			{
				return
					p.Taxonomy.HasValue() || p.Gender.HasValue() || p.AgeGroup.HasValue() || p.Color.HasValue() ||
					p.Size.HasValue() || p.Material.HasValue() || p.Pattern.HasValue() || p.ItemGroupId.HasValue() ||
					!p.Export || p.Multipack != 0 || p.IsBundle.HasValue || p.IsAdult.HasValue || p.EnergyEfficiencyClass.HasValue() ||
					p.CustomLabel0.HasValue() || p.CustomLabel1.HasValue() || p.CustomLabel2.HasValue() || p.CustomLabel3.HasValue() || p.CustomLabel4.HasValue();
			}

			return false;
		}
	}
}
