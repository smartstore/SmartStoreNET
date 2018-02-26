using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.Localization
{
	public interface ILocalizationFileResolver
	{
	}

	public class LocalizationFileResolveResult
	{
		public string Culture { get; set; }
		public string VirtualPath { get; set; }
	}
}
