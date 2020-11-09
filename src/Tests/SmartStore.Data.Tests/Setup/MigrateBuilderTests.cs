using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SmartStore.Core.Domain.Configuration;
using SmartStore.Core.Domain.Localization;
using SmartStore.Data.Setup;
using SmartStore.Tests;

namespace SmartStore.Data.Tests.Setup
{
    [TestFixture]
    public class MigrateBuilderTests : PersistenceTest
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            var settings = context.Set<Setting>();
            settings.RemoveRange(settings.ToList());

            var resources = context.Set<LocaleStringResource>();
            resources.RemoveRange(resources.ToList());

            var langs = context.Set<Language>();
            langs.RemoveRange(langs.ToList());

            langs.Add(new Language { UniqueSeoCode = "en", Name = "English", Published = true, LanguageCulture = "en-US" });
            langs.Add(new Language { UniqueSeoCode = "de", Name = "Deutsch", Published = true, LanguageCulture = "de-DE" });

            context.SaveChanges();
        }

        [Test]
        public void Can_add_resource_entries()
        {
            var resources = context.Set<LocaleStringResource>();
            resources.Any().ShouldBeFalse();

            var entries = GetDefaultResourceEntries();
            var migrator = new LocaleResourcesMigrator(context);
            migrator.Migrate(entries);

            ReloadContext();
            resources = context.Set<LocaleStringResource>();

            resources.ToList().Count.ShouldEqual(6);
        }

        [Test]
        public void Can_delete_resource_entries()
        {
            var resources = context.Set<LocaleStringResource>();
            resources.Any().ShouldBeFalse();

            var entries = GetDefaultResourceEntries();
            var migrator = new LocaleResourcesMigrator(context);
            migrator.Migrate(entries);

            var builder = new LocaleResourcesBuilder();
            builder.DeleteFor("de", "Res1", "Res2", "Res3");
            builder.DeleteFor("en", "Res1");
            context.DetachEntities<Language>();
            migrator.Migrate(builder.Build());

            resources.ToList().Count.ShouldEqual(2);

            builder.Reset();
            builder.DeleteFor("en", "Res2");
            context.DetachEntities<Language>();
            migrator.Migrate(builder.Build());

            resources.ToList().Count.ShouldEqual(1);
        }

        [Test]
        public void Can_update_resource_entries()
        {
            var resources = context.Set<LocaleStringResource>();
            resources.Any().ShouldBeFalse();

            var entries = GetDefaultResourceEntries();
            var migrator = new LocaleResourcesMigrator(context);
            migrator.Migrate(entries);

            context.DetachEntities<Language>();

            var builder = new LocaleResourcesBuilder();
            builder.AddOrUpdate("Res1").Value("NewValue1");
            migrator.Migrate(builder.Build());

            resources.ToList().Count.ShouldEqual(6);

            var updated = resources.Where(x => x.ResourceName == "Res1").ToList();
            updated.Count.ShouldEqual(2);
            updated.Each(x => x.ResourceValue.ShouldEqual("NewValue1"));
        }

        [Test]
        public void Can_delete_and_update_resource_entries()
        {
            var resources = context.Set<LocaleStringResource>();
            resources.Any().ShouldBeFalse();

            var entries = GetDefaultResourceEntries();
            var migrator = new LocaleResourcesMigrator(context);
            migrator.Migrate(entries);

            context.DetachEntities<Language>();

            var builder = new LocaleResourcesBuilder();
            builder.Delete("Res1");
            builder.AddOrUpdate("Res1").Value("NewValue1");
            migrator.Migrate(builder.Build());

            resources.ToList().Count.ShouldEqual(6);

            var updated = resources.Where(x => x.ResourceName == "Res1").ToList();
            updated.Count.ShouldEqual(2);
            updated.Each(x => x.ResourceValue.ShouldEqual("NewValue1"));
        }

        private IEnumerable<LocaleResourceEntry> GetDefaultResourceEntries()
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



        [Test]
        public void Can_add_setting_entries()
        {
            var settings = context.Set<Setting>();
            settings.Any().ShouldBeFalse();

            var entries = GetDefaultSettingEntries();
            var migrator = new SettingsMigrator(context);
            migrator.Migrate(entries);

            ReloadContext();
            settings = context.Set<Setting>();

            settings.ToList().Count.ShouldEqual(8);
        }

        [Test]
        public void Can_delete_and_add_setting_entries()
        {
            var settings = context.Set<Setting>();
            settings.Any().ShouldBeFalse();

            var entries = GetDefaultSettingEntries();
            var migrator = new SettingsMigrator(context);
            migrator.Migrate(entries);

            var builder = new SettingsBuilder();
            builder.Delete("type1.setting1", "type2.setting1");
            migrator.Migrate(builder.Build());

            settings.ToList().Count.ShouldEqual(6);

            builder.Reset();
            builder.DeleteGroup("type1");
            migrator.Migrate(builder.Build());
            settings.ToList().Count.ShouldEqual(3);

            builder.Reset();
            builder.Add("type3.Setting1", true);
            builder.Add("type3.Setting2", 20);
            migrator.Migrate(builder.Build());
            var db = settings.ToList();
            db.Count.ShouldEqual(5);

            var st = settings.Where(x => x.Name == "type3.Setting2").FirstOrDefault();
            st.Value.ShouldEqual("20");
        }

        private IEnumerable<SettingEntry> GetDefaultSettingEntries()
        {
            var builder = new SettingsBuilder();
            builder.Add("Type1.Setting1", true);
            builder.Add("Type1.Setting2", 10);
            builder.Add("Type1.Setting3", "SomeString");
            builder.Add("Type1.Setting4", DateTime.Now);
            builder.Add("Type2.Setting1", false);
            builder.Add("Type2.Setting2", 5);
            builder.Add("Type2.Setting3", "SomeString2");
            builder.Add("Type2.Setting4", DateTime.UtcNow);

            return builder.Build();
        }

    }

}
