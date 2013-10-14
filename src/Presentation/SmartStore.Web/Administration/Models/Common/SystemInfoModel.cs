using System;
using System.Collections.Generic;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

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
		public double DatabaseSize { get; set; }

        public class LoadedAssembly : ModelBase
        {
            public string FullName { get; set; }
            public string Location { get; set; }
        }

    }
}