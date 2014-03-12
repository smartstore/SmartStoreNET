using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Data.Setup
{

	public class SeedDataConfiguration
	{
		public SeedDataConfiguration()
		{
			this.SeedSampleData = true;
			this.StoreMediaInDB = true;
			this.ProgressMessageCallback = (x) => { }; // Noop
		}

		public string DefaultUserName { get; set; }
		public string DefaultUserPassword { get; set; }
		public Language Language { get; set; }
		public InvariantSeedData Data { get; set; }
		public bool SeedSampleData { get; set; }
		public bool StoreMediaInDB { get; set; }
		public Action<string> ProgressMessageCallback { get; set; }
	}

}
