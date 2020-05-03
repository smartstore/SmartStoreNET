using System;
using System.Linq;
using System.Collections.Generic;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Data.Setup
{
    public abstract partial class InvariantSeedData
    {
        public IList<Category> CategoriesFirstLevel()
        {
            var sampleImagesPath = this._sampleImagesPath;
            var categoryTemplateInGridAndLines = this.CategoryTemplates().Where(pt => pt.ViewPath == "CategoryTemplate.ProductsInGridOrLines").FirstOrDefault();

            #region category definitions

            var categoryFurniture = new Category
            {
                Name = "مبلمان",
                Alias = "Furniture",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("category/furniture.jpg"),
                Published = true,
                DisplayOrder = 1,
                MetaTitle = "مبلمان",
                ShowOnHomePage = true
            };

            var categoryApple = new Category
            {
                Name = "اپل",
                Alias = "Apple",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("category/apple.png"),
                Published = true,
                DisplayOrder = 1,
                MetaTitle = "اپل",
                ShowOnHomePage = true
            };

            var categorySports = new Category
            {
                Name = "ورزشی",
                Alias = "Sports",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("category/sports.jpg"),
                Published = true,
                DisplayOrder = 1,
                MetaTitle = "ورزشی",
                ShowOnHomePage = true
            };

            var categoryBooks = new Category
            {
                Name = "کتاب",
                Alias = "Books",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("category/emblem_library.png", GetSeName("Books")),
                Published = true,
                DisplayOrder = 1,
                MetaTitle = "کتاب"
            };

            //var categoryComputers = new Category
            //{
            //	Name = "Computers",
            //             Alias = "Computers",
            //	CategoryTemplateId = categoryTemplateInGridAndLines.Id,
            //             Picture = CreatePicture("category/computers.png"),
            //	Published = true,
            //	DisplayOrder = 2,
            //	MetaTitle = "Computers"
            //};

            var categoryFashion = new Category
            {
                Name = "پوشاک",
                Alias = "Fashion",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("category/fashion.jpg"),
                Published = true,
                DisplayOrder = 2,
                MetaTitle = "پوشاک",
                ShowOnHomePage = true,
                BadgeText = "فروش ویژه",
                BadgeStyle = 4
            };

            var categoryGaming = new Category
            {
                Name = "بازی",
                Alias = "Gaming",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("product/ps4_bundle_minecraft.jpg", GetSeName("Gaming")),
                Published = true,
                DisplayOrder = 3,
                ShowOnHomePage = true,
                MetaTitle = "بازی"
            };

            //var categoryCellPhones = new Category
            //{
            //	Name = "Cell phones",
            //             Alias = "Cell phones",
            //	CategoryTemplateId = categoryTemplateInGridAndLines.Id,
            //	//ParentCategoryId = categoryElectronics.Id,
            //             Picture = CreatePicture("category/cellphone.png"),
            //	Published = true,
            //	DisplayOrder = 4,
            //	MetaTitle = "Cell phones"
            //};

            var categoryDigitalDownloads = new Category
            {
                Name = "محصولات دیجیتال",
                Alias = "Digital Products",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("category/digitalproducts.jpg"),
                Published = true,
                DisplayOrder = 6,
                MetaTitle = "محصولات دیجیتال",
                ShowOnHomePage = true
            };

            var categoryGiftCards = new Category
            {
                Name = "گیفت کارت",
                Alias = "Gift Cards",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("category/gift-cards.png"),
                Published = true,
                DisplayOrder = 12,
                MetaTitle = "گیفت کارت",
                ShowOnHomePage = true,
            };

            var categoryWatches = new Category
            {
                Name = "ساعت",
                Alias = "Watches",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("category/watches.png"),
                Published = true,
                DisplayOrder = 10,
                MetaTitle = "ساعت",
                ShowOnHomePage = true,
                BadgeText = "%",
                BadgeStyle = 5
            };

            #endregion

            var entities = new List<Category>
            {
                categoryApple, categorySports, categoryBooks, categoryFurniture, categoryDigitalDownloads, categoryGaming,
                categoryGiftCards, categoryFashion, categoryWatches
            };

            this.Alter(entities);
            return entities;
        }

        public IList<Category> CategoriesSecondLevel()
        {
            var sampleImagesPath = this._sampleImagesPath;
            var categoryTemplateInGridAndLines = this.CategoryTemplates().Where(pt => pt.ViewPath == "CategoryTemplate.ProductsInGridOrLines").FirstOrDefault();

            #region category definitions

            #region new

            var categoryFashionJackets = new Category
            {
                Name = "ژاکت",
                Alias = "Jackets",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("category/jackets.jpg"),
                Published = true,
                ParentCategoryId = _ctx.Set<Category>().Where(x => x.Alias == "Fashion").First().Id,
                DisplayOrder = 1,
                MetaTitle = "ژاکت",
                ShowOnHomePage = true
            };

            var categoryFashionLeatherJackets = new Category
            {
                Name = "کت چرمی",
                Alias = "Leather jackets",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("category/leather_jackets.jpg"),
                Published = true,
                ParentCategoryId = _ctx.Set<Category>().Where(x => x.Alias == "Fashion").First().Id,
                DisplayOrder = 1,
                MetaTitle = "کت چرمی",
                ShowOnHomePage = true
            };

            var categoryFashionShoes = new Category
            {
                Name = "کفش",
                Alias = "Shoes",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("category/shoes.png"),
                Published = true,
                ParentCategoryId = _ctx.Set<Category>().Where(x => x.Alias == "Fashion").First().Id,
                DisplayOrder = 1,
                MetaTitle = "کفش",
                ShowOnHomePage = true
            };

            var categoryFashionTrousers = new Category
            {
                Name = "شلوار",
                Alias = "Pants",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("category/trousers.png"),
                Published = true,
                ParentCategoryId = _ctx.Set<Category>().Where(x => x.Alias == "Fashion").First().Id,
                DisplayOrder = 1,
                MetaTitle = "شلوار",
                ShowOnHomePage = true
            };

            var categoryFashionTracksuits = new Category
            {
                Name = "ورزشی",
                Alias = "Tracksuits",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("category/tracksuit.png"),
                Published = true,
                ParentCategoryId = _ctx.Set<Category>().Where(x => x.Alias == "Fashion").First().Id,
                DisplayOrder = 1,
                MetaTitle = "ورزشی",
                ShowOnHomePage = true
            };

            #endregion



            var categorySportsGolf = new Category
            {
                Name = "گلف",
                Alias = "Golf",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("category/golf.jpg"),
                Published = true,
                ParentCategoryId = _ctx.Set<Category>().Where(x => x.Alias == "Sports").First().Id,
                DisplayOrder = 1,
                MetaTitle = "گلف",
                ShowOnHomePage = true
            };

            var categorySportsSunglasses = new Category
            {
                Name = "عینک آفتابی",
                Alias = "Sunglasses",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("category/glasses.png"),
                Published = true,
                ParentCategoryId = _ctx.Set<Category>().Where(x => x.Alias == "Fashion").First().Id,
                DisplayOrder = 1,
                MetaTitle = "عینک آفتابی",
                ShowOnHomePage = true
            };

            var categorySportsSoccer = new Category
            {
                Name = "فوتبال",
                Alias = "Soccer",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("category/soccer.png"),
                Published = true,
                ParentCategoryId = _ctx.Set<Category>().Where(x => x.Alias == "Sports").First().Id,
                DisplayOrder = 1,
                MetaTitle = "فوتبال",
                ShowOnHomePage = true
            };

            var categorySportsBasketball = new Category
            {
                Name = "بسکتبال",
                Alias = "Basketball",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("category/basketball.png"),
                Published = true,
                ParentCategoryId = _ctx.Set<Category>().Where(x => x.Alias == "Sports").First().Id,
                DisplayOrder = 1,
                MetaTitle = "بسکتبال",
                ShowOnHomePage = true
            };

            var categoryBooksSpiegel = new Category
            {
                Name = "پرفروش ها",
                Alias = "SPIEGEL-Bestseller",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("category/0000930_spiegel-bestseller.png", GetSeName("SPIEGEL-Bestseller")),
                Published = true,
                ParentCategoryId = _ctx.Set<Category>().Where(x => x.Alias == "Books").First().Id,
                DisplayOrder = 1,
                MetaTitle = "پرفروش ها"
            };

            var categoryBooksCookAndEnjoy = new Category
            {
                Name = "آشپزی",
                Alias = "Cook and enjoy",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                MediaFile = CreatePicture("category/0000936_kochen-geniesen.jpeg", GetSeName("Cook and enjoy")),
                Published = true,
                ParentCategoryId = _ctx.Set<Category>().Where(x => x.Alias == "Books").First().Id,
                DisplayOrder = 2,
                MetaTitle = "آشپزی"
            };

            //var categoryDesktops = new Category
            //{
            //	Name = "Desktops",
            //             Alias = "Desktops",
            //	CategoryTemplateId = categoryTemplateInGridAndLines.Id,
            //	ParentCategoryId = _ctx.Set<Category>().Where(x => x.Alias == "Computers").First().Id,
            //	Picture = CreatePicture("category/desktops.png"),
            //	Published = true,
            //	DisplayOrder = 1,
            //	MetaTitle = "Desktops"
            //};

            //var categoryNotebooks = new Category
            //{
            //	Name = "Notebooks",
            //             Alias = "Notebooks",
            //	CategoryTemplateId = categoryTemplateInGridAndLines.Id,
            //	ParentCategoryId = _ctx.Set<Category>().Where(x => x.Alias == "Computers").First().Id,
            //             Picture = CreatePicture("category/notebooks.png"),
            //	Published = true,
            //	DisplayOrder = 2,
            //	MetaTitle = "Notebooks"
            //};

            var categoryGamingAccessories = new Category
            {
                Name = "لوازم جانبی بازی",
                Alias = "Gaming Accessories",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                ParentCategoryId = _ctx.Set<Category>().Where(x => x.Alias == "Gaming").First().Id,
                MediaFile = CreatePicture("category/gaming_accessories.png"),
                Published = true,
                DisplayOrder = 2,
                MetaTitle = "لوازم جانبی بازی"
            };

            var categoryGamingGames = new Category
            {
                Name = "بازی",
                Alias = "Games",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                ParentCategoryId = _ctx.Set<Category>().Where(x => x.Alias == "Gaming").First().Id,
                MediaFile = CreatePicture("category/games.jpg"),
                Published = true,
                DisplayOrder = 3,
                MetaTitle = "بازی"
            };

            #endregion

            var entities = new List<Category>
            {
                categorySportsSunglasses,categorySportsSoccer, categorySportsBasketball,categorySportsGolf, categoryBooksSpiegel, categoryBooksCookAndEnjoy,
                categoryGamingAccessories, categoryGamingGames, categoryFashionJackets, categoryFashionLeatherJackets, categoryFashionShoes, categoryFashionTrousers,
                categoryFashionTracksuits
            };

            this.Alter(entities);
            return entities;
        }
    }
}
