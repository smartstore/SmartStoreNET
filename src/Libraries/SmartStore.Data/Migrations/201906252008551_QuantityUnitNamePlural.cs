namespace SmartStore.Data.Migrations
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using SmartStore.Core.Data;
    using SmartStore.Core.Domain.Directory;
    using SmartStore.Core.Domain.Localization;
    using SmartStore.Data.Setup;

    public partial class QuantityUnitNamePlural : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.QuantityUnit", "NamePlural", c => c.String(nullable: false, maxLength: 50, defaultValue: ""));
        }

        public override void Down()
        {
            DropColumn("dbo.QuantityUnit", "NamePlural");
        }

        public bool RollbackOnFailure => true;

        public void Seed(SmartObjectContext context)
        {
            context.MigrateLocaleResources(MigrateLocaleResources);
            context.SaveChanges();

            if (!DataSettings.DatabaseIsInstalled())
            {
                return;
            }

            var quPluralDe = new Dictionary<string, string>
            {
                { "Stück", "Stück" },
                { "Schachtel", "Schachteln" },
                { "Paket", "Pakete" },
                { "Palette", "Paletten" },
                { "Kiste", "Kisten" },
                { "Einheit", "Einheiten" },
                { "Sack", "Säcke" },
                { "Tüte", "Tüten" },
                { "Dose", "Dosen" },
                { "Stange", "Stangen" },
                { "Flasche", "Flaschen" },
                { "Glas", "Gläser" },
                { "Bund", "Bünde" },
                { "Rolle", "Rollen" },
                { "Fass", "Fässer" },
                { "Set", "Sets" }
            };

            var quPluralEn = new Dictionary<string, string>
            {
                { "Piece", "Pieces" },
                { "Box", "Boxes" },
                { "Parcel", "Parcels" },
                { "Palette", "Pallets" },
                { "Unit", "Units" },
                { "Sack", "Sacks" },
                { "Bag", "Bags" },
                { "Can", "Cans" },
                { "Tin", "Tins" },
                { "Packet", "Packets" },
                { "Package", "Packages" },
                { "Bar", "Bars" },
                { "Bottle", "Bottles" },
                { "Glass", "Glasses" },
                { "Bunch", "Bunches" },
                { "Roll", "Rolls" },
                { "Cup", "Cups" },
                { "Bundle", "Bundles" },
                { "Barrel", "Barrels" },
                { "Set", "Sets" },
                { "Bucket", "Buckets" }
            };

            using (var scope = new DbContextScope(ctx: context, validateOnSave: false, hooksEnabled: false, autoCommit: false))
            {
                var languages = context.Set<Language>().ToDictionary(x => x.Id, x => x);
                var defaultLang = languages.Values.OrderBy(x => x.DisplayOrder).First();

                // Quantity units.
                var quantityUnits = context.Set<QuantityUnit>().ToList();
                if (quantityUnits.Any())
                {
                    foreach (var quantityUnit in quantityUnits)
                    {
                        var name = quantityUnit.Name.EmptyNull();
                        string namePlural = null;

                        if (defaultLang.UniqueSeoCode.IsCaseInsensitiveEqual("de"))
                        {
                            quPluralDe.TryGetValue(name, out namePlural);
                        }
                        else if (defaultLang.UniqueSeoCode.IsCaseInsensitiveEqual("en"))
                        {
                            quPluralEn.TryGetValue(name, out namePlural);
                        }

                        quantityUnit.NamePlural = namePlural.NullEmpty() ?? name;
                    }

                    scope.Commit();
                }

                // Localized properties.
                var lpSet = context.Set<LocalizedProperty>();
                var pluralCount = lpSet
                    .Where(x => x.LocaleKeyGroup == "QuantityUnit" && x.LocaleKey == "NamePlural")
                    .Count();

                if (pluralCount == 0)
                {
                    var localizedProperties = lpSet
                        .Where(x => x.LocaleKeyGroup == "QuantityUnit" && x.LocaleKey == "Name")
                        .ToList();

                    if (localizedProperties.Any())
                    {
                        foreach (var lp in localizedProperties)
                        {
                            if (languages.TryGetValue(lp.LanguageId, out var language))
                            {
                                var name = lp.LocaleValue.EmptyNull();
                                string namePlural = null;

                                if (language.UniqueSeoCode.IsCaseInsensitiveEqual("de"))
                                {
                                    quPluralDe.TryGetValue(name, out namePlural);
                                }
                                else if (language.UniqueSeoCode.IsCaseInsensitiveEqual("en"))
                                {
                                    quPluralEn.TryGetValue(name, out namePlural);
                                }

                                if (namePlural.HasValue())
                                {
                                    lpSet.Add(new LocalizedProperty
                                    {
                                        EntityId = lp.EntityId,
                                        LanguageId = lp.LanguageId,
                                        LocaleKeyGroup = lp.LocaleKeyGroup,
                                        LocaleKey = "NamePlural",
                                        LocaleValue = namePlural
                                    });
                                }
                            }
                        }

                        scope.Commit();
                    }
                }
            }
        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate("Common.Plural", "Plural", "Mehrzahl");

            builder.AddOrUpdate("Admin.Configuration.QuantityUnit.Fields.NamePlural",
                "Name plural",
                "Name Mehrzahl",
                "Sets the name in plural. Example: \"Barrels\" for the unit \"Barrel\".",
                "Legt den Namen in Mehrzahl fest. Beispiel: \"Fässer\" für die Verpackungseinheit \"Fass\".");
        }
    }
}
