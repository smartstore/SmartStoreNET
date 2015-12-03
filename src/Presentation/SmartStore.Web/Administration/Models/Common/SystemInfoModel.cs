using System;
using System.Collections.Generic;
using SmartStore.Web.Framework;
using SmartStore.Utilities;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Common
{
    public class SystemInfoModel : ModelBase
    {
        public SystemInfoModel()
        {
            this.LoadedAssemblies = new List<LoadedAssembly>();
        }

        [SmartResourceDisplayName("Admin.System.SystemInfo.ASPNETInfo")]
        public string AspNetInfo { get; set; }

        [SmartResourceDisplayName("Admin.System.SystemInfo.IsFullTrust")]
        public string IsFullTrust { get; set; }

        [SmartResourceDisplayName("Admin.System.SystemInfo.AppVersion")]
        public string AppVersion { get; set; }

		[SmartResourceDisplayName("Admin.System.SystemInfo.AppDate")]
		public DateTime AppDate { get; set; }

        [SmartResourceDisplayName("Admin.System.SystemInfo.OperatingSystem")]
        public string OperatingSystem { get; set; }

        [SmartResourceDisplayName("Admin.System.SystemInfo.ServerLocalTime")]
        public DateTime ServerLocalTime { get; set; }

        [SmartResourceDisplayName("Admin.System.SystemInfo.ServerTimeZone")]
        public string ServerTimeZone { get; set; }

        [SmartResourceDisplayName("Admin.System.SystemInfo.UTCTime")]
        public DateTime UtcTime { get; set; }

		[SmartResourceDisplayName("Admin.System.SystemInfo.HTTPHOST")]
		public string HttpHost { get; set; }

        [SmartResourceDisplayName("Admin.System.SystemInfo.LoadedAssemblies")]
        public IList<LoadedAssembly> LoadedAssemblies { get; set; }

		[SmartResourceDisplayName("Admin.System.SystemInfo.DatabaseSize")]
		public long DatabaseSize { get; set; }
		public string DatabaseSizeString
		{
			get
			{
				return (DatabaseSize == 0 ? "" : Prettifier.BytesToString(DatabaseSize));
			}
		}

		[SmartResourceDisplayName("Admin.System.SystemInfo.UsedMemorySize")]
		public long UsedMemorySize { get; set; }
		public string UsedMemorySizeString
		{
			get
			{
				return Prettifier.BytesToString(UsedMemorySize);
			}
		}

		[SmartResourceDisplayName("Admin.System.SystemInfo.DataProviderFriendlyName")]
		public string DataProviderFriendlyName { get; set; }

		public bool ShrinkDatabaseEnabled { get; set; }

        public class LoadedAssembly : ModelBase
        {
            public string FullName { get; set; }
            public string Location { get; set; }
        }

    }
}