using System;

namespace SmartStore.Data.Setup
{
	
	public interface ILocaleResourcesProvider
	{
		void AlterLocaleResources(LocaleResourcesBuilder builder);
	}

}
