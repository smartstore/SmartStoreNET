using SmartStore.Core.Domain.Stores;
using SmartStore.Tests;
using NUnit.Framework;

namespace SmartStore.Data.Tests.Stores
{
	[TestFixture]
	public class StorePersistenceTests : PersistenceTest
	{
		[Test]
		public void Can_save_and_load_store()
		{
			var store = new Store
			{
				Name = "Computer store",
				Url = "http://www.yourStore.com",
				Hosts = "yourStore.com,www.yourStore.com",
				LogoPictureId = 0,
				DisplayOrder = 1
			};

			var fromDb = SaveAndLoadEntity(store);
			fromDb.ShouldNotBeNull();
			fromDb.Name.ShouldEqual("Computer store");
			fromDb.Url.ShouldEqual("http://www.yourStore.com");
			fromDb.Hosts.ShouldEqual("yourStore.com,www.yourStore.com");
			fromDb.LogoPictureId.ShouldEqual(0);
			fromDb.DisplayOrder.ShouldEqual(1);
		}
	}
}
