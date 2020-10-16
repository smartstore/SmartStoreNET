using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Discounts;

namespace SmartStore.Data.Setup
{
    public abstract partial class InvariantSeedData
    {
        public IList<Manufacturer> Manufacturers()
        {
            var imagesPath = _sampleImagesPath;
            var gridOrLinesTemplate = ManufacturerTemplates().Where(x => x.ViewPath == "ManufacturerTemplate.ProductsInGridOrLines").FirstOrDefault();
            var discounts = _ctx.Set<Discount>().Where(x => x.DiscountTypeId == (int)DiscountType.AssignedToManufacturers).ToList();

            var manufacturerJackWolfskin = new Manufacturer
            {
                Name = "Jack-Wolfskin",
                ManufacturerTemplateId = gridOrLinesTemplate.Id,
                MediaFile = CreatePicture("brand/Jack_Wolfskin.png"),
                Published = true,
                DisplayOrder = 1
            };

            var manufacturerEASports = new Manufacturer
            {
                Name = "EA Sports",
                ManufacturerTemplateId = gridOrLinesTemplate.Id,
                MediaFile = CreatePicture("brand/EA_Sports.png"),
                Published = true,
                DisplayOrder = 1
            };

            var manufacturerWarnerHome = new Manufacturer
            {
                Name = "Warner Home Video Games",
                ManufacturerTemplateId = gridOrLinesTemplate.Id,
                MediaFile = CreatePicture("brand/wb.png", GetSeName("Warner Home Video Games")),
                Published = true,
                DisplayOrder = 1
            };

            var manufacturerBreitling = new Manufacturer
            {
                Name = "Breitling",
                ManufacturerTemplateId = gridOrLinesTemplate.Id,
                MediaFile = CreatePicture("brand/breitling.png"),
                Published = true,
                DisplayOrder = 1
            };
            if (discounts.Any())
            {
                manufacturerBreitling.HasDiscountsApplied = true;
                manufacturerBreitling.AppliedDiscounts.Add(discounts.First());
            }

            var manufacturerTissot = new Manufacturer
            {
                Name = "Tissot",
                ManufacturerTemplateId = gridOrLinesTemplate.Id,
                MediaFile = CreatePicture("brand/Tissot.png"),
                Published = true,
                DisplayOrder = 1
            };

            var manufacturerSeiko = new Manufacturer
            {
                Name = "Seiko",
                ManufacturerTemplateId = gridOrLinesTemplate.Id,
                MediaFile = CreatePicture("brand/seiko.png"),
                Published = true,
                DisplayOrder = 1
            };
            if (discounts.Any())
            {
                manufacturerSeiko.HasDiscountsApplied = true;
                manufacturerSeiko.AppliedDiscounts.Add(discounts.First());
            }

            var manufacturerTitleist = new Manufacturer
            {
                Name = "Titleist",
                ManufacturerTemplateId = gridOrLinesTemplate.Id,
                MediaFile = CreatePicture("brand/titleist.png"),
                Published = true,
                DisplayOrder = 1
            };

            var manufacturerPuma = new Manufacturer
            {
                Name = "Puma",
                ManufacturerTemplateId = gridOrLinesTemplate.Id,
                MediaFile = CreatePicture("brand/puma.jpg"),
                Published = true,
                DisplayOrder = 1
            };

            var manufacturerNike = new Manufacturer
            {
                Name = "Nike",
                ManufacturerTemplateId = gridOrLinesTemplate.Id,
                MediaFile = CreatePicture("brand/nike.png"),
                Published = true,
                DisplayOrder = 1
            };

            var manufacturerAdidas = new Manufacturer
            {
                Name = "Adidas",
                ManufacturerTemplateId = gridOrLinesTemplate.Id,
                MediaFile = CreatePicture("brand/adidas.png"),
                Published = true,
                DisplayOrder = 1
            };

            var manufacturerRayban = new Manufacturer
            {
                Name = "Ray-Ban",
                ManufacturerTemplateId = gridOrLinesTemplate.Id,
                MediaFile = CreatePicture("brand/ray-ban.jpg"),
                Published = true,
                DisplayOrder = 1
            };

            var manufacturerOakley = new Manufacturer
            {
                Name = "Oakley",
                ManufacturerTemplateId = gridOrLinesTemplate.Id,
                MediaFile = CreatePicture("brand/oakley.png"),
                Published = true,
                DisplayOrder = 1
            };

            var manufacturerApple = new Manufacturer
            {
                Name = "Apple",
                ManufacturerTemplateId = gridOrLinesTemplate.Id,
                MediaFile = CreatePicture("brand/apple.png"),
                Published = true,
                DisplayOrder = 1
            };

            var manufacturerMicrosoft = new Manufacturer
            {
                Name = "Microsoft",
                ManufacturerTemplateId = gridOrLinesTemplate.Id,
                MediaFile = CreatePicture("brand/microsoft-icon.png", GetSeName("Microsoft")),
                Published = true,
                DisplayOrder = 6
            };

            var manufacturerFestina = new Manufacturer
            {
                Name = "Festina",
                ManufacturerTemplateId = gridOrLinesTemplate.Id,
                MediaFile = CreatePicture("brand/festina.png"),
                Published = true,
                DisplayOrder = 17
            };

            var manufacturerCertina = new Manufacturer
            {
                Name = "Certina",
                ManufacturerTemplateId = gridOrLinesTemplate.Id,
                MediaFile = CreatePicture("brand/certina.png"),
                Published = true,
                DisplayOrder = 18
            };

            var manufacturerSony = new Manufacturer
            {
                Name = "Sony",
                ManufacturerTemplateId = gridOrLinesTemplate.Id,
                MediaFile = CreatePicture("brand/sony.png"),
                Published = true,
                DisplayOrder = 19
            };

            var manufacturerUbisoft = new Manufacturer
            {
                Name = "Ubisoft",
                ManufacturerTemplateId = gridOrLinesTemplate.Id,
                MediaFile = CreatePicture("brand/ubisoft.png"),
                Published = true,
                DisplayOrder = 20
            };

            //var manufacturerMeyAndEdlich = new Manufacturer
            //{
            //    Name = "Mey-And-Edlich",
            //    ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
            //    MediaFile = CreatePicture("brand/MeyAndEdlich.jpg", GetSeName("Mey Edlich")),
            //    Published = true,
            //    DisplayOrder = 1
            //};

            //var manufacturerWilson = new Manufacturer
            //{
            //    Name = "Wilson",
            //    ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
            //    MediaFile = CreatePicture("brand/wilson.png"),
            //    Published = true,
            //    DisplayOrder = 1
            //};
            //var manufacturerAndroid = new Manufacturer
            //{
            //    Name = "Android",
            //    ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
            //    MediaFile = CreatePicture("brand/android.png"),
            //    Published = true,
            //    DisplayOrder = 2
            //};

            //var manufacturerLG = new Manufacturer
            //{
            //    Name = "LG",
            //    ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
            //    MediaFile = CreatePicture("brand/lg.png"),
            //    Published = true,
            //    DisplayOrder = 3
            //};

            //var manufacturerDell = new Manufacturer
            //{
            //    Name = "Dell",
            //    ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
            //    MediaFile = CreatePicture("brand/dell.png"),
            //    Published = true,
            //    DisplayOrder = 4
            //};

            //var manufacturerHP = new Manufacturer
            //{
            //    Name = "HP",
            //    ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
            //    MediaFile = CreatePicture("brand/hp.png"),
            //    Published = true,
            //    DisplayOrder = 5
            //};

            //var manufacturerSamsung = new Manufacturer
            //{
            //    Name = "Samsung",
            //    ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
            //    MediaFile = CreatePicture("brand/samsung.png"),
            //    Published = true,
            //    DisplayOrder = 7
            //};

            //var manufacturerAcer = new Manufacturer
            //{
            //    Name = "Acer",
            //    ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
            //    MediaFile = CreatePicture("brand/acer.jpg"),
            //    Published = true,
            //    DisplayOrder = 8
            //};

            //var manufacturerTrekStor = new Manufacturer
            //{
            //    Name = "TrekStor",
            //    ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
            //    MediaFile = CreatePicture("brand/trekstor.png"),
            //    Published = true,
            //    DisplayOrder = 9
            //};

            //var manufacturerWesternDigital = new Manufacturer
            //{
            //    Name = "Western Digital",
            //    ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
            //    MediaFile = CreatePicture("brand/westerndigital.png"),
            //    Published = true,
            //    DisplayOrder = 10
            //};

            //var manufacturerMSI = new Manufacturer
            //{
            //    Name = "MSI",
            //    ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
            //    MediaFile = CreatePicture("brand/msi.png"),
            //    Published = true,
            //    DisplayOrder = 11
            //};

            //var manufacturerCanon = new Manufacturer
            //{
            //    Name = "Canon",
            //    ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
            //    MediaFile = CreatePicture("brand/canon.png"),
            //    Published = true,
            //    DisplayOrder = 12
            //};

            //var manufacturerCasio = new Manufacturer
            //{
            //    Name = "Casio",
            //    ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
            //    MediaFile = CreatePicture("brand/casio.png"),
            //    Published = true,
            //    DisplayOrder = 13
            //};

            //var manufacturerPanasonic = new Manufacturer
            //{
            //    Name = "Panasonic",
            //    ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
            //    MediaFile = CreatePicture("brand/panasonic.png"),
            //    Published = true,
            //    DisplayOrder = 14
            //};

            //var manufacturerBlackBerry = new Manufacturer
            //{
            //    Name = "BlackBerry",
            //    ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
            //    MediaFile = CreatePicture("brand/blackberry.png"),
            //    Published = true,
            //    DisplayOrder = 15
            //};

            //var manufacturerHTC = new Manufacturer
            //{
            //    Name = "HTC",
            //    ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
            //    MediaFile = CreatePicture("brand/htc.png"),
            //    Published = true,
            //    DisplayOrder = 16
            //};

            var entities = new List<Manufacturer>
            {
                manufacturerEASports, manufacturerWarnerHome, manufacturerBreitling, manufacturerTissot, manufacturerSeiko,
                manufacturerTitleist, manufacturerApple, manufacturerFestina, manufacturerCertina,
                manufacturerSony, manufacturerUbisoft, manufacturerOakley, manufacturerRayban, manufacturerAdidas,
                manufacturerPuma, manufacturerNike,  manufacturerJackWolfskin, manufacturerMicrosoft,
            };

            Alter(entities);
            return entities;
        }
    }
}
