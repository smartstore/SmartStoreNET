using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Threading;
using SmartStore.Utilities;
using SmartStore.Utilities.Threading;

namespace SmartStore.Core.Data
{
    public partial class DataSettings
    {
        private static readonly ReaderWriterLockSlim s_rwLock = new ReaderWriterLockSlim();
        private static DataSettings s_current = null;
        private static Func<DataSettings> s_settingsFactory = new Func<DataSettings>(() => new DataSettings());
        private static bool? s_installed = null;
        private static bool s_TestMode = false;

        protected const char SEPARATOR = ':';
        protected const string FILENAME = "Settings.txt";

        private DataSettings()
        {
            RawDataSettings = new Dictionary<string, string>();
        }

        #region Static members

        public static void SetDefaultFactory(Func<DataSettings> factory)
        {
            Guard.NotNull(factory, nameof(factory));

            lock (s_rwLock.GetWriteLock())
            {
                s_settingsFactory = factory;
            }
        }

        public static DataSettings Current
        {
            get
            {
                using (s_rwLock.GetUpgradeableReadLock())
                {
                    if (s_current == null)
                    {
                        using (s_rwLock.GetWriteLock())
                        {
                            if (s_current == null)
                            {
                                s_current = s_settingsFactory();
                                s_current.Load();
                            }
                        }
                    }
                }

                return s_current;
            }
        }

        public static bool DatabaseIsInstalled()
        {
            if (s_TestMode)
                return false;

            if (!s_installed.HasValue)
            {
                s_installed = Current.IsValid();
            }

            return s_installed.Value;
        }

        internal static void SetTestMode(bool isTestMode)
        {
            s_TestMode = isTestMode;
        }

        public static void Reload()
        {
            using (s_rwLock.GetWriteLock())
            {
                s_current = null;
                s_installed = null;
            }
        }

        public static void Delete()
        {
            if (s_current?.TenantPath != null)
            {
                using (s_rwLock.GetWriteLock())
                {
                    string filePath = Path.Combine(CommonHelper.MapPath(s_current.TenantPath), FILENAME);
                    try
                    {
                        File.Delete(filePath);
                    }
                    finally
                    {
                        s_current = null;
                        s_installed = null;
                    }
                }
            }
        }

        #endregion

        #region Instance members

        public string TenantName
        {
            get;
            private set;
        }

        public string TenantPath
        {
            get;
            private set;
        }

        public Version AppVersion
        {
            get;
            set;
        }

        public string DataProvider
        {
            get;
            set;
        }

        public string ProviderInvariantName
        {
            get
            {
                if (this.DataProvider.HasValue() && this.DataProvider.IsCaseInsensitiveEqual("sqlserver"))
                    return "System.Data.SqlClient";

                // SqlCe should always be the default provider
                return "System.Data.SqlServerCe.4.0";
            }
        }

        public string ProviderFriendlyName
        {
            get
            {
                if (this.DataProvider.HasValue() && this.DataProvider.IsCaseInsensitiveEqual("sqlserver"))
                    return "SQL Server";

                // SqlCe should always be the default provider
                return "SQL Server Compact (SQL CE)";
            }
        }

        public bool IsSqlServer => this.DataProvider.HasValue() && this.DataProvider.IsCaseInsensitiveEqual("sqlserver");

        public string DataConnectionString
        {
            get;
            set;
        }

        public string DataConnectionType
        {
            get;
            set;
        }

        public IDictionary<string, string> RawDataSettings
        {
            get;
            private set;
        }

        public bool IsValid()
        {
            return this.DataProvider.HasValue() && this.DataConnectionString.HasValue();
        }

        public virtual bool Load()
        {
            using (s_rwLock.GetWriteLock())
            {
                this.Reset();

                var curTenant = ResolveTenant();
                var tenantPath = "~/App_Data/Tenants/" + curTenant;
                string filePath = Path.Combine(CommonHelper.MapPath(tenantPath), FILENAME);

                this.TenantName = curTenant;
                this.TenantPath = tenantPath;

                if (File.Exists(filePath) && !s_TestMode)
                {
                    string text = File.ReadAllText(filePath);
                    var settings = ParseSettings(text);
                    if (settings.Any())
                    {
                        this.RawDataSettings.AddRange(settings);
                        if (settings.ContainsKey("AppVersion"))
                        {
                            this.AppVersion = new Version(settings["AppVersion"]);
                        }
                        if (settings.ContainsKey("DataProvider"))
                        {
                            this.DataProvider = settings["DataProvider"];
                            this.DataConnectionType = this.IsSqlServer
                                ? typeof(SqlConnection).AssemblyQualifiedName
                                : typeof(SqlCeConnection).AssemblyQualifiedName;
                        }
                        if (settings.ContainsKey("DataConnectionString"))
                        {
                            this.DataConnectionString = settings["DataConnectionString"];
                        }

                        return this.IsValid();
                    }
                }

                return false;
            }
        }

        public void Reset()
        {
            using (s_rwLock.GetWriteLock())
            {
                this.RawDataSettings.Clear();
                this.TenantName = null;
                this.TenantPath = null;
                this.AppVersion = null;
                this.DataProvider = null;
                this.DataConnectionString = null;
                this.DataConnectionType = null;

                s_installed = null;
            }
        }

        public virtual bool Save()
        {
            if (!this.IsValid())
                return false;

            using (s_rwLock.GetWriteLock())
            {
                string filePath = Path.Combine(CommonHelper.MapPath(this.TenantPath), FILENAME);
                if (!File.Exists(filePath))
                {
                    using (File.Create(filePath))
                    {
                        // we use 'using' to close the file after it's created
                    }
                }

                var text = SerializeSettings();
                File.WriteAllText(filePath, text);

                return true;
            }
        }

        #endregion

        #region Instance helpers

        protected virtual string ResolveTenant()
        {
            var tenantsBaseDir = CommonHelper.MapPath("~/App_Data/Tenants");
            var curTenantFile = Path.Combine(tenantsBaseDir, "current.txt");

            string curTenant = null;

            if (File.Exists(curTenantFile))
            {
                curTenant = File.ReadAllText(curTenantFile).EmptyNull().Trim().NullEmpty();
                if (curTenant != curTenant.EmptyNull().ToValidPath())
                {
                    // File contains invalid path string
                    curTenant = null;
                }

                if (curTenant != null && !Directory.Exists(Path.Combine(tenantsBaseDir, curTenant)))
                {
                    // Specified Tenant directory does not exist
                    curTenant = null;
                }
            }

            curTenant = curTenant ?? "Default";

            var tenantPath = Path.Combine(tenantsBaseDir, curTenant);

            if (curTenant.IsCaseInsensitiveEqual("Default") && !Directory.Exists(tenantPath))
            {
                // The Default tenant dir should be created if it doesn't exist
                Directory.CreateDirectory(tenantPath);
            }

            return curTenant.TrimEnd('/');
        }

        protected virtual IDictionary<string, string> ParseSettings(string text)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (text.IsEmpty())
                return result;

            var settings = new List<string>();
            using (var reader = new StringReader(text))
            {
                string str;
                while ((str = reader.ReadLine()) != null)
                    settings.Add(str);
            }

            foreach (var setting in settings)
            {
                var separatorIndex = setting.IndexOf(SEPARATOR);
                if (separatorIndex == -1)
                {
                    continue;
                }
                string key = setting.Substring(0, separatorIndex).Trim();
                string value = setting.Substring(separatorIndex + 1).Trim();

                if (key.HasValue() && value.HasValue())
                {
                    result.Add(key, value);
                }
            }

            return result;
        }

        protected virtual string SerializeSettings()
        {
            return string.Format("AppVersion: {0}{3}DataProvider: {1}{3}DataConnectionString: {2}{3}",
                this.AppVersion.ToString(),
                this.DataProvider,
                this.DataConnectionString,
                Environment.NewLine);
        }

        #endregion
    }
}
