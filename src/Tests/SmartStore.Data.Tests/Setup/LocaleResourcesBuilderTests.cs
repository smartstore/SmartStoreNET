using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using SmartStore.Tests;
using SmartStore.Data.Setup;
using SmartStore.Core.Domain.Localization;
using NUnit.Framework;

namespace SmartStore.Data.Tests.Setup
{

	[TestFixture]
	public class LocaleResourcesBuilderTests : PersistenceTest
	{

		public override void SetUp()
		{
			base.SetUp();

			var langs = context.Set<Language>();
			if (!langs.Any())
			{
				langs.Add(new Language { UniqueSeoCode = "en", Name = "English", Published = true, LanguageCulture = "en-US" });
				langs.Add(new Language { UniqueSeoCode = "de", Name = "Deutsch", Published = true, LanguageCulture = "de-DE" });
				context.SaveChanges();
			}
		}

		[Test]
		public void Can_add_entries()
		{
			var resources = context.Set<LocaleStringResource>();
			resources.Any().ShouldBeFalse();

			var entries = GetDefaultEntries();
			var migrator = new LocaleResourcesMigrator(context);
			migrator.Migrate(entries);

			ReloadContext();
			resources = context.Set<LocaleStringResource>();

			resources.ToList().Count.ShouldEqual(6);
			resources.RemoveRange(resources.ToList());
			context.SaveChanges();
		}

		[Test]
		public void Can_delete_entries()
		{
			var resources = context.Set<LocaleStringResource>();
			resources.Any().ShouldBeFalse();

			var entries = GetDefaultEntries();
			var migrator = new LocaleResourcesMigrator(context);
			migrator.Migrate(entries);

			var builder = new LocaleResourcesBuilder();
			builder.DeleteFor("de", "Res1", "Res2", "Res3");
			builder.DeleteFor("en", "Res1");
			migrator.Migrate(builder.Build());

			resources.ToList().Count.ShouldEqual(2);

			builder.Reset();
			builder.DeleteFor("en", "Res2");
			migrator.Migrate(builder.Build());
			resources.ToList().Count.ShouldEqual(1);

			resources.RemoveRange(resources.ToList());
			context.SaveChanges();
		}

		[Test]
		public void Can_update_entries()
		{
			var resources = context.Set<LocaleStringResource>();
			resources.Any().ShouldBeFalse();

			var entries = GetDefaultEntries();
			var migrator = new LocaleResourcesMigrator(context);
			migrator.Migrate(entries);

			var builder = new LocaleResourcesBuilder();
			builder.AddOrUpdate("Res1").Value("NewValue1");
			migrator.Migrate(builder.Build());

			resources.ToList().Count.ShouldEqual(6);

			var updated = resources.Where(x => x.ResourceName == "Res1").ToList();
			updated.Count.ShouldEqual(2);
			updated.Each(x => x.ResourceValue.ShouldEqual("NewValue1"));

			resources.RemoveRange(resources.ToList());
			context.SaveChanges();
		}

		[Test]
		public void Can_delete_and_update_entries()
		{
			var resources = context.Set<LocaleStringResource>();
			resources.Any().ShouldBeFalse();

			var entries = GetDefaultEntries();
			var migrator = new LocaleResourcesMigrator(context);
			migrator.Migrate(entries);

			var builder = new LocaleResourcesBuilder();
			builder.Delete("Res1");
			builder.AddOrUpdate("Res1").Value("NewValue1");
			migrator.Migrate(builder.Build());

			resources.ToList().Count.ShouldEqual(6);

			var updated = resources.Where(x => x.ResourceName == "Res1").ToList();
			updated.Count.ShouldEqual(2);
			updated.Each(x => x.ResourceValue.ShouldEqual("NewValue1"));

			resources.RemoveRange(resources.ToList());
			context.SaveChanges();
		}

		private IEnumerable<LocaleResourceEntry> GetDefaultEntries()
		{
			var builder = new LocaleResourcesBuilder();
			builder.AddOrUpdate("Res1").Value("en", "Value1");
			builder.AddOrUpdate("Res2").Value("en", "Value2");
			builder.AddOrUpdate("Res3").Value("en", "Value3");
			builder.AddOrUpdate("Res1").Value("de", "Wert1");
			builder.AddOrUpdate("Res2").Value("de", "Wert2");
			builder.AddOrUpdate("Res3").Value("de", "Wert3");

			return builder.Build();
		}

	}

}
