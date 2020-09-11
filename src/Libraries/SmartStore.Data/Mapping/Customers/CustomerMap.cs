using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Data.Mapping.Customers
{
    public partial class CustomerMap : EntityTypeConfiguration<Customer>
    {
        public CustomerMap()
        {
            ToTable("Customer");
            HasKey(c => c.Id);
            Property(u => u.Username).HasMaxLength(500);
            Property(u => u.Email).HasMaxLength(500);
            Property(u => u.SystemName).HasMaxLength(500);
            Property(u => u.Password).HasMaxLength(500);
            Property(u => u.PasswordSalt).HasMaxLength(500);
            Property(u => u.LastIpAddress).HasMaxLength(100);

            Property(u => u.Title).HasMaxLength(100);
            Property(u => u.Salutation).HasMaxLength(50);
            Property(u => u.FirstName).HasMaxLength(225);
            Property(u => u.LastName).HasMaxLength(225);
            Property(u => u.FullName).HasMaxLength(450);
            Property(u => u.Company).HasMaxLength(255);
            Property(u => u.CustomerNumber).HasMaxLength(100);

            Ignore(u => u.PasswordFormat);

            HasMany<Address>(c => c.Addresses)
                .WithMany()
                .Map(m => m.ToTable("CustomerAddresses"));

            HasOptional<Address>(c => c.BillingAddress);
            HasOptional<Address>(c => c.ShippingAddress);
        }
    }
}