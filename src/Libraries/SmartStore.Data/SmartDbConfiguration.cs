using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Data.Initializers;

namespace SmartStore.Data
{

	public class SmartDbConfiguration : DbConfiguration
	{
		public SmartDbConfiguration()
		{
			IEfDataProvider provider = null;
			try
			{
				provider = (new EfDataProviderFactory(DataSettings.Current).LoadDataProvider()) as IEfDataProvider;
			}
			catch { /* SmartStore is not installed yet! */ }

			if (provider != null)
			{
				base.SetDefaultConnectionFactory(provider.GetConnectionFactory());
				base.SetDatabaseInitializer(provider.GetDatabaseInitializer());
			}
		}
	}

	//internal class DbDependencyResolver : IDbDependencyResolver
	//{
	//	public object GetService(Type type, object key)
	//	{
	//		if (type == typeof(Func<DbContext>))
	//		{
	//			return new SmartObjectContext("Data Source=MURATCAKIR-PC\\SQLEXPRESS;Initial Catalog=SmartStoreNET4;Integrated Security=False;Persist Security Info=False;User ID=sa;Password=leoman;Enlist=False;Pooling=True;Min Pool Size=1;Max Pool Size=100;MultipleActiveResultSets=True;Connect Timeout=15;User Instance=False");
				
	//			// runs only during EF migrations.
	//			var dataSettingsManager = new DataSettingsManager();
	//			var dataProviderSettings = dataSettingsManager.LoadSettings();

	//			if (dataProviderSettings != null && dataProviderSettings.IsValid())
	//			{
	//				return new Func<DbContext>(() =>
	//				{
	//					var ctxType = key as Type;
	//					if (ctxType == null)
	//					{
	//						return new SmartObjectContext(dataProviderSettings.DataConnectionString);
	//					}
	//					else
	//					{
	//						var result = Activator.CreateInstance(ctxType, dataProviderSettings.DataConnectionString) as DbContext;
	//						return result;
	//					}
	//				});
	//			}
	//		}

	//		return null;
	//	}

	//	public IEnumerable<object> GetServices(Type type, object key)
	//	{
	//		return Enumerable.Empty<object>();
	//	}
	//}

}
