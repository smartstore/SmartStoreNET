using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartStore.Core.Events;
using SmartStore.Data.Setup;
using Autofac;

namespace SmartStore.Services.Media.Migration
{
    public class SeedingDbMigrationConsumer : IConsumer
    {
        public void Handle(SeedingDbMigrationEvent message, IComponentContext componentContext)
        {
            if (message.MigrationName == MediaMigrator.MigrationName)
            {
                componentContext.Resolve<MediaMigrator>().Migrate();
            }
            else if (message.MigrationName == MediaMigrator3.MigrationName)
            {
                componentContext.Resolve<MediaMigrator3>().Migrate();
            }
        }
    }
}
