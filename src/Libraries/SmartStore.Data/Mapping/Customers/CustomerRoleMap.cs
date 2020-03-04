using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Data.Mapping.Customers
{
    public partial class CustomerRoleMap : EntityTypeConfiguration<CustomerRole>
    {
        public CustomerRoleMap()
        {
            ToTable("CustomerRole");
            HasKey(cr => cr.Id);
            Property(cr => cr.Name).IsRequired().HasMaxLength(255);
            Property(cr => cr.SystemName).HasMaxLength(255);

            HasMany(cr => cr.RuleSets)
                .WithMany(rs => rs.CustomerRoles)
                .Map(m => m.ToTable("RuleSet_CustomerRole_Mapping"));
        }
    }
}