using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SmartStore.Data.Migrations;

namespace SmartStore.Data.Tests
{
	[SetUpFixture]
	public class GlobalSetup
	{
		[SetUp]
		public void SetUp()
		{
			var ctx = new SmartObjectContext(GetTestDbName());
			Database.SetInitializer(new DropCreateDatabaseAlways<SmartObjectContext>());
			ctx.Database.Initialize(true);
		}

		[TearDown]
		public void TearDown()
		{
			var ctx = new SmartObjectContext(GetTestDbName());
			ctx.Database.Delete();
		}

		public static string GetTestDbName()
		{
			string testDbName = "Data Source=" + (System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)) + @"\SmartStore.Data.Tests.Db.sdf;Persist Security Info=False";
			return testDbName;
		}  
	}
}
