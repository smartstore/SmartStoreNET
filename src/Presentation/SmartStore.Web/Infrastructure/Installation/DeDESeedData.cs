using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Configuration;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Logging;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.News;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Polls;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Domain.Topics;
using SmartStore.Rules.Domain;
using SmartStore.Data.Setup;

namespace SmartStore.Web.Infrastructure.Installation
{
    public class DeDESeedData : InvariantSeedData
    {
        private readonly IDictionary<string, TaxCategory> _taxCategories = new Dictionary<string, TaxCategory>();
        private DeliveryTime _defaultDeliveryTime;

        protected override void Alter(Customer entity)
        {
            base.Alter(entity);

            if (entity.SystemName == SystemCustomerNames.SearchEngine)
            {
                entity.AdminComment = "System-Gastkonto für Suchmaschinenanfragen.";
            }
            else if (entity.SystemName == SystemCustomerNames.BackgroundTask)
            {
                entity.AdminComment = "Systemkonto für geplante Aufgaben.";
            }
            else if (entity.SystemName == SystemCustomerNames.PdfConverter)
            {
                entity.AdminComment = "Systemkonto für den PDF-Konverter.";
            }
        }

        protected override void Alter(IList<MeasureDimension> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.SystemKeyword)
                .Alter("mm", x =>
                {
                    x.Name = "Millimeter";
                    x.Ratio = 0.001M;
                })
                .Alter("cm", x =>
                {
                    x.Name = "Zentimeter";
                    x.Ratio = 0.01M;
                })
                .Alter("m", x =>
                {
                    x.Name = "Meter";
                    x.Ratio = 1M;
                })
                .Alter("inch", x =>
                {
                    x.Name = "Zoll";
                    x.Ratio = 0.0254M;
                })
                .Alter("ft", x =>
                {
                    x.Name = "Fuß";
                    x.Ratio = 0.3048M;
                });
        }

        protected override void Alter(IList<MeasureWeight> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.SystemKeyword)
                .Alter("oz", x =>
                {
                    x.Name = "Unze";
                    x.Ratio = 0.02835M;
                    x.DisplayOrder = 10;
                })
                .Alter("lb", x =>
                {
                    x.Name = "Pfund";
                    x.Ratio = 0.4536M;
                    x.DisplayOrder = 10;
                })
                .Alter("kg", x =>
                {
                    x.Name = "Kilogramm";
                    x.Ratio = 1M;
                    x.DisplayOrder = 1;
                })
                .Alter("g", x =>
                {
                    x.Name = "Gramm";
                    x.Ratio = 0.001M;
                    x.DisplayOrder = 2;
                })
                .Alter("l", x =>
                {
                    x.Name = "Liter";
                    x.Ratio = 1M;
                    x.DisplayOrder = 3;
                })
                .Alter("ml", x =>
                {
                    x.Name = "Milliliter";
                    x.Ratio = 0.001M;
                    x.DisplayOrder = 4;
                });
        }

        protected override void Alter(IList<ShippingMethod> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.DisplayOrder)
                .Alter(0, x =>
                {
                    x.Name = "Abholung";
                    x.Description = "Holen Sie Ihre Bestellung direkt bei uns ab.";
                })
                .Alter(1, x =>
                {
                    x.Name = "Versand";
                    x.Description = "Ihre Bestellung wird Ihnen durch unsere Versandpartner zugestellt.";
                })
                .Alter(2, x =>
                {
                    x.Name = "Kostenloser Versand";
                });
        }

        protected override void Alter(IList<Currency> entities)
        {
            base.Alter(entities);

            // unpublish all currencies
            entities.Each(x => x.Published = false);

            entities.WithKey(x => x.DisplayLocale)
               .Alter("de-DE", x =>
               {
                   x.Published = true;
                   x.Rate = 1M;
                   x.DisplayOrder = -10;
               })
               .Alter("de-CH", x =>
               {
                   x.Rate = 1.20M;
                   x.DisplayOrder = -5;
               })
               .Alter("en-US", x => x.Rate = 1.29M)
               .Alter("en-AU", x => x.Rate = 1.24M)
               .Alter("en-CA", x => x.Rate = 1.28M)
               .Alter("tr-TR", x => x.Rate = 2.31M)
               .Alter("zh-CN", x => x.Rate = 8.02M)
               .Alter("zh-HK", x => x.Rate = 9.98M)
               .Alter("ja-JP", x => x.Rate = 106.21M)
               .Alter("ru-RU", x => x.Rate = 40.16M)
               .Alter("sv-SE", x => x.Rate = 8.60M);
        }

        protected override void Alter(IList<CustomerRole> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.Name)
               .Alter("Administrators", x =>
               {
                   x.Name = "Administratoren";
               })
               .Alter("Forum Moderators", x =>
               {
                   x.Name = "Foren Moderatoren";
               })
               .Alter("Registered", x =>
               {
                   x.Name = "Registriert";
               })
               .Alter("Guests", x =>
               {
                   x.Name = "Gäste";
               })
               .Alter("Inactive new customers", x =>
               {
                   x.Name = "Inaktive Neukunden";
               });
        }

        protected override void Alter(Address entity)
        {
            base.Alter(entity);
            var cCountry = base.DbContext.Set<Country>().Where(x => x.ThreeLetterIsoCode == "DEU");

            entity.FirstName = "Max";
            entity.LastName = "Mustermann";
            entity.Email = "admin@meineshopurl.de";
            entity.Company = "Max Mustermann";
            entity.Address1 = "Musterweg 1";
            entity.City = "Musterstadt";
            entity.StateProvince = cCountry.FirstOrDefault().StateProvinces.FirstOrDefault();
            entity.Country = cCountry.FirstOrDefault();
            entity.ZipPostalCode = "12345";
        }


        protected override string TaxNameBooks => "Ermäßigt";
        protected override string TaxNameDigitalGoods => "Normal";
        protected override string TaxNameJewelry => "Normal";
        protected override string TaxNameApparel => "Normal";
        protected override string TaxNameFood => "Ermäßigt";
        protected override string TaxNameElectronics => "Normal";
        protected override string TaxNameTaxFree => "Befreit";
        public override decimal[] FixedTaxRates => new decimal[] { 19, 7, 0 };

        protected override void Alter(IList<TaxCategory> entities)
        {
            base.Alter(entities);

            // Clear all tax categories
            entities.Clear();

            // Add de-DE specific ones
            _taxCategories.Add("Normal", new TaxCategory { DisplayOrder = 0, Name = "Normal" });
            _taxCategories.Add("Ermäßigt", new TaxCategory { DisplayOrder = 1, Name = "Ermäßigt" });
            _taxCategories.Add(TaxNameTaxFree, new TaxCategory { DisplayOrder = 2, Name = TaxNameTaxFree });

            foreach (var taxCategory in _taxCategories.Values)
            {
                entities.Add(taxCategory);
            }
        }

        protected override void Alter(IList<Country> entities)
        {
            base.Alter(entities);

            #region Countries

            entities.Each(x => x.Published = false);
            entities.WithKey(x => x.NumericIsoCode)

                .Alter(276, x =>
                {
                    x.Name = "Deutschland";
                    x.DisplayOrder = -10;
                    x.Published = true;
                    #region Provinces
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Baden-Württemberg",
                        Abbreviation = "BW",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Bayern",
                        Abbreviation = "BY",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Berlin",
                        Abbreviation = "BE",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Brandenburg",
                        Abbreviation = "BB",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Bremen",
                        Abbreviation = "HB",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Hamburg",
                        Abbreviation = "HH",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Hessen",
                        Abbreviation = "HE",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Mecklenburg-Vorpommern",
                        Abbreviation = "MV",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Niedersachsen",
                        Abbreviation = "NI",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Nordrhein-Westfalen",
                        Abbreviation = "NW",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Rheinland-Pfalz",
                        Abbreviation = "RP",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Saarland",
                        Abbreviation = "SL",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Sachsen",
                        Abbreviation = "SN",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Sachsen-Anhalt",
                        Abbreviation = "ST",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Schleswig-Holstein",
                        Abbreviation = "SH",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Thüringen",
                        Abbreviation = "TH",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    #endregion Provinces
                })
                .Alter(40, x =>
                {
                    x.Name = "Österreich";
                    x.DisplayOrder = -5;
                    #region Provinces
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Burgenland",
                        Abbreviation = "Bgld.",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Kärnten",
                        Abbreviation = "Ktn.",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Niderösterreich",
                        Abbreviation = "NÖ",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Oberösterreich",
                        Abbreviation = "OÖ",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Salzburg",
                        Abbreviation = "Sbg.",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Steiermark",
                        Abbreviation = "Stmk.",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Tirol",
                        Abbreviation = "T",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Vorarlberg",
                        Abbreviation = "Vbg.",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Wien",
                        Abbreviation = "W",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    #endregion Provinces
                })
                .Alter(756, x =>
                {
                    x.Name = "Schweiz";
                    x.DisplayOrder = -1;
                    #region Provinces
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Zürich",
                        Abbreviation = "ZH",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Bern",
                        Abbreviation = "BE",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Luzern",
                        Abbreviation = "LU",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Uri",
                        Abbreviation = "UR",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Schwyz",
                        Abbreviation = "SZ",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Obwalden",
                        Abbreviation = "OW",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Nidwalden",
                        Abbreviation = "ST",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Glarus",
                        Abbreviation = "GL",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Zug",
                        Abbreviation = "ZG",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Freiburg",
                        Abbreviation = "FR",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Solothurn",
                        Abbreviation = "SO",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Basel-Stadt",
                        Abbreviation = "BS",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Basel-Landschaft",
                        Abbreviation = "BL",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Schaffhausen",
                        Abbreviation = "SH",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Appenzell Ausserrhoden",
                        Abbreviation = "AR",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Appenzell Innerrhoden",
                        Abbreviation = "AI",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "St. Gallen",
                        Abbreviation = "SG",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Graubünden",
                        Abbreviation = "GR",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Aargau",
                        Abbreviation = "AG",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Thurgau",
                        Abbreviation = "TG",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Tessin",
                        Abbreviation = "Ti",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Waadt",
                        Abbreviation = "VD",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Wallis",
                        Abbreviation = "VS",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Neuenburg",
                        Abbreviation = "NE",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Genf",
                        Abbreviation = "GE",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Jura",
                        Abbreviation = "JU",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    #endregion Provinces
                })
                .Alter(840, x =>
                {
                    x.Name = "Vereinigte Staaten von Amerika";
                    x.DisplayOrder = 100;
                })
                .Alter(124, x =>
                {
                    x.Name = "Kanada";
                    x.DisplayOrder = 100;
                })
                .Alter(32, x =>
                {
                    x.Name = "Argentinien";
                    x.DisplayOrder = 100;
                })
                .Alter(51, x =>
                {
                    x.Name = "Armenien";
                    x.DisplayOrder = 100;
                })
                .Alter(533, x =>
                {
                    x.Name = "Aruba";
                    x.DisplayOrder = 100;
                })
                .Alter(36, x =>
                {
                    x.Name = "Australien";
                    x.DisplayOrder = 100;
                })
                .Alter(31, x =>
                {
                    x.Name = "Aserbaidschan";
                    x.DisplayOrder = 100;
                })
                .Alter(44, x =>
                {
                    x.Name = "Bahamas";
                    x.DisplayOrder = 100;
                })
                .Alter(50, x =>
                {
                    x.Name = "Bangladesh";
                    x.DisplayOrder = 100;
                })
                .Alter(112, x =>
                {
                    x.Name = "Weissrussland";
                    x.DisplayOrder = 100;
                })
                .Alter(56, x =>
                {
                    x.Name = "Belgien";
                    x.DisplayOrder = 100;
                })
                .Alter(84, x =>
                {
                    x.Name = "Belize";
                    x.DisplayOrder = 100;
                })
                .Alter(60, x =>
                {
                    x.Name = "Bermudas";
                    x.DisplayOrder = 100;
                })
                .Alter(68, x =>
                {
                    x.Name = "Bolivien";
                    x.DisplayOrder = 100;
                })
                .Alter(70, x =>
                {
                    x.Name = "Bosnien-Herzegowina";
                    x.DisplayOrder = 100;
                })
                .Alter(76, x =>
                {
                    x.Name = "Brasilien";
                    x.DisplayOrder = 100;
                })
                .Alter(100, x =>
                {
                    x.Name = "Bulgarien";
                    x.DisplayOrder = 100;
                })
                .Alter(136, x =>
                {
                    x.Name = "Kaiman Inseln";
                    x.DisplayOrder = 100;
                })
                .Alter(152, x =>
                {
                    x.Name = "Chile";
                    x.DisplayOrder = 100;
                })
                .Alter(156, x =>
                {
                    x.Name = "China";
                    x.DisplayOrder = 100;
                })
                .Alter(170, x =>
                {
                    x.Name = "Kolumbien";
                    x.DisplayOrder = 100;
                })
                .Alter(188, x =>
                {
                    x.Name = "Costa Rica";
                    x.DisplayOrder = 100;
                })
                .Alter(191, x =>
                {
                    x.Name = "Kroatien";
                    x.DisplayOrder = 100;
                })
                .Alter(192, x =>
                {
                    x.Name = "Kuba";
                    x.DisplayOrder = 100;
                })
                .Alter(196, x =>
                {
                    x.Name = "Zypern";
                    x.DisplayOrder = 100;
                })
                .Alter(203, x =>
                {
                    x.Name = "Tschechische Republik";
                    x.DisplayOrder = 100;
                })
                .Alter(208, x =>
                {
                    x.Name = "Dänemark";
                    x.DisplayOrder = 100;
                })
                .Alter(214, x =>
                {
                    x.Name = "Dominikanische Republik";
                    x.DisplayOrder = 100;
                })
                .Alter(218, x =>
                {
                    x.Name = "Ecuador";
                    x.DisplayOrder = 100;
                })
                .Alter(818, x =>
                {
                    x.Name = "Ägypten";
                    x.DisplayOrder = 100;
                })
                .Alter(246, x =>
                {
                    x.Name = "Finnland";
                    x.DisplayOrder = 100;
                })
                .Alter(250, x =>
                {
                    x.Name = "Frankreich";
                    x.DisplayOrder = 100;
                })
                .Alter(268, x =>
                {
                    x.Name = "Georgien";
                    x.DisplayOrder = 100;
                })
                .Alter(292, x =>
                {
                    x.Name = "Gibraltar";
                    x.DisplayOrder = 100;
                })
                .Alter(300, x =>
                {
                    x.Name = "Griechenland";
                    x.DisplayOrder = 100;
                })
                .Alter(320, x =>
                {
                    x.Name = "Guatemala";
                    x.DisplayOrder = 100;
                })
                .Alter(344, x =>
                {
                    x.Name = "Hong Kong";
                    x.DisplayOrder = 100;
                })
                .Alter(348, x =>
                {
                    x.Name = "Ungarn";
                    x.DisplayOrder = 100;
                })
                .Alter(356, x =>
                {
                    x.Name = "Indien";
                    x.DisplayOrder = 100;
                })
                .Alter(360, x =>
                {
                    x.Name = "Indonesien";
                    x.DisplayOrder = 100;
                })
                .Alter(372, x =>
                {
                    x.Name = "Irland";
                    x.DisplayOrder = 100;
                })
                .Alter(376, x =>
                {
                    x.Name = "Israel";
                    x.DisplayOrder = 100;
                })
                .Alter(380, x =>
                {
                    x.Name = "Italien";
                    x.DisplayOrder = 100;
                })
                .Alter(388, x =>
                {
                    x.Name = "Jamaika";
                    x.DisplayOrder = 100;
                })
                .Alter(392, x =>
                {
                    x.Name = "Japan";
                    x.DisplayOrder = 100;
                })
                .Alter(400, x =>
                {
                    x.Name = "Jordanien";
                    x.DisplayOrder = 100;
                })
                .Alter(398, x =>
                {
                    x.Name = "Kasachstan";
                    x.DisplayOrder = 100;
                })
                .Alter(408, x =>
                {
                    x.Name = "Nord Korea";
                    x.DisplayOrder = 100;
                })
                .Alter(414, x =>
                {
                    x.Name = "Kuwait";
                    x.DisplayOrder = 100;
                })
                .Alter(458, x =>
                {
                    x.Name = "Malaysia";
                    x.DisplayOrder = 100;
                })
                .Alter(484, x =>
                {
                    x.Name = "Mexiko";
                    x.DisplayOrder = 100;
                })
                .Alter(528, x =>
                {
                    x.Name = "Niederlande";
                    x.DisplayOrder = 100;
                })
                .Alter(554, x =>
                {
                    x.Name = "Neuseeland";
                    x.DisplayOrder = 100;
                })
                .Alter(578, x =>
                {
                    x.Name = "Norwegen";
                    x.DisplayOrder = 100;
                })
                .Alter(586, x =>
                {
                    x.Name = "Pakistan";
                    x.DisplayOrder = 100;
                })
                .Alter(600, x =>
                {
                    x.Name = "Paraguay";
                    x.DisplayOrder = 100;
                })
                .Alter(604, x =>
                {
                    x.Name = "Peru";
                    x.DisplayOrder = 100;
                })
                .Alter(608, x =>
                {
                    x.Name = "Philippinen";
                    x.DisplayOrder = 100;
                })
                .Alter(616, x =>
                {
                    x.Name = "Polen";
                    x.DisplayOrder = 100;
                })
                .Alter(620, x =>
                {
                    x.Name = "Portugal";
                    x.DisplayOrder = 100;
                })
                .Alter(630, x =>
                {
                    x.Name = "Puerto Rico";
                    x.DisplayOrder = 100;
                })
                .Alter(634, x =>
                {
                    x.Name = "Qatar";
                    x.DisplayOrder = 100;
                })
                .Alter(642, x =>
                {
                    x.Name = "Rumänien";
                    x.DisplayOrder = 100;
                })
                .Alter(643, x =>
                {
                    x.Name = "Rußland";
                    x.DisplayOrder = 100;
                })
                .Alter(682, x =>
                {
                    x.Name = "Saudi Arabien";
                    x.DisplayOrder = 100;
                })
                .Alter(702, x =>
                {
                    x.Name = "Singapur";
                    x.DisplayOrder = 100;
                })
                .Alter(703, x =>
                {
                    x.Name = "Slowakei";
                    x.DisplayOrder = 100;
                })
                .Alter(705, x =>
                {
                    x.Name = "Slowenien";
                    x.DisplayOrder = 100;
                })
                .Alter(710, x =>
                {
                    x.Name = "Südafrika";
                    x.DisplayOrder = 100;
                })
                .Alter(724, x =>
                {
                    x.Name = "Spanien";
                    x.DisplayOrder = 100;
                })
                .Alter(752, x =>
                {
                    x.Name = "Schweden";
                    x.DisplayOrder = 100;
                })
                .Alter(158, x =>
                {
                    x.Name = "Taiwan";
                    x.DisplayOrder = 100;
                })
                .Alter(764, x =>
                {
                    x.Name = "Thailand";
                    x.DisplayOrder = 100;
                })
                .Alter(792, x =>
                {
                    x.Name = "Türkei";
                    x.DisplayOrder = 100;
                })
                .Alter(804, x =>
                {
                    x.Name = "Ukraine";
                    x.DisplayOrder = 100;
                })
                .Alter(784, x =>
                {
                    x.Name = "Vereinigte Arabische Emirate";
                    x.DisplayOrder = 100;
                })
                .Alter(826, x =>
                {
                    x.Name = "Großbritannien";
                    x.DisplayOrder = 100;
                })
                .Alter(581, x =>
                {
                    x.Name = "United States Minor Outlying Islands";
                    x.DisplayOrder = 100;
                })
                .Alter(858, x =>
                {
                    x.Name = "Uruguay";
                    x.DisplayOrder = 100;
                })
                .Alter(860, x =>
                {
                    x.Name = "Usbekistan";
                    x.DisplayOrder = 100;
                })
                .Alter(862, x =>
                {
                    x.Name = "Venezuela";
                    x.DisplayOrder = 100;
                })
                .Alter(688, x =>
                {
                    x.Name = "Serbien";
                    x.DisplayOrder = 100;
                })
                .Alter(4, x =>
                {
                    x.Name = "Afghanistan";
                    x.DisplayOrder = 100;
                })
                .Alter(8, x =>
                {
                    x.Name = "Albanien";
                    x.DisplayOrder = 100;
                })
                .Alter(12, x =>
                {
                    x.Name = "Algerien";
                    x.DisplayOrder = 100;
                })
                .Alter(16, x =>
                {
                    x.Name = "Samoa";
                    x.DisplayOrder = 100;
                })
                .Alter(20, x =>
                {
                    x.Name = "Andorra";
                    x.DisplayOrder = 100;
                })
                .Alter(24, x =>
                {
                    x.Name = "Angola";
                    x.DisplayOrder = 100;
                })
                .Alter(660, x =>
                {
                    x.Name = "Anguilla";
                    x.DisplayOrder = 100;
                })
                .Alter(10, x =>
                {
                    x.Name = "Antarktis";
                    x.DisplayOrder = 100;
                })
                .Alter(28, x =>
                {
                    x.Name = "Antigua und Barbuda";
                    x.DisplayOrder = 100;
                })
                .Alter(48, x =>
                {
                    x.Name = "Bahrain";
                    x.DisplayOrder = 100;
                })
                .Alter(52, x =>
                {
                    x.Name = "Barbados";
                    x.DisplayOrder = 100;
                })
                .Alter(204, x =>
                {
                    x.Name = "Benin";
                    x.DisplayOrder = 100;
                })
                .Alter(64, x =>
                {
                    x.Name = "Bhutan";
                    x.DisplayOrder = 100;
                })
                .Alter(72, x =>
                {
                    x.Name = "Botswana";
                    x.DisplayOrder = 100;
                })
                .Alter(74, x =>
                {
                    x.Name = "Bouvet Inseln";
                    x.DisplayOrder = 100;
                })
                .Alter(86, x =>
                {
                    x.Name = "Britisch-Indischer Ozean";
                    x.DisplayOrder = 100;
                })
                .Alter(96, x =>
                {
                    x.Name = "Brunei";
                    x.DisplayOrder = 100;
                })
                .Alter(854, x =>
                {
                    x.Name = "Burkina Faso";
                    x.DisplayOrder = 100;
                })
                .Alter(108, x =>
                {
                    x.Name = "Burundi";
                    x.DisplayOrder = 100;
                })
                .Alter(116, x =>
                {
                    x.Name = "Kambodscha";
                    x.DisplayOrder = 100;
                })
                .Alter(120, x =>
                {
                    x.Name = "Kamerun";
                    x.DisplayOrder = 100;
                })
                .Alter(132, x =>
                {
                    x.Name = "Kap Verde";
                    x.DisplayOrder = 100;
                })
                .Alter(140, x =>
                {
                    x.Name = "Zentralafrikanische Republik";
                    x.DisplayOrder = 100;
                })
                .Alter(148, x =>
                {
                    x.Name = "Tschad";
                    x.DisplayOrder = 100;
                })
                .Alter(162, x =>
                {
                    x.Name = "Christmas Island";
                    x.DisplayOrder = 100;
                })
                .Alter(166, x =>
                {
                    x.Name = "Kokosinseln";
                    x.DisplayOrder = 100;
                })
                .Alter(174, x =>
                {
                    x.Name = "Komoren";
                    x.DisplayOrder = 100;
                })
                .Alter(178, x =>
                {
                    x.Name = "Kongo";
                    x.DisplayOrder = 100;
                })
                .Alter(184, x =>
                {
                    x.Name = "Cook Inseln";
                    x.DisplayOrder = 100;
                })
                .Alter(384, x =>
                {
                    x.Name = "Elfenbeinküste";
                    x.DisplayOrder = 100;
                })
                .Alter(262, x =>
                {
                    x.Name = "Djibuti";
                    x.DisplayOrder = 100;
                })
                .Alter(212, x =>
                {
                    x.Name = "Dominika";
                    x.DisplayOrder = 100;
                })
                .Alter(222, x =>
                {
                    x.Name = "El Salvador";
                    x.DisplayOrder = 100;
                })
                .Alter(226, x =>
                {
                    x.Name = "Äquatorial Guinea";
                    x.DisplayOrder = 100;
                })
                .Alter(232, x =>
                {
                    x.Name = "Eritrea";
                    x.DisplayOrder = 100;
                })
                .Alter(233, x =>
                {
                    x.Name = "Estland";
                    x.DisplayOrder = 100;
                })
                .Alter(231, x =>
                {
                    x.Name = "Äthiopien";
                    x.DisplayOrder = 100;
                })
                .Alter(238, x =>
                {
                    x.Name = "Falkland Inseln";
                    x.DisplayOrder = 100;
                })
                .Alter(234, x =>
                {
                    x.Name = "Färöer Inseln";
                    x.DisplayOrder = 100;
                })
                .Alter(242, x =>
                {
                    x.Name = "Fidschi";
                    x.DisplayOrder = 100;
                })
                .Alter(254, x =>
                {
                    x.Name = "Französisch Guyana";
                    x.DisplayOrder = 100;
                })
                .Alter(258, x =>
                {
                    x.Name = "Französisch Polynesien";
                    x.DisplayOrder = 100;
                })
                .Alter(260, x =>
                {
                    x.Name = "Französisches Süd-Territorium";
                    x.DisplayOrder = 100;
                })
                .Alter(266, x =>
                {
                    x.Name = "Gabun";
                    x.DisplayOrder = 100;
                })
                .Alter(270, x =>
                {
                    x.Name = "Gambia";
                    x.DisplayOrder = 100;
                })
                .Alter(288, x =>
                {
                    x.Name = "Ghana";
                    x.DisplayOrder = 100;
                })
                .Alter(304, x =>
                {
                    x.Name = "Grönland";
                    x.DisplayOrder = 100;
                })
                .Alter(308, x =>
                {
                    x.Name = "Grenada";
                    x.DisplayOrder = 100;
                })
                .Alter(312, x =>
                {
                    x.Name = "Guadeloupe";
                    x.DisplayOrder = 100;
                })
                .Alter(316, x =>
                {
                    x.Name = "Guam";
                    x.DisplayOrder = 100;
                })
                .Alter(324, x =>
                {
                    x.Name = "Guinea";
                    x.DisplayOrder = 100;
                })
                .Alter(624, x =>
                {
                    x.Name = "Guinea Bissau";
                    x.DisplayOrder = 100;
                })
                .Alter(328, x =>
                {
                    x.Name = "Guyana";
                    x.DisplayOrder = 100;
                })
                .Alter(332, x =>
                {
                    x.Name = "Haiti";
                    x.DisplayOrder = 100;
                })
                .Alter(334, x =>
                {
                    x.Name = "Heard und McDonald Islands";
                    x.DisplayOrder = 100;
                })
                .Alter(340, x =>
                {
                    x.Name = "Honduras";
                    x.DisplayOrder = 100;
                })
                .Alter(352, x =>
                {
                    x.Name = "Island";
                    x.DisplayOrder = 100;
                })
                .Alter(364, x =>
                {
                    x.Name = "Iran";
                    x.DisplayOrder = 100;
                })
                .Alter(368, x =>
                {
                    x.Name = "Irak";
                    x.DisplayOrder = 100;
                })
                .Alter(404, x =>
                {
                    x.Name = "Kenia";
                    x.DisplayOrder = 100;
                })
                .Alter(296, x =>
                {
                    x.Name = "Kiribati";
                    x.DisplayOrder = 100;
                })
                .Alter(410, x =>
                {
                    x.Name = "Süd Korea";
                    x.DisplayOrder = 100;
                })
                .Alter(417, x =>
                {
                    x.Name = "Kirgisistan";
                    x.DisplayOrder = 100;
                })
                .Alter(418, x =>
                {
                    x.Name = "Laos";
                    x.DisplayOrder = 100;
                })
                .Alter(428, x =>
                {
                    x.Name = "Lettland";
                    x.DisplayOrder = 100;
                })
                .Alter(422, x =>
                {
                    x.Name = "Libanon";
                    x.DisplayOrder = 100;
                })
                .Alter(426, x =>
                {
                    x.Name = "Lesotho";
                    x.DisplayOrder = 100;
                })
                .Alter(430, x =>
                {
                    x.Name = "Liberia";
                    x.DisplayOrder = 100;
                })
                .Alter(434, x =>
                {
                    x.Name = "Libyen";
                    x.DisplayOrder = 100;
                })
                .Alter(438, x =>
                {
                    x.Name = "Liechtenstein";
                    x.DisplayOrder = 100;
                })
                .Alter(440, x =>
                {
                    x.Name = "Litauen";
                    x.DisplayOrder = 100;
                })
                .Alter(442, x =>
                {
                    x.Name = "Luxemburg";
                    x.DisplayOrder = 100;
                })
                .Alter(446, x =>
                {
                    x.Name = "Macao";
                    x.DisplayOrder = 100;
                })
                .Alter(807, x =>
                {
                    x.Name = "Mazedonien";
                    x.DisplayOrder = 100;
                })
                .Alter(450, x =>
                {
                    x.Name = "Madagaskar";
                    x.DisplayOrder = 100;
                })
                .Alter(454, x =>
                {
                    x.Name = "Malawi";
                    x.DisplayOrder = 100;
                })
                .Alter(462, x =>
                {
                    x.Name = "Malediven";
                    x.DisplayOrder = 100;
                })
                .Alter(466, x =>
                {
                    x.Name = "Mali";
                    x.DisplayOrder = 100;
                })
                .Alter(470, x =>
                {
                    x.Name = "Malta";
                    x.DisplayOrder = 100;
                })
                .Alter(584, x =>
                {
                    x.Name = "Marshall Inseln";
                    x.DisplayOrder = 100;
                })
                .Alter(474, x =>
                {
                    x.Name = "Martinique";
                    x.DisplayOrder = 100;
                })
                .Alter(478, x =>
                {
                    x.Name = "Mauretanien";
                    x.DisplayOrder = 100;
                })
                .Alter(480, x =>
                {
                    x.Name = "Mauritius";
                    x.DisplayOrder = 100;
                })
                .Alter(175, x =>
                {
                    x.Name = "Mayotte";
                    x.DisplayOrder = 100;
                })
                .Alter(583, x =>
                {
                    x.Name = "Mikronesien";
                    x.DisplayOrder = 100;
                })
                .Alter(498, x =>
                {
                    x.Name = "Moldavien";
                    x.DisplayOrder = 100;
                })
                .Alter(492, x =>
                {
                    x.Name = "Monaco";
                    x.DisplayOrder = 100;
                })
                .Alter(496, x =>
                {
                    x.Name = "Mongolei";
                    x.DisplayOrder = 100;
                })
                .Alter(500, x =>
                {
                    x.Name = "Montserrat";
                    x.DisplayOrder = 100;
                })
                .Alter(504, x =>
                {
                    x.Name = "Marokko";
                    x.DisplayOrder = 100;
                })
                .Alter(508, x =>
                {
                    x.Name = "Mocambique";
                    x.DisplayOrder = 100;
                })
                .Alter(104, x =>
                {
                    x.Name = "Birma";
                    x.DisplayOrder = 100;
                })
                .Alter(516, x =>
                {
                    x.Name = "Namibia";
                    x.DisplayOrder = 100;
                })
                .Alter(520, x =>
                {
                    x.Name = "Nauru";
                    x.DisplayOrder = 100;
                })
                .Alter(524, x =>
                {
                    x.Name = "Nepal";
                    x.DisplayOrder = 100;
                })
                .Alter(530, x =>
                {
                    x.Name = "Niederländische Antillen";
                    x.DisplayOrder = 100;
                })
                .Alter(540, x =>
                {
                    x.Name = "Neukaledonien";
                    x.DisplayOrder = 100;
                })
                .Alter(558, x =>
                {
                    x.Name = "Nicaragua";
                    x.DisplayOrder = 100;
                })
                .Alter(562, x =>
                {
                    x.Name = "Niger";
                    x.DisplayOrder = 100;
                })
                .Alter(566, x =>
                {
                    x.Name = "Nigeria";
                    x.DisplayOrder = 100;
                })
                .Alter(570, x =>
                {
                    x.Name = "Niue";
                    x.DisplayOrder = 100;
                })
                .Alter(574, x =>
                {
                    x.Name = "Norfolk Inseln";
                    x.DisplayOrder = 100;
                })
                .Alter(580, x =>
                {
                    x.Name = "Marianen";
                    x.DisplayOrder = 100;
                })
                .Alter(512, x =>
                {
                    x.Name = "Oman";
                    x.DisplayOrder = 100;
                })
                .Alter(585, x =>
                {
                    x.Name = "Palau";
                    x.DisplayOrder = 100;
                })
                .Alter(591, x =>
                {
                    x.Name = "Panama";
                    x.DisplayOrder = 100;
                })
                .Alter(598, x =>
                {
                    x.Name = "Papua Neuguinea";
                    x.DisplayOrder = 100;
                })
                .Alter(612, x =>
                {
                    x.Name = "Pitcairn";
                    x.DisplayOrder = 100;
                })
                .Alter(638, x =>
                {
                    x.Name = "Reunion";
                    x.DisplayOrder = 100;
                })
                .Alter(646, x =>
                {
                    x.Name = "Ruanda";
                    x.DisplayOrder = 100;
                })
                .Alter(659, x =>
                {
                    x.Name = "St. Kitts Nevis Anguilla";
                    x.DisplayOrder = 100;
                })
                .Alter(662, x =>
                {
                    x.Name = "Saint Lucia";
                    x.DisplayOrder = 100;
                })
                .Alter(670, x =>
                {
                    x.Name = "St. Vincent";
                    x.DisplayOrder = 100;
                })
                .Alter(882, x =>
                {
                    x.Name = "Samoa";
                    x.DisplayOrder = 100;
                })
                .Alter(674, x =>
                {
                    x.Name = "San Marino";
                    x.DisplayOrder = 100;
                })
                .Alter(678, x =>
                {
                    x.Name = "Sao Tome";
                    x.DisplayOrder = 100;
                })
                .Alter(686, x =>
                {
                    x.Name = "Senegal";
                    x.DisplayOrder = 100;
                })
                .Alter(690, x =>
                {
                    x.Name = "Seychellen";
                    x.DisplayOrder = 100;
                })
                .Alter(694, x =>
                {
                    x.Name = "Sierra Leone";
                    x.DisplayOrder = 100;
                })
                .Alter(90, x =>
                {
                    x.Name = "Solomon Inseln";
                    x.DisplayOrder = 100;
                })
                .Alter(706, x =>
                {
                    x.Name = "Somalia";
                    x.DisplayOrder = 100;
                })
                .Alter(239, x =>
                {
                    x.Name = "South Georgia, South Sandwich Isl.";
                    x.DisplayOrder = 100;
                })
                .Alter(144, x =>
                {
                    x.Name = "Sri Lanka";
                    x.DisplayOrder = 100;
                })
                .Alter(654, x =>
                {
                    x.Name = "St. Helena";
                    x.DisplayOrder = 100;
                })
                .Alter(666, x =>
                {
                    x.Name = "St. Pierre und Miquelon";
                    x.DisplayOrder = 100;
                })
                .Alter(736, x =>
                {
                    x.Name = "Sudan";
                    x.DisplayOrder = 100;
                })
                .Alter(740, x =>
                {
                    x.Name = "Surinam";
                    x.DisplayOrder = 100;
                })
                .Alter(744, x =>
                {
                    x.Name = "Svalbard und Jan Mayen Islands";
                    x.DisplayOrder = 100;
                })
                .Alter(748, x =>
                {
                    x.Name = "Swasiland";
                    x.DisplayOrder = 100;
                })
                .Alter(760, x =>
                {
                    x.Name = "Syrien";
                    x.DisplayOrder = 100;
                })
                .Alter(762, x =>
                {
                    x.Name = "Tadschikistan";
                    x.DisplayOrder = 100;
                })
                .Alter(834, x =>
                {
                    x.Name = "Tansania";
                    x.DisplayOrder = 100;
                })
                .Alter(768, x =>
                {
                    x.Name = "Togo";
                    x.DisplayOrder = 100;
                })
                .Alter(772, x =>
                {
                    x.Name = "Tokelau";
                    x.DisplayOrder = 100;
                })
                .Alter(776, x =>
                {
                    x.Name = "Tonga";
                    x.DisplayOrder = 100;
                })
                .Alter(780, x =>
                {
                    x.Name = "Trinidad Tobago";
                    x.DisplayOrder = 100;
                })
                .Alter(788, x =>
                {
                    x.Name = "Tunesien";
                    x.DisplayOrder = 100;
                })
                .Alter(795, x =>
                {
                    x.Name = "Turkmenistan";
                    x.DisplayOrder = 100;
                })
                .Alter(796, x =>
                {
                    x.Name = "Turks und Kaikos Inseln";
                    x.DisplayOrder = 100;
                })
                .Alter(798, x =>
                {
                    x.Name = "Tuvalu";
                    x.DisplayOrder = 100;
                })
                .Alter(800, x =>
                {
                    x.Name = "Uganda";
                    x.DisplayOrder = 100;
                })
                .Alter(548, x =>
                {
                    x.Name = "Vanuatu";
                    x.DisplayOrder = 100;
                })
                .Alter(336, x =>
                {
                    x.Name = "Vatikan";
                    x.DisplayOrder = 100;
                })
                .Alter(704, x =>
                {
                    x.Name = "Vietnam";
                    x.DisplayOrder = 100;
                })
                .Alter(92, x =>
                {
                    x.Name = "Virgin Island (Brit.)";
                    x.DisplayOrder = 100;
                })
                .Alter(850, x =>
                {
                    x.Name = "Virgin Island (USA)";
                    x.DisplayOrder = 100;
                })
                .Alter(876, x =>
                {
                    x.Name = "Wallis et Futuna";
                    x.DisplayOrder = 100;
                })
                .Alter(732, x =>
                {
                    x.Name = "Westsahara";
                    x.DisplayOrder = 100;
                })
                .Alter(887, x =>
                {
                    x.Name = "Jemen";
                    x.DisplayOrder = 100;
                })
                .Alter(894, x =>
                {
                    x.Name = "Sambia";
                    x.DisplayOrder = 100;
                })
                .Alter(716, x =>
                {
                    x.Name = "Zimbabwe";
                    x.DisplayOrder = 100;
                });

            #endregion Countries
        }

        protected override void Alter(IList<Topic> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.SystemName)
                .Alter("AboutUs", x =>
                {
                    x.Title = "Über uns";
                    x.Body = "<p>Fügen Sie hier Informationen über den Shop ein. Diesen Text können Sie auch im Administrations-Bereich editieren.</p>";
                })
                .Alter("CheckoutAsGuestOrRegister", x =>
                {
                    x.Title = "";
                    x.Body = "<p><strong>Anmelden und Zeit sparen!</strong><br />Melden Sie sich an und geniessen Sie folgende Vorteile:</p><ul><li>Schnell und einfach einkaufen</li><li>Jederzeit Zugriff auf Bestellstatus und Bestellhistorie</li></ul>";
                })
                .Alter("ConditionsOfUse", x =>
                {
                    x.Title = "AGB";
                    x.Body = "<p>Fügen Sie Ihre AGB hier ein. Diesen Text können Sie auch im Administrations-Bereich editieren.</p>";
                })
                .Alter("ContactUs", x =>
                {
                    x.Title = "Kontakt";
                    x.Body = "<p>Fügen Sie Ihre Kontaktdaten hier ein. Diesen Text können Sie auch im Administrations-Bereich editieren.</p>";
                })
                .Alter("ForumWelcomeMessage", x =>
                {
                    x.Title = "Foren";
                    x.Body = "<p>Fügen Sie eine Willkommens-Nachricht für das Forum hier ein. Diesen Text können Sie auch im Administrations-Bereich editieren.</p>";
                })
                .Alter("HomePageText", x =>
                {
                    x.Title = "Herzlich Willkommen";
                    x.Body = "<p>Fügen Sie eine Willkommens-Nachricht für den Online-Shop hier ein. Diesen Text können Sie auch im Administrations-Bereich editieren.</p>";
                })
                .Alter("LoginRegistrationInfo", x =>
                {
                    x.Title = "Anmeldung/Registrierung";
                    x.Body = "<p><strong>Noch nicht registriert?</strong></p><p>Erstellen Sie jetzt Ihr Kunden-Konto und erleben Sie unsere Vielfalt. Mit einem Konto können Sie künftig schneller bestellen und haben stets eine optimale Übersicht über Ihre laufenden sowie bisherigen Bestellungen.</p>";
                })
                .Alter("PrivacyInfo", x =>
                {
                    x.ShortTitle = "Datenschutz";
                    x.Title = "Datenschutzerklärung";
                    x.Body = "<p>Legen Sie Ihre Datenschutzerkl&#228;rung hier fest. Diesen Text können Sie auch im Administrations-Bereich editieren.</p>";
                })
                .Alter("ShippingInfo", x =>
                {
                    x.ShortTitle = "Versandinfos";
                    x.Title = "Versand und Rücksendungen";
                    x.Body = "<p>Informationen zu Versand und Rücksendungen. Diesen Text können Sie auch im Administrations-Bereich editieren.</p>";
                })
                .Alter("Imprint", x =>
                {
                    x.Title = "Impressum";
                    x.Body = @"<p>
                            <div>http://www.[mein-shop].de ist ein kommerzielles Angebot der</div>
                            <div>
                                MusterFirma<br>
                                Musterstr. 123<br>
                                44135 Dortmund<br>
                            </div>
                            <div>
                                Geschäftsführer: Max Mustermann<br>
                                Verantwortlich für den Inhalt der Website: Max Mustermann
                            </div>
                            <div>
                                Telefon: 0231/123 456<br>
                                Fax: 0231/123789<br>
                                E-Mail: info@[mein-shop].de<br>
                            </div>
                            <div>
                                SteuerNr.: 1234567890<br>
                                USt.-IdNr.: DE1234567890<br>
                            </div>
                            <div>
                                <Name und Anschrift des Auslandsvertreters>
                            </div>
                            <div>
                                <Zuständige Aufsichtsbehörde>
                            </div>
                            <div>
                                <Kammer>
                            </div>
                            <div>
                                <Gesetzliche Berufsbezeichnung>
                            </div>
                            <div>
                                <Verweis auf die berufsrechtlichen Regelungen>
                            </div>
                            </p>";
                })

                .Alter("Disclaimer", x =>
                {
                    x.Title = "Widerrufsrecht";
                    x.Body = "<p>Fügen Sie Ihr Widerrufsrecht hier ein. Diesen Text können Sie auch im Administrations-Bereich editieren.</p>";
                })
                .Alter("PaymentInfo", x =>
                {
                    x.Title = "Zahlungsarten";
                    x.Body = "<p>Fügen Sie Informationen zu Zahlungsarten hier ein. Diesen Text können Sie auch im Administrations-Bereich editieren.</p>";
                });
        }

        protected override void Alter(UrlRecord entity)
        {
            base.Alter(entity);

            if (entity.EntityName == "Topic")
            {
                switch (entity.Slug)
                {
                    case "aboutus":
                        entity.Slug = "ueber-uns";
                        break;
                    case "conditionsofuse":
                        entity.Slug = "agb";
                        break;
                    case "contactus":
                        entity.Slug = "kontakt";
                        break;
                    case "privacyinfo":
                        entity.Slug = "datenschutz";
                        break;
                    case "shippinginfo":
                        entity.Slug = "versand-und-rueckgabe";
                        break;
                    case "imprint":
                        entity.Slug = "impressum";
                        break;
                    case "disclaimer":
                        entity.Slug = "widerrufsrecht";
                        break;
                    case "paymentinfo":
                        entity.Slug = "zahlungsarten";
                        break;
                }
            }
        }

        protected override void Alter(IList<ISettings> settings)
        {
            base.Alter(settings);

            var defaultDimensionId = DbContext.Set<MeasureDimension>().FirstOrDefault(x => x.SystemKeyword == "m")?.Id;
            var defaultWeightId = DbContext.Set<MeasureWeight>().FirstOrDefault(x => x.SystemKeyword == "kg")?.Id;
            var defaultCountryId = DbContext.Set<Country>().FirstOrDefault(x => x.TwoLetterIsoCode == "DE")?.Id;

            settings
                .Alter<MeasureSettings>(x =>
                {
                    x.BaseDimensionId = defaultDimensionId ?? x.BaseDimensionId;
                    x.BaseWeightId = defaultWeightId ?? x.BaseWeightId;
                })
                .Alter<SeoSettings>(x =>
                {
                    x.MetaTitle = "Mein Shop";
                })
                .Alter<OrderSettings>(x =>
                {
                    x.ReturnRequestActions = "Reparatur,Ersatz,Gutschein";
                    x.ReturnRequestReasons = "Falschen Artikel erhalten,Falsch bestellt,Ware fehlerhaft bzw. defekt";
                    x.NumberOfDaysReturnRequestAvailable = 14;
                })
                .Alter<ShippingSettings>(x =>
                {
                    x.EstimateShippingEnabled = false;
                })
                .Alter<TaxSettings>(x =>
                {
                    x.TaxBasedOn = TaxBasedOn.ShippingAddress;
                    x.TaxDisplayType = TaxDisplayType.IncludingTax;
                    x.DisplayTaxSuffix = true;
                    x.ShippingIsTaxable = true;
                    x.ShippingPriceIncludesTax = true;
                    x.EuVatEnabled = true;
                    x.EuVatShopCountryId = defaultCountryId ?? x.EuVatShopCountryId;
                    x.EuVatAllowVatExemption = true;
                    x.EuVatUseWebService = false;
                    x.EuVatEmailAdminWhenNewVatSubmitted = true;
                });
        }

        protected override void Alter(IList<ActivityLogType> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.SystemKeyword)
                .Alter("AddNewCategory", x =>
                    {
                        x.Name = "Erstellen einer Warengruppe";
                    })
                .Alter("AddNewCheckoutAttribute", x =>
                    {
                        x.Name = "Neues Checkout Attribut hinzugefügt";
                    })
                .Alter("AddNewCustomer", x =>
                    {
                        x.Name = "Neuen Kunden hinzugefügt";
                    })
                .Alter("AddNewCustomerRole", x =>
                    {
                        x.Name = "Neue Kundengruppe hinzugefügt";
                    })
                .Alter("AddNewDiscount", x =>
                    {
                        x.Name = "Neuer Rabatt hinzugefügt";
                    })
                .Alter("AddNewGiftCard", x =>
                    {
                        x.Name = "Neuer Geschenkgutschein hinzugefügt";
                    })
                .Alter("AddNewManufacturer", x =>
                    {
                        x.Name = "Neuer Hersteller hinzugefügt";
                    })
                .Alter("AddNewProduct", x =>
                    {
                        x.Name = "Neues Produkt hinzugefügt";
                    })
                .Alter("AddNewProductAttribute", x =>
                    {
                        x.Name = "Neues Produktattribut hinzugefügt";
                    })
                .Alter("AddNewSetting", x =>
                    {
                        x.Name = "Neue Einstellung hinzugefügt";
                    })
                .Alter("AddNewSpecAttribute", x =>
                    {
                        x.Name = "Neues Spezifikationsattribut hinzugefügt";
                    })
                .Alter("AddNewWidget", x =>
                    {
                        x.Name = "Neues Widget hinzugefügt";
                    })
                .Alter("DeleteCategory", x =>
                    {
                        x.Name = "Warengruppe gelöscht";
                    })
                .Alter("DeleteCheckoutAttribute", x =>
                    {
                        x.Name = "Checkout-Attribut gelöscht";
                    })
                .Alter("DeleteCustomer", x =>
                    {
                        x.Name = "Kunde gelöscht";
                    })
                .Alter("DeleteCustomerRole", x =>
                    {
                        x.Name = "Kundengruppe gelöscht";
                    })
                .Alter("DeleteDiscount", x =>
                    {
                        x.Name = "Rabatt gelöscht";
                    })
                .Alter("DeleteGiftCard", x =>
                    {
                        x.Name = "Geschenkgutschein gelöscht";
                    })
                .Alter("DeleteManufacturer", x =>
                    {
                        x.Name = "Hersteller gelöscht";
                    })
                .Alter("DeleteProduct", x =>
                    {
                        x.Name = "Produkt gelöscht";
                    })
                .Alter("DeleteProductAttribute", x =>
                    {
                        x.Name = "Produktattribut gelöscht";
                    })
                .Alter("DeleteReturnRequest", x =>
                    {
                        x.Name = "Rücksendeanforderung gelöscht";
                    })
                .Alter("DeleteSetting", x =>
                    {
                        x.Name = "Einstellung gelöscht";
                    })
                .Alter("DeleteSpecAttribute", x =>
                    {
                        x.Name = "Spezifikationsattribut gelöscht";
                    })
                .Alter("DeleteWidget", x =>
                    {
                        x.Name = "Widget gelöscht";
                    })
                .Alter("EditCategory", x =>
                    {
                        x.Name = "Warengruppe bearbeitet";
                    })
                .Alter("EditCheckoutAttribute", x =>
                    {
                        x.Name = "Checkout-Attribut bearbeitet";
                    })
                .Alter("EditCustomer", x =>
                    {
                        x.Name = "Kunde bearbeitet";
                    })
                .Alter("EditCustomerRole", x =>
                    {
                        x.Name = "Kundengruppe bearbeitet";
                    })
                .Alter("EditDiscount", x =>
                    {
                        x.Name = "Rabatt bearbeitet";
                    })
                .Alter("EditGiftCard", x =>
                    {
                        x.Name = "Geschenkgutschein bearbeitet";
                    })
                .Alter("EditManufacturer", x =>
                    {
                        x.Name = "Hersteller bearbeitet";
                    })
                .Alter("EditProduct", x =>
                    {
                        x.Name = "Produkt bearbeitet";
                    })
                .Alter("EditProductAttribute", x =>
                    {
                        x.Name = "Produktattribut bearbeitet";
                    })
                .Alter("EditPromotionProviders", x =>
                    {
                        x.Name = "Promotion-Provider bearbeiten";
                    })
                .Alter("EditReturnRequest", x =>
                    {
                        x.Name = "Rücksendewunsch bearbeitet";
                    })
                .Alter("EditSettings", x =>
                    {
                        x.Name = "Einstellungen bearbeitet";
                    })
                .Alter("EditSpecAttribute", x =>
                    {
                        x.Name = "Spezifikationsattribut bearbeitet";
                    })
                .Alter("EditWidget", x =>
                    {
                        x.Name = "Widget bearbeitet";
                    })
                .Alter("EditOrder", x =>
                {
                    x.Name = "Auftrag bearbeitet";
                })
                .Alter("PublicStore.ViewCategory", x =>
                    {
                        x.Name = "Öffentlicher Shop. Hat eine Warengruppen-Detailseite angesehen";
                    })
                .Alter("PublicStore.ViewManufacturer", x =>
                    {
                        x.Name = "Öffentlicher Shop. Hat eine Hersteller-Detailseite angesehen";
                    })
                .Alter("PublicStore.ViewProduct", x =>
                    {
                        x.Name = "Öffentlicher Shop. Hat eine Produkt-Detailseite angesehen";
                    })
                .Alter("PublicStore.PlaceOrder", x =>
                    {
                        x.Name = "Öffentlicher Shop. Hat einen neuen Auftrag erteilt";
                    })
                .Alter("PublicStore.SendPM", x =>
                    {
                        x.Name = "Öffentlicher Shop. PN an Kunden geschickt";
                    })
                .Alter("PublicStore.ContactUs", x =>
                    {
                        x.Name = "Öffentlicher Shop. Kontaktformular benutzt";
                    })
                .Alter("PublicStore.AddToCompareList", x =>
                    {
                        x.Name = "Öffentlicher Shop. Produkt zur Vergleichsliste hinzugefügt";
                    })
                .Alter("PublicStore.AddToShoppingCart", x =>
                    {
                        x.Name = "Öffentlicher Shop. Produkt in den Warenkorb gelegt";
                    })
                .Alter("PublicStore.AddToWishlist", x =>
                    {
                        x.Name = "Öffentlicher Shop. Produkt zur Wunschliste hinzugefügt";
                    })
                .Alter("PublicStore.Login", x =>
                    {
                        x.Name = "Öffentlicher Shop. Anmeldung";
                    })
                .Alter("PublicStore.Logout", x =>
                    {
                        x.Name = "Öffentlicher Shop. Abmeldung";
                    })
                .Alter("PublicStore.AddProductReview", x =>
                    {
                        x.Name = "Öffentlicher Shop. Produktrezension hinzugefügt";
                    })
                .Alter("PublicStore.AddNewsComment", x =>
                    {
                        x.Name = "Öffentlicher Shop. News-Kommentar hinzugefügt";
                    })
                .Alter("PublicStore.AddBlogComment", x =>
                    {
                        x.Name = "Öffentlicher Shop. Blogeintrag hinzugefügt";
                    })
                .Alter("PublicStore.AddForumTopic", x =>
                    {
                        x.Name = "Öffentlicher Shop. Foren-Thema erstellt";
                    })
                .Alter("PublicStore.EditForumTopic", x =>
                    {
                        x.Name = "Öffentlicher Shop. Foren-Thema bearbeitet";
                    })
                .Alter("PublicStore.DeleteForumTopic", x =>
                    {
                        x.Name = "Öffentlicher Shop. Foren-Thema gelöscht";
                    })
                .Alter("PublicStore.AddForumPost", x =>
                    {
                        x.Name = "Öffentlicher Shop. Foren-Beitrag erstellt";
                    })
                .Alter("PublicStore.EditForumPost", x =>
                    {
                        x.Name = "Öffentlicher Shop. Foren-Beitrag bearbeitet";
                    })
                .Alter("PublicStore.DeleteForumPost", x =>
                    {
                        x.Name = "Öffentlicher Shop. Foren-Beitrag gelöscht";
                    })
                .Alter("EditThemeVars", x =>
                {
                    x.Name = "Theme-Variablen geändert";
                })
                .Alter("ResetThemeVars", x =>
                {
                    x.Name = "Theme-Variablen zurückgesetzt";
                })
                .Alter("ImportThemeVars", x =>
                {
                    x.Name = "Theme Variablen importiert";
                })
                .Alter("ExportThemeVars", x =>
                {
                    x.Name = "Theme Variablen exportiert";
                });

        }

        protected override void Alter(IList<ScheduleTask> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.Type)
                .Alter("SmartStore.Services.Messages.QueuedMessagesSendTask, SmartStore.Services", x =>
                    {
                        x.Name = "E-Mail senden";
                    })
                .Alter("SmartStore.Services.Messages.QueuedMessagesClearTask, SmartStore.Services", x =>
                    {
                        x.Name = "E-Mail Queue bereinigen";
                    })
                .Alter("SmartStore.Services.Media.TransientMediaClearTask, SmartStore.Services", x =>
                {
                    x.Name = "Temporäre Uploads bereinigen";
                })
                .Alter("SmartStore.Services.Customers.DeleteGuestsTask, SmartStore.Services", x =>
                    {
                        x.Name = "Gastbenutzer löschen";
                    })
                .Alter("SmartStore.Services.Caching.ClearCacheTask, SmartStore.Services", x =>
                    {
                        x.Name = "Cache bereinigen";
                    })
                .Alter("SmartStore.Services.Messages.QueuedMessagesSendTask, SmartStore.Services", x =>
                    {
                        x.Name = "E-Mail senden";
                    })
                .Alter("SmartStore.Services.Directory.UpdateExchangeRateTask, SmartStore.Services", x =>
                    {
                        x.Name = "Wechselkurse aktualisieren";
                    })
                .Alter("SmartStore.Services.Common.TempFileCleanupTask, SmartStore.Services", x =>
                    {
                        x.Name = "Temporäre Dateien bereinigen";
                    })
                .Alter("SmartStore.Services.Customers.TargetGroupEvaluatorTask, SmartStore.Services", x =>
                {
                    x.Name = "Zuordnungen von Kunden zu Kundengruppen aktualisieren";
                })
                .Alter("SmartStore.Services.Catalog.ProductRuleEvaluatorTask, SmartStore.Services", x =>
                {
                    x.Name = "Zuordnungen von Produkten zu Warengruppen aktualisieren";
                });
        }

        protected override void Alter(IList<SpecificationAttribute> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.DisplayOrder)
                .Alter(1, x =>
                {
                    x.Name = "CPU-Hersteller";
                })
                .Alter(2, x =>
                {
                    x.Name = "Farbe";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 1).First().Name = "Weiß";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 2).First().Name = "Schwarz";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 3).First().Name = "Beige";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 4).First().Name = "Rot";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 5).First().Name = "Blau";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 6).First().Name = "Grün";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 7).First().Name = "Gelb";
                })
                .Alter(3, x =>
                {
                    x.Name = "Festplatten-Kapazität";
                })
                .Alter(4, x =>
                {
                    x.Name = "Arbeitsspeicher";
                })
                .Alter(5, x =>
                {
                    x.Name = "Betriebssystem";
                })
                .Alter(6, x =>
                {
                    x.Name = "Anschluss";
                })
                .Alter(7, x =>
                {
                    x.Name = "Geschlecht";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 1).First().Name = "Herren";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 2).First().Name = "Damen";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 3).First().Name = "Unisex";
                })
                .Alter(8, x =>
                {
                    x.Name = "Material";
                    x.SpecificationAttributeOptions.First(y => y.DisplayOrder == 1).Name = "Edelstahl";
                    x.SpecificationAttributeOptions.First(y => y.DisplayOrder == 2).Name = "Titan";
                    x.SpecificationAttributeOptions.First(y => y.DisplayOrder == 3).Name = "Kunststoff";
                    x.SpecificationAttributeOptions.First(y => y.DisplayOrder == 4).Name = "Aluminium";
                    x.SpecificationAttributeOptions.First(y => y.DisplayOrder == 5).Name = "Leder";
                    x.SpecificationAttributeOptions.First(y => y.DisplayOrder == 6).Name = "Nylon";
                    x.SpecificationAttributeOptions.First(y => y.DisplayOrder == 7).Name = "Silikon";
                    x.SpecificationAttributeOptions.First(y => y.DisplayOrder == 8).Name = "Keramik";
                    x.SpecificationAttributeOptions.First(y => y.DisplayOrder == 9).Name = "Baumwolle";
                    x.SpecificationAttributeOptions.First(y => y.DisplayOrder == 10).Name = "100% Bio-Baumwolle";
                    x.SpecificationAttributeOptions.First(y => y.DisplayOrder == 11).Name = "Polyamid";
                    x.SpecificationAttributeOptions.First(y => y.DisplayOrder == 12).Name = "Gummi";
                    x.SpecificationAttributeOptions.First(y => y.DisplayOrder == 13).Name = "Holz";
                    x.SpecificationAttributeOptions.First(y => y.DisplayOrder == 14).Name = "Glas";
                    x.SpecificationAttributeOptions.First(y => y.DisplayOrder == 15).Name = "Elasthan";
                    x.SpecificationAttributeOptions.First(y => y.DisplayOrder == 16).Name = "Polyester";
                })
                .Alter(9, x =>
                {
                    x.Name = "Technische Ausführung";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 1).First().Name = "Automatik, selbstaufziehend";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 2).First().Name = "Automatik";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 3).First().Name = "Quarz, batteriebetrieben";
                })
                .Alter(10, x =>
                {
                    x.Name = "Verschluss";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 1).First().Name = "Faltschließe";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 2).First().Name = "Sicherheitsfaltschließe";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 3).First().Name = "Dornschließe";
                })
                .Alter(11, x =>
                {
                    x.Name = "Glas";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 1).First().Name = "Mineral";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 2).First().Name = "Saphir";
                })
                .Alter(12, x =>
                {
                    x.Name = "Sprache";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 1).First().Name = "Deutsch";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 2).First().Name = "Englisch";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 3).First().Name = "Französisch";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 4).First().Name = "Italienisch";
                })
                .Alter(13, x =>
                {
                    x.Name = "Ausgabe";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 1).First().Name = "Gebunden";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 2).First().Name = "Taschenbuch";
                })
                .Alter(14, x =>
                {
                    x.Name = "Genre";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 1).First().Name = "Abenteuer";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 2).First().Name = "Science-Fiction";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 3).First().Name = "Geschichte";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 4).First().Name = "Internet & Computer";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 5).First().Name = "Krimi";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 6).First().Name = "Autos";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 7).First().Name = "Roman";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 8).First().Name = "Kochen & Backen";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 9).First().Name = "Sachbuch";
                })
                .Alter(15, x =>
                {
                    x.Name = "Computer-Typ";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 1).First().Name = "Desktop";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 2).First().Name = "All-in-One";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 3).First().Name = "Laptop";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 4).First().Name = "Tablet";
                })
                .Alter(16, x =>
                {
                    x.Name = "Massenspeicher-Typ";
                })
                .Alter(17, x =>
                {
                    x.Name = "Größe (externe HDD)";
                })
                .Alter(18, x =>
                {
                    x.Name = "MP3-Qualität";
                })
                .Alter(19, x =>
                {
                    x.Name = "Genre";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 1).First().Name = "Blues";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 2).First().Name = "Jazz";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 3).First().Name = "Disko";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 4).First().Name = "Pop";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 5).First().Name = "Funk";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 6).First().Name = "Klassik";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 7).First().Name = "R&B";
                })
                .Alter(20, x =>
                {
                    x.Name = "Hersteller";
                })
                .Alter(21, x =>
                {
                    x.Name = "Für wen";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 1).First().Name = "Für ihn";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 2).First().Name = "Für sie";
                })
                .Alter(22, x =>
                {
                    x.Name = "Angebot";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 1).First().Name = "Räumung";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 2).First().Name = "Permanent günstigster Preis";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 3).First().Name = "Aktion";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 4).First().Name = "Preisreduzierung";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 5).First().Name = "Angebotspreis";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 6).First().Name = "Tagesangebot";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 7).First().Name = "Wochenangebot";
                })
                .Alter(23, x =>
                {
                    x.Name = "Größe";
                })
                .Alter(24, x =>
                {
                    x.Name = "Durchmesser";
                })
                .Alter(25, x =>
                {
                    x.Name = "Verschluss";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 1).First().Name = "Schnappverschluss";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 2).First().Name = "Faltverschluss";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 3).First().Name = "Stechverschluss";
                })
                .Alter(26, x =>
                {
                    x.Name = "Form";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 1).First().Name = "Oval";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 2).First().Name = "Rund";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 3).First().Name = "Herzförmig";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 4).First().Name = "Winkelförmig";
                })
                .Alter(27, x =>
                {
                    x.Name = "Speicherkapazität";
                })
                .Alter(28, x =>
                {
                    x.Name = "Scheibenmaterial";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 1).First().Name = "Mineral";
                    x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 2).First().Name = "Saphir";
                });
        }

        protected override void Alter(IList<ProductAttribute> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.Alias)
                .Alter("color", x =>
                {
                    x.Name = "Farbe";
                })
                .Alter("custom-text", x =>
                {
                    x.Name = "Eigener Text";
                })
                .Alter("hdd", x =>
                {
                    x.Name = "HDD";
                })
                .Alter("os", x =>
                {
                    x.Name = "Betriebssystem";
                })
                .Alter("processor", x =>
                {
                    x.Name = "Prozessor";
                })
                .Alter("ram", x =>
                {
                    x.Name = "Arbeitsspeicher";
                })
                .Alter("size", x =>
                {
                    x.Name = "Größe";
                })
                .Alter("software", x =>
                {
                    x.Name = "Software";
                })
                .Alter("game", x =>
                {
                    x.Name = "Spiel";
                })
                .Alter("iphone-color", x =>
                {
                    x.Name = "Farbe";
                })
                .Alter("ipad-color", x =>
                {
                    x.Name = "Farbe";
                })
                .Alter("memory-capacity", x =>
                {
                    x.Name = "Speicherkapazität";
                })
                .Alter("width", x =>
                {
                    x.Name = "Weite";
                })
                .Alter("length", x =>
                {
                    x.Name = "Länge";
                })
                .Alter("plate", x =>
                {
                    x.Name = "Tischplatte";
                })
                .Alter("plate-thickness", x =>
                {
                    x.Name = "Stärke der Tischplatte";
                })
                .Alter("ballsize", x =>
                {
                    x.Name = "Ballgröße";
                })
                .Alter("leather-color", x =>
                {
                    x.Name = "Lederfarbe";
                })
                .Alter("seat-shell", x =>
                {
                    x.Name = "Sitzschale";
                })
                .Alter("base", x =>
                {
                    x.Name = "Fußgestell";
                })
                .Alter("style", x =>
                {
                    x.Name = "Ausführung";
                })
                .Alter("framecolor", x =>
                {
                    x.Name = "Rahmenfarbe";
                })
                .Alter("lenscolor", x =>
                {
                    x.Name = "Glasfarbe";
                })
                .Alter("lenstype", x =>
                 {
                     x.Name = "Glas";
                 });
        }

        protected override void Alter(IList<ProductAttributeOptionsSet> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.Name)
                .Alter("General colors", x => x.Name = "Allgemeine Farben");
        }

        protected override void Alter(IList<ProductAttributeOption> entities)
        {
            base.Alter(entities);

            entities.Where(x => x.Alias == "red").Each(x => x.Name = "Rot");
            entities.Where(x => x.Alias == "green").Each(x => x.Name = "Grün");
            entities.Where(x => x.Alias == "blue").Each(x => x.Name = "Blau");
            entities.Where(x => x.Alias == "yellow").Each(x => x.Name = "Gelb");
            entities.Where(x => x.Alias == "black").Each(x => x.Name = "Schwarz");
            entities.Where(x => x.Alias == "white").Each(x => x.Name = "Weiß");
            entities.Where(x => x.Alias == "gray").Each(x => x.Name = "Grau");
            entities.Where(x => x.Alias == "silver").Each(x => x.Name = "Silber");
            entities.Where(x => x.Alias == "brown").Each(x => x.Name = "Braun");
        }

        protected override void Alter(IList<ProductVariantAttribute> entities)
        {
            base.Alter(entities);

            entities.Where(x => x.ProductAttribute.Alias == "color" || x.ProductAttribute.Alias == "leather-color").Each(x =>
            {
                x.ProductVariantAttributeValues.Where(y => y.Alias == "black").Each(y => y.Name = "Schwarz");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "white").Each(y => y.Name = "Weiß");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "silver").Each(y => y.Name = "Silber");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "red").Each(y => y.Name = "Rot");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "gray" || y.Alias == "charcoal").Each(y => y.Name = "Grau");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "maroon").Each(y => y.Name = "Rotbraun");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "blue").Each(y => y.Name = "Blau");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "purple").Each(y => y.Name = "Violett");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "green").Each(y => y.Name = "Grün");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "anthracite").Each(y => y.Name = "Anthrazit");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "brown").Each(y => y.Name = "Braun");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "dark-brown").Each(y => y.Name = "Dunkelbraun");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "natural").Each(y => y.Name = "Naturfarben");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "biscuit").Each(y => y.Name = "Biskuit");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "dark-green").Each(y => y.Name = "Dunkelgrün");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "light-grey").Each(y => y.Name = "Hellgrau");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "dark-red").Each(y => y.Name = "Dunkelrot");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "hazelnut").Each(y => y.Name = "Haselnuss");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "fuliginous").Each(y => y.Name = "Rauchfarbig");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "tomato-red").Each(y => y.Name = "Tomatenrot");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "yellow").Each(y => y.Name = "Gelb");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "mint").Each(y => y.Name = "Mintgrün");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "lightblue").Each(y => y.Name = "Hellblau");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "turquoise").Each(y => y.Name = "Türkis");
            });

            entities.Where(x => x.ProductAttribute.Alias == "iphone-color" || x.ProductAttribute.Alias == "ipad-color").Each(x =>
            {
                x.ProductVariantAttributeValues.Where(y => y.Alias == "black").Each(y => y.Name = "Schwarz");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "silver").Each(y => y.Name = "Silber");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "spacegray").Each(y => y.Name = "Space-Grau");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "purple").Each(y => y.Name = "Violett");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "lightblue").Each(y => y.Name = "Hellblau");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "turquoise").Each(y => y.Name = "Türkis");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "yellow").Each(y => y.Name = "Gelb");
            });

            entities.Where(x => x.ProductAttribute.Alias == "controller").Each(x =>
            {
                x.ProductVariantAttributeValues.Where(y => y.Alias == "without_controller").Each(y => y.Name = "Ohne Controller");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "with_controller").Each(y => y.Name = "Mit Controller");
            });

            entities.Where(x => x.ProductAttribute.Alias == "game").Each(x =>
            {
                x.ProductVariantAttributeValues.Where(y => y.Alias == "prince-of-persia-the-forgotten-sands").Each(y => y.Name = "Prince of Persia \"Die vergessene Zeit\"");
            });

            entities.Where(x => x.ProductAttribute.Alias == "seat-shell").Each(x =>
            {
                x.ProductVariantAttributeValues.Where(y => y.Alias == "cherry").Each(y => y.Name = "Kirsche");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "walnut").Each(y => y.Name = "Walnuss");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "wooden-black-lacquered").Each(y => y.Name = "Holz schwarz lackiert");
            });

            entities.Where(x => x.ProductAttribute.Alias == "base").Each(x =>
            {
                x.ProductVariantAttributeValues.Where(y => y.Alias == "top-edge-polished").Each(y => y.Name = "Oberkante poliert");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "completely-polished").Each(y => y.Name = "Vollständig poliert");
            });

            entities.Where(x => x.ProductAttribute.Alias == "plate").Each(x =>
            {
                x.ProductVariantAttributeValues.Where(y => y.Alias == "clear-glass").Each(y => y.Name = "Klarglas");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "sandblasted-glass").Each(y => y.Name = "Sandgestrahltes Glas");
            });

            entities.Where(x => x.ProductAttribute.Alias == "material").Each(x =>
            {
                x.ProductVariantAttributeValues.Where(y => y.Alias == "leather-special").Each(y => y.Name = "Leder Spezial");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "leather-aniline").Each(y => y.Name = "Leder Anilin");
                x.ProductVariantAttributeValues.Where(y => y.Alias == "mixed-linen").Each(y => y.Name = "Leinen gemischt");
            });


        }

        protected override void Alter(IList<ProductTemplate> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.ViewPath)
                .Alter("Product", x =>
                {
                    x.Name = "Standard Produkt Vorlage";
                });
        }

        protected override void Alter(IList<CategoryTemplate> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.ViewPath)
                .Alter("CategoryTemplate.ProductsInGridOrLines", x =>
                {
                    x.Name = "Products in Grid or Lines";
                });
        }

        protected override void Alter(IList<ManufacturerTemplate> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.ViewPath)
                .Alter("ManufacturerTemplate.ProductsInGridOrLines", x =>
                {
                    x.Name = "Products in Grid or Lines";
                });
        }

        protected override void Alter(IList<Category> entities)
        {
            base.Alter(entities);

            var names = new Dictionary<string, string>
            {
                { "Books", "Bücher" },
                { "Cell phones", "Smartphones" },
                { "Chairs", "Sessel" },
                { "Cook and enjoy", "Kochen und Genießen" },
                { "Computers", "Computer" },
                { "Desktops", "Desktop Computer" },
                { "Digital Products", "Digitale Produkte" },
                { "Fashion", "Mode" },
                { "Furniture", "Möbel" },
                { "Games", "Spiele" },
                { "Gaming Accessories", "Zubehör" },
                { "Gift cards", "Geschenkgutscheine" },
                { "Jackets", "Jacken" },
                { "Lounger", "Liegen" },
                { "Lamps", "Lampen" },
                { "Notebooks", "Notebook" },
                { "Shoes", "Schuhe" },
                { "Sports", "Sport" },
                { "Soccer", "Fußball" },
                { "Sunglasses", "Sonnenbrillen" },
                { "Tables", "Tische" },
                { "Trousers", "Hosen" },
                { "Watches", "Uhren" }
            };

            var alterer = entities.WithKey(x => x.MetaTitle);

            foreach (var kvp in names)
            {
                alterer.Alter(kvp.Key, x => x.Name = kvp.Value);
            }

            entities.Where(x => x.BadgeText.IsCaseInsensitiveEqual("NEW")).Each(x => x.BadgeText = "NEU");
        }

        private void AlterFashionProducts(IList<Product> entities)
        {
            entities.WithKey(x => x.Sku)
                .Alter("Fashion-112355", x =>
                {
                    x.ShortDescription = "Der Sneaker-Klassiker!";
                    x.FullDescription = "<p>Seit 1912 und bis heute unerreicht: Der Converse All Star Sneaker. Ein Schuh für jede Gelegenheit.</p>";
                })
                .Alter("Fashion-987693502", x =>
                {
                    x.Name = "Ärmelloses Shirt Meccanica";
                    x.ShortDescription = "Frauen Shirt mit trendigem Aufdruck";
                    x.FullDescription = "<p>Auch im Sommer geht der Ducati Stil mit der Mode! Mit dem ärmellosen Shirt Meccanica kann jede Frau ihrer Leidenschaft für Ducati mit einem bequemen und vielseitigen Kleidungsstück Ausdruck verleihen. Das Shirt gibt es in schwarz und vintagerot. Es trägt auf der Vorderseite den traditionellen Schriftzug in Plastisoldruck, wodurch er noch deutlicher und strahlender wird, während sich auf der Rückseite im Nackenbereich das berühmte Logo mit den typischen \"Flügeln\" der fünfziger Jahre befindet.</p>";
                })
                .Alter("Fashion-JN1107", x =>
                {
                    x.Name = "Damen Sport-Jacke";
                    x.FullDescription = "<p>Leichtes wind- und wasserabweisendes Gewebe, Futter aus weichem Single-Jersey Strickbündchen an Arm und Bund, 2 seitliche Taschen mit Reißverschluss, Kapuze in leicht tailliertem Schnitt.</p><ul><li>Oberstoff: 100%</li><li>Polyamid Futterstoff: 65% Polyester, 35% Baumwolle</li><li>Futterstoff 2: 100% Polyester</li></ul>";
                })
                .Alter("Fashion-65986524", x =>
                {
                    x.ShortDescription = "Moderne Jeans in Easy Comfort Fit";
                    x.FullDescription = "<p>Echte Five-Pocket-Jeans von Joker mit zusätzlicher, aufgesetzter Uhrentasche. Dank Easy Comfort Fit mit normaler Leibhöhe und bequemer Beinweite passend für jeden Figurtyp. Gerader Beinverlauf.</p><ul><li>Material: weicher, leichterer Premium-Denim aus 100% Baumwolle</li><li>Bundweite (Zoll): 29-46</li><li>Beinlänge (Zoll): 30 bis 38</li></ul>";
                })
                .Alter("jack-1305851", x =>
                {
                    x.Name = "KANUKA POINT JACKET M";
                    x.ShortDescription = "SOFTSHELLJACKE MÄNNER";
                    x.FullDescription = "<p>Sportliches Design für sportliche Touren: Die KANUKA POINT ist so gern in Bewegung wie du. Die Softshelljacke besteht aus superelastischem und sehr atmungsaktivem Material, das sich unterwegs jeder deiner Bewegungen anpasst. Deswegen nimmst du mit der KANUKA POINT jeden Pass mit Leichtigkeit.Und musst dir auch bei Kraxeleien zum Gipfel keine Gedanken um deine Jacke machen, denn ihr Material hält einiges aus.Auch bei Wind und leichten Schauern bleibst du gelassen.</p>";
                })
                .Alter("Wolfskin-4032541", x =>
                {
                    x.ShortDescription = "MÄNNER FREIZEITSCHUHE";
                    x.FullDescription = "<p>Du bist immer auf dem Sprung: zum Kino, zur neueröffneten Bar oder zum nächsten Stadtfest. Der stylishe COOGEE XT LOW ist DER Schuh für dein Leben in der Stadt. Denn er verbindet Funktion mit Style.</p>";
                })
                .Alter("Adidas-C77124", x =>
                {
                    x.Name = "SUPERSTAR SCHUH";
                    x.MetaTitle = "SUPERSTAR SCHUH";
                    x.ShortDescription = "DER STREETWEAR-KLASSIKER MIT DER SHELL TOE.";
                    x.FullDescription = "<p>Der adidas Superstar wurde erstmals 1969 herausgebracht und machte seinem Namen schon bald alle Ehre. Heute gilt er als Streetstyle-Legende. In dieser Version kommt der Schuh mit einem bequemen Obermaterial aus Full-Grain-Leder. Perfekt wird der Look durch die klassische Shell Toe aus Gummi für mehr Strapazierfähigkeit.</p>";
                });
        }

        private void AlterFurnitureProducts(IList<Product> entities)
        {
            entities.WithKey(x => x.Sku)
                .Alter("Furniture-lc6", x =>
                {
                    x.Name = "Le Corbusier LC 6 Esstisch (1929)";
                    x.ShortDescription = "Esstisch LC6, Designer: Le Corbusier, B x H x T: 225 x 69/74 (verstellbar) x 85 cm, Unterkonstruktion: Stahlrohr, Glasplatte: klar oder sandgestrahlt, 15 oder 19 mm, höhenverstellbar.";
                    x.FullDescription = "<p>Vier kleine Teller tragen eine Platte aus Glas. Darunter erstreckt sich in klarer Struktur die Konstruktion aus Stahlrohr. Der LC6 ist echter Klassiker der Bauhaus-Kunst und dient in Kombination mit den Drehstühlen LC7 als formschöne Le Corbusier-Essecke. Darüber hinaus findet man den Tisch auch vermehrt in Büros oder in Hallen. Er ist höhenverstellbar und kann so dem jeweiligen Zweck perfekt angepasst werden.</p><p>Der formschöne Tisch von Le Corbusier ist mit klarer oder mit sandgestrahlter Glasplatte erhältlich. Die Unterkonstruktion besteht aus ovalen Stahlrohren.</p>";
                })
                .Alter("Furniture-ball-chair", x =>
                {
                    x.Name = "Eero Aarnio Kugelsessel (1966)";
                    x.FullDescription = "<p>Der Ball Chair oder auch Globe Chair genannt, ist ein echtes Meisterwerk des legendären Designers Eero Aarnio. Der Kugelsessel aus den sechziger Jahren hat Designergeschichte geschrieben. Der eiförmig gestaltet Sessel ruht auf einem Trompetenfuss und wird nicht zu letzt aufgrund seiner Form und der ruhigen Atmophäre im Innern dieses Möbels besonders geschätzt. Das Design des Möbelkörpers lässt  Geräusche und störende Außenweltelemente in den Hintergurnd tretten. Ein Platz, wie geschaffen zum ausruhen und entspannen. Mit der großen Auswahl an Farben passt passt sich der Eyeball Chair jeder Wohn- und Arbeitsumgebung gekonnt an. Ein Sessel, der sich durch zeitloses Design auszeichnet und die Moderne immer im Blick haben wird. Der Ball Chair ist 360° zu drehen, um den Blick auf die Umgebung zu veränderen. Die Aussenschale in Fiberglas weiss oder schwarz. Der Bezug ist in Leder oder Linen Mixed.<p><p>Abmessung: Breite 102 cm, Tiefe 87 cm, Höhe 124 cm, Sitzhöhe: 44 cm.</p>";
                })
                .Alter("Furniture-lounge-chair", x =>
                {
                    x.Name = "Charles Eames Lounge Sessel (1956)";
                    x.ShortDescription = "Club Sessel, Lounge Chair, Designer: Charles Eames, Breite 80 cm, Tiefe 80 cm, Höhe 60 cm, Sitzschale: Sperrholz, Fuß (drehbar): Aluminiumguss, Kissen (gepolstert) mit Lederbezug.";
                    x.FullDescription = "<p>So sitzt man in einem Baseball-Handschuh. Das war jedenfalls eine der Vorstellungen, die Charles Eames beim Entwurf dieses Clubsessels im Kopf hatte. Der Lounge Chair sollte ein Komfort-Sessel sein, in den man luxuriös einsinken kann. Durch die Konstruktion aus drei miteinander verbundenen, beweglichen Sitzschalen und einer bequemen Polsterung gelang Charles Eames die Umsetzung. Eigentlich ist der Clubsessel mit drehbarem Fuß ein Gegensatz zu den Bauhaus-Charakteristiken, die Minimalismus und Funktionalität in den Vordergrund stellten. Dennoch wurde er zu einem Klassiker der Bauhaus-Geschichte und sorgt noch heute in vielen Wohnräumen und Clubs für absoluten Komfort mit Stil.</p><p>Abmessung: Breite 80 cm, Tiefe 60 cm,  Höhe Gesamt 80 cm (Höhe Rückenlehne: 60 cm). CBM: 0,70.</p><p>Verarbeitung: Lounge Chair mit Sitzschale aus schichtverleimten gebogenen Sperrholz mit Palisander furniert, Nussbaum natur oder in schwarz. Drehbarer Fuß aus Aluminiumguss schwarz mit polierten Kanten oder auch wahlweise vollständig verchromt. Aufwendige Polsterung der Kissen in Leder.</p><p>Alle POLSTEREINHEITEN sind bei dem EAMES LOUNGE CHAIR (Sitz, Armlehne, Rückenlehne, Kopflehne) abnehmbar.</p><p></p>";
                })
                .Alter("Furniture-cube-chair", x =>
                {
                    x.Name = "Josef Hoffmann Sessel Kubus (1910)";
                    x.ShortDescription = "Sessel Kubus, Designer: Josef Hoffmann, Breite 93 cm, Tiefe 72 cm, Höhe 77 cm, Grundgestell: massives Buchenholz, Polsterung: fester Polyurethan Schaum (formbeständig), Bezug: Leder";
                    x.FullDescription = "<p>Der Sessel Kubus von Josef Hoffmann hält, was der Name verspricht und das gleich in zweierlei Hinsicht. Er besteht aus vielen Quadraten, sowohl was die Konstruktion angeht als auch im Bezug auf das Design der Oberfläche. Zudem war der Kubus mit seiner rein geometrischen Form eine Art Vorbote des Kubismus. Der Sessel von Josef Hoffmann wurde 1910 entworfen und steht noch heute als Nachbau in zahlreichen Geschäfts- und Wohnräumen.</p><p>Ursprünglich war der Kubus ein Clubsessel. Zusammen mit dem zwei- und dem dreisitzigen Sofa der Serie entsteht eine gemütliche Sitzecke mit einer kultivierten und gehobenen Ausstrahlung. Das Grundgestell des Sessels besteht aus Holz. Die formbeständige Polsterung ist mit Leder überzogen und wurde mit einer speziellen Nähtechnik optisch zu Quadraten geformt.</p><p>Abmessung: Breite 93 cm, Tiefe 72 cm, Höhe 77 cm. CBM: 0,70.</p>";
                })
                .Alter("LC2 DS/23-1", x =>
                {
                    x.Name = "Le Corbusier LC2 Sofa, 3-Sitzer (1929)";
                    x.MetaTitle = "Le Corbusier LC2 Sofa, 3-Sitzer (1929)";
                    x.ShortDescription = "Sofa 3-Sitzer LC 2, Designer: Le Corbusier, Stahlrohrrahmen (Chrom), Kissen aus Polyurethan Schaum und Dacronwatte, Sitzpolster mit Daunenauflage, Bezug: Leder"; x.FullDescription = "<p>Das 3-Sitzer-Sofa LC2 ist zusammen mit dem gleichnamigen Zweisitzer die vollkommene Ergänzung der bekannten Corbusier-Sessel. Zusammen ergibt sich eine formvollendete Sitzgruppe für Lobbys, Lofts oder Salons mit hohem Design-Anspruch. " +
                    "Auch wenn die Corbusier Sofas das Kürzel des Designers (LC) tragen, sind sie nicht von ihm entworfen worden. Sie lehnen sich lediglich eng an die von Le Corbusier entworfenen Sitzmöbel LC2 und LC3 an. Der Optik und dem Komfort schadet dieser Umstand jedoch nicht. " +
                    "Das Gestell dieses Sofas besteht aus einem aufwendig gebogenen Stahlrohrahmen in Chrom. Die Lederkissen sind mit Polyurethan Schaum und Dacronwatte gefüllt. Die Sitzfläche des Sofas wurde für einen optimalen Sitzkomfort zusätzlich mit Daunenfedern aufgepolstert.</p>" +
                    "<p>Abmessungen: B x T x H: 180 x 70 x 67 cm, Sitzhöhe: ca. 45 cm</p>";
                })
                .Alter("JH DS/82-1", x =>
                {
                    x.Name = "Josef Hoffmann Sofa 2-Sitzer Cubus (1910)";
                    x.MetaTitle = "Josef Hoffmann Sofa 2-Sitzer Cubus (1910)";
                    x.ShortDescription = "Sofa Kubus, 2-Sitzer, Designer: Josef Hoffmann, Breite 166 cm, Tiefe 72 cm, Höhe 77 cm, Grundgestell: massives Buchenholz, Polsterung: fester Polyurethan Schaum (formbeständig), Bezug: Leder";
                    x.FullDescription = "<p>Der Zweisitzer aus der Kubus-Serie von Josef Hoffmann ist ein stilvoller Blickfang in Wohn- und Geschäftsräumen. Gemeinsam mit dem Sessel Kubus und dem Kubus Dreisitzer entsteht eine stilvolle Sitzgruppe für Empfangshallen und große Wohnzimmer." +
                    "Das Sofa von Josef Hoffmann ist mit einer formbeständige Polsterung versehen, mit Leder überzogen und zeigt durch eine spezielle Nähtechnik zahlreiche Quadrate, die sich zu einem Gesamtbild Formen. " +
                    "Das Grundgestell besteht aus Buchenholz.Die rein geometrische Form dieses Hoffman Entwurfs war auch Vorreiter für den Kubismus, der Anfang des 20.Jahrhunderts seine Hochphase erreichte. " +
                    "</p><p>Abmessung:  Breite 166 cm, Tiefe 72 cm, Höhe 77 cm, cbm:  1,50</p>";
                })
                .Alter("LR 556", x =>
                {
                    x.Name = "Mies van der Rohe Barcelona - Loveseat Sofa (1929)";
                    x.MetaTitle = "Mies van der Rohe Barcelona - Loveseat Sofa (1929)";
                    x.ShortDescription = "Sessel Barcelona lang, Designer: Mies van der Rohe, L x T x H: 147 x 75 x 75 cm, verchromtes Gestell aus Spezialfederstahl, Bespannung: Kernlederstreifen, Polster mit Polyurethanschaum-Kern, Bezug: Leder";
                    x.FullDescription = "<p>Der Loveseat Sofa Barcelona ist eines der bekanntesten Möbelstücke der Bauhaus-Ära. Er wurde von Mies van der Rohe entworfen und 1929 bei der Weltausstellung in Barcelona vorgestellt. Mies van der Rohe widmete ihn dem spanischen Königspaar. Der Barcelona Sofa in der langen Ausführung eignet sich hervorragend als Sitzmöbel für Ausstellungen oder Geschäftsräume. " +
                    "Zusammen mit der schmalen Ausführung und einem Tisch von Mies van der Rohe entsteht außerdem eine stilvolle Sitzecke für Wohnräume. " +
                    "Der Loveseat Sofa Barcelona hat ein Gestell aus besonders hochwertigem Spezialfederstahl.Als Bespannung dienen Kernlederstreifen. " +
                    "Darauf liegt die Polsterung mit Lederbezug.Einzelne Quadrate ergeben hierbei ein symmetrisches, stilvolles Bild.</p>" +
                    "<p>Abmessung:  Länge 147 cm, Tiefe 75 cm, Höhe 75 cm, cbm:  0,96 </p>";
                })
                .Alter("IN 200", x =>
                {
                    x.Name = "Isamu Noguchi Couchtisch, Coffee Table (1945)";
                    x.MetaTitle = "Isamu Noguchi Couchtisch, Coffee Table (1945)";
                    x.ShortDescription = "Couchtisch, Designer: Isamu Noguchi, B x H x T: 128 x 40 x 92,5 cm, Untergestell: Holz, Tischplatte: Kristallglas, 15 oder 19 mm";
                    x.FullDescription = "<p>Der Kaffeetisch von Isamu Noguchi hat einst sogar den Präsidenten des New Yorker Museums für moderne Kunst beeindruckt. " +
                    "Für ihn ist er nämlich ursprünglich entworfen worden. Das geschwungene Untergestell aus Esche ist ein eleganter Blickfang. " +
                    "Es wirkt unaufdringlich und kommt durch die durchsichtige, dreiseitige Glasplatte perfekt zur Geltung. " +
                    "Ein Bauhaus - Möbel, das heute in vielen Räumen mit gehobener Ausstattung als Beistelltisch genutzt wird. " +
                    "Es passt perfekt in die Lounge, ins Wohnzimmer und in Empfangsräume.</p>" +
                    "<p>Abmessungen:  Breite 128 cm, Höhe 40 cm, Tiefe 92,5 cm </p>";
                })
                .Alter("LM T/98", x =>
                {
                    x.Name = "Ludwig Mies van der Rohe Tisch Barcelona (1930)";
                    x.MetaTitle = "Ludwig Mies van der Rohe Tisch Barcelona (1930)";
                    x.ShortDescription = "Tisch Barcelona, Designer: Mies van der Rohe, Breite 90 cm, Höhe 46 cm, Tiefe 90 cm, Untergestell: verchromter Flachstahl, Tischplatte: Glas (12 mm)";
                    x.FullDescription = "<p>Dieser Tisch von Mies van der Rohe passt zur berühmten Barcelona-Serie aus Sessel und Hocker, die für den spanischen König entworfen und  1929 bei der Weltausstellung präsentiert wurde. " +
                    "Der Couchtisch wurde zwar erst einige Zeit später von Mies van der Rohe für das Haus „Tugendhat“ angefertigt, bildet aber mit den Möbeln der Barcelona-Serie eine attraktive Sitzecke für Büros und Wohnräume. " +
                    "Der Tisch von Mies van der Rohe besteht aus einem Gestell aus Flachstahl und einer 12 mm dicken Glasplatte.Unter der durchsichtigen Platte erscheint durch die Konstruktion ein verchromtes „X“.</p>" +
                    "<p>Abmessung: Breite 90 cm, Höhe 46 cm, Tiefe 90 cm, Glasplatte: 12 mm </p>";
                });
        }

        protected override void Alter(IList<Product> entities)
        {
            base.Alter(entities);

            try
            {
                string ps3FullDescription = "<table cellspacing=\"0\" cellpadding=\"1\"><tbody><tr><td>Prozessortyp&nbsp;</td><td>Cell Processor&nbsp;</td></tr><tr><td>Arbeitsspeicher (RAM)nbsp;</td><td>256 MB&nbsp;</td></tr><tr><td>Grafikchipsatz&nbsp;</td><td>nVidia RSX&nbsp;</td></tr><tr><td>Taktfrequenz&nbsp;</td><td>3.200 MHz&nbsp;</td></tr><tr><td>Abmessungen&nbsp;</td><td>290 x 60 x 230 mm&nbsp;</td></tr><tr><td>Gewicht&nbsp;</td><td>2.100 g&nbsp;</td></tr><tr><td>Speichermedium&nbsp;</td><td>Blu-ray&nbsp;</td></tr><tr><td>Stromverbrauch in Betrieb&nbsp;</td><td>190 Watt&nbsp;</td></tr><tr><td>Plattform&nbsp;</td><td>Playstation 3 (PS3)&nbsp;</td></tr><tr><td>Akku-Laufzeit&nbsp;</td><td>0 h&nbsp;</td></tr><tr><td>Anschlüsse&nbsp;</td><td>2x USB 2.0, AV-Ausgang, digitaler optischer Ausgang (SPDIF), HDMI&nbsp;</td></tr><tr><td>Soundmodi&nbsp;</td><td>AAC, Dolby igital, Dolby Digital Plus, Dolby TrueHD, DTS, DTS-HD, LPCM 7.1-Kanal&nbsp;</td></tr><tr><td>Unterstützte Auflösungen&nbsp;</td><td>576i, 576p, 720p, 1080i, 1080p Full HD&nbsp;</td></tr><tr><td>Serie&nbsp;</td><td>Sony Playstation 3&nbsp;</td></tr><tr><td>Veröffentlichungsjahr&nbsp;</td><td>2012&nbsp;</td></tr><tr><td>Mitgelieferte Hardware&nbsp;</td><td>Dual Shock 3-Controller&nbsp;</td></tr><tr><td>Farbe&nbsp;</td><td>schwarz&nbsp;</td></tr><tr><td>USK-Freigabe&nbsp;</td><td>0 Jahre&nbsp;</td></tr><tr><td>PEGI-Freigabe&nbsp;</td><td>3 Jahre&nbsp;</td></tr><tr><td>RAM-Typ&nbsp;</td><td>XDR-DRAM&nbsp;</td></tr><tr><td>Controller-Akku-Laufzeit&nbsp;</td><td>30 h&nbsp;</td></tr><tr><td>WLAN-Standard&nbsp;</td><td>IEEE 802.11 b/g&nbsp;</td></tr><tr><td>LAN-Standard&nbsp;</td><td>Gigabit Ethernet (10/100/1000 Mbit/s)&nbsp;</td></tr><tr><td>Daten-Kommunikation&nbsp;</td><td>Bluetooth 2.0 + EDR, Netzwerk (Ethernet), WLAN (Wi-Fi)&nbsp;</td></tr><tr><td>Controller-Eigenschaften&nbsp;</td><td>Beschleunigungssensor, Lagesensor (Gyrosensor), Headset-nschluss, Vibration&nbsp;</td></tr><tr><td>Spielsteuerungen&nbsp;</td><td>Bewegungssteuerung, Controller&nbsp;</td></tr><tr><td>Spielfunktionen&nbsp;</td><td>Community, Kindersicherung, Plattformübergreifendes Spielen, Remote Gaming, Sony PlayStation Network, Sony PlayStation Plus, Streaming (DLNA), Streaming (PlayStation Now/Gaikai)&nbsp;</td></tr><tr><td>Marketplace&nbsp;</td><td>Sony PlayStation Store&nbsp;</td></tr><tr><td>Internetfunktionen&nbsp;</td><td>Chat, Video Chat, Voice Chat, Webbrowser&nbsp;</td></tr><tr><td>Multimedia-Funktionen&nbsp;</td><td>Audio-CD-Wiedergabe, Blu-ray-Wiedergabe, DVD-Wiedergabe, Internet-Radio, Video-Wiedergabe&nbsp;</td></tr><tr><td>Streaming-ienste&nbsp;</td><td>Animax, Lovefilm, Maxdome, Mubi, Music on Demand, Sony Music Unlimited, Sony Video Unlimited, TuneIn, VidZone, Video on Demand, Watchever, YouTube&nbsp;</td></tr><tr><td>Ausstattung</td><td>onlinefähig/eingebautes Netzteil/3D-Ready</td></tr><tr><td>Sonstiges</td><td>bis zu 7 kabellose lageempfindliche Controller (Bluetooth) / PSP-Connectivity / keine Abwärtskompatibilität zu PlayStation 2-Spielen / Herunterladen von Filmen von Hollywood Studios aus dem Playstation Network, übertragbar auf PSP / Toploader-Laufwerk / Cross-Plattform-Funktionen (PS3 und PS Vita): Remote Play (Zugriff auf kompatible Inhalte auf PS3), Cross Buy (Spiel für anderes System kostenlos oder günstiger (online) dazukaufen), Cross-Goods (In-Game-Objekte für beide Systeme), Cross-Save (gespeichertes Spiel auf anderem System weiterspielen), Cross-Controller (PS Vita als Controller), Cross-Play (PSV vs. PS3), PlayStation Network-Konto erforderlich / 256 MB GDDR3 Grafikspeicher&nbsp;</td></tr></tbody></table>";
                string ps4FullDescription = "<ul><li>PlayStation 4, die neueste Generation des Entertainment Systems, definiert reichhaltiges und beeindruckendes Gameplay, völlig neu.</li><li>Den Kern der PS4 bilden ein leistungsstarker, eigens entwickelter Chip mit acht x86-Kernen (64 bit) sowie ein hochmoderner optimierter Grafikprozessor.</li><li>Ein neuer, hochsensibler SIXAXIS-Sensor ermöglicht mit dem DualShock 4 Wireless Controller eine erstklassige Bewegungssteuerung.</li><li>Der DualShock 4 bietet als Neuerungen ein Touchpad, eine Share-Taste, einen eingebauten Lautsprecher und einen Headset-Anschluss.</li><li>PS4 integriert Zweitbildschirme, darunter PS Vita, Smartphones und Tablets, damit Spieler ihre Lieblingsinhalte überall hin mitnehmen können.</li></ul>";

                entities.WithKey(x => x.MetaTitle)

                #region Category Sports

                #region Category Golf

                .Alter("Titleist SM6 Tour Chrome", x =>
                {
                    x.ShortDescription = "Für Golfspieler, die ein Maximum an Schlagkontrolle und Feedback wünschen.";
                    x.FullDescription = "<p><strong>Inspiriert von den besten Eisenspielern der Welt</strong></p><p>Die neuen 'Spin Milled 6'-Wedges etablieren eine neue Leistungsklasse in drei Schlüsselbereichen des Wedge-Spiels: Präzise Längenschritte, Schlagvielfalt und maximaler Spin.&nbsp;</p><p>  <br />  Für jeden Loft wird der Schwerpunkt des Wedges einzeln bestimmt. Daher bieten die SM6 eine besonders präzise Längen- und Flugkurvenkontrolle in Verbindung mit großartigem Schlaggefühl.&nbsp;  <br />  Bob Vokeys tourerpobte Sohlenschliffe erlauben allen Golfern mehr Schlagvielfalt, angepasst auf deren persönliches Schwungprofil und die jeweiligen Bodenverhältnissen.</p><p>  <br />  Zu den absolut exakt und mit 100%iger Qualitätskontrolle gefrästen Rillen wurde eine neue, parallele Schlagflächen-Textur entwickelt. Das Ergebnis ist eine beständig höhere Kantenschärfe für mehr Spin.</p><p></p><ul>  <li>Präzise Längen und Flugkurvenkontrolle dank progressiv platziertem Schwerpunkt.</li>  <li>Verbesserte Schlagvielfalt aufgrund der erprobten Sohlenschliffe von Bob Vokey.</li>  <li>TX4-Rillen erzeugen mehr Spin durch eine neue Oberfläche und Kantenschärfe.</li>  <li>Vielfältige Personalisierungsmöglichkeiten.</li></ul><p></p><p></p><p></p>";
                })
                .Alter("Titleist Pro V1x", x =>
                {
                    x.ShortDescription = "Golfball mit hohem Ballflug";
                    x.FullDescription = "<p>Auf den neuen Titleist Pro V1x vertrauen die Spitzenspieler. Hoher Ballflug, weiches Schlaggefühl und mehr Spin im kurzen Spiel sind die Vorteile der V1x-Ausführung.Perfekte Gesamtleistung vom führenden Hersteller. Der neue Titleist Pro V1-Golfball ist exakt definiert und verspricht durchdringenden Ballflug bei sehr weichem Schlaggefühl.</p>";
                })
                .Alter("Supreme Golfball", x =>
                {
                    x.ShortDescription = "Trainingsbälle mit perfekten Flugeigenschaften";
                    x.FullDescription = "<p>Perfekter Golf-Übungsball mit den Eigenschaften wie das 'Original', aber in glasbruchsicherer Ausführng. Massiver Kern, ein idealer Trainingsball für Hof und Garten. Farben: weiß, gelb, orange.</p>";
                })
                .Alter("GBB Epic Sub Zero Driver", x =>
                {
                    x.ShortDescription = "Geringer Spin für gutes Golfen!";
                    x.FullDescription = "<p>Ihr Spiel gewinnt mit dem GBB Epic Sub Zero Driver. Ein Golfschläger mit extrem wenig Spin und das bei phänomenaler Hochgeschwindigkeits-Charakteristik.&nbsp;</p>";
                })

                #endregion Category Golf

                #region Category Soccer

                .Alter("Nike Strike Football", x =>
                {
                    x.Name = "Nike Strike Fußball";
                    x.ShortDescription = "HERVORRAGENDES BALLGEFÜHL. GUTE SICHTBARKEIT.";
                    x.FullDescription = "<p>Verbessert das Spiel jeden Tag mit dem Nike Strike Football. Verstärkter Gummi behält seine Form für zuversichtliche und konsequente Kontrolle. Eine herausragende Visual Power Grafik in schwarz, grün und orange ist am besten für Ball Tracking, trotz dunkler oder schlechter Bedingungen.</p><p></p><ul>  <li>Visual Power Grafik hilft, eine echte Lesung auf Flugtrajektorie zu geben.</li>  <li>Strukturiertes Gehäuse bietet überlegene Note.</li>  <li>Verstärkte Gummiblase unterstützt Luft- und Formbeibehaltung.</li>  <li>66% Gummi / 15% Polyurethan / 13% Polyester / 7% EVA.</li></ul>";
                })
                .Alter("Evopower 5.3 Trainer HS Ball", x =>
                {
                    x.ShortDescription = "Einsteiger Trainingsball.";
                    x.FullDescription = "<p>Einsteiger Trainingsball. <br /> Konstruiert aus 32 Platten mit gleichen Flächen für reduzierte Naht und eine vollkommen runde Form.  <br />  Handgestickte Platten mit mehrschichtigem gewebtem Rücken für mehr Stabilität und Aerodynamik.</p>";
                })
                .Alter("Torfabrik official game ball", x =>
                {
                    x.ShortDescription = "Einsteiger Trainingsball.";
                    x.FullDescription = "<p>Einsteiger Trainingsball.  <br />  Konstruiert aus 32 Platten mit gleichen Flächen für reduzierte Naht und eine vollkommen runde Form.  <br />  Handgestickte Platten mit mehrschichtigem gewebtem Rücken für mehr Stabilität und Aerodynamik.</p>";
                })
                .Alter("Adidas TANGO SALA BALL", x =>
                {
                    x.ShortDescription = "Farbe White/Black/Solar Red";
                    x.FullDescription = "<h2 style='box-sizing: border-box; outline: 0px; margin-right: 0px; margin-bottom: 32px; margin-left: 0px; padding: 0px; border: 0px; font-variant-numeric: inherit; font-weight: inherit; font-stretch: inherit; font-size: 32px; line-height: 30.4px; font-family: adilight, Arial, Helvetica, Verdana, sans-serif; vertical-align: baseline; background-image: initial; background-position: initial; background-size: initial; background-repeat: initial; background-attachment: initial; background-origin: initial; background-clip: initial; max-height: 999999px; text-transform: uppercase; letter-spacing: 6px; text-align: center; color: rgb(0, 0, 0);'>TANGO PASADENA BALL</h2><div class='product-details-description clearfix' style='box-sizing: border-box; outline: 0px; margin: 0px; padding: 0px; border: 0px; font-variant-numeric: inherit; font-stretch: inherit; font-size: 14px; line-height: inherit; font-family: adihausregular, Arial, Helvetica, Verdana, sans-serif; vertical-align: baseline; background-image: initial; background-position: initial; background-size: initial; background-repeat: initial; background-attachment: initial; background-origin: initial; background-clip: initial; max-height: 999999px; zoom: 1; color: rgb(0, 0, 0);'>  <div class='prod-details para-small' itemprop='description' style='box-sizing: border-box; outline: 0px; margin: 0px; padding: 0px; border: 0px; font-style: inherit; font-variant: inherit; font-weight: inherit; font-stretch: inherit; line-height: 24px; vertical-align: baseline; background: transparent; max-height: 999999px; color: rgb(54, 55, 56); width: 441.594px; float: left;'>Der adidas Tango Pasadena Ball wurde speziell für harte Trainingseinheiten und hitzige Kämpfe auf dem Fußballplatz gemacht. Er hat die bestmögliche FIFA-Bewertung bekommen und verfügt über einen handgenähten Körper, dem kein Training und kein Spiel etwas anhaben können.  </div>  <div class='prod-details para-small' itemprop='description' style='box-sizing: border-box; outline: 0px; margin: 0px; padding: 0px; border: 0px; font-style: inherit; font-variant: inherit; font-weight: inherit; font-stretch: inherit; line-height: 24px; vertical-align: baseline; background: transparent; max-height: 999999px; color: rgb(54, 55, 56); width: 441.594px; float: left;'>  </div>  <ul class='bullets_list para-small' style='box-sizing: border-box; outline: 0px; margin-right: 0px; margin-bottom: 0px; margin-left: 16px; padding: 0px; border: 0px; font-style: inherit; font-variant: inherit; font-weight: inherit; font-stretch: inherit; line-height: 20px; vertical-align: baseline; background: transparent; max-height: 999999px; list-style-position: initial; list-style-image: initial; color: rgb(54, 55, 56); width: 441.594px; float: right;'>    <li style='box-sizing: border-box; outline: 0px; margin: 0px; padding: 0px; border: 0px; font-style: inherit; font-variant: inherit; font-weight: inherit; font-stretch: inherit; font-size: inherit; line-height: 24px; font-family: inherit; vertical-align: baseline; background: transparent; max-height: 999999px;'>Handgenäht für hohe Strapazierfähigkeit und gutes Ballgefühl</li>    <li style='box-sizing: border-box; outline: 0px; margin: 0px; padding: 0px; border: 0px; font-style: inherit; font-variant: inherit; font-weight: inherit; font-stretch: inherit; font-size: inherit; line-height: 24px; font-family: inherit; vertical-align: baseline; background: transparent; max-height: 999999px;'>FIFA-Höchstwertung: Der Ball hat Tests in den Kategorien Gewicht, Wasseraufnahme, Form- und Größenbeständigkeit erfolgreich bestanden</li>    <li style='box-sizing: border-box; outline: 0px; margin: 0px; padding: 0px; border: 0px; font-style: inherit; font-variant: inherit; font-weight: inherit; font-stretch: inherit; font-size: inherit; line-height: 24px; font-family: inherit; vertical-align: baseline; background: transparent; max-height: 999999px;'>Latex-Blase für optimales Rücksprungverhalten</li>    <li style='box-sizing: border-box; outline: 0px; margin: 0px; padding: 0px; border: 0px; font-style: inherit; font-variant: inherit; font-weight: inherit; font-stretch: inherit; font-size: inherit; line-height: 24px; font-family: inherit; vertical-align: baseline; background: transparent; max-height: 999999px;'>100 % Polyurethan</li>  </ul></div>";
                })

                #endregion Category Soccer

                #region Category Basketball

                .Alter("Evolution High School Game Basketball", x =>
                {
                    x.ShortDescription = "Für alle Positionen auf allen Spielstufen, Spieltag und jeden Tag";
                    x.FullDescription = "<p>Die Wilson Evolution High School Spiel Basketball hat exklusive Mikrofaser-Composite-Leder-Konstruktion mit tiefen geprägten Kieselsteinen, um Ihnen die ultimative in Gefühl und Kontrolle.</p><p>Die patentierte Cushion Core Technologie erhöht die Haltbarkeit für längeres Spiel.</p><p>Diese Mikrofaser-Composite Evolution High School Basketball ist mit Composite-Kanäle für besseren Griff kieselig, hilft Spieler heben ihr Spiel auf die nächste Ebene.</p><p>Für alle Positionen auf allen Spielstufen, Spieltag und jeden Tag, liefert Wilson die Skill-Building-Performance, die Spieler verlangen. Diese Registern-Größe 29,5 'Wilson Basketball ist ein idealer Basketball für High-School - Spieler, und ist entweder für Freizeit - Nutzung oder für Liga - Spiele konzipiert.</ p >< p > Es ist NCAA und NFHS genehmigt, so dass Sie wissen, es ist ein qualitativ hochwertiger Basketball, der Ihnen helfen wird hone Ihr Shooting, Passing und Ball - Handling - Fähigkeiten.</ p >< p > Nehmen Sie Ihr Team den ganzen Weg zur Meisterschaft mit dem Wilson Evolution High School Game Basketball.</p>";
                })
                .Alter("All-Court Basketball", x =>
                {
                    x.ShortDescription = "Ein langlebiger Basketball für alle Oberflächen";
                    x.FullDescription = "<p></p><div>  <h2>All-Court Prep Ball  </h2>  <h4>Ein langlebiger Basketball für alle Oberflächen  </h4>  <div class='product-details-description clearfix'>    <div class='prod-details para-small' itemprop='description'>    </div>    <div class='prod-details para-small' itemprop='description'>Ob auf Parkett oder auf Asphalt – der adidas All-Court Prep Ball hat nur ein Ziel: den Korb. Dieser Basketball besteht aus langlebigem Kunstleder, was ihn sowohl für Hallenplätze als auch für Spiele im Freien prädestiniert.    </div>    <div class='prod-details para-small' itemprop='description'>    </div>    <ul class='bullets_list para-small'>      <li>Verbundüberzug aus Kunstleder</li>      <li>Für drinnen und draußen geeignet</li>      <li>Wird unaufgepumpt geliefert</li>    </ul>  </div></div>";
                })

                #endregion Category Basketball

                #endregion Category Sports

                #region Category Sunglasses

                .Alter("Radar EV Prizm Sports Sunglasses", x =>
                {
                    x.Name = "Radar EV Prizm Sports Sonnenbrille";
                    x.ShortDescription = "";
                    x.FullDescription = "<p><strong>RADAR&nbsp;EV PATH&nbsp;PRIZM&nbsp;ROAD</strong></p><p>Ein neuer Meilenstein in der Geschichte des Performance-Designs: Die Radar® EV setzt den Innovationen eines ohnehin schon revolutionären Designs mit einem größeren Glas für ein erweitertes Blickfeld nach oben noch eins drauf. Vom Komfort und Schutz des Rahmens aus O Matter® bis zum griffigen Halt der Unobtainium®-Komponenten ist dieses Premium-Design im innovativen und stilvollen Erbe der Radar verwurzelt.</p><p><strong>EIGENSCHAFTEN</strong></p><ul>  <li>PRIZM™ ist eine neue Glastechnologie von Oakley, die die Sicht für spezielle Sportarten und Umgebungsbedingungen optimiert.</li>  <li>Path-Gläser für eine bessere Performance gegenüber traditionellen Gläsern, die Ihre Wangen berühren, und ein erweitertes Blickfeld</li>  <li>Speziell konstruiert für maximalen Luftstrom zur kühlenden Belüftung</li>  <li>Ohrbügel und Nasenpads aus Unobtainium® für einen sicheren Sitz der Gläser, der sich bei Schweiß sogar verstärkt</li>  <li>Wechselglassystem für sekundenschnelles Auswechseln der Gläser zur optimalen Sichtanpassung an jedes Sportumfeld</li></ul>";
                })
                .Alter("Custom Flak Sunglasses", x =>
                {
                    x.Name = "Custom Flak® Sportsonnenbrille";
                    x.ShortDescription = "";
                    x.FullDescription = "Jede Brille wird  in Handarbeit für Sie zusammengesetzt.";
                })
                .Alter("Ray-Ban Top Bar RB 3183", x =>
                {
                    x.Name = "Ray-Ban Top Bar RB 3183";
                    x.ShortDescription = "";
                    x.FullDescription = "<p>Die Sonnenbrille Ray-Ban ® RB3183 mir ihrer aerodynamischen Form eine reminiszenzist an Geschwindigkeit. Eine rechteckige Form und das auf den</p><p>Bügeln aufgedruckte klassische Ray-Ban Logo zeichnet dieses leichte Halbrand-Modell aus.</p>";
                })
                .Alter("ORIGINAL WAYFARER AT COLLECTION", x =>
                {
                    x.ShortDescription = "Die Ray-Ban Original Wayfarer ist der bekannteste Style in der Geschichte der Sonnenbrillen. Mit dem original Design von 1952 ist die Wayfarer bei Prominenten, Musikern, Künstlern und Mode Experten beliebt. ";
                    x.FullDescription = "";
                })

                #endregion Category Sunglasses

                #region Category Gift Cards

                .Alter("$10 Virtual Gift Card", x =>
                {
                    x.Name = "10 € Geschenkgutschein";
                    x.ShortDescription = "5 € Geschenkgutschein. Eine ideale Geschenkidee.";
                    x.FullDescription = "<p>Wenn in letzter Minute mal wieder ein Geschenk fehlt oder man nicht weiß, was man schenken soll, dann bietet sich der Kauf eines Geschenkgutscheins an.</p>";
                })
                .Alter("$25 Virtual Gift Card", x =>
                {
                    x.Name = "25 € Geschenkgutschein";
                    x.ShortDescription = "25 € Geschenkgutschein. Eine ideale Geschenkidee.";
                    x.FullDescription = "<p>Wenn in letzter Minute mal wieder ein Geschenk fehlt oder man nicht weiß, was man schenken soll, dann bietet sich der Kauf eines Geschenkgutscheins an.</p>";
                })
                .Alter("$50 Virtual Gift Card", x =>
                {
                    x.Name = "50 € Geschenkgutschein";
                    x.ShortDescription = "50 € Geschenkgutschein. Eine ideale Geschenkidee.";
                    x.FullDescription = "<p>Wenn in letzter Minute mal wieder ein Geschenk fehlt oder man nicht weiß, was man schenken soll, dann bietet sich der Kauf eines Geschenkgutscheins an.</p>";
                })
                .Alter("$100 Virtual Gift Card", x =>
                {
                    x.Name = "100 € Geschenkgutschein";
                    x.ShortDescription = "100 € Geschenkgutschein. Eine ideale Geschenkidee.";
                    x.FullDescription = "<p>Wenn in letzter Minute mal wieder ein Geschenk fehlt oder man nicht weiß, was man schenken soll, dann bietet sich der Kauf eines Geschenkgutscheins an.</p>";
                })

                #endregion

                #region Category Books

                .Alter("Überman: The novel", x =>
                {
                    x.Name = "Überman: Der Roman";
                    x.ShortDescription = "Gebundene Ausgabe";
                    x.FullDescription = "<p> Nach Der Schatten des Windes und Das Spiel des Engels der neue große Barcelona-Roman von Carlos Ruiz Zafón. - Barcelona, Weihnachten 1957. Der Buchhändler Daniel Sempere und sein Freund Fermín werden erneut in ein großes Abenteuer hineingezogen. In der Fortführung seiner Welterfolge nimmt Carlos Ruiz Zafón den Leser mit auf eine fesselnde Reise in sein Barcelona. Unheimlich und spannend, mit unglaublicher Sogkraft und viel Humor schildert der Roman die Geschichte von Fermín, der 'von den Toten auferstanden ist und den Schlüssel zur Zukunft hat'. Fermíns Lebensgeschichte verknüpft die Fäden von Der Schatten des Windes mit denen aus Das Spiel des Engels. Ein meisterliches Vexierspiel, das die Leser rund um die Welt in Bann hält. </p> <p> Produktinformation<br> Gebundene Ausgabe: 416 Seiten<br> Verlag: S. Fischer Verlag; Auflage: 1 (25. Oktober 2012)<br> Sprache: Deutsch<br> ISBN-10: 3100954025<br> ISBN-13: 978-3100954022<br> Originaltitel: El prisionero del cielo<br> Größe und/oder Gewicht: 21,4 x 13,6 x 4,4 cm<br> </p>";
                })
                .Alter("Best Grilling Recipes", x =>
                {
                    x.Name = "Beste Grill-Rezepte";
                    x.ShortDescription = "Mehr als 100 regionale Favoriten Grill-Rezepte getestet und und für den Freiluft-Koch perfektioniert";
                    x.FullDescription = "<p>Bei einer kurvenreichen Reise quer durchs Land entdecken Sie Grillhütten mit Angeboten wie zart geräuchertem Baltimore Pit Beef und saftigen St. Louis Schweinesteaks. Um Ihnen das Beste dieser verborgenen Juwelen zusammen mit all den Klassikern zu bringen, haben die Redakteure des Cook's Country-Magazins das Land durchkämmt und dann ihre Favoriten getestet und perfektioniert. Große und kleine HEre-Traditionen werden in den Hinterhof gebracht, von Hawaiis Rotisserie-Favoriten, dem goldfarbenen Huli Huli Chicken, bis hin zu den 'fall-off-the-bone' Chicago Barbecued Ribs. In Kansas City dreht sich alles um die Soße, und für unsere frechen Kansas City Sticky Ribs haben wir ein überraschend inhaltsstoffreiches Wurzelbier gefunden. Wir nehmen auch die besten Seiten in Angriff.</p>" +
                    "<p>Sie wissen nicht, wo oder wie Sie anfangen sollen? Dieses Kochbuch beginnt mit einer leicht verständlichen Fibel, die Neueinsteiger begeistern wird. Egal, ob Sie die Menge unterhalten oder einfach nur lernen wollen, perfekte Burger zu machen, Best Grilling Recipes zeigt Ihnen den Weg.</p>";
                })
                .Alter("Cooking for Two", x =>
                {
                    x.Name = "Kochen für Zwei";
                    x.ShortDescription = "Mehr als 200 narrensichere Rezepte für Wochenenden und besondere Anlässe (Hardcover)";
                    x.FullDescription = "<p> In 'Kochen für Zwei' war es das Ziel der Testküche, traditionelle Rezepte so zurechtzuschneiden, dass sie nur zu zweit serviert werden können - mit maßgeschneiderten Kochtechniken und cleveren Einkaufstipps, die die Verschwendung von Lebensmitteln und Geld reduzieren. Großartige Lasagne beginnt ihren Glanz zu verlieren, wenn man die Reste am vierten Tag in Folge isst. Es mag zwar offensichtlich erscheinen, dass ein Rezept für vier Personen einfach halbiert werden kann, um zu funktionieren, aber unsere Tests haben bewiesen, dass dies nicht immer der Fall ist; das Kochen mit kleineren Mengen von Zutaten erfordert oft unterschiedliche Zubereitungstechniken, Garzeit, Temperatur und den Anteil der Zutaten. Dies traf insbesondere zu, als wir an verkleinerten Desserts arbeiteten; Backen ist eine unnachgiebige Wissenschaft, in der jede Änderung der Rezeptmengen oft Änderungen der Backzeiten und -temperaturen erfordert.</p>" +
                    "<p>Fester Einband: 352 Seiten<br/>Herausgeber: Amerikas Testküche(Mai 2009)<br/>Sprachen: Deutsch, Englisch<br /><br />ISBN - 10: 1933615435<br />ISBN - 13: 978 - 1933615431 </p>";
                })
                .Alter("Car of superlatives", x =>
                {
                    x.Name = "Autos der Superlative: Die Stärksten, die Ersten, die Schönsten, Die Schnellsten";
                    x.ShortDescription = "Gebundene Ausgabe";
                    x.FullDescription = "<p>Für manche ist das Auto nur ein nützliches Fortbewegungsmittel.<br> Für alle anderen gibt es 'Autos - Das ultimative Handbuch' des Technik-Kenners Michael Dörflinger. Mit authentischen Bildern, allen wichtigen Daten und jeder Menge Infos präsentiert es die schnellsten, die innovativsten, die stärksten, die ungewöhnlichsten und die erfolgreichsten Exemplare der Automobilgeschichte. Ein umfassendes Handbuch zum gezielten Nachschlagen und ausgiebigen Schmökern. </p>";
                })
                .Alter("Picture Atlas Motorcycles", x =>
                {
                    x.Name = "Bildatlas Motorräder: Mit mehr als 350 brillanten Abbildungen";
                    x.ShortDescription = "Gebundene Ausgabe";
                    x.FullDescription = "<p> Motorräder stehen wie kein anderes Fortbewegungsmittel für den großen Traum von Freiheit und Abenteuer. Dieser reich illustrierte Bildatlas porträtiert in brillanten Farbfotografien und informativen Texten die berühmtesten Zweiräder der Motorradgeschichte weltweit. Von der urtümlichen Dampfmaschine unter dem Fahrradsattel des ausgehenden 19. Jahrhunderts bis hin zu den kraftstrotzenden, mit modernster Elektronik und Computertechnik ausgestatteten Superbikes unserer Tage zeichnet er ein eindrucksvolles Bild der Entwicklung und Fabrikation edler und rasanter Motorräder. Dem Mythos des motorisierten Zweirads wird dabei ebenso nachgegangen wie dem Motorrad als modernem Lifestyle-Produkt unserer Zeit. Länderspezifische Besonderheiten, firmenhistorische Hintergrundinformationen sowie spannende Geschichten und Geschichtliches über die Menschen, die eine der wegweisendsten Erfindungen der letzten Jahrhunderte vorantrieben und weiterentwickelten, machen diesen umfangreichen Bildband zu einem unvergleichlichen Nachschlagewerk für jeden Motorradliebhaber und Technikbegeisterten. </p> <p> • Umfassende Geschichte der legendärsten Modelle aller bedeutenden Motorradhersteller weltweit<br> • Mit mehr als 350 brillanten Farbaufnahmen und fesselnden Hintergrundtexten<br> • Mit informativen Zeichnungen, beeindruckenden Detailaufnahmen und erläuternden Info-Kästen<br> </p> <p> Inhalt • 1817 1913: Die Anfänge einer Erfolgsgeschichte<br> • 1914 1945: Massenmobilität<br> • 1946 1990: Kampf um den Weltmarkt<br> • Ab 1991: Das moderne Motorrad<br> • Kultobjekt Motorrad: Von der Fortbewegung zum Lifestyle<br> </p>";
                })
                .Alter("The Car Book", x =>
                {
                    x.Name = "Das Auto-Buch. Die große Chronik mit über 1200 Modellen";
                    x.ShortDescription = "Gebundene Ausgabe";
                    x.FullDescription = "<p> Marken, Modelle, Meilensteine<br> Das Auto - für manche ein Gebrauchsgegenstand, für andere Ausdruck des Lebensstils, Kultobjekt und große Leidenschaft. Nur wenige Erfindungen haben das Leben so verändert wie die des Automobils vor gut 125 Jahren - ein Grund mehr für diese umfangreiche Chronik. Das Auto-Buch lässt die Geschichte des Automobils lebendig werden. Es stellt über 1200 wichtige Modelle vor - von Karl Benz' Motorwagen über legendäre Kultautos bis zu modernsten Hybridfahrzeugen. Es erklärt die Meilensteine der Motortechnik und porträtiert die großen Marken und ihre Konstrukteure. Steckbriefe vom Kleinwagen bis zur Limousine und schicken Rennwagen jeder Epoche laden zum Stöbern und Entdecken ein. Der umfassendste und bestbebildert Bildband auf dem Markt - darüber freut sich jeder Autoliebhaber! </p> <p> Gebundene Ausgabe: 360 Seiten<br> Verlag: Dorling Kindersley Verlag (27. September 2012)<br> Sprache: Deutsch<br> ISBN-10: 3831022062<br> ISBN-13: 978-3831022069<br> Größe und/oder Gewicht: 30,6 x 25,8 x 2,8 cm<br> </p>";
                })
                .Alter("Fast Cars", x =>
                {
                    x.Name = "Fast Cars, Bildkalender 2013";
                    x.ShortDescription = "Spiralbindung";
                    x.FullDescription = "<p> Großformat: 48,5 x 34 cm.<br> Dieser imposante Bildkalender mit silberner Ringbindung begeistert mit eindrucksvollen Aufnahmen von exklusiven Sportwagen. Wer Autos nicht nur als reine Nutzfahrzeuge begreift, findet hier die begehrtesten Statussymbole überhaupt: Die schnellen Fahrzeuge sind wirkungsvoll auf den gestochen scharfen, farbintensiven Fotos in Szene gesetzt und vermitteln Freiheit, Geschwindigkeit, Stärke und höchste technische Vollkommenheit. </p> <p> Angefangen vom 450 PS-starken Maserati GranTurismo MC Stradale über den stilvoll-luxuriösen Aston Martin Virage Volante bis zu dem nur in geringen Stückzahlen produzierten Mosler MT900S Photon begleiten die schnellen Flitzer mit Stil und Eleganz durch die Monate. Neben dem Kalendarium lenkt ein weiteres Foto den Blick auf sehenswerte Details. Dazu gibt es die wesentlichen Informationen zu jedem Sportwagen in englischer Sprache. Nach Ablauf des Jahres sind die hochwertigen Fotos eingerahmt ein absoluter Blickfang an der Wand eines jeden Liebhabers schneller Autos. Auch als Geschenk ist dieser schöne Jahresbegleiter wunderbar geeignet. 12 Kalenderblätter, neutrales und dezent gehaltenes Kalendarium. Gedruckt auf Papier aus nachhaltiger Forstwirtschaft. </p> <p> Für Freunde von luxuriösen Oldtimern ebenfalls bei ALPHA EDITION erhältlich: der großformatige Classic Cars Bildkalender 2013: ISBN 9783840733376. </p> <p> Produktinformation<br> Spiralbindung: 14 Seiten<br> Verlag: Alpha Edition (1. Juni 2012)<br> Sprache: Deutsch<br> ISBN-10: 3840733383<br> ISBN-13: 978-3840733383<br> Größe und/oder Gewicht: 48,8 x 34,2 x 0,6 cm<br> </p>";
                })
                .Alter("Motorcycle Adventures", x =>
                {
                    x.Name = "Motorrad-Abenteuer: Fahrtechnik für Reise-Enduros";
                    x.ShortDescription = "Gebundene Ausgabe";
                    x.FullDescription = "<p> Moderne Reise-Enduros sind ideale Motorräder für eine Abenteuerreise. Ihre Technik ist jedoch komplex, ihr Gewicht beträchtlich. Das Fahrverhalten verändert sich je nach Zuladung und Strecke. Bevor die Reise losgeht, sollte man unbedingt ein Fahrtraining absolvieren. <br> Dieses hervorragend illustrierte Praxisbuch zeigt anhand vieler aussagekräftiger Serienfotos das richtige Fahren im Gelände in Sand und Schlamm, auf Schotter und Fels mit Gepäck und ohne. Neben dem Fahrtraining werden zahlreiche Informationen und Tipps zur Auswahl des richtigen Motorrades, zur Reiseplanung und zu praktischen Fragen unterwegs gegeben. </p>";
                })
                .Alter("The Prisoner of Heaven: A Novel", x =>
                {
                    x.Name = "Der Gefangene des Himmels";
                    x.ShortDescription = "Gebundene Ausgabe";
                    x.FullDescription = "<p>Der Gefangene des Himmels ist ein Roman des spanischen Autors Carlos Ruiz Zafón. </p><p>Er erschien 2011 bei Planeta S.A. in Barcelona unter dem Titel El prisionero del cielo.</p><p> Die deutsche Übersetzung stammt von Peter Schwaar und erschien 2012 im S. Fischer Verlag Frankfurt/Main. Der Roman ist der dritte Teil der Romantetralogie Friedhof der vergessenen Bücher, die noch die Bände Der Schatten des Windes, Das Spiel des Engels und Das Labyrinth der Lichter umfasst. Die wichtigsten Personen sind aus den beiden vorangegangenen Bänden bereits vertraut. Der dritte Roman beschreibt ihr Leben in den Jahren 1957–60 sowie in Rückblenden in den Jahren 1939–41./<p>";
                })

                #endregion Category Books

                // Category Computer is not implemented in shop yet > Add product(s) to InvariantSeedData.Products
                #region Category Computer

                .Alter("Dell Inspiron One 23", x =>
                {
                    x.ShortDescription = "Dieser 58 cm (23'')-All-in-One-PC mit Full HD, Windows 8 und leistungsstarken Intel® Core™ Prozessoren der dritten Generation ermöglicht eine praktische Interaktion mit einem Touchscreen.";
                    x.FullDescription = "<p>Extrem leistungsstarker All-in-One PC mit Windows 8, Intel® Core™ i7 Prozessor, riesiger 2TB Festplatte und Blu-Ray Laufwerk.  </p>  <p>  Intel® Core™ i7-3770S Prozessor ( 3,1 GHz, 6 MB Cache) Windows 8 64bit , Deutsch<br> 8 GB1 DDR3 SDRAM bei 1600 MHz<br> 2 TB-Serial ATA-Festplatte (7.200 U/min)<br> 1GB AMD Radeon HD 7650<br> </p>";
                    x.Price = 589.00M;
                    x.DeliveryTime = _defaultDeliveryTime;
                    x.ManageInventoryMethod = ManageInventoryMethod.DontManageStock;
                    x.OrderMinimumQuantity = 1;
                    x.OrderMaximumQuantity = 10000;
                    x.StockQuantity = 10000;
                    x.NotifyAdminForQuantityBelow = 1;
                    x.AllowBackInStockSubscriptions = false;
                    x.Published = true;
                    x.IsShipEnabled = true;
                })
                .Alter("Dell Optiplex 3010 DT Base", x =>
                {
                    x.ShortDescription = "SONDERANGEBOT: Zusätzliche 50 € Rabatt auf alle Dell OptiPlex Desktops ab einem Wert von 549 €. Online-Coupon: W8DWQ0ZRKTM1, gültig bis 4.12.2013";
                    x.FullDescription = "<p>Ebenfalls im Lieferumfang dieses Systems enthalten</p> <p> 1 Jahr Basis-Service - Vor-Ort-Service am nächsten Arbeitstag - kein Upgrade ausgewählt Keine Asset-Tag erforderlich</p> <p> Die folgenden Optionen sind in Ihren Auftrag aufgenommene Standardauswahlen.<br> German (QWERTZ) Dell KB212-B QuietKey USB Keyboard Black<br> X11301001<br> WINDOWS LIVE<br> OptiPlex™ Bestellung - Deutschland<br> OptiPlex™ Intel® Core™ i3 Aufkleber<br> Optische Software nicht erforderlich, Betriebssystemsoftware ausreichend<br> </p>";
                    x.Price = 419.00M;
                    x.DeliveryTime = _defaultDeliveryTime;
                    x.ManageInventoryMethod = ManageInventoryMethod.DontManageStock;
                    x.OrderMinimumQuantity = 1;
                    x.OrderMaximumQuantity = 10000;
                    x.StockQuantity = 10000;
                    x.NotifyAdminForQuantityBelow = 1;
                    x.AllowBackInStockSubscriptions = false;
                    x.Published = true;
                    x.IsShipEnabled = true;
                })
                .Alter("Acer Aspire One 8.9", x =>
                {
                    x.Name = "Acer Aspire One 8.9\" Mini-Notebook Case - (Schwarz)";
                    x.ShortDescription = "Acer definiert mit dem Aspire One mobile Konnektivität neu, dem revolutionären Spaß und Power Netbook in der zierlichen 8.9\" Größe. ";
                    x.FullDescription = "<p> Von der Betätigung des Powerknopfes an, ist das Aspire One in nur wenigen Sekunden betriebsbereit. Sobald an, ist die Arbeit sehr einfach: ein Heimarbeitsplatz der die heute benötigten vier Bereiche abdeckt, verbunden bleiben, arbeiten, spielen und Ihr Leben unterwegs organisieren. Und der Aspire One ist etwas Besonderes, Sie können alles so individualisieren das es für Sie das Richtige ist. Schnell, einfach und unbeschreiblich schick. Ihr Style ist Ihre Unterschrift. Es ist Ihre Identität, Ihre Persönlichkeit und Ihre Visitenkarte. Ihr Style zeigt Ihrer Umwelt wie Sie sind und wie Sie Ihr Leben leben, online und offline. Das alles benötigen Sie, um Sie selbst zu sein. Ihr Style kommt in verschiedenen Farben, jede mit einem individuellen Charakter. Kleiner als ein durchschnittliches Tagebuch, das Aspire One bringt Freiheit in Ihre Hände. </p> <p> Allgemein<br> Betriebssystem: Microsoft Windows XP Home Edition, Linux Linpus Lite <br> Herstellergarantie: 1 Jahr Garantie<br> Systemtyp: Netbook<br> MPN: LU.S080B.069, LU.S050B.081, LU.S040B.079, LU.S090B.079, LU.S040B.198, LU.S040A.048, LU.S050A.050, LU.S050B.080, LU.S040B.078, 099915639, LU.S050A.074, LU.S360A.203, LU.S450B.030, LU.S050B.159<br> Speicher<br> RAM: 1 GB ( 1 x 512 MB + 512 MB (integriert) ), 1 GB<br> Max. unterstützter RAM-Speicher: 1.5 GB<br> Technologie: DDR2 SDRAM<br> Geschwindigkeit: 533 MHz   <br> Formfaktor: SO DIMM 200-polig  <br> Anz. Steckplätze: 1<br> Leere Steckplätze: 0, 1<br> Display<br> Typ: 22.6 cm ( 8.9\" )<br> Auflösung: 1024 x 600 ( WSVGA )<br> Breitwand: Ja<br> LCD-Hintergrundbeleuchtung: LED-Hintergrundbeleuchtung     <br> Farbunterstützung: 262.144 Farben, 24 Bit (16,7 Millionen Farben)<br> Besonderheiten: CrystalBrite<br> Batterie<br> Betriebszeit: Bis zu 7 Stunden, Bis zu 3 Stunden<br> Kapazität: 2600 mAh, 2200 mAh<br> Technologie: 6 Zellen Lithium-Ionen, 3 Zellen Lithium-Ionen, Lithium-Ionen<br> Herstellergarantie<br> Service & Support:<br> Reisegarantie - 1 Jahr, Begrenzte Garantie - 1 Jahr, Internationale Garantie - 1 Jahr<br> Begrenzte Garantie - 1 Jahr, Reisegarantie - 1 Jahr<br> Begrenzte Garantie - 1 Jahr, Begrenzte Garantie - 1 Jahr<br> Reisegarantie - 1 Jahr<br> Navigation<br>Empfänger: GPS<br></p>";
                    x.Price = 210.60M;
                    x.DeliveryTime = _defaultDeliveryTime;
                    x.ManageInventoryMethod = ManageInventoryMethod.DontManageStock;
                    x.OrderMinimumQuantity = 1;
                    x.OrderMaximumQuantity = 10000;
                    x.StockQuantity = 10000;
                    x.NotifyAdminForQuantityBelow = 1;
                    x.AllowBackInStockSubscriptions = false;
                    x.Published = true;
                    x.IsShipEnabled = true;
                })

                #endregion Category Computer 

                #region Category Apple

                .Alter("iPhone Plus", x =>
                {
                    x.ShortDescription = "Das ist iPhone. Das iPhone macht vieles von dem, was das iPhone zum iPhone macht, noch einmal viel besser. Es hat fortschrittliche neue Kamerasysteme. Die beste Leistung und Batterielaufzeit, die ein iPhone je hatte. Beeindruckende Stereo-Lautsprecher. Das hellste iPhone Display. Mit noch mehr Farben. Schutz vor Spritzwasser. Und es sieht so großartig aus, wie es ist. Das ist das iPhone.";
                    x.FullDescription = "";
                })
                .Alter("AirPods", x =>
                {
                    x.ShortDescription = "Einfach. Kabellos. Magisch.Du nimmst sie aus dem Case und sie sind bereit für all deine Geräte. Du steckst sie in die Ohren und sie verbinden sich sofort. Du sprichst hinein und deine Stimme ist klar zu verstehen. Die neuen AirPods. Einfachheit und Technologie, verbunden wie nie zuvor. Für ein Ergebnis, das einfach magisch ist.";
                    x.FullDescription = "<p>  <br />  Die AirPods verändern für immer, wie du Kopfhörer verwendest. Wenn du deine AirPods aus dem Ladecase nimmst, schalten sie sich ein und verbinden sich mit deinem iPhone, iPad, Mac oder deiner Apple Watch.(1) Audio wird automatisch wiedergegeben, sobald du sie im Ohr hast, und pausiert, wenn du sie herausnimmst. Um die Lautstärke anzupassen, den Song zu wechseln, jemanden anzurufen oder dir den Weg sagen zu lassen, aktiviere einfach Siri mit einem Doppeltipp.  <br />  Die AirPods werden vom speziell entwickelten Apple W1 Chip gesteuert und erkennen durch optische Sensoren und einen Beschleunigungssensor, ob sie in deinem Ohr sind. Der W1 Chip leitet die Audiosignale automatisch weiter und aktiviert das Mikrofon – egal, ob du beide oder nur einen verwendest. Und wenn du gerade telefonierst oder mit Siri sprichst, filtert ein weiterer Beschleunigungsmesser mit wellenbündelnden Mikrofonen Hintergrundgeräusche heraus und hebt deine Stimme hervor. Da der extrem energieeffiziente W1 Chip die Batterieleistung so gut steuert, bieten die AirPods eine einzigartige Wiedergabedauer von bis zu 5 Std. pro Aufladung.(2) Und dank des Ladecase, das mehrere zusätzliche Aufladungen für insgesamt über 24 Std. Wiedergabe bietet, halten sie locker bei allem mit, was du so machst.(3) Schnell mal aufladen? Nach nur 15 Minuten im Ladecase kannst du 3 Stunden Musik hören.(4)</p><p><strong>Technische Daten</strong>  <br />  Bluetooth  <br />  Drahtlose Technologien  <br />  <strong>Gewicht</strong>  <br />  AirPods (jeweils): 4 g  <br />  Ladecase: 38 g  <br />  <strong>Abmessungen</strong>  <br />  AirPods (jeweils): 16,5 x 18,0 x 40,5 mm  <br />  Ladecase: 44,3 x 21,3 x 53,5 mm  <br />  <strong>Anschlüsse</strong>  <br />  AirPods: Bluetooth  <br />  Ladecase: Lightning Connector  <br />  <strong>AirPods Sensoren (jeweils):</strong>  <br />  Zwei Beamforming Mikrofone  <br />  Zwei optische Sensoren  <br />  Bewegungsbeschleunigungsmesser  <br />  Stimmbeschleunigungsmesser  <br />  <strong>Stromversorgung und Batterie</strong>  <br />  AirPods mit Ladecase: Mehr als 24 Stunden Wiedergabe, (3) bis zu 11 Stunden Sprechdauer(6)  <br />  AirPods (einzelne Ladung): Bis zu 5 Stunden Wiedergabe,(2) bis zu 2 Stunden Sprechdauer(5)  <br />  15 Minuten im Case entspricht 3 Stunden Wiedergabe(4) oder mehr als 1 Stunde Sprechdauer(7)</p><p></p>";
                })
                .Alter("Ultimate Apple Pro Hipster Bundle", x =>
                {
                    x.ShortDescription = "Sparen Sie mit diesem Set 5%!";
                    x.FullDescription = "<p>Als Apple-Fan und Hipster ist es Ihr Grundbedürfnis immer die neusten Apple-Produkte zu haben.&nbsp;  <br />  Damit Sie nicht vier Mal im Jahr vor dem Apple-Store nächtigen müssen, abonnieren Sie einfach das <strong>Ultimate Apple Pro Hipster Set im Jahres Abo</strong>!</p><p></p>";
                })
                .Alter("9,7' iPad", x =>
                {
                    x.ShortDescription = "Macht einfach Spaß. Lernen, spielen, surfen, kreativ werden. Mit dem iPad hast du ein unglaubliches Display, großartige Leistung und Apps für alles, was du gerne machst. Überall. Einfach. Magisch.";
                    x.FullDescription = "<ul>  <li>9,7'' Retina Display mit True Tone und Antireflex-Beschichtung (24,63 cm Diagonale)</li>  <li>A9X Chip der dritten Generation mit 64-Bit Desktoparchitektur</li>  <li>Touch ID Fingerabdrucksensor</li></ul>";
                })
                .Alter("Watch Series 2", x =>
                {
                    x.Name = "Watch Series 2";
                })

                #endregion Category Apple

                #region Category Digital Goods & Instant Downloads

                .Alter("Antonio Vivaldi: spring", x =>
                {
                    x.Name = "Antonio Vivaldi: Der Frühling";
                    x.ShortDescription = "MP3, 320 kbit/s";
                    x.FullDescription = "<p>Antonio Vivaldi: Der Fr&uuml;hling</p> <p><b>Antonio Lucio Vivaldi</b><span>&nbsp;(*&nbsp;</span>4. M&auml;rz<span>&nbsp;</span>1678<span>&nbsp;in&nbsp;</span>Venedig<span>; &dagger;&nbsp;</span>28. Juli<span>&nbsp;</span>1741<span>&nbsp;in&nbsp;</span>Wien<span>) war ein venezianischer&nbsp;</span>Komponist<span>&nbsp;und&nbsp;</span>Violinist<span>&nbsp;im&nbsp;</span>Barock<span>.</span></p> <p><b>Die vier Jahreszeiten</b>&nbsp;(italienisch&nbsp;<span lang=\"it\" class=\"lang\"><i>Le quattro stagioni</i></span>) hei&szlig;t das wohl bekannteste Werk&nbsp;Antonio Vivaldis. Es handelt sich um vier&nbsp;Violinkonzerte&nbsp;mit au&szlig;ermusikalischen&nbsp;Programmen; jedes Konzert portr&auml;tiert eine&nbsp;Jahreszeit. Dazu ist den einzelnen Konzerten jeweils ein &ndash; vermutlich von Vivaldi selbst geschriebenes &ndash;&nbsp;Sonett&nbsp;vorangestellt; fortlaufende Buchstaben vor den einzelnen Zeilen und an den entsprechenden Stellen in der&nbsp;Partitur&nbsp;ordnen die verbale Beschreibung der Musik zu.</p> <p>Vivaldi hatte bereits zuvor immer wieder mit au&szlig;ermusikalischen Programmen experimentiert, die sich h&auml;ufig in seinen Titeln niederschlagen; die genaue Ausdeutung von Einzelstellen der Partitur ist aber f&uuml;r ihn ungew&ouml;hnlich. Seine Erfahrung als virtuoser Geiger erlaubte ihm den Zugriff auf besonders wirkungsvolle Spieltechniken; als Opernkomponist hatte er einen starken Sinn f&uuml;r Effekte entwickelt; beides kam ihm hier zugute.</p> <p>Wie der Titel bereits nahelegt, werden vor allem Naturerscheinungen imitiert &ndash; sanfte Winde, heftige St&uuml;rme und Gewitter sind Elemente, die in allen vier Konzerten auftreten. Hinzu kommen verschiedene Vogelstimmen und sogar ein Hund, weiter menschliche Bet&auml;tigungen wie etwa die Jagd, ein Bauerntanz, das Schlittschuhlaufen einschlie&szlig;lich Stolpern und Hinfallen bis hin zum schweren Schlaf eines Betrunkenen.</p> <p>Das Werk stammt aus dem Jahre 1725 und ist in zwei Druckausgaben erhalten, die offenbar mehr oder weniger zeitgleich in Amsterdam und Paris erschienen.</p> <p><span><br /></span></p>";
                })
                .Alter("Ludwig van Beethoven: Für Elise", x =>
                {
                    x.Name = "Ludwig van Beethoven: Für Elise";
                    x.ShortDescription = "Ludwig van Beethoven: Für Elise. Eine von Beethovens populärsten Kompositionen.";
                    x.FullDescription = "<p> Die früheste, 1973 bekannt gewordene Fassung der „Kernmelodie“[5] notierte Beethoven 1808 in ein Skizzenbuch zur Pastorale. Einige aus dem Skizzenbuch herausgelöste Seiten bilden heute das Autograph Mus. ms. autograph. Beethoven Landsberg 10 der Staatsbibliothek Preußischer Kulturbesitz in Berlin. Die Melodie, die eindeutig als Kern des Klavierstückes WoO 59 zu erkennen ist,[2] befindet sich in den Zeilen 6 und 7 der Seite 149. Es handelt sich um eine einstimmige, sechzehntaktige Melodie, die sich besonders bei den Auftakten des Mittelteiles und bei den Schlusswendungen der Takte 7 und 15 sowie durch das Fehlen des zweitaktigen Orgelpunktes auf E von späteren Fassungen unterscheidet.[2] Diese Melodie nahm Beethoven 1810 wieder auf, modifizierte sie und fügte ihr weitere Teile hinzu. Das geschah in Beethovens Handschrift BH 116[6] und vermutlich auch in dem Autograph, das zu Babette Bredl gelangte und von Ludwig Nohl abgeschrieben und 1867 erstmals veröffentlicht wurde.[7][8] </p> <p> In BH 116 lassen sich drei Arbeitsphasen erkennen: eine erste Niederschrift im Jahre 1810, Korrekturen daran von 1810 und eine Bearbeitung aus dem Jahre 1822. Die Bearbeitung von 1822 hatte das Ziel, das Klavierstück in eine für eine Veröffentlichung taugliche Fassung zu bringen. Es sollte als No 12 den Schluss eines Zyklus von Bagatellen bilden. Dieser Plan wurde allerdings nicht ausgeführt.[9] 1822 überschrieb Beethoven das Klavierstück mit „molto grazioso“. Er verschob die Begleitfiguren des A-Teils in der linken Hand um ein Sechzehntel nach rechts und entlastete dabei den Taktanfang. Außerdem führte er die Begleitfigur teilweise in eine tiefere Lage und weitete damit den Klang aus.[10] Im Teil B kehrte Beethoven zu einer melodisch und rhythmisch komplizierteren, 1810 verworfenen Fassung zurück. Den vermutlichen Gesamtaufbau des Klavierstückes ließ er nicht völlig unangetastet und fügte vier bisher ungenutzte Takte als Überleitung zum Teil B ein. Vier 1822 notierte Einleitungstakte, die zum A-Teil passen, strich er dagegen wieder.[11] Bei der Anweisung für die Reprise des letztmals wiederkehrenden Teiles A schrieb er „una corda“ vor, was sich auf diesen Teil selbst beziehen kann oder nur auf den neu entworfenen, dreitaktigen, wahrscheinlich akkordisch gedachten, aber nur einstimmig notierten Schluss.[12] Eine vollständige Fassung als Resultat der Bearbeitung von 1822 stellte Beethoven nicht her.[13][14] </p>";
                })
                .Alter("Ebook 'Stone of the Wise' in 'Lorem ipsum'", x =>
                {
                    x.Name = "Ludwig van Beethoven: Für Elise";
                    x.ShortDescription = "E-Book, 465 pages";
                })

                #endregion Category Digital Goods & Instant Downloads

                #region Category Watches

                .Alter("Certina DS Podium Big Size", x =>
                {
                    x.Name = "Certina DS Podium Big Size Herrenchronograph";
                    x.ShortDescription = "Die Transocean Chronograph interpretiert die sachliche Ästhetik klassischer Chronografen der 1950er- und 1960er-Jahre in einem entschieden zeitgenössischen Stil neu. ";
                    x.FullDescription = "<p><strong>Produktbeschreibung</strong></p> <ul> <li>Artikelnr.: 3528 C001.617.26.037.00</li> <li>Certina DS Podium Big Size Herrenchronograph</li> <li>Schweizer ETA Werk</li> <li>Silberfarbenes Edelstahlgeh&auml;use mit schwarzer L&uuml;nette</li> <li>Wei&szlig;es Zifferblatt mit silberfarbenen Ziffern und Indizes</li> <li>Schwarzes Lederarmband mit Faltschliesse</li> <li>Kratzfestes Saphirglas</li> <li>Datumsanzeige</li> <li>Tachymeterskala</li> <li>Chronograph mit Stoppfunktion</li> <li>Durchmesser: 42 mm</li> <li>Wasserdichtigkeits -Klassifizierung 10 Bar (nach ISO 2281): Perfekt zum Schwimmen und Schnorcheln</li> <li>100 Tage Niedrigpreisgarantie, bei uhrzeit.org kaufen Sie ohne Preisrisiko!</li> </ul>";
                })
                .Alter("TRANSOCEAN CHRONOGRAPH", x =>
                {
                    x.ShortDescription = "Die Transocean Chronograph interpretiert die sachliche Ästhetik klassischer Chronografen der 1950er- und 1960er-Jahre in einem entschieden zeitgenössischen Stil neu. ";
                    x.FullDescription = "<p>Die Transocean Chronograph interpretiert die sachliche Ästhetik klassischer Chronografen der 1950er- und 1960er-Jahre in einem entschieden zeitgenössischen Stil neu. In ihrem auf das Wesentliche reduzierten, formschönen Gehäuse arbeitet das vollständig in den Breitling-Ateliers konzipierte und hergestellte Hochleistungskaliber 01.</p><p></p><table>  <tbody>    <tr>      <td style='width: 162px;'>Kaliber      </td>      <td style='width: 205px;'>Breitling 01 (Manufakturkaliber)      </td>    </tr>    <tr>      <td style='width: 162px;'>Werk      </td>      <td style='width: 205px;'>Mechanisch, Automatikaufzug      </td>    </tr>    <tr>      <td style='width: 162px;'>Gangreserve      </td>      <td style='width: 205px;'>Min. 70 Stunden      </td>    </tr>    <tr>      <td style='width: 162px;'>Chronograf      </td>      <td style='width: 205px;'>1/4-Sekunde, 30 Minuten, 12 Stunden      </td>    </tr>    <tr>      <td style='width: 162px;'>Halbschwingungen      </td>      <td style='width: 205px;'>28 800 a/h      </td>    </tr>    <tr>      <td style='width: 162px;'>Rubine      </td>      <td style='width: 205px;'>47 Rubine      </td>    </tr>    <tr>      <td style='width: 162px;'>Kalender      </td>      <td style='width: 205px;'>Fenster      </td>    </tr>  </tbody></table>";
                })
                .Alter("Tissot T-Touch Expert Solar", x =>
                {
                    x.ShortDescription = "Der Strahlenkranz der Tissot T-Touch Expert Solar auf dem Zifferblatt sorgt einerseits dafür, dass die mit Super-LumiNova® beschichteten Indexe und Zeiger im Dunkeln leuchten und lädt andererseits den Akku der Uhr. Dieses Modell ist in jeder Beziehung ein Kraftpaket.";
                    x.FullDescription = "<p>Das T-Touch Expert Solar ist ein wichtiges neues Modell im Tissot Sortiment.</p><p>Tissots Pioniergeist ist das, was 1999 zur Schaffung von taktilen Uhren geführt hat.</p><p>Heute ist es der erste, der eine Touchscreen-Uhr mit Sonnenenergie präsentiert und seine Position als Marktführer in der taktilen Technologie in der Uhrmacherei bestätigt.</p><p>Extrem gut entworfen, zeigt es saubere Linien in Sport und zeitlose Stücke.</p><p>Angetrieben von Solarenergie mit 25 Features wie Wettervorhersage, Höhenmesser, zweite Zeitzone und Kompass ist es der perfekte Reisebegleiter.</p>";
                })
                .Alter("Seiko Mechanical Automatic SRPA49K1", x =>
                {
                    x.Name = "Seiko Automatikuhr SRPA49K1";
                    x.ShortDescription = "Der perfekte Begleiter für den Alltag! Die formschöne Automatikuhr besticht durch ansprechendes Design und ergänzt stilvoll nahezu jedes Outfit.";
                    x.FullDescription = "<p><strong>Seiko 5 Sport Automatikuhr SRPA49K1 SRPA49</strong></p><p></p><ul>  <li>Unidirektionale drehbare Lünette</li>  <li>Tages- und Datumsanzeige</li>  <li>Siehe durch Fall zurück</li>  <li>100M Wasserresistenz</li>  <li>Edelstahlgehäuse</li>  <li>Automatische Bewegung</li>  <li>24 Juwelen</li>  <li>Kaliber: 4R36</li></ul>";
                })

                #endregion Category Watches

                #region Category Gaming

                .Alter("Playstation 3 Super Slim", x =>
                {
                    x.ShortDescription = "Die Sony PlayStation 3 ist die Multi-Media-Console für die nächste Generation digitalem Home-Entertainment. Mit der Blu-Ray-Technologie genießen Sie Filme in HD.";
                    x.FullDescription = ps3FullDescription;
                })
                .Alter("DUALSHOCK 3 Wireless Controller", x =>
                {
                    x.ShortDescription = "Ausgestattet mit SIXAXIS™ Motion-sensing-Technologie und Drucksensoren für jede Aktionsschaltfläche, bietet die DUALSHOCK®3 wireless Controller für die PlayStation ® 3 die intuitivste Gameplay-Erfahrung.";
                    x.FullDescription = "<ul><li><h4>Gewicht und Maße</h4><ul><li>Größe und Gewicht (ca.) : 27 x 23,5 x 4 cm ; 191 g</li></ul></li></ul>";
                })
                .Alter("Assassin's Creed III", x =>
                {
                    x.ShortDescription = "Eliminieren Sie Ihre Gegner mit einem erweiterten Waffenarsenal, darunter Bögen, Pistolen, Tomahawk und die charakteristische Klinge des Assassinenordens. Erkunden Sie dicht bevölkerte Städte entlang der ausgedehnten und gefährlichen Grenze zur Wildnis, wo es von wilden Tieren nur so wimmelt. Eine ganz neue Spielengine zeigt die Brutalität und Schönheit einer Nation während ihres epischen Kampfes um Unabhängigkeit.";
                    x.FullDescription = "<p>Vor dem Hintergrund der Amerikanischen Revolution im späten 18. Jahrhundert präsentiert Assassin’s Creed III einen neuen Helden: Ratohnhaké:ton, der teils uramerikanischer, teils englischer Abstammung ist. Er nennt sich selbst Connor und wird die neue Stimme der Gerechtigkeit im uralten Krieg zwischen Assassinen und Templern. Der Spieler wird zum Assassinen im Krieg um Freiheit und gegen Tyrannei in der aufwändigsten und flüssigsten Kampferfahrung der Reihe. Assassin’s Creed III umfasst die Amerikanische Revolution und nimmt den Spieler mit auf eine Reise durch das lebhafte, ungezähmte Grenzland, vorbei an geschäftigen Kolonialstädten, bis hin zu den erbittert umkämpften und chaotischen Schlachtfeldern, auf denen George Washingtons Kontinentalarmee mit der eindrucksvollen Britischen Armee zusammenstieß.</p><p>Das 18. Jahrhundert in Nordamerika. Nach mehr als 20 Jahren voller Konflikte stehen die 13 amerikanischen Kolonien und die Britische Krone am Rande eines handfesten Krieges. Die Schlachtlinien werden vorbereitet. Blutvergießen ist unvermeidbar. Aus der Asche dieses brennenden Dorfes wird ein neuer Assassine auferstehen. Als Sohn mohikanischer und britischer Vorfahren wird sein Kampf für Freiheit und Gerechtigkeit in den Wirren der Revolution Gestalt annehmen.</p>";
                })
                .Alter("PlayStation 3 Assassin's Creed III Bundle", x =>
                {
                    x.ShortDescription = "500GB PlayStation®3 Console, 2 × DUALSHOCK®3 wireless controller und Assassin's Creed® III.";
                    x.FullDescription = ps3FullDescription;
                    x.BundleTitleText = "Produktset besteht aus";
                })
                .Alter("PlayStation 4", x =>
                {
                    x.ShortDescription = "In Zusammenarbeit mit einigen der kreativsten Köpfe der Industrie entstanden, bietet die PlayStation® 4 atemberaubende und einzigartige Gaming-Erlebnis.";
                    x.FullDescription = ps4FullDescription;
                })
                .Alter("Playstation 4 Pro", x =>
                {
                    x.ShortDescription = "Die Sony PlayStation 4 Pro ist die Multi-Media-Konsole für die nächste Generation der digitalen Home Entertainment. Es bietet die Blu-ray-Technologie, mit der Sie Filme in High Definition genießen können.";
                    x.FullDescription = ps4FullDescription;
                })
                .Alter("FIFA 17 - PlayStation 4", x =>
                {
                    x.ShortDescription = "Powered by Frostbite";
                    x.FullDescription = "<ul>  <li>Powered by Frostbite: Einer der führenden Game-Engines der Branche, Frostbite liefert authentische, wahrheitsgetreue Action, nimmt Spieler auf neue Fußball-Welten und stellt Fans zu Charakteren voller Tiefe und Emotionen in der FIFA 17 vor.</li>  <li>Die Reise: Zum ersten Mal in der FIFA, lebe deine Geschichte auf und abseits des Platzes als der nächste aufsteigende Star der Premier League, Alex Hunter. Spielen Sie auf jedem Club in der Premier League, für authentische Manager und neben einigen der besten Spieler auf dem Planeten.</li>  <li>Erleben Sie brandneue Welten in der FIFA 17, während Sie sich durch die emotionalen Höhen und Tiefen der Reise bewegen.</li>  <li>Komplette Innovation in der Art und Weise, wie Spieler denken und bewegen, körperlich mit Gegnern interagieren und im Angriff ausführen, bringt euch die volle Kontrolle über jeden Moment auf dem Spielfeld.</li></ul>";
                })
                .Alter("Horizon Zero Dawn - PlayStation 4", x =>
                {
                    x.ShortDescription = "Erleben Sie eine lebendige, üppige Welt, die von geheimnisvollen mechanisierten Kreaturen bewohnt wird";
                    x.FullDescription = "<Ul> <li> Eine üppige Post-Apokalyptische Welt - Wie haben Maschinen diese Welt dominiert und was ist ihr Zweck? Was ist mit der Zivilisation passiert? Scour jede Ecke eines Reiches mit alten Reliquien und geheimnisvollen Gebäuden gefüllt, um Ihre Vergangenheit aufzudecken und die vielen Geheimnisse eines vergessenen Landes zu entdecken. </ Li>  <li> Natur und Maschinen Collide - Horizon Zero Dawn stellt zwei kontrastierende Elemente vor, die eine lebendige Welt mit der wunderschönen Natur reichen und sie mit einer beeindruckenden hochentwickelten Technologie füllen. Diese Ehe schafft eine dynamische Kombination für Erkundung und Gameplay. </ Li> <li> Defy Overwhelming Odds - Die Gründung des Kampfes in Horizon Zero Dawn ist auf die Geschwindigkeit und Schlauheit von Aloy im Vergleich zu der Rohstärke und Größe der Maschinen gebaut. Um einen viel größeren und technologisch überlegenen Feind zu überwinden, muss Aloy jede Unze ihres Wissens, ihrer Intelligenz und ihrer Beweglichkeit nutzen, um jede Begegnung zu überleben. </ Li> <li> Cutting Edge Open World Tech - Atemberaubend detaillierte Wälder, Und atmosphärische Ruinen einer vergangenen Zivilisation verschmelzen in einer Landschaft, die mit wechselnden Wettersystemen und einem vollen Tag / Nacht-Zyklus lebendig ist. </ Li> </ ul>";
                })
                .Alter("LEGO Worlds - PlayStation 4", x =>
                {
                    x.ShortDescription = "Erleben Sie eine Galaxie von Welten, die ganz aus LEGO-Steinen hergestellt wurden.";
                    x.FullDescription = "<Ul>   <Li> Erleben Sie eine Galaxie von Welten, die vollständig aus LEGO-Ziegeln hergestellt wurden. </ Li>   <Li> LEGO Worlds ist eine offene Umgebung von prozessual generierten Welten, die ganz aus LEGO-Steinen bestehen, die man mit LEGO-Modellen frei manipulieren und dynamisch bevölkern kann. </ Li>   <Li> Schaffen Sie alles, was Sie sich vorstellen können, einen Ziegelstein zu einer Zeit, oder verwenden Sie groß angelegte Landschafts-Werkzeuge, um riesige Gebirgszüge zu schaffen und Ihre Welt mit tropischen Inseln zu platzieren. </ Li>   <Li> Entdecken Sie mit Hubschraubern, Drachen, Motorrädern oder sogar Gorillas und entsperren Sie Schätze, die Ihr Gameplay verbessern. </ Li>   <Li> Beobachten Sie Ihre Kreationen durch Charaktere und Kreaturen, die mit Ihnen und einander in unerwarteter Weise interagieren, zum Leben. </ Li></ Ul><P></ P>";
                })
                .Alter("Minecraft - Playstation 4 Edition", x =>
                {
                    x.ShortDescription = "Third-Person Action-Abenteuer Titel Set.";
                    x.FullDescription = "<P>Aufbau! Kunst! Erforschen! </p> <p> Die kritisch gefeierte Minecraft kommt zu PlayStation 4 und bietet größere Welten und größere Distanz als die PS3- und PS-Vita-Editionen. </ P> <p> Erstellen Sie Ihre eigene Welt, dann bauen Sie Erforschen und erobern Wenn die Nacht fällt die Monster erscheinen, so sicher sein, einen Schutz zu errichten, bevor sie ankommen. </ P> <p> Die Welt ist nur durch Ihre Phantasie begrenzt! Größere Welten und größere Distanz als PS3 und PS Vita Editions Beinhaltet alle Features aus der PS3-Version Importieren Sie Ihre PS3 und PS Vita Welten auf die PS4-Bearbeitung. </ P>";
                })
                .Alter("PlayStation 4 Minecraft Bundle", x =>
                {
                    x.ShortDescription = "100GB PlayStation®4 system, 2 × DUALSHOCK®4 wireless controller unf Minecraft für PS4 Edition.";
                    x.FullDescription = "'<ul>  <li>PlayStation 4, die neueste Generation des Entertainment Systems, definiert reichhaltiges und beeindruckendes Gameplay, völlig neu.</li>  <li>Den Kern der PS4 bilden ein leistungsstarker, eigens entwickelter Chip mit acht x86-Kernen (64 bit) sowie ein hochmoderner optimierter Grafikprozessor.</li>  <li>Ein neuer, hochsensibler SIXAXIS-Sensor ermöglicht mit dem DualShock 4 Wireless Controller eine erstklassige Bewegungssteuerung.</li>  <li>Der DualShock 4 bietet als Neuerungen ein Touchpad, eine Share-Taste, einen eingebauten Lautsprecher und einen Headset-Anschluss.</li>  <li>PS4 integriert Zweitbildschirme, darunter PS Vita, Smartphones und Tablets, damit Spieler ihre Lieblingsinhalte überall hin mitnehmen können.</li></ul>";
                })
                .Alter("DUALSHOCK 4 Wireless Controller", x =>
                {
                    x.ShortDescription = "Durch Kombination klassischer Steuerelemente mit innovativen neuen Möglichkeiten des Spielens, ist der Wireless Controller DUALSHOCK® 4 der evolutionäre Controller für eine neue Ära des Gaming.";
                    x.FullDescription = "<div><div><p>Der DualShock 4 Controller bietet einige neue Features, die völlig neue Wege des Spielens ermöglichen und wohlüberlegt mit Unterstützung aus der Entwickler-Community zusammengestellt wurden. Die “Share”-Taste erlaubt es Ihnen ganz einfach, Gameplay in Echtzeit über Streaming-Seiten wie Ustream zu veröffentlichen. Dort können andere Gamer Spiele kommentieren oder sogar direkt beitreten und aushelfen. Daneben können Sie über die “Share”-Taste Bilder oder Videos zu Facebook hochladen. Auf der Vorderseite des DualShock 4 befindet sich eine LED-Leuchte, die in unterschiedlichen Farben erstrahlen kann, um die Farbe des Charakters im Spiel abzubilden und einen Spieler so leicht zu identifizieren. Die Farben können dem Spieler auch nützliche Informationen liefern, zum Beispiel wenn der Charakter im Spiel Schaden nimmt.</p> <p>Der DualShock 4 wurde zusammen mit einem zweiten Peripherie-Gerät entwickelt, einer Kamera (nicht im Lieferumfang enthalten), die die Tiefe der Umgebung vor ihr wahrnehmen kann und die mithilfe der LED-Leuchte die Position des Controllers im dreidimensionalen Raum bestimmen kann.</p> <p>Der DualShock 4 bietet auf seiner Vorderseite ein Touchpad und somit eine neue Input-Methode. Zusätzlich gibt es einen eingebauten Lautsprecher und einen Headset-Anschluss, um hochklassige Soundeffekte aus den Spielen zu übertragen. Mithilfe eines Headsets (nicht im Lieferumfang enthalten) können Sie während des Online-Gamings mit Ihren Freunden chatten und Soundeffekte aus dem Controller hören können. Der DualShock 4 adaptiert die bekannte Form des kabellosen DualShock 3 Controllers und bietet einige entscheidende Verbesserungen:</p> <ul> <li>Ein neuer, hochsensibler SIXAXIS-Sensor ermöglicht eine erstklassige Bewegungssteuerung.</li> <li>Die Dual-Analogsticks wurden verbessert und bieten eine größere Präzision, ein besseres Material auf den Oberflächen sowie eine verbesserte Form, um eine noch genauere Steuerung zu ermöglichen.</li> <li>Die L2/R2-Tasten oben auf dem Controller wurden abgerundet und sind jetzt einfacher und flüssiger zu bedienen.</li> <li>Eine neue “Options”-Taste kombiniert die Funktionen der “Select”- und “Start”-Tasten auf dem DualShock 3 zur Steuerung der Ingame-Menüs.</li> </ul> <h4>Technische Spezifikationen</h4> <ul> <li>Außenabmessungen Ca. 162 × 52 × 98 mm (B × H × T) (vorläufig) </li><li>Gewicht Ca. 210 g (vorläufig) </li><li><b>Tasten / Schalter:</b> PS-Taste, SHARE-Taste, OPTIONS-Taste, Richtungstasten (oben/unten/links/rechts), Aktionstasten (Dreieck, Kreis, Kreuz, Quadrat), R1/L1/R2/L2-Taste, linker Stick / L3-Taste, rechter Stick / R3-Taste, Pad-Taste </li><li><b>Touchpad:</b> 2-Punkt-Touchpad, Klick-Mechanismus, kapazitiv</li> <li><b>Bewegungssensor:</b> Sechsachsiges Motion-Sensing-System (dreiachsiges Gyroskop, dreiachsiger Beschleunigungssensor) </li><li><b>Sonstige Funktionen:</b> Lichtbalken, Vibration, integrierter MonoLautsprecher</li> <li><b>Anschluss:</b> USB (Micro B), Erweiterungs-Port, Stereo-Kopfhörerbuchse </li><li><b>Wireless-Kommunikation:</b> Bluetooth 2.1+EDR</li> <li><b>Batterie:</b> Typ Eingebauter Lithium-Ionen-Akku</li> <li><b>Spannung:</b> 3,7 V Gleichspannung (vorläufig)</li> <li><b>Kapazität:</b> 1000 mAh (vorläufig)</li> <p></p> <p> <i>Kurzfristige Änderungen des Herstellers vorbehalten.</i> </p> </ul></div><div></div></div>";
                })
                .Alter("PlayStation 4 Camera", x =>
                {
                    x.Name = "PlayStation 4 Kamera";
                    x.ShortDescription = "Eine Kamera, die die Tiefe der Umgebung vor ihr wahrnehmen kann und die mithilfe der LED-Leuchte die Position des Controllers im dreidimensionalen Raum bestimmen kann.";
                    x.FullDescription = "<p>Die neue Kamera besitzt vier Mikrofone mit der eine präzise Geräusch-Erkennung und -Ortung möglich ist und wird den PlayStation Move-Motion-Controller (nicht im Lieferumfang enthalten) mit einer größeren Präzision als je zuvor unterstützen.</p><p><ul><li><b>Farbe:</b> Jet Black</li> <li><b>Außenabmessungen:</b> Ca. 186 × 27 × 27 mm (B × H × T) (vorläufig)</li> <li><b>Gewicht:</b> Ca. 183 g (vorläufig)</li> <li><b>Videopixel:</b> (Maximum) 2 × 1280 × 800 Pixel</li> <li><b>Videobildrate:</b> 1280×800 Pixel bei 60 fps, 640×400 Pixel bei 120 fps, 320×192 Pixel bei 240 fps</li> <li><b>Videoformat:</b> RAW, YUV (unkomprimiert)</li> <li><b>Objektiv:</b> Zwei Objektive, F-Wert/F2.0 Fixfokus</li> <li><b>Erfassungsbereich</b> 30 cm ～ ∞</li> <li><b>Sichtfeld</b> 85°</li> <li><b>Mikrofon:</b> 4-Kanal Mikrofon-Array</li> <li><b>Verbindungsart:</b> Spezieller PS4-Stecker (AUX-Stecker)</li> <li><b>Kabellänge:</b> Ca. 2 m (vorläufig)</li> <p></p><p> <i>Kurzfristige Änderungen des Herstellers vorbehalten.</i> </p></ul></p>";
                })
                .Alter("PlayStation 4 Bundle", x =>
                {
                    x.ShortDescription = "PlayStation®4 Console, DUALSHOCK®4 wireless controller und PS4 Kamera.";
                    x.FullDescription = ps4FullDescription;
                    x.BundleTitleText = "Produktset besteht aus";
                })
                .Alter("Accessories for unlimited gaming experience", x =>
                {
                    x.Name = "Zubehör für unbegrenzte Gaming-Erlebnis";
                    x.ShortDescription = "Die Zukunft des Gaming ist jetzt mit dynamischen, verbundenen Spiele, starke Grafikleistung und Geschwindigkeit, intelligente Personalisierung, integrierter sozialer Fähigkeiten und innovative Second-Screen-Funktionen. Der geniale Höhepunkt der kreativsten Köpfe in der Industrie, bietet PlayStation® 4 eine einzigartige Spielumgebung, die Ihnen den Atem rauben wird.";
                    x.FullDescription = "<ul><li>Tauchen Sie ein in eine neue Welt des Spielens mit starker Grafikleistung und Geschwindigkeit.</li><li>Beseitigen Sie längere Ladezeiten der gespeicherte Spiele mit Suspend-Modus.</li><li>Sofortiges spielen ohne zu warten dank Herunterladen und Aktialisierung im Hintergrund.</li><li>Teilen Sie sofort Bilder und Videos Ihrer Lieblings-Gaming-Momente auf Facebook mit dem DUALSHOCK® 4 Controller.</li><li>Ausgestrahlt, während Sie Echtzeit über Ustream spielen.</li></ul>";
                })
                .Alter("Watch Dogs", x =>
                {
                    x.ShortDescription = "Hacken Sie die Stadt und machen Sie sie zu Ihrer Waffe. Bewegen Sie sich dynamisch durch die Stadt, nutzen Sie Abkürzungen durch Gebäude, klettern Sie auf Dächer und über Hindernisse";
                    x.FullDescription = "<p>Es braucht nur eine Fingerbewegung und wir sind mit unseren Freunden verbunden. Wir kaufen die aktuellsten Gadgets und Ausrüstungen. Wir finden heraus, was in der Welt passiert. Aber mit der gleichen Bewegung entsteht ein digitaler Schatten, der sich zunehmend vergrößert. Mit jeder Verbindung hinterlassen wir eine digitale Spur, die jede Bewegung von uns aufzeichnet: unsere Vorlieben sowie unsere Abneigungen. Und es sind nicht nur die Menschen – jede Großstadt ist heutzutage vernetzt. Die städtische Infrastruktur wird von komplexen Systemen überwacht und kontrolliert.</p><p>In Watch Dogs heißt dieses System Central Operating System (ctOS). Es kontrolliert den Großteil der Technologie der Stadt und verfügt über wichtige Informationen über jeden Einwohner. Du spielst Aiden Pearce, einen brillanten Hacker und ehemaligen Gangster, dessen kriminelle Vergangenheit zu einer blutigen Familientragödie führte. Jetzt bist du auf der Jagd nach den Leuten, die deiner Familie Leid zugefügt haben, und du hast die Möglichkeit jeden in deinem Umfeld zu überwachen und zu hacken, indem du alles manipulierst, was mit dem Netzwerk der Stadt verbunden ist. Greife auf die allgegenwärtigen Überwachungskameras zu, lade persönliche Informationen herunter, um eine Zielperson zu finden, kontrolliere Ampeln und öffentliche Verkehrsmittel… und vieles mehr. Nutze die Stadt Chicago als deine ultimative Waffe und nimm auf deine persönliche Art Rache.</p><p><ul> <li><strong>Hacke die Stadt</strong> - <i>Watch Dogs</i> spielt in einer vollständig simulierten Stadt, in der du in Echtzeit die öffentlichen Verkehrsmittel mit deinem Smartphone kontrollieren kannst. Alles, was mit dem ctOS der Stadt verbunden ist, kann zu deiner Waffe werden.</li> <li><strong>Gerechtigkeit der Straße</strong> - In einer Stadt, in der auf Gewalt am besten mit Gewalt geantwortet wird, hast du die Fähigkeiten, um den Kampf auf die Straßen zu bringen. Insgesamt wirst du Zugriff auf ein Arsenal von über 30 traditionellen Waffen haben.</li> <li><strong>Volle Power</strong> - Setze dich hinter das Lenkrad von mehr als 65 Fahrzeugen, jedes ausgestattet mit Handling und Fahrphysik auf dem neusten Stand der Technik, und erkunde die riesige Stadt während du Missionen erfüllst.</li> <li> <strong>Alles unter Kontrolle</strong> - Die komplett neue Disrupt Engine, die eigenes für <i>Watch Dogs</i> programmiert wurde, nutzt fortschrittliche Technologien, um ein beeindruckende Grafik und eine unglaublich realistische Spielerfahrung zu bieten. </li> <li> <strong>Dynamische Navigation</strong> - <i>Watch Dogs</i> bietet dir nicht nur die Möglichkeit das ctOS zu deinem Vorteil zu nutzen, sondern auch deine Umgebung. Nimm Abkürzungen durch Gebäude oder klettere über die Dächer, um dein Ziel in einem realistischen Chicago zu erreichen. </li> </ul></p>";
                })
                .Alter("Prince of Persia", x =>
                {
                    x.Name = "Prince of Persia \"Die vergessene Zeit\"";
                    x.ShortDescription = "Mit Prince of Persia: Die Vergessene Zeit erscheint eine völlig neue Geschichte aus dem Prince of Persia-Universum. Auf den Spieler warten bisher unbekannte Charaktere und neuartige Kräfte, mit denen die Natur und die Zeit kontrolliert werden können. Die Handlung des Spiels ist zwischen Prince of Persia The Sands of Time und Prince of Persia Warrior Within angesiedelt.";
                    x.FullDescription = "<p>Prince of Persia: Die Vergessene Zeit stellt das nächste Kapitel aus dem von Fans geliebten Sands of Time-Universum dar. Nach seinem Abenteuer in Azad reist der Prinz in das Königreich seines Bruders. Doch eine mächtige Armee belagert den königlichen Palast und droht diesen zu vernichten. Um das Königreich vor dem sicheren Untergang zu bewahren, entschließt sich der Prinz, die uralten Kräfte des Sands nutzen – der Beginn eines epischen Abenteuers. Der Prinz muss sich als Anführer beweisen und lernen, dass große Macht oft einen hohen Preis fordert.</p><p>Klassisches Gameplay mit neuen Elementen Prince of Persia: Die Vergessene Zeit handelt von den Ereignissen zwischen Prince of Persia The Sands of Time und Prince of Persia: Warrior Within und eröffnet den Fans ein völlig neues Kapitel der Prince of Persia-Reihe. Dadurch verleiht das Spiel dem Sands of Time-Universum noch mehr Tiefe. Den Prinzen erwarten epische Kämpfe gegen mehrere Feinde gleichzeitig sowie spektakuläre Akrobatikeinlagen in schwindelerregender Höhe und gewaltigen Spielumgebungen. Glücklicherweise verfügt er in über besondere Kräfte, durch die er die Natur und die Zeit kontrollieren kann, denn Prince of Persia: Die Vergessene Zeit wird seine außerordentlichen Talente fordern wie niemals zuvor.</p><p>Prince of Persia: Die Vergessene Zeit entführt den Spieler zu unvergesslichen Schauplätzen, die durch die fortschrittliche und preisgekrönte Anvil-Engine zum Leben erweckt werden. Während vor den Toren des Königreichs ein schrecklicher Krieg tobt, werden die Fähigkeiten des Prinzen wie noch nie zuvor auf die Probe gestellt. Das epische Abenteuer des Prinzen hält etliche atemberaubende Momente bereit, die dem Spieler während des gesamten Abenteuers die Sprache verschlagen werden.</p><p>Herrschaft über die Natur. Da der Prinz die Natur und die Zeit beeinflussen kann, hat er stets die volle Kontrolle über seine Umgebung und einen gewaltigen Vorteil gegenüber seinen Widersachern. Er wird feststellen, dass seine Macht über die Natur in Kombination mit der Fähigkeit, die Zeit zurückzudrehen, ganz besonders durchschlagende Effekte haben kann. Somit eröffnen sich neue Wege und Ansätze Gebiete zu überwinden oder Rätsel zu lösen. Abwechslung und Spannung erwartet den Spieler dabei während des gesamten Abenteuers.</p><p><ul><li><b>Klassisches Gameplay mit neuen Elementen:</b> Den Prinzen erwarten epische Kämpfe gegen mehrere Feinde gleichzeitig sowie spektakuläre Akrobatikeinlagen in schwindelerregender Höhe und gewaltigen Spielumgebungen.</li><li><b>Eine kinoreife Spielerfahrung:</b> Prince of Persia: Die Vergessene Zeit entführt den Spieler zu unvergesslichen Schauplätzen, die durch die fortschrittliche und preisgekrönte Anvil-Engine zum Leben erweckt werden.</li><li><b>Herrschaft über die Natur:</b> Da der Prinz die Natur und die Zeit beeinflussen kann, hat er stets die volle Kontrolle über seine Umgebung und einen gewaltigen Vorteil gegenüber seinen Widersachern.</li><li><b>Rückkehr der beliebten Serie:</b> Prince of Persia: Die Vergessene Zeit handelt von den Ereignissen zwischen Prince of Persia The Sands of Time und Prince of Persia: Warrior Within und eröffnet den Fans ein völlig neues Kapitel der Prince of Persia-Reihe.</li></ul></p>";
                })
                .Alter("Driver San Francisco", x =>
                {
                    x.ShortDescription = "Gangsterboss Charles Jericho ist wieder auf freiem Fuß und wird zu einer enormen Bedrohung für ganz San Francisco. Nur ein Mann vermag ihn jetzt noch aufzuhalten. Er hat die Straßen hunderter Städte befahren und sein ganzes Leben damit verbracht, Verbrecher hinter Schloss und Riegel zu bringen. Nun gibt es kein Zurück mehr! Um Jericho zur Strecke zu bringen muss er alles riskieren und er weiß, dass es sich dabei um seine vielleicht letzte Fahrt handeln könnte. Sein Name ist John Tanner. Er ist der Driver.";
                    x.FullDescription = "<p>Mit Ubisoft Reflections kehren die Entwickler des Originals zurück und erschaffen mit Driver San Francisco einen brandneuen Ableger der weltweit über 14 Millionen Mal verkauften Driver-Reihe. Der Spieler schlüpft in die Rolle von Detective John Tanner und wird in eine unerbittliche Jagd in den Straßen der weltberühmten Stadt an der Bucht verwickelt. Dank eines revolutionären Features wird es nun möglich, nahtlos zwischen hunderten lizensierten Wagen hin und her zu wechseln. Der Spieler bleibt so immer mitten in der Action. Mir seiner zeitlose Atmosphäre, einzigartigem Fahrverhalten und einer grunderneuerten Spielbarkeit wird Driver San Francisco zu einer klassischen und kinoreifen Verfolgungsjagd in einer offenen Spielwelt.</p><p><ul> <li><strong>Verfolgungsjagd pur:</strong><br>Zurück zu den Ursprüngen des cinematischen Fahrerlebnis von DRIVER: Weiche Radaufhängungen, lange Drifts, enge Kurven und High-Speed Verfolgungen im dichten Verkehr. Über 120 lizensierte Fahrzeuge stehen in der intensiven Hatz durch die Straßen San Franciscos zur Verfügung.<br><br></li> <li><strong>Eine unerbitterliche Hetzjagd:</strong><br>Auf Tanners von Rache erfüllter Jagd nach Jericho enthüllt der Spieler eine spannende Geschichte. Er verfolgt Tanners PS geladenen Überlebenskampf durch San Francisco bis zu dem Punkt, an dem es kein Zurück mehr gibt. <br><br></li> <li><strong>Shift:</strong><br>Während Tanner sich von der Folgen eines dramatischen Unfalls erholt, wird ihm bewusst, dass er eine neue Fähigkeit erlang hat: Shift. Sie ermöglicht es ihm nahtlos zwischen Fahrzeugen hin und her zu wechseln um deren Kontrolle zu übernehmen. Dies hat eine beispiellose Intensität, Vielfältigkeit und Freiheit zur Folge: Es wird möglich, in schnellere Vehikel zu wechseln, mit zivilen Fahrzeugen seine Kontrahenten auszuschalten oder in den Wagen des Gegner zu schlüpfen, um ihn ins Verderben zu stürzen.<br><br></li> <li><strong>Ein Spielplatz für Verfolgungsjagden:</strong><br>Ein mehr als 200km umfassendes Straßen-Netzwerk reicht bis über die Golden Gate Bridge und ist gespickt mit vielen Sehenswürdigkeiten von San Francisco. Der Spieler wechselt von einem zum nächsten Fahrzeug und taucht ab in das Leben verschiedenster Stadtbewohner. Eine schwindelerregende Vielzahl an Charakteren, alle mit einem ganz eigenen Blick auf eine Stadt im Ausnahmezustand.<br><br></li> <li><strong>Wahnwitziger Mehrspieler-Action:</strong><br>Neun mitreißende Online-Modi, lassen die Spieler dank der SHIFT-Funktion nahezu zu jeder Zeit an jedem beliebigen Ort auftauchen.&nbsp; Sowohl offline im Splitscreen als auch online setzt man zum Rammen, Abhängen und Überholen seines Freundes an.<br><br> </li> <li><strong>Und vieles mehr:</strong><br>Wie ein echter Regisseur kann der Spieler seine besten Stunts in packenden Filmen festhalten, bearbeiten und mit seinen Freunden teilen. Wer sein fahrerisches Können unter Beweis stellen will, kann sich in 20 Rennen und bei 80 Herausforderungen austoben. Über 60 bekannte Songs und natürlich das originale Driver-Thema sorgen während der Action auf heißem Asphalt für ebenso heiße Ohren.</li> </ul></p>";
                })
                .Alter("PlayStation 3 plus game cheaper", x =>
                {
                    x.Name = "PlayStation 3 plus Spiel günstiger";
                    x.ShortDescription = "Unser besonderes Angebot: PlayStation 3 plus ein Spiel Ihrer Wahl günstiger.";
                    x.FullDescription = ps3FullDescription;
                });

                #endregion Category Gaming

                AlterFashionProducts(entities);
                AlterFurnitureProducts(entities);
            }
            catch (Exception ex)
            {
                throw new SeedDataException("AlterProduct", ex);
            }
        }

        protected override void Alter(IList<ForumGroup> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.Name)
                .Alter("General", x =>
                {
                    x.Name = "Allgemein";
                });
        }

        protected override void Alter(IList<Forum> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.Name)
                .Alter("New Products", x =>
                {
                    x.Name = "Neue Produkte";
                    x.Description = "Diskutieren Sie aktuelle oder neue Produkte";
                })
                .Alter("Packaging & Shipping", x =>
                {
                    x.Name = "Verpackung & Versand";
                    x.Description = "Haben Sie Fragen oder Anregungen zu Verpackung & Versand?";
                });
        }

        protected override void Alter(IList<Discount> entities)
        {
            base.Alter(entities);

            var names = new Dictionary<string, string>
            {
                { "10% for certain manufacturers", "10% bei bestimmten Herstellern" },
                { "20% order total discount", "20% auf den Bestellwert" },
                { "20% for certain categories", "20% bei bestimmten Warengruppen" },
                { "25% on certain products", "25% auf bestimmte Produkte" },
                { "5% on weekend orders", "5% bei Bestellungen am Wochenende" },
                { "Sample discount with coupon code", "Beispiel Rabatt mit Coupon-Code" },
            };

            var alterer = entities.WithKey(x => x.Name);

            foreach (var kvp in names)
            {
                alterer.Alter(kvp.Key, x => x.Name = kvp.Value);
            }
        }

        protected override void Alter(IList<DeliveryTime> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.DisplayOrder)
                .Alter(0, x =>
                {
                    x.Name = "sofort lieferbar";
                })
                .Alter(1, x =>
                {
                    x.Name = "2-5 Werktage";
                })
                .Alter(2, x =>
                {
                    x.Name = "7 Werktage";
                });

            _defaultDeliveryTime = entities.First();
        }

        protected override void Alter(IList<QuantityUnit> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.DisplayOrder)
                .Alter(0, x =>
                {
                    x.Name = "Stück";
                    x.NamePlural = "Stück";
                    x.Description = "Stück";
                })
                .Alter(1, x =>
                {
                    x.Name = "Schachtel";
                    x.NamePlural = "Schachteln";
                    x.Description = "Schachtel";
                })
                .Alter(2, x =>
                {
                    x.Name = "Paket";
                    x.NamePlural = "Pakete";
                    x.Description = "Paket";
                })
                .Alter(3, x =>
                {
                    x.Name = "Palette";
                    x.NamePlural = "Paletten";
                    x.Description = "Palette";
                })
                .Alter(4, x =>
                {
                    x.Name = "Einheit";
                    x.NamePlural = "Einheiten";
                    x.Description = "Einheit";
                })
                .Alter(5, x =>
                {
                    x.Name = "Sack";
                    x.NamePlural = "Säcke";
                    x.Description = "Sack";
                })
                .Alter(6, x =>
                {
                    x.Name = "Tüte";
                    x.NamePlural = "Tüten";
                    x.Description = "Tüte";
                })
                .Alter(7, x =>
                {
                    x.Name = "Dose";
                    x.NamePlural = "Dosen";
                    x.Description = "Dose";
                })
                .Alter(8, x =>
                {
                    x.Name = "Packung";
                    x.NamePlural = "Packungen";
                    x.Description = "Packung";
                })
                .Alter(9, x =>
                {
                    x.Name = "Stange";
                    x.NamePlural = "Stangen";
                    x.Description = "Stange";
                })
                .Alter(10, x =>
                {
                    x.Name = "Flasche";
                    x.NamePlural = "Flaschen";
                    x.Description = "Flasche";
                })
                .Alter(11, x =>
                {
                    x.Name = "Glas";
                    x.NamePlural = "Gläser";
                    x.Description = "Glas";
                })
                .Alter(12, x =>
                {
                    x.Name = "Bund";
                    x.NamePlural = "Bünde";
                    x.Description = "Bund";
                })
                .Alter(13, x =>
                {
                    x.Name = "Rolle";
                    x.NamePlural = "Rollen";
                    x.Description = "Rolle";
                })
                .Alter(14, x =>
                {
                    x.Name = "Becher";
                    x.NamePlural = "Becher";
                    x.Description = "Becher";
                })
                .Alter(15, x =>
                {
                    x.Name = "Bündel";
                    x.NamePlural = "Bündel";
                    x.Description = "Bündel";
                })
                .Alter(16, x =>
                {
                    x.Name = "Fass";
                    x.NamePlural = "Fässer";
                    x.Description = "Fass";
                })
                .Alter(17, x =>
                {
                    x.Name = "Set";
                    x.NamePlural = "Sets";
                    x.Description = "Set";
                })
                .Alter(18, x =>
                {
                    x.Name = "Eimer";
                    x.NamePlural = "Eimer";
                    x.Description = "Eimer";
                });
        }

        protected override void Alter(IList<Store> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.DisplayOrder)
                .Alter(1, x =>
                {
                    x.Name = "Mein Shop-Name";
                    x.Url = "http://www.mein-shop.de/";
                    x.Hosts = "mein-shop.de,www.mein-shop.de";
                });
        }

        protected override void Alter(IList<ProductTag> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.Name)
                .Alter("gift", x =>
                {
                    x.Name = "Geschenk";
                })
                .Alter("book", x =>
                {
                    x.Name = "Buch";
                })
                .Alter("cooking", x =>
                {
                    x.Name = "Kochen";
                })
                .Alter("cars", x =>
                {
                    x.Name = "Autos";
                })
                .Alter("motorbikes", x =>
                {
                    x.Name = "Motorräder";
                })
                .Alter("download", x =>
                {
                    x.Name = "Download";
                })
                .Alter("watches", x =>
                {
                    x.Name = "Uhren";
                });
        }

        protected override void Alter(IList<EmailAccount> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.DisplayName)
                .Alter("General contact", x =>
                {
                    x.DisplayName = "Kontakt";
                    x.Email = "kontakt@meineshopurl.de";
                    x.Host = "localhost";
                })
                .Alter("Sales representative", x =>
                {
                    x.DisplayName = "Vertrieb";
                    x.Email = "vertrieb@meineshopurl.de";
                    x.Host = "localhost";
                })
                .Alter("Customer support", x =>
                {
                    x.DisplayName = "Kundendienst / Support";
                    x.Email = "kundendienst@meineshopurl.de";
                    x.Host = "localhost";
                });
        }

        protected override void Alter(IList<BlogPost> entities)
        {
            base.Alter(entities);
        }

        protected override void Alter(IList<NewsItem> entities)
        {
            base.Alter(entities);
        }

        protected override void Alter(IList<Poll> entities)
        {
            var defaultLanguage = base.DbContext.Set<Language>().FirstOrDefault();
            base.Alter(entities);

            entities.WithKey(x => x.DisplayOrder)
                .Alter(1, x =>
                {
                    x.Language = defaultLanguage;
                    x.Name = "Wie gefällt Ihnen der Shop?";
                    x.PollAnswers.Clear();
                    x.PollAnswers.Add(new PollAnswer()
                    {
                        Name = "Ausgezeichnet",
                        DisplayOrder = 1,
                    });
                    x.PollAnswers.Add(new PollAnswer()
                    {
                        Name = "Gut",
                        DisplayOrder = 2,
                    });
                    x.PollAnswers.Add(new PollAnswer()
                    {
                        Name = "Geht so",
                        DisplayOrder = 3,
                    });
                    x.PollAnswers.Add(new PollAnswer()
                    {
                        Name = "Schlecht",
                        DisplayOrder = 4,
                    });
                })
                .Alter(2, x =>
                {
                    x.Language = defaultLanguage;
                    x.Name = "Wie oft kaufen Sie Online ein?";
                    x.PollAnswers.Clear();
                    x.PollAnswers.Add(new PollAnswer()
                    {
                        Name = "Täglich",
                        DisplayOrder = 1,
                    });
                    x.PollAnswers.Add(new PollAnswer()
                    {
                        Name = "Wöchentlich",
                        DisplayOrder = 2,
                    });
                    x.PollAnswers.Add(new PollAnswer()
                    {
                        Name = "Alle zwei Wochen",
                        DisplayOrder = 3,
                    });
                    x.PollAnswers.Add(new PollAnswer()
                    {
                        Name = "Einmal im Monat",
                        DisplayOrder = 4,
                    });
                });
        }

        protected override void Alter(IList<PollAnswer> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.DisplayOrder)
                .Alter(1, x =>
                {
                    x.Name = "Ausgezeichnet";
                })
                .Alter(2, x =>
                {
                    x.Name = "Gut";
                })
                .Alter(3, x =>
                {
                    x.Name = "Geht so";
                })
                .Alter(4, x =>
                {
                    x.Name = "Schlecht";
                })
                .Alter(5, x =>
                {
                    x.Name = "Täglich";
                })
                .Alter(6, x =>
                {
                    x.Name = "Wöchentlich";
                })
                .Alter(7, x =>
                {
                    x.Name = "Alle zwei Wochen";
                })
                .Alter(8, x =>
                {
                    x.Name = "Einmal im Monat";
                });
        }

        protected override void Alter(IList<Campaign> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.Name)
                .Alter("Reminder of inactive new customers", x =>
                {
                    x.Name = "Erinnerung von inaktiven Neukunden";
                    x.Subject = "Neue, aufregende Produkte warten auf Sie entdeckt zu werden.";
                });
        }

        protected override void Alter(IList<RuleSetEntity> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.Name)
                .Alter("Weekends", x =>
                {
                    x.Name = "Wochenenden";
                })
                .Alter("Major customers", x =>
                {
                    x.Name = "Wichtige Kunden";
                    x.Description = "3 oder mehr Bestellungen und aktueller Bestellwert mindestens 200,- Euro.";
                })
                .Alter("Sale", x =>
                {
                    x.Name = "Sale";
                    x.Description = "Produkte mit angewendeten Rabatten.";
                })
                .Alter("Inactive new customers", x =>
                {
                    x.Name = "Inaktive Neukunden";
                    x.Description = "Eine abgeschlossene, mindestens 90 Tage zurückliegende Bestellung.";
                });
        }
    }
}
