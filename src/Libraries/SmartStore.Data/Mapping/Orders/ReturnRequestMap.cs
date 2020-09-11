using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.Data.Mapping.Orders
{
    public partial class ReturnRequestMap : EntityTypeConfiguration<ReturnRequest>
    {
        public ReturnRequestMap()
        {
            this.ToTable("ReturnRequest");
            this.HasKey(rr => rr.Id);
            this.Property(rr => rr.ReasonForReturn).IsRequired();
            this.Property(rr => rr.RequestedAction).IsRequired();
            this.Property(rr => rr.AdminComment).HasMaxLength(4000);

            this.Ignore(rr => rr.ReturnRequestStatus);

            this.HasRequired(rr => rr.Customer)
                .WithMany(c => c.ReturnRequests)
                .HasForeignKey(rr => rr.CustomerId);
        }
    }
}