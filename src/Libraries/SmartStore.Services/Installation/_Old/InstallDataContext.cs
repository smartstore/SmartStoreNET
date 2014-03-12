//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using SmartStore.Core.Domain.Localization;

//namespace SmartStore.Services.Installation
//{
    
//	public class InstallDataContext
//	{
//		public InstallDataContext()
//		{
//			this.InstallSampleData = true;
//			this.StoreMediaInDB = true;
//			this.ProgressCallback = (x) => { }; // Noop
//		}

//		public string DefaultUserName { get; set; }
//		public string DefaultUserPassword { get; set; }
//		public Language Language { get; set; }
//		public InvariantInstallationData InstallData { get; set; }
//		public bool InstallSampleData { get; set; }
//		public bool StoreMediaInDB { get; set; }
//		public Action<int> ProgressCallback { get; set; }
//	}

//}
