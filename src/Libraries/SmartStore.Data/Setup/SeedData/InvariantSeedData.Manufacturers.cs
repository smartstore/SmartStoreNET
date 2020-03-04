using System;
using System.Linq;
using System.Collections.Generic;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Data.Setup
{
    public abstract partial class InvariantSeedData
    {
        public IList<Manufacturer> Manufacturers()
        {
            //pictures
            var sampleImagesPath = this._sampleImagesPath;

            var manufacturerTemplateInGridAndLines =
                this.ManufacturerTemplates().Where(pt => pt.ViewPath == "ManufacturerTemplate.ProductsInGridOrLines").FirstOrDefault();

            //var categoryTemplateInGridAndLines =
            //    this.CategoryTemplates().Where(pt => pt.Name == "Products in Grid or Lines").FirstOrDefault();

            //

            #region Jack Wolfskin

            var manufacturerJackWolfskin = new Manufacturer
            {
                Name = "Jack-Wolfskin",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/Jack_Wolfskin.png"),
                Published = true,
                DisplayOrder = 1
            };

            #endregion

            #region Mey & Edlich

            var manufacturerMeyAndEdlich = new Manufacturer
            {
                Name = "Mey-And-Edlich",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/MeyAndEdlich.jpg", GetSeName("Mey Edlich")),
                Published = true,
                DisplayOrder = 1
            };

            #endregion

            #region EA Sports

            var manufacturerEASports = new Manufacturer
            {
                Name = "EA Sports",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/EA_Sports.png"),
                Published = true,
                DisplayOrder = 1
            };

            #endregion EA Sports

            #region Warner Home Video Games

            var manufacturerWarnerHome = new Manufacturer
            {
                Name = "Warner Home Video Games",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/wb.png", GetSeName("Warner Home Video Games")),
                Published = true,
                DisplayOrder = 1
            };

            #endregion Warner Home Video Games

            #region Breitling

            var manufacturerBreitling = new Manufacturer
            {
                Name = "Breitling",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/breitling.png"),
                Published = true,
                DisplayOrder = 1
            };

            #endregion Breitling

            #region Tissot

            var manufacturerTissot = new Manufacturer
            {
                Name = "Tissot",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/Tissot.png"),
                Published = true,
                DisplayOrder = 1
            };

            #endregion Tissot

            #region Seiko

            var manufacturerSeiko = new Manufacturer
            {
                Name = "Seiko",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/seiko.png"),
                Published = true,
                DisplayOrder = 1
            };

            #endregion Seiko

            #region Titleist

            var manufacturerTitleist = new Manufacturer
            {
                Name = "Titleist",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/titleist.png"),
                Published = true,
                DisplayOrder = 1
            };

            #endregion Titleist

            #region Puma

            var manufacturerPuma = new Manufacturer
            {
                Name = "Puma",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/puma.jpg"),
                Published = true,
                DisplayOrder = 1
            };

            #endregion Puma

            #region Nike

            var manufacturerNike = new Manufacturer
            {
                Name = "Nike",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/nike.png"),
                Published = true,
                DisplayOrder = 1
            };

            #endregion Nike

            #region Wilson

            var manufacturerWilson = new Manufacturer
            {
                Name = "Wilson",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/wilson.png"),
                Published = true,
                DisplayOrder = 1
            };

            #endregion Wilson

            #region Adidas

            var manufacturerAdidas = new Manufacturer
            {
                Name = "Adidas",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/adidas.png"),
                Published = true,
                DisplayOrder = 1
            };

            #endregion Adidas

            #region Ray-ban

            var manufacturerRayban = new Manufacturer
            {
                Name = "Ray-Ban",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/ray-ban.jpg"),
                Published = true,
                DisplayOrder = 1
            };

            #endregion Ray-ban

            #region Oakley

            var manufacturerOakley = new Manufacturer
            {
                Name = "Oakley",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/oakley.png"),
                Published = true,
                DisplayOrder = 1
            };

            #endregion Oakley

            #region Apple

            var manufacturerApple = new Manufacturer
            {
                Name = "Apple",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/apple.png"),
                Published = true,
                DisplayOrder = 1
            };

            #endregion Apple

            #region Android

            var manufacturerAndroid = new Manufacturer
            {
                Name = "Android",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/android.png"),
                Published = true,
                DisplayOrder = 2
            };

            #endregion Android

            #region LG

            var manufacturerLG = new Manufacturer
            {
                Name = "LG",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/lg.png"),
                Published = true,
                DisplayOrder = 3
            };

            #endregion LG

            #region Dell

            var manufacturerDell = new Manufacturer
            {
                Name = "Dell",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/dell.png"),
                Published = true,
                DisplayOrder = 4
            };

            #endregion Dell

            #region HP

            var manufacturerHP = new Manufacturer
            {
                Name = "HP",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/hp.png"),
                Published = true,
                DisplayOrder = 5
            };

            #endregion HP

            #region Microsoft

            var manufacturerMicrosoft = new Manufacturer
            {
                Name = "Microsoft",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/microsoft-icon.png", GetSeName("Microsoft")),
                Published = true,
                DisplayOrder = 6
            };

            #endregion Microsoft

            #region Samsung

            var manufacturerSamsung = new Manufacturer
            {
                Name = "Samsung",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/samsung.png"),
                Published = true,
                DisplayOrder = 7
            };

            #endregion Samsung

            #region Acer

            var manufacturerAcer = new Manufacturer
            {
                Name = "Acer",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/acer.jpg"),
                Published = true,
                DisplayOrder = 8
            };

            #endregion Acer

            #region TrekStor

            var manufacturerTrekStor = new Manufacturer
            {
                Name = "TrekStor",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/trekstor.png"),
                Published = true,
                DisplayOrder = 9
            };

            #endregion TrekStor

            #region Western Digital

            var manufacturerWesternDigital = new Manufacturer
            {
                Name = "Western Digital",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/westerndigital.png"),
                Published = true,
                DisplayOrder = 10
            };

            #endregion Western Digital

            #region MSI

            var manufacturerMSI = new Manufacturer
            {
                Name = "MSI",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/msi.png"),
                Published = true,
                DisplayOrder = 11
            };

            #endregion MSI

            #region Canon

            var manufacturerCanon = new Manufacturer
            {
                Name = "Canon",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/canon.png"),
                Published = true,
                DisplayOrder = 12
            };

            #endregion Canon

            #region Casio

            var manufacturerCasio = new Manufacturer
            {
                Name = "Casio",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/casio.png"),
                Published = true,
                DisplayOrder = 13
            };

            #endregion Casio

            #region Panasonic

            var manufacturerPanasonic = new Manufacturer
            {
                Name = "Panasonic",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/panasonic.png"),
                Published = true,
                DisplayOrder = 14
            };

            #endregion Panasonic

            #region BlackBerry

            var manufacturerBlackBerry = new Manufacturer
            {
                Name = "BlackBerry",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/blackberry.png"),
                Published = true,
                DisplayOrder = 15
            };

            #endregion BlackBerry

            #region HTC

            var manufacturerHTC = new Manufacturer
            {
                Name = "HTC",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/htc.png"),
                Published = true,
                DisplayOrder = 16
            };

            #endregion HTC

            #region Festina

            var manufacturerFestina = new Manufacturer
            {
                Name = "Festina",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/festina.png"),
                Published = true,
                DisplayOrder = 17
            };

            #endregion Festina

            #region Certina

            var manufacturerCertina = new Manufacturer
            {
                Name = "Certina",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/certina.png"),
                Published = true,
                DisplayOrder = 18
            };

            #endregion Certina

            #region Sony

            var manufacturerSony = new Manufacturer
            {
                Name = "Sony",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/sony.png"),
                Published = true,
                DisplayOrder = 19
            };

            #endregion Sony

            #region Ubisoft

            var manufacturerUbisoft = new Manufacturer
            {
                Name = "Ubisoft",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("brand/ubisoft.png"),
                Published = true,
                DisplayOrder = 20
            };

            #endregion Ubisoft

            var entities = new List<Manufacturer>
            {
              manufacturerEASports,manufacturerWarnerHome,manufacturerBreitling,manufacturerTissot,manufacturerSeiko, manufacturerTitleist, manufacturerApple,
              manufacturerSamsung,manufacturerLG,manufacturerTrekStor, manufacturerWesternDigital,manufacturerDell, manufacturerMSI,
              manufacturerCanon, manufacturerCasio, manufacturerPanasonic, manufacturerBlackBerry, manufacturerHTC, manufacturerFestina, manufacturerCertina,
              manufacturerHP, manufacturerAcer, manufacturerSony, manufacturerUbisoft, manufacturerOakley, manufacturerRayban, manufacturerAdidas, manufacturerWilson,
              manufacturerPuma,manufacturerNike, manufacturerMeyAndEdlich, manufacturerJackWolfskin, manufacturerMicrosoft
            };

            this.Alter(entities);
            return entities;
        }
    }
}
