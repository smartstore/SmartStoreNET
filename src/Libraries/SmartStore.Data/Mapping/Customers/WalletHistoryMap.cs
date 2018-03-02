using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Data.Mapping.Customers
{
	public partial class WalletHistoryMap : EntityTypeConfiguration<WalletHistory>
	{
		public WalletHistoryMap()
		{
			ToTable("WalletHistory");
			HasKey(x => x.Id);

			Property(x => x.Amount).HasPrecision(18, 4);
			Property(x => x.AmountBalance).HasPrecision(18, 4);

			HasRequired(x => x.Customer)
				.WithMany(x => x.WalletHistory)
				.HasForeignKey(x => x.CustomerId);

			HasOptional(x => x.UsedWithOrder)
				.WithOptionalDependent(x => x.WalletHistoryEntry)
				.WillCascadeOnDelete(false);
		}
	}
}
