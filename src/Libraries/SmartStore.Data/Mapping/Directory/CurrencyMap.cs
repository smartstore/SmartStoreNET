using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Directory;

namespace SmartStore.Data.Mapping.Directory
{
    public partial class CurrencyMap : EntityTypeConfiguration<Currency>
    {
        public CurrencyMap()
        {
            this.ToTable("Currency");
            this.HasKey(c => c.Id);
            this.Property(c => c.Name).IsRequired().HasMaxLength(50);
            this.Property(c => c.CurrencyCode).IsRequired().HasMaxLength(5);
            this.Property(c => c.DisplayLocale).HasMaxLength(50);
            this.Property(c => c.CustomFormatting).HasMaxLength(50);
            this.Property(c => c.Rate).HasPrecision(18, 8); // // With virtual currencies (e.g. BitCoin) being so precise, we need to store rates up to 8 decimal places
            this.Property(c => c.DomainEndings).HasMaxLength(1000);
            this.Property(o => o.RoundOrderTotalDenominator).HasPrecision(18, 4);
        }
    }
}