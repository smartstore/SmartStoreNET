namespace SmartStore.Data.Setup
{

    public interface ILocaleResourcesProvider
    {
        void MigrateLocaleResources(LocaleResourcesBuilder builder);
    }

}
