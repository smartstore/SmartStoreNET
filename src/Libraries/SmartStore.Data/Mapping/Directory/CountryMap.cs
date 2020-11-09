using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Directory;

namespace SmartStore.Data.Mapping.Directory
{
    public partial class CountryMap : EntityTypeConfiguration<Country>
    {
        public CountryMap()
        {
            ToTable("Country");
            HasKey(c => c.Id);
            Property(c => c.Name).IsRequired().HasMaxLength(100);
            Property(c => c.TwoLetterIsoCode).HasMaxLength(2);
            Property(c => c.ThreeLetterIsoCode).HasMaxLength(3);

            HasOptional(x => x.DefaultCurrency)
                .WithMany()
                .HasForeignKey(x => x.DefaultCurrencyId)
                .WillCascadeOnDelete(false);
        }
    }
}