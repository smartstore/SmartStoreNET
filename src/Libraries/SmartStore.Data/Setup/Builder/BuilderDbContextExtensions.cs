using System;

namespace SmartStore.Data.Setup
{
    public static class BuilderDbContextExtensions
    {
        #region Resource building

        public static void MigrateLocaleResources(this SmartObjectContext ctx, Action<LocaleResourcesBuilder> fn, bool updateTouchedResources = false)
        {
            Guard.NotNull(ctx, nameof(ctx));
            Guard.NotNull(fn, nameof(fn));

            var builder = new LocaleResourcesBuilder();
            fn(builder);
            var entries = builder.Build();

            var migrator = new LocaleResourcesMigrator(ctx);
            migrator.Migrate(entries, updateTouchedResources);
        }

        #endregion

        #region Settings building

        public static void MigrateSettings(this SmartObjectContext ctx, Action<SettingsBuilder> fn)
        {
            Guard.NotNull(ctx, nameof(ctx));
            Guard.NotNull(fn, nameof(fn));

            var builder = new SettingsBuilder();
            fn(builder);
            var entries = builder.Build();

            var migrator = new SettingsMigrator(ctx);
            migrator.Migrate(entries);
        }

        #endregion
    }
}
