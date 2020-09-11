namespace SmartStore.Core.Data
{
    public class DbQuerySettings
    {
        public DbQuerySettings(bool ignoreAcl, bool ignoreMultiStore)
        {
            this.IgnoreAcl = ignoreAcl;
            this.IgnoreMultiStore = ignoreMultiStore;
        }

        public bool IgnoreAcl { get; private set; }
        public bool IgnoreMultiStore { get; private set; }

        public static DbQuerySettings Default { get; } = new DbQuerySettings(false, false);
    }
}
