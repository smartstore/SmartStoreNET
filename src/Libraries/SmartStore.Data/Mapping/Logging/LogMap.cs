using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Logging;

namespace SmartStore.Data.Mapping.Logging
{
    public partial class LogMap : EntityTypeConfiguration<Log>
    {
        public LogMap()
        {
            this.ToTable("Log");
            this.HasKey(l => l.Id);
            this.Property(l => l.ShortMessage).IsRequired().HasMaxLength(4000);
            this.Property(l => l.Logger).IsRequired().HasMaxLength(400);
            this.Property(l => l.FullMessage).IsMaxLength();
            this.Property(l => l.IpAddress).HasMaxLength(200);
            this.Property(l => l.UserName).HasMaxLength(100);
            this.Property(l => l.HttpMethod).HasMaxLength(10);
            this.Property(l => l.PageUrl).HasMaxLength(1500);
            this.Property(l => l.ReferrerUrl).HasMaxLength(1500);

            this.Ignore(l => l.LogLevel);

            this.HasOptional(l => l.Customer)
                .WithMany()
                .HasForeignKey(l => l.CustomerId)
            .WillCascadeOnDelete(true);

        }
    }
}