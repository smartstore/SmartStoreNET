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
			Property(x => x.AmountBalancePerStore).HasPrecision(18, 4);
			Property(x => x.Message).HasMaxLength(1000);
			Property(x => x.AdminComment).HasMaxLength(4000);

			HasRequired(x => x.Customer)
				.WithMany(x => x.WalletHistory)
				.HasForeignKey(x => x.CustomerId);

			HasOptional(x => x.Order)
				.WithMany(x => x.WalletHistory)
				.HasForeignKey(x => x.OrderId)
				.WillCascadeOnDelete(false);
		}
	}
}
