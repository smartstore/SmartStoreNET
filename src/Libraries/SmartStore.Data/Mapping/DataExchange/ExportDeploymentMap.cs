using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain;

namespace SmartStore.Data.Mapping.DataExchange
{
	public class ExportDeploymentMap : EntityTypeConfiguration<ExportDeployment>
	{
		public ExportDeploymentMap()
		{
			this.ToTable("ExportDeployment");
			this.HasKey(x => x.Id);

			this.Property(x => x.Name).IsRequired().HasMaxLength(100);
			this.Property(x => x.ResultInfo).IsMaxLength();
			this.Property(x => x.Username).HasMaxLength(400);
			this.Property(x => x.Password).HasMaxLength(400);
			this.Property(x => x.Url).HasMaxLength(4000);
			this.Property(x => x.FileSystemPath).HasMaxLength(400);
			this.Property(x => x.SubFolder).HasMaxLength(400);
			this.Property(x => x.EmailAddresses).HasMaxLength(4000);
			this.Property(x => x.EmailSubject).HasMaxLength(400);

			this.Ignore(x => x.DeploymentType);

			this.HasRequired(x => x.Profile)
				.WithMany(x => x.Deployments)
				.HasForeignKey(x => x.ProfileId)
				.WillCascadeOnDelete(true);
		}
	}
}
