using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Common
{
	[Serializable]
	public class CheckUpdateResult : ModelBase
	{
		public bool UpdateAvailable { get; set; }
		public string CurrentVersion { get; set; }
		public string LanguageCode { get; set; }
		
		public string Version { get; set; }
		public string FullName { get; set; }
		public string ReleaseNotes { get; set; }
		public string InfoUrl { get; set; }
		public string DownloadUrl { get; set; }
		public DateTime ReleaseDateUtc { get; set; }
		public bool IsStable { get; set; }
		public bool AutoUpdatePossible { get; set; }
		public string AutoUpdatePackageUrl { get; set; }
	}
}