using System;
using System.Linq;
using System.Collections.Generic;
using SmartStore.Core.Domain.Catalog;
using System.IO;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Directory;

namespace SmartStore.Data.Setup
{
    public abstract partial class InvariantSeedData
    {
        public IList<ProductTag> ProductTags()
        {
            #region tag apple
            var productTagApple = new ProductTag
            {
                Name = "apple"
            };

            _ctx.Set<Product>().Where(pt => pt.MetaTitle == "iPhone Plus").First().ProductTags.Add(productTagApple);

            #endregion tag apple

            #region tag gift
            var productTagGift = new ProductTag
            {
                Name = "gift"
            };

            _ctx.Set<Product>().Where(pt => pt.MetaTitle == "$10 Virtual Gift Card").First().ProductTags.Add(productTagGift);
            _ctx.Set<Product>().Where(pt => pt.MetaTitle == "$25 Virtual Gift Card").First().ProductTags.Add(productTagGift);
            _ctx.Set<Product>().Where(pt => pt.MetaTitle == "$50 Virtual Gift Card").First().ProductTags.Add(productTagGift);
            _ctx.Set<Product>().Where(pt => pt.MetaTitle == "$100 Virtual Gift Card").First().ProductTags.Add(productTagGift);

            #endregion tag gift

            #region tag book
            var productTagBook = new ProductTag
            {
                Name = "book"
            };

            _ctx.Set<Product>().Where(pt => pt.MetaTitle == "Überman: The novel").First().ProductTags.Add(productTagBook);
            _ctx.Set<Product>().Where(pt => pt.MetaTitle == "Best Grilling Recipes").First().ProductTags.Add(productTagBook);
            _ctx.Set<Product>().Where(pt => pt.MetaTitle == "Car of superlatives").First().ProductTags.Add(productTagBook);
            _ctx.Set<Product>().Where(pt => pt.MetaTitle == "Picture Atlas Motorcycles").First().ProductTags.Add(productTagBook);
            _ctx.Set<Product>().Where(pt => pt.MetaTitle == "The Car Book").First().ProductTags.Add(productTagBook);
            _ctx.Set<Product>().Where(pt => pt.MetaTitle == "Fast Cars").First().ProductTags.Add(productTagBook);
            _ctx.Set<Product>().Where(pt => pt.MetaTitle == "Motorcycle Adventures").First().ProductTags.Add(productTagBook);

            #endregion tag book

            #region tag cooking
            var productTagCooking = new ProductTag
            {
                Name = "cooking"
            };

            _ctx.Set<Product>().Where(pt => pt.MetaTitle == "Überman: The novel").FirstOrDefault().ProductTags.Add(productTagCooking);
            _ctx.Set<Product>().Where(pt => pt.MetaTitle == "Best Grilling Recipes").FirstOrDefault().ProductTags.Add(productTagCooking);

            #endregion tag cooking

            #region tag cars
            var productTagCars = new ProductTag
            {
                Name = "cars"
            };

            _ctx.Set<Product>().Where(pt => pt.MetaTitle == "The Car Book").FirstOrDefault().ProductTags.Add(productTagCars);
            _ctx.Set<Product>().Where(pt => pt.MetaTitle == "Fast Cars").FirstOrDefault().ProductTags.Add(productTagCars);

            #endregion tag cars

            #region tag motorbikes
            var productTagMotorbikes = new ProductTag
            {
                Name = "motorbikes"
            };

            _ctx.Set<Product>().Where(pt => pt.MetaTitle == "Fast Cars").FirstOrDefault().ProductTags.Add(productTagMotorbikes);
            _ctx.Set<Product>().Where(pt => pt.MetaTitle == "Motorcycle Adventures").FirstOrDefault().ProductTags.Add(productTagMotorbikes);

            #endregion tag motorbikes

            #region tag mp3
            var productTagMP3 = new ProductTag
            {
                Name = "mp3"
            };

            _ctx.Set<Product>().Where(pt => pt.MetaTitle == "Antonio Vivaldi: spring").FirstOrDefault().ProductTags.Add(productTagMP3);
            _ctx.Set<Product>().Where(pt => pt.MetaTitle == "Ludwig van Beethoven: Für Elise").FirstOrDefault().ProductTags.Add(productTagMP3);

            #endregion tag mp3

            #region tag download
            var productTagDownload = new ProductTag
            {
                Name = "download"
            };

            _ctx.Set<Product>().Where(pt => pt.MetaTitle == "Antonio Vivaldi: spring").FirstOrDefault().ProductTags.Add(productTagDownload);
            _ctx.Set<Product>().Where(pt => pt.MetaTitle == "Ludwig van Beethoven: Für Elise").FirstOrDefault().ProductTags.Add(productTagDownload);

            #endregion tag download

            #region tag watches
            var productTagWatches = new ProductTag
            {
                Name = "watches"
            };

            _ctx.Set<Product>().Where(pt => pt.MetaTitle == "Certina DS Podium Big Size").FirstOrDefault().ProductTags.Add(productTagWatches);

            #endregion tag download

            var entities = new List<ProductTag>
            {
               productTagGift, productTagBook, productTagCooking, productTagCars, productTagMotorbikes,
               productTagMP3, productTagDownload
            };

            this.Alter(entities);
            return entities;
        }

        private List<Product> GetFashionProducts(Dictionary<int, SpecificationAttribute> specAttributes)
        {
            var result = new List<Product>();
            var productTemplateSimple = _ctx.Set<ProductTemplate>().First(x => x.ViewPath == "Product");
            var firstDeliveryTime = _ctx.Set<DeliveryTime>().First(x => x.DisplayOrder == 0);
            var fashionCategory = _ctx.Set<Category>().First(x => x.Alias == "Fashion");
            var specialPriceEndDate = DateTime.UtcNow.AddMonths(1);
            var specOptionCotton = _ctx.Set<SpecificationAttribute>().First(x => x.DisplayOrder == 8).SpecificationAttributeOptions.First(x => x.DisplayOrder == 9);

            // Converse All Star
            var converseAllStar = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Converse All Star",
                MetaTitle = "Converse All Star",
                ShortDescription = "The classical sneaker!",
                FullDescription = "<p>Since 1912 and to this day unrivalled: the converse All Star sneaker. A shoe for every occasion.</p>",
                Sku = "Fashion-112355",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Published = true,
                ShowOnHomePage = true,
                Price = 79.90M,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                IsShipEnabled = true,
                DeliveryTime = firstDeliveryTime,
                DisplayOrder = 1
            };

            converseAllStar.ProductCategories.Add(new ProductCategory
            {
                Category = fashionCategory,
                DisplayOrder = 1
            });

            var allStarImages = new string[] { "product/allstar_converse.jpg", "product/allstar_hi_charcoal.jpg", "product/allstar_hi_maroon.jpg", "product/allstar_hi_navy.jpg",
                "product/allstar_hi_purple.jpg", "product/allstar_hi_white.jpg" };

            for (var i = 0; i < allStarImages.Length; ++i)
            {
                converseAllStar.ProductPictures.Add(new ProductMediaFile
                {
                    MediaFile = CreatePicture(allStarImages[i]),
                    DisplayOrder = i + 1
                });
            }

            converseAllStar.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                SpecificationAttributeOption = specAttributes[8].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 9)
            });

            result.Add(converseAllStar);

            // Shirt Meccanica
            var shirtMeccanica = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Sleeveless shirt Meccanica",
                MetaTitle = "Sleeveless shirt Meccanica",
                ShortDescription = "Woman shirt with trendy imprint",
                FullDescription = "<p>Also in summer, the Ducati goes with fashion style! With the sleeveless shirt Meccanica, every woman can express her passion for Ducati with a comfortable and versatile piece of clothing. The shirt is available in black and vintage red. It carries on the front the traditional lettering in plastisol print, which makes it even clearer and more radiant, while on the back in the neck area is the famous logo with the typical \"wings\" of the fifties.</p>",
                Sku = "Fashion-987693502",
                ManufacturerPartNumber = "987693502",
                Gtin = "987693502",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Published = true,
                Price = 38.00M,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                IsShipEnabled = true,
                DeliveryTime = firstDeliveryTime,
                DisplayOrder = 4
            };

            shirtMeccanica.ProductCategories.Add(new ProductCategory
            {
                Category = fashionCategory,
                DisplayOrder = 1
            });

            shirtMeccanica.TierPrices.Add(new TierPrice
            {
                Quantity = 10,
                Price = 36.00M
            });
            shirtMeccanica.TierPrices.Add(new TierPrice
            {
                Quantity = 50,
                Price = 29.00M
            });

            var shirtMeccanicaImages = new string[] { "product/shirt_meccanica_black_1.jpg", "product/shirt_meccanica_black_2.jpg", "product/shirt_meccanica_black_3.jpg",
                "product/shirt_meccanica_red_1.jpg", "product/shirt_meccanica_red_2.jpg", "product/shirt_meccanica_red_3.jpg", "product/shirt_meccanica_red_4.jpg"  };

            for (var i = 0; i < shirtMeccanicaImages.Length; ++i)
            {
                shirtMeccanica.ProductPictures.Add(new ProductMediaFile
                {
                    MediaFile = CreatePicture(shirtMeccanicaImages[i]),
                    DisplayOrder = i + 1
                });
            }

            shirtMeccanica.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                SpecificationAttributeOption = specAttributes[8].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 9)
            });

            result.Add(shirtMeccanica);

            // Ladies jacket
            var ladiesJacket = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Ladies Sports Jacket",
                MetaTitle = "Ladies Sports Jacket",
                FullDescription = "<p>Lightweight wind and water repellent fabric, lining of soft single jersey knit cuffs on arm and waistband. 2 side pockets with zipper, hood in slightly waisted cut.</p><ul><il>Material: 100% polyamide</il><il>Lining: 65% polyester, 35% cotton</il><il>Lining 2: 100% polyester.</il></ul>",
                Sku = "Fashion-JN1107",
                ManufacturerPartNumber = "JN1107",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Published = true,
                Price = 55.00M,
                OldPrice = 60.00M,
                ProductCost = 20.00M,
                SpecialPrice = 52.99M,
                SpecialPriceStartDateTimeUtc = new DateTime(2017, 5, 1, 0, 0, 0),
                SpecialPriceEndDateTimeUtc = specialPriceEndDate,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                IsShipEnabled = true,
                DeliveryTime = firstDeliveryTime,
                DisplayOrder = 2
            };

            ladiesJacket.ProductCategories.Add(new ProductCategory
            {
                Category = fashionCategory,
                DisplayOrder = 1
            });

            var ladiesJacketImages = new string[] { "product/ladies_jacket_silver.jpg", "product/ladies_jacket_black.jpg", "product/ladies_jacket_red.jpg",
                "product/ladies_jacket_orange.jpg", "product/ladies_jacket_green.jpg", "product/ladies_jacket_blue.jpg", "product/ladies_jacket_navy.jpg",  };

            for (var i = 0; i < ladiesJacketImages.Length; ++i)
            {
                ladiesJacket.ProductPictures.Add(new ProductMediaFile
                {
                    MediaFile = CreatePicture(ladiesJacketImages[i]),
                    DisplayOrder = i + 1
                });
            }

            ladiesJacket.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                SpecificationAttributeOption = specAttributes[8].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 11)
            });

            result.Add(ladiesJacket);

            // Clark Premium Blue Jeans
            var clarkJeans = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Clark Premium Blue Jeans",
                MetaTitle = "Clark Premium Blue Jeans",
                ShortDescription = "Modern Jeans in Easy Comfort Fit",
                FullDescription = "<p>Real five-pocket jeans by Joker with additional, set-up pocket. Thanks to easy comfort fit with normal rise and comfortable leg width suitable for any character.</p><ul><li>Material: softer, lighter premium denim made of 100% cotton.</li><li>Waist (inch): 29-46</li><li>leg (inch): 30 to 38</li></ul>",
                Sku = "Fashion-65986524",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Published = true,
                Price = 109.90M,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                IsShipEnabled = true,
                DeliveryTime = firstDeliveryTime,
                DisplayOrder = 5
            };

            clarkJeans.ProductCategories.Add(new ProductCategory
            {
                Category = fashionCategory,
                DisplayOrder = 1
            });

            clarkJeans.ProductPictures.Add(new ProductMediaFile
            {
                MediaFile = CreatePicture("product/clark_premium_jeans.jpg"),
                DisplayOrder = 1
            });

            clarkJeans.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                SpecificationAttributeOption = specAttributes[8].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 9)
            });

            result.Add(clarkJeans);

            return result;
        }

        private List<Product> GetFurnitureProducts(Dictionary<int, SpecificationAttribute> specAttributes)
        {
            var result = new List<Product>();
            var productTemplateSimple = _ctx.Set<ProductTemplate>().First(x => x.ViewPath == "Product");
            var thirdDeliveryTime = _ctx.Set<DeliveryTime>().First(x => x.DisplayOrder == 2);
            var furnitureCategory = _ctx.Set<Category>().First(x => x.MetaTitle == "Furniture");

            // Le Corbusier LC 6 table
            var corbusierTable = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Le Corbusier LC 6 dining table (1929)",
                MetaTitle = "Le Corbusier LC 6 dining table (1929)",
                ShortDescription = "Dining table LC 6, designer: Le Corbusier, W x H x D: 225 x 69/74 (adjustable) x 85 cm, substructure: steel pipe, glass plate: Clear or sandblasted, 15 or 19 mm, height-adjustable.",
                FullDescription = "<p>Four small plates carry a glass plate. The structure of the steel pipe is covered in clear structures. The LC6 is a true classic of Bauhaus art and is used in combination with the swivel chairs LC7 as a form-beautiful Le Corbusier dining area. In addition, the table is also increasingly found in offices or in halls. It is height-adjustable and can thus be perfectly adapted to the respective purpose.</p><p>Le Corbusier's beautifully shaped table is available with a clear or sandblasted glass plate. The substructure consists of oval steel tubes.</p>",
                Sku = "Furniture-lc6",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Published = true,
                Price = 749.00M,
                HasTierPrices = true,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                IsShipEnabled = true,
                DeliveryTime = thirdDeliveryTime
            };

            corbusierTable.ProductCategories.Add(new ProductCategory
            {
                Category = furnitureCategory,
                DisplayOrder = 1
            });

            var corbusierTableImages = new string[] { "product/corbusier_lc6_table_1.jpg", "product/corbusier_lc6_table_2.jpg", "product/corbusier_lc6_table_3.jpg",
                "product/corbusier_lc6_table_4.jpg" };

            for (var i = 0; i < corbusierTableImages.Length; ++i)
            {
                corbusierTable.ProductPictures.Add(new ProductMediaFile
                {
                    MediaFile = CreatePicture(corbusierTableImages[i]),
                    DisplayOrder = i + 1
                });
            }

            corbusierTable.TierPrices.Add(new TierPrice
            {
                Quantity = 2,
                Price = 647.10M
            });
            corbusierTable.TierPrices.Add(new TierPrice
            {
                Quantity = 4,
                Price = 636.65M
            });

            corbusierTable.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                SpecificationAttributeOption = specAttributes[8].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });
            corbusierTable.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 2,
                SpecificationAttributeOption = specAttributes[8].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 14)
            });

            result.Add(corbusierTable);

            // Ball Chair
            var ballChair = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Eero Aarnio Ball Chair (1966)",
                MetaTitle = "Eero Aarnio Ball Chair (1966)",
                FullDescription = "<p>The ball chair, or also called the globe chair, is a real masterpiece of the legendary designer Eero Aarnio. The ball chair from the Sixties has written designer history. The egg designed armchair rests on a trumpet foot and is not lastly appreciated due to its shape and the quiet atmosphere inside this furniture. The design of the furniture body allows noise and disturbing outer world elements in the Hintergurnd us. A place as created for resting and relaxing. With its wide range of colours, the eyeball chair fits in every living and working environment. A chair that stands out for its timeless design and always has the modern look. The ball chair is 360° to rotate to change the view of the surroundings. The outer shell in fiberglass white or black. The upholstery is mixed in leather or linen.</p><p>Dimension: Width 102 cm, depth 87 cm, height 124 cm, seat height: 44 cm.</p>",
                Sku = "Furniture-ball-chair",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Published = true,
                Price = 2199.00M,
                HasTierPrices = true,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                IsShipEnabled = true,
                DeliveryTime = thirdDeliveryTime
            };

            ballChair.ProductCategories.Add(new ProductCategory
            {
                Category = furnitureCategory,
                DisplayOrder = 1
            });

            ballChair.ProductPictures.Add(new ProductMediaFile
            {
                MediaFile = CreatePicture("product/ball_chair_white.jpg"),
                DisplayOrder = 1
            });
            ballChair.ProductPictures.Add(new ProductMediaFile
            {
                MediaFile = CreatePicture("product/ball_chair_black.jpg"),
                DisplayOrder = 2
            });

            ballChair.TierPrices.Add(new TierPrice
            {
                Quantity = 2,
                Price = 1979.10M
            });
            ballChair.TierPrices.Add(new TierPrice
            {
                Quantity = 4,
                Price = 1869.15M
            });

            ballChair.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                SpecificationAttributeOption = specAttributes[8].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 3)
            });
            ballChair.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 2,
                SpecificationAttributeOption = specAttributes[8].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 5)
            });

            result.Add(ballChair);

            // Lounge chair
            var loungeChair = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Charles Eames Lounge Chair (1956)",
                MetaTitle = "Charles Eames Lounge Chair (1956)",
                ShortDescription = "Club lounge chair, designer: Charles Eames, width 80 cm, depth 80 cm, height 60 cm, seat shell: plywood, foot (rotatable): Aluminium casting, cushion (upholstered) with leather cover.",
                FullDescription = "<p>That's how you sit in a baseball glove. In any case, this was one of the ideas Charles Eames had in mind when designing this club chair. The lounge chair should be a comfort armchair, in which one can sink luxuriously. Through the construction of three interconnected, movable seat shells and a comfortable upholstery Charles Eames succeeded in the implementation. In fact, the club armchair with a swiveling foot is a contrast to the Bauhaus characteristics that emphasized minimalism and functionality. Nevertheless, he became a classic of Bauhaus history and still provides in many living rooms and clubs for absolute comfort with style.</p><p>Dimensions: Width 80 cm, depth 60 cm, height total 80 cm (height backrest: 60 cm). CBM: 0.70.</p><p>Lounge chair with seat shell of laminated curved plywood with rosewood veneer, walnut nature or in black. Rotatable base made of aluminium cast black with polished edges or optionally fully chromed. Elaborate upholstery of pillows in leather.</p><p>All upholstery units are removable at the Eames Lounge chair (seat, armrest, backrest, headrest).</p>",
                Sku = "Furniture-lounge-chair",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Published = true,
                ShowOnHomePage = true,
                Price = 1799.00M,
                OldPrice = 1999.00M,
                HasTierPrices = true,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                IsShipEnabled = true,
                DeliveryTime = thirdDeliveryTime
            };

            loungeChair.ProductCategories.Add(new ProductCategory
            {
                Category = furnitureCategory,
                DisplayOrder = 1
            });

            loungeChair.ProductPictures.Add(new ProductMediaFile
            {
                MediaFile = CreatePicture("product/charles_eames_lounge_chair_white.jpg"),
                DisplayOrder = 1
            });
            loungeChair.ProductPictures.Add(new ProductMediaFile
            {
                MediaFile = CreatePicture("product/charles_eames_lounge_chair_black.jpg"),
                DisplayOrder = 2
            });

            loungeChair.TierPrices.Add(new TierPrice
            {
                Quantity = 2,
                Price = 1709.05M
            });
            loungeChair.TierPrices.Add(new TierPrice
            {
                Quantity = 4,
                Price = 1664.08M
            });
            loungeChair.TierPrices.Add(new TierPrice
            {
                Quantity = 6,
                Price = 1619.10M
            });

            loungeChair.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                SpecificationAttributeOption = specAttributes[8].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 13)
            });
            loungeChair.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 2,
                SpecificationAttributeOption = specAttributes[8].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 5)
            });
            loungeChair.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                SpecificationAttributeOption = specAttributes[8].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 4)
            });

            result.Add(loungeChair);

            // Cube chair
            var cubeChair = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Josef Hoffmann cube chair (1910)",
                MetaTitle = "Josef Hoffmann cube chair (1910)",
                ShortDescription = "Armchair Cube, Designer: Josef Hoffmann, width 93 cm, depth 72 cm, height 77 cm, basic frame: solid beech wood, upholstery: solid polyurethane foam (shape resistant), Upholstery: leather",
                FullDescription = "<p>The cube chair by Josef Hoffmann holds what the name promises and that is the same in two respects. It consists of many squares, both in terms of construction and in relation to the design of the surface. In addition, the cube, with its purely geometric form, was a kind of harbinger of cubism. The chair by Josef Hoffmann was designed in 1910 and still stands today as a replica in numerous business and residential areas.</p><p>Originally, the cube was a club chair. Together with the two-and three-seater sofa of the series, a cosy sitting area with a sophisticated charisma is created. The basic frame of the armchair is made of wood. The form-resistant upholstery is covered with leather and has been shaped visually to squares with a special sewing.</p><p>Dimensions: Width 93 cm, depth 72 cm, height 77 cm. CBM: 0.70.</p>",
                Sku = "Furniture-cube-chair",
                ProductTemplateId = productTemplateSimple.Id,
                AllowCustomerReviews = true,
                Published = true,
                ShowOnHomePage = true,
                Price = 2299.00M,
                HasTierPrices = true,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                IsShipEnabled = true,
                DeliveryTime = thirdDeliveryTime
            };

            cubeChair.ProductCategories.Add(new ProductCategory
            {
                Category = furnitureCategory,
                DisplayOrder = 1
            });

            cubeChair.ProductPictures.Add(new ProductMediaFile
            {
                MediaFile = CreatePicture("product/hoffmann_cube_chair_black.jpg"),
                DisplayOrder = 1
            });

            cubeChair.TierPrices.Add(new TierPrice
            {
                Quantity = 4,
                Price = 1899.05M
            });
            cubeChair.TierPrices.Add(new TierPrice
            {
                Quantity = 6,
                Price = 1799.10M
            });

            cubeChair.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                SpecificationAttributeOption = specAttributes[8].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 5)
            });

            result.Add(cubeChair);

            return result;
        }

        public IList<Product> Products()
        {
            var specialPriceEndDate = DateTime.UtcNow.AddMonths(1);

            var productTemplate = _ctx.Set<ProductTemplate>().First(x => x.ViewPath == "Product");
            var firstDeliveryTime = _ctx.Set<DeliveryTime>().First(x => x.DisplayOrder == 0);
            var secondDeliveryTime = _ctx.Set<DeliveryTime>().First(x => x.DisplayOrder == 1);
            var thirdDeliveryTime = _ctx.Set<DeliveryTime>().First(x => x.DisplayOrder == 2);

            var manufacturers = _ctx.Set<Manufacturer>().ToList().ToDictionarySafe(x => x.Name, x => x);
            var categories = _ctx.Set<Category>().ToList().ToDictionarySafe(x => x.Alias, x => x);
            var specAttributes = _ctx.Set<SpecificationAttribute>().ToList().ToDictionarySafe(x => x.DisplayOrder, x => x);


            //#region category shoes

            //var categoryShoes = _ctx.Set<Category>().First(x => x.Alias == "Shoes");

            //#region product Jack Wolfskin COOGEE LOW M

            //var productJackWolfskinCooGeeLowM = new Product
            //{
            //    ProductType = ProductType.SimpleProduct,
            //    Name = "COOGEE LOW M",
            //    MetaTitle = "COOGEE LOW M",
            //    ShortDescription = "MÄNNER FREIZEITSCHUHE",
            //    FullDescription = "<p>Du bist immer auf dem Sprung: zum Kino, zur neueröffneten Bar oder zum nächsten Stadtfest. Der stylische COOGEE LOW ist DER Schuh für dein Leben in der Stadt. Denn er verbindet Funktion mit Style. Die sportliche EVA - Zwischensohle ist nicht nur leicht und flexibel, sondern dämpft deine Ferse mit ihrer Erhöhung im hinteren Bereich auch besonders gut und ist sehr komfortabel.So sind deine Füße auch nach vielen Stunden noch fit – du wirst es merken, wenn nach einem langen Tag z.B.noch eine spontane Party ansteht. Die Passform haben wir nach dem Vorbild einer Socke gestaltet, sie ist locker und komfortabel.Dennoch bleibt der Schuh in seiner schmalen Form. Dank des atmungsaktiven Obermaterials bleibt das Fußklima angenehm, egal, wie warm es draussen ist.Gleichzeitig ist der Schuh sehr pflegeleicht.Worauf wartest du also noch ? Ab in die City!</p>",
            //    Sku = "Wolfskin-4032541",
            //    ProductTemplateId = productTemplate.Id,
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    Price = 69.90M,
            //    OldPrice = 99.95M,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    IsShipEnabled = true,
            //    DeliveryTime = firstDeliveryTime,
            //    DisplayOrder = 5
            //};

            //AddProductPicture(productJackWolfskinCooGeeLowM, "product/wolfskin_shoes_coogee_1.jpg", "jack-wolfskin-shoes-coogee-low-m-1");

            //productJackWolfskinCooGeeLowM.ProductCategories.Add(new ProductCategory { Category = categoryShoes, DisplayOrder = 1 });

            //productJackWolfskinCooGeeLowM.ProductManufacturers.Add(new ProductManufacturer
            //{
            //    Manufacturer = _ctx.Set<Manufacturer>().Where(c => c.Name == "Jack-Wolfskin").Single(),
            //    DisplayOrder = 1,
            //});

            //#endregion product Jack Wolfskin COOGEE LOW M

            //#region product Adidas SUPERSTAR SCHUH

            //var productAdidasSuperstarSchuh = new Product
            //{
            //    ProductType = ProductType.SimpleProduct,
            //    Name = "SUPERSTAR SCHUH",
            //    MetaTitle = "SUPERSTAR SCHUH",
            //    ShortDescription = "DER STREETWEAR-KLASSIKER MIT DER SHELL TOE.",
            //    FullDescription = "<p>Der adidas Superstar wurde erstmals 1969 herausgebracht und machte seinem Namen schon bald alle Ehre. Heute gilt er als Streetstyle-Legende. In dieser Version kommt der Schuh mit einem bequemen Obermaterial aus Full-Grain-Leder. Perfekt wird der Look durch die klassische Shell Toe aus Gummi für mehr Strapazierfähigkeit.</p>",
            //    Sku = "Adidas-C77124",
            //    ProductTemplateId = productTemplate.Id,
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    Price = 99.95M,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    IsShipEnabled = true,
            //    DeliveryTime = firstDeliveryTime,
            //    DisplayOrder = 5
            //};

            //AddProductPicture(productAdidasSuperstarSchuh, "product/adidas_superstar_schuh_1.jpg", "adidas-superstar-schuh-1");

            //productJackWolfskinCooGeeLowM.ProductCategories.Add(new ProductCategory { Category = categoryShoes, DisplayOrder = 1 });

            //productJackWolfskinCooGeeLowM.ProductManufacturers.Add(new ProductManufacturer
            //{
            //    Manufacturer = _ctx.Set<Manufacturer>().Where(c => c.Name == "Adidas").Single(),
            //    DisplayOrder = 1,
            //});


            //#endregion product Adidas SUPERSTAR SCHUH
            //#endregion category shoes

            #region category jackets

            var categoryJackets = _ctx.Set<Category>().First(x => x.Alias == "Jackets");

            #region product Jack Wolfskin KANUKA POINT

            var productJackWolfsKinKanukaPoint = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "KANUKA POINT JACKET M",
                MetaTitle = "KANUKA POINT JACKET M",
                ShortDescription = "SOFTSHELLJACKE MÄNNER",
                FullDescription = "<p>Sportliches Design für sportliche Touren: Die KANUKA POINT ist so gern in Bewegung wie du. Die Softshelljacke besteht aus superelastischem und sehr atmungsaktivem Material, das sich unterwegs jeder deiner Bewegungen anpasst. Deswegen nimmst du mit der KANUKA POINT jeden Pass mit Leichtigkeit.Und musst dir auch bei Kraxeleien zum Gipfel keine Gedanken um deine Jacke machen, denn ihr Material hält einiges aus.Auch bei Wind und leichten Schauern bleibst du gelassen.</p>",
                Sku = "jack-1305851",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                Price = 83.90M,
                OldPrice = 119.95M,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                IsShipEnabled = true,
                DeliveryTime = firstDeliveryTime,
                DisplayOrder = 5
            };

            AddProductPicture(productJackWolfsKinKanukaPoint, "product/jack_wolfskin_kanuka_point_1.png");

            productJackWolfsKinKanukaPoint.ProductCategories.Add(new ProductCategory { Category = categoryJackets, DisplayOrder = 1 });

            productJackWolfsKinKanukaPoint.ProductManufacturers.Add(new ProductManufacturer
            {
                Manufacturer = _ctx.Set<Manufacturer>().Where(c => c.Name == "Jack-Wolfskin").Single(),
                DisplayOrder = 1,
            });

            #endregion product Jack Wolfskin KANUKA POINT
            #endregion category jackets


            #region category golf

            #region product Titleist SM6 Tour Chrome

            var productTitleistSM6TourChrome = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Titleist SM6 Tour Chrome",
                IsEsd = false,
                ShortDescription = "For golfers who want maximum impact control and feedback.",
                FullDescription = "​​<p><strong>Inspired by the best iron players in the world</strong> </p> <p>The new 'Spin Milled 6' wages establish a new performance class in three key areas of the Wedge game: precise length steps, bounce and maximum spin. </p> <p>   <br />   For each loft the center of gravity of the wedge is determined individually. Therefore, the SM6 offers a particularly precise length and flight curve control combined with great impact.   <br />   Bob Vokey's tourer-puffed sole cleat allows all golfers more bounce, adapted to their personal swing profile and the respective ground conditions. </p> <p>   <br />   A new, parallel face texture was developed for the absolutely exact and with 100% quality control machined grooves. The result is a consistently higher edge sharpness for more spin. </p> <p> </p> <ul>   <li>Precise lengths and flight curve control thanks to progressively placed center of gravity.</li>   <li>Improved bounce due to Bob Vokey's proven soles.</li>   <li>TX4 grooves produce more spin through a new surface and edge sharpness.</li>   <li>Multiple personalization options.</li> </ul> ",
                Sku = "P-7004",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Titleist SM6 Tour Chrome",
                Price = 164.95M,
                OldPrice = 199.95M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = thirdDeliveryTime.Id
            };

            AddProductPicture(productTitleistSM6TourChrome, "product/titleist_sm6_tour_chrome.jpg");

            productTitleistSM6TourChrome.ProductCategories.Add(new ProductCategory { Category = categories["Golf"], DisplayOrder = 1 });

            productTitleistSM6TourChrome.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Titleist"], DisplayOrder = 1 });

            #endregion product Titleist SM6 Tour Chrome

            #region product Titleist Pro V1x

            var productTitleistProV1x = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Titleist Pro V1x",
                IsEsd = false,
                ShortDescription = "Golf ball with high ball flight",
                FullDescription = "​​The top players rely on the new Titleist Pro V1x. High ball flight, soft feel and more spin in the short game are the advantages of the V1x version. Perfect performance from the leading manufacturer. The new Titleist Pro V1 golf ball is exactly defined and promises penetrating ball flight with very soft hit feeling.",
                Sku = "P-7001",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Titleist Pro V1x",
                Price = 2.1M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = thirdDeliveryTime.Id
            };

            AddProductPicture(productTitleistProV1x, "product/titleist-pro-v1x.jpg");

            productTitleistProV1x.ProductCategories.Add(new ProductCategory { Category = categories["Golf"], DisplayOrder = 1 });

            productTitleistProV1x.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Titleist"], DisplayOrder = 1 });

            #endregion product Titleist Pro V1x

            #region product Supreme Golfball

            var productSupremeGolfball = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Supreme Golfball",
                IsEsd = false,
                ShortDescription = "Training balls with perfect flying characteristics",
                FullDescription = "​Perfect golf exercise ball with the characteristics like the 'original', but in a glass-fracture-proof execution. Massive core, an ideal training ball for yard and garden. Colors: white, yellow, orange.",
                Sku = "P-7002",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Supreme Golfball",
                Price = 1.9M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = thirdDeliveryTime.Id
            };

            AddProductPicture(productSupremeGolfball, "product/supremeGolfball_1.jpg");
            AddProductPicture(productSupremeGolfball, "product/supremeGolfball_2.jpg");

            productSupremeGolfball.ProductCategories.Add(new ProductCategory { Category = categories["Golf"], DisplayOrder = 1 });

            productSupremeGolfball.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Titleist"], DisplayOrder = 1 });

            #endregion product Supreme Golfball

            #region product GBB Epic Sub Zero Driver

            var productGBBEpicSubZeroDriver = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "GBB Epic Sub Zero Driver",
                IsEsd = false,
                ShortDescription = "Low spin for good golfing!",
                FullDescription = "Your game wins with the GBB Epic Sub Zero Driver. A golf club with an extremely low spin and the phenomenal high-speed characteristic.",
                Sku = "P-7003",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "GBB Epic Sub Zero Driver",
                Price = 489M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = thirdDeliveryTime.Id
            };

            AddProductPicture(productGBBEpicSubZeroDriver, "product/gbb-epic-sub-zero-driver.jpg");

            productGBBEpicSubZeroDriver.ProductCategories.Add(new ProductCategory { Category = categories["Golf"], DisplayOrder = 1 });

            productGBBEpicSubZeroDriver.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Titleist"], DisplayOrder = 1 });

            #endregion product GBB Epic Sub Zero Driver

            #endregion category golf

            #region category Soccer

            #region product Nike Strike Football

            var productNikeStrikeFootball = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Nike Strike Football",
                IsEsd = false,
                ShortDescription = "GREAT TOUCH. HIGH VISIBILITY.",
                FullDescription = "<p><strong>Enhance play everyday, with the Nike Strike Football. </strong> </p> <p>Reinforced rubber retains its shape for confident and consistent control. A stand out Visual Power graphic in black, green and orange is best for ball tracking, despite dark or inclement conditions. </p> <ul>   <li>Visual Power graphic helps give a true read on flight trajectory.</li>   <li>Textured casing offers superior touch.</li>   <li>Reinforced rubber bladder supports air and shape retention.</li>   <li>66% rubber/ 15% polyurethane/ 13% polyester/ 7% EVA.</li> </ul> ",
                Sku = "P-5004",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Nike Strike Football",
                Price = 59.90M,
                OldPrice = 69.90M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = thirdDeliveryTime.Id,
                HasTierPrices = true
            };

            AddProductPicture(productNikeStrikeFootball, "product/nike-strike-football.jpg");

            productNikeStrikeFootball.ProductCategories.Add(new ProductCategory { Category = categories["Soccer"], DisplayOrder = 1 });

            productNikeStrikeFootball.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Nike"], DisplayOrder = 1 });

            productNikeStrikeFootball.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Manufacturer -> Nike
                SpecificationAttributeOption = specAttributes[20].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 20)
            });
            productNikeStrikeFootball.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Material -> rubber
                SpecificationAttributeOption = specAttributes[8].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 12)
            });

            productNikeStrikeFootball.TierPrices.Add(new TierPrice { Quantity = 6, Price = 26.90M });
            productNikeStrikeFootball.TierPrices.Add(new TierPrice { Quantity = 12, Price = 24.90M });
            productNikeStrikeFootball.TierPrices.Add(new TierPrice { Quantity = 24, Price = 22.90M });

            #endregion product Nike Strike Football

            #region product Evopower 5.3 Trainer HS Ball

            var productNikeEvoPowerBall = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Evopower 5.3 Trainer HS Ball",
                IsEsd = false,
                ShortDescription = "Entry level training ball.",
                FullDescription = "<p><strong>Entry level training ball.</strong></ p >< p > Constructed from 32 panels with equal surface areas for reduced seam-stress and a perfectly round shape.Handstitched panels with multilayered woven backing for enhanced stability and aerodynamics.</ p >",
                Sku = "P-5003",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Evopower 5.3 Trainer HS Ball",
                Price = 59.90M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = thirdDeliveryTime.Id
            };

            AddProductPicture(productNikeEvoPowerBall, "product/nike-vopower-53-trainer-hs-ball.jpg");

            productNikeEvoPowerBall.ProductCategories.Add(new ProductCategory { Category = categories["Soccer"], DisplayOrder = 1 });

            productNikeEvoPowerBall.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Nike"], DisplayOrder = 1 });

            productNikeEvoPowerBall.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Manufacturer -> Nike
                SpecificationAttributeOption = specAttributes[20].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 20)
            });
            productNikeEvoPowerBall.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Material -> leather
                SpecificationAttributeOption = specAttributes[8].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 5)
            });

            #endregion Evopower 5.3 Trainer HS Ball

            #region product Torfabrik official game ball

            var productTorfabrikOfficialGameBall = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Torfabrik official game ball",
                IsEsd = false,
                ShortDescription = "Available in different colors",
                FullDescription = "",
                Sku = "P-5002",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Torfabrik official game ball",
                Price = 59.90M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = thirdDeliveryTime.Id
            };

            AddProductPicture(productTorfabrikOfficialGameBall, "product/torfabrik-offizieller-spielball_white.png", "official-game-ball-white");
            AddProductPicture(productTorfabrikOfficialGameBall, "product/torfabrik-offizieller-spielball_red.png", "official-game-ball-red");
            AddProductPicture(productTorfabrikOfficialGameBall, "product/torfabrik-offizieller-spielball_yellow.png", "official-game-ball-yellow");
            AddProductPicture(productTorfabrikOfficialGameBall, "product/torfabrik-offizieller-spielball_blue.png", "official-game-ball-blue");
            AddProductPicture(productTorfabrikOfficialGameBall, "product/torfabrik-offizieller-spielball_green.png", "official-game-ball-green");

            productTorfabrikOfficialGameBall.ProductCategories.Add(new ProductCategory { Category = categories["Soccer"], DisplayOrder = 1 });

            productTorfabrikOfficialGameBall.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Adidas"], DisplayOrder = 1 });

            productTorfabrikOfficialGameBall.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Manufacturer -> Adidas
                SpecificationAttributeOption = specAttributes[20].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 19)
            });
            productTorfabrikOfficialGameBall.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Material -> leather
                SpecificationAttributeOption = specAttributes[8].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 5)
            });

            #endregion Torfabrik official game ball

            #region product Adidas TANGO SALA BALL

            var productAdidasTangoSalaBall = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Adidas TANGO SALA BALL",
                IsEsd = false,
                ShortDescription = "In different colors",
                FullDescription = "<p><strong>TANGO SALA BALL</strong>   <br />   A SALA BALL TO MATCH YOUR INDOOR PLAYMAKING. </p> <p>Take the game indoors. With a design nod to the original Tango ball that set the performance standard, this indoor soccer is designed for low rebound and enhanced control for futsal. Machine-stitched for a soft touch and high durability. </p> <ul>   <li>Machine-stitched for soft touch and high durability</li>   <li>Low rebound for enhanced ball control</li>   <li>Butyl bladder for best air retention</li>   <li>Requires inflation</li>   <li>100% natural rubber</li>   <li>Imported</li> </ul> <p> </p> ",
                Sku = "P-5001",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Adidas TANGO SALA BALL",
                Price = 59.90M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = thirdDeliveryTime.Id
            };

            AddProductPicture(productAdidasTangoSalaBall, "product/adidas-tango-pasadena-ball-white.png");
            AddProductPicture(productAdidasTangoSalaBall, "product/adidas-tango-pasadena-ball-yellow.jpg");
            AddProductPicture(productAdidasTangoSalaBall, "product/adidas-tango-pasadena-ball-red.jpg");
            AddProductPicture(productAdidasTangoSalaBall, "product/adidas-tango-pasadena-ball-green.jpg");
            AddProductPicture(productAdidasTangoSalaBall, "product/adidas-tango-pasadena-ball-gray.jpg");
            AddProductPicture(productAdidasTangoSalaBall, "product/adidas-tango-pasadena-ball-brown.jpg");
            AddProductPicture(productAdidasTangoSalaBall, "product/adidas-tango-pasadena-ball-blue.jpg");

            productAdidasTangoSalaBall.ProductCategories.Add(new ProductCategory { Category = categories["Soccer"], DisplayOrder = 1 });

            productAdidasTangoSalaBall.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Adidas"], DisplayOrder = 1 });

            productAdidasTangoSalaBall.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Manufacturer -> Adidas
                SpecificationAttributeOption = specAttributes[20].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 19)
            });
            productAdidasTangoSalaBall.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Material -> leather
                SpecificationAttributeOption = specAttributes[8].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 5)
            });

            #endregion Adidas TANGO SALA BALL

            #endregion category Soccer

            #region category Basketball

            #region Wilson Evolution High School Game Basketball

            var productEvolutionHighSchoolGameBasketball = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Evolution High School Game Basketball",
                IsEsd = false,
                ShortDescription = "For all positions on all levels, match day and every day",
                FullDescription = "<p>The Wilson Evolution High School Game Basketball has exclusive microfiber composite leather construction with deep embossed pebbles to give you the ultimate in feel and control.</p><p>Its patented Cushion Core Technology enhances durability for longer play. This microfiber composite Evolution high school basketball is pebbled with composite channels for better grip, helping players raise their game to the next level.</p><p>For all positions at all levels of play, game day and every day, Wilson delivers the skill-building performance that players demand.</p><p>This regulation-size 29.5' Wilson basketball is an ideal basketball for high school players, and is designed for either recreational use or for league games. It is NCAA and NFHS approved, so you know it's a high-quality basketball that will help you hone your shooting, passing and ball-handling skills.</p><p>Take your team all the way to the championship with the Wilson Evolution High School Game Basketball.</p>",
                Sku = "P-4001",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Evolution High School Game Basketball",
                Price = 25.90M,
                OldPrice = 29.90M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = thirdDeliveryTime.Id,
                HasTierPrices = true
            };

            AddProductPicture(productEvolutionHighSchoolGameBasketball, "product/evolution-high-school-game-basketball.jpg");

            productEvolutionHighSchoolGameBasketball.ProductCategories.Add(new ProductCategory { Category = categories["Basketball"], DisplayOrder = 1 });

            productEvolutionHighSchoolGameBasketball.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Adidas"], DisplayOrder = 1 });

            productEvolutionHighSchoolGameBasketball.TierPrices.Add(new TierPrice { Quantity = 6, Price = 24.90M });
            productEvolutionHighSchoolGameBasketball.TierPrices.Add(new TierPrice { Quantity = 12, Price = 22.90M });
            productEvolutionHighSchoolGameBasketball.TierPrices.Add(new TierPrice { Quantity = 24, Price = 20.90M });

            #endregion Wilson Evolution High School Game Basketball

            #region All Court Basketball

            var productAllCourtBasketball = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "All-Court Basketball",
                IsEsd = false,
                ShortDescription = "A durable Basketball for all surfaces",
                FullDescription = "<p><strong>All-Court Prep Ball</strong> </p> <p>A durable basketball for all surfaces. </p> <p>Whether on parquet or on asphalt - the adidas All-Court Prep Ball hat has only one goal: the basket. This basketball is made of durable artificial leather, was also predestined for indoor games also for outdoor games. </p> <ul>   <li>Composite cover made of artificial leather</li>   <li>suitable for indoors and outdoors</li>   <li>Delivered unpumped</li> </ul> ",
                Sku = "P-4002",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "All-Court Basketball",
                Price = 25.90M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = thirdDeliveryTime.Id
            };

            AddProductPicture(productAllCourtBasketball, "product/all-court-basketball.png");

            productAllCourtBasketball.ProductCategories.Add(new ProductCategory { Category = categories["Basketball"], DisplayOrder = 1 });

            productAllCourtBasketball.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Adidas"], DisplayOrder = 1 });

            #endregion All Court Basketball

            #endregion category Basketball

            #region category sunglasses

            #region product Top bar

            var productRayBanTopBar = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Ray-Ban Top Bar RB 3183",
                IsEsd = false,
                ShortDescription = "The Ray-Ban Original Wayfarer is the most famous style in the history of sunglasses. With the original design from 1952 the Wayfarer is popular with celebrities, musicians, artists and fashion experts.",
                FullDescription = "<p>The Ray-Ban ® RB3183 sunglasses give me their aerodynamic shape a reminiscence of speed.</p><p>A rectangular shape and the classic Ray-Ban logo imprinted on the straps characterize this light Halbrand model.</p>",
                Sku = "P-3004",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Ray-Ban Top Bar RB 3183",
                Price = 139M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = thirdDeliveryTime.Id
            };

            AddProductPicture(productRayBanTopBar, "product/RayBanTopBar_1.jpg", "rayban-top-bar-1");
            AddProductPicture(productRayBanTopBar, "product/RayBanTopBar_2.jpg", "rayban-top-bar-2");
            AddProductPicture(productRayBanTopBar, "product/RayBanTopBar_3.jpg", "rayban-top-bar-3");

            productRayBanTopBar.ProductCategories.Add(new ProductCategory { Category = categories["Sunglasses"], DisplayOrder = 1 });

            productRayBanTopBar.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Ray-Ban"], DisplayOrder = 1 });

            #endregion product Top bar

            #region product ORIGINAL WAYFARER AT COLLECTION

            var productOriginalWayfarer = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "ORIGINAL WAYFARER AT COLLECTION",
                IsEsd = false,
                ShortDescription = "The Ray-Ban Original Wayfarer is the most famous style in the history of sunglasses. With the original design from 1952 the Wayfarer is popular with celebrities, musicians, artists and fashion experts.",
                FullDescription = "<p><strong>Radar® EV Path™ PRIZM™ Road</strong> </p> <p>A new milestone in the heritage of performance, Radar® EV takes breakthroughs of a revolutionary design even further with a taller lens that extends the upper field of view. From the comfort and protection of the O Matter® frame to the grip of its Unobtanium® components, this premium design builds on the legacy of Radar innovation and style. </p> <p><strong>Features</strong> </p> <ul>   <li>PRIZM™ is a revolutionary lens technology that fine-tunes vision for specific sports and environments. See what you’ve been missing. Click here to learn more about Prizm Lens Technology.</li>   <li>Path lenses enhance performance if traditional lenses touch your cheeks and help extend the upper field of view</li>   <li>Engineered for maximized airflow for optimal ventilation to keep you cool</li>   <li>Unobtanium® earsocks and nosepads keep glasses in place, increasing grip despite perspiration</li>   <li>Interchangeable Lenses let you change lenses in seconds to optimize vision in any sport environment</li> </ul>",
                Sku = "P-3003",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "ORIGINAL WAYFARER AT COLLECTION",
                Price = 149M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = thirdDeliveryTime.Id
            };

            AddProductPicture(productOriginalWayfarer, "product/OriginalWayfarer_1.jpg", "wayfarer-blue-gray-classic-black-1");
            AddProductPicture(productOriginalWayfarer, "product/OriginalWayfarer_2.jpg", "wayfarer-blue-gray-classic-black-2");
            AddProductPicture(productOriginalWayfarer, "product/OriginalWayfarer_3.jpg", "wayfarer-gray-course-black");
            AddProductPicture(productOriginalWayfarer, "product/OriginalWayfarer_4.jpg", "wayfarer-brown-course-havana");
            AddProductPicture(productOriginalWayfarer, "product/OriginalWayfarer_5.jpg", "wayfarer-green-classic-havana-black");
            AddProductPicture(productOriginalWayfarer, "product/OriginalWayfarer_6.jpg", "wayfarer-blue-gray-classic-black-3");

            productOriginalWayfarer.ProductCategories.Add(new ProductCategory { Category = categories["Sunglasses"], DisplayOrder = 1 });

            productOriginalWayfarer.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Ray-Ban"], DisplayOrder = 1 });

            #endregion product ORIGINAL WAYFARER AT COLLECTION

            #region product Radar EV Prizm Sports Sunglasses

            var productRadarEVPrizmSportsSunglasses = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Radar EV Prizm Sports Sunglasses",
                IsEsd = false,
                ShortDescription = "",
                FullDescription = "<p><strong>Radar® EV Path™ PRIZM™ Road</strong> </p> <p>A new milestone in the heritage of performance, Radar® EV takes breakthroughs of a revolutionary design even further with a taller lens that extends the upper field of view. From the comfort and protection of the O Matter® frame to the grip of its Unobtanium® components, this premium design builds on the legacy of Radar innovation and style. </p> <p><strong>Features</strong> </p> <ul>   <li>PRIZM™ is a revolutionary lens technology that fine-tunes vision for specific sports and environments. See what you’ve been missing. Click here to learn more about Prizm Lens Technology.</li>   <li>Path lenses enhance performance if traditional lenses touch your cheeks and help extend the upper field of view</li>   <li>Engineered for maximized airflow for optimal ventilation to keep you cool</li>   <li>Unobtanium® earsocks and nosepads keep glasses in place, increasing grip despite perspiration</li>   <li>Interchangeable Lenses let you change lenses in seconds to optimize vision in any sport environment</li> </ul>",
                Sku = "P-3001",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Radar EV Prizm Sports Sunglasses",
                Price = 149M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = thirdDeliveryTime.Id
            };

            AddProductPicture(productRadarEVPrizmSportsSunglasses, "product/radar_ev_prizm.jpg");

            productRadarEVPrizmSportsSunglasses.ProductCategories.Add(new ProductCategory { Category = categories["Sunglasses"], DisplayOrder = 1 });

            productRadarEVPrizmSportsSunglasses.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Oakley"], DisplayOrder = 1 });

            #endregion product Radar EV Prizm Sports Sunglasses

            #region product Custom Flak Sunglasses

            var productCustomFlakSunglasses = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Custom Flak Sunglasses",
                IsEsd = false,
                ShortDescription = "",
                FullDescription = "<p><strong>Radar® EV Path™ PRIZM™ Road</strong> </p> <p>A new milestone in the heritage of performance, Radar® EV takes breakthroughs of a revolutionary design even further with a taller lens that extends the upper field of view. From the comfort and protection of the O Matter® frame to the grip of its Unobtanium® components, this premium design builds on the legacy of Radar innovation and style. </p> <p><strong>Features</strong> </p> <ul>   <li>PRIZM™ is a revolutionary lens technology that fine-tunes vision for specific sports and environments. See what you’ve been missing. Click here to learn more about Prizm Lens Technology.</li>   <li>Path lenses enhance performance if traditional lenses touch your cheeks and help extend the upper field of view</li>   <li>Engineered for maximized airflow for optimal ventilation to keep you cool</li>   <li>Unobtanium® earsocks and nosepads keep glasses in place, increasing grip despite perspiration</li>   <li>Interchangeable Lenses let you change lenses in seconds to optimize vision in any sport environment</li> </ul>",
                Sku = "P-3002",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Custom Flak Sunglasses",
                Price = 179M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = thirdDeliveryTime.Id
            };

            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlakSunglasses.jpg", "customflak");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlakSunglasses_black_white.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_matteblack_gray.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_matteblack_clear.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_matteblack_jadeiridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_matteblack_positiverediridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_matteblack_rubyiridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_matteblack_sapphireiridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_matteblack_violetiridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_matteblack_24kiridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_matteblack_fireiridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_orangeflare_24kiridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_orangeflare_clear.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_orangeflare_fireiridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_orangeflare_gray.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_orangeflare_jadeiridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_orangeflare_positiverediridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_orangeflare_rubyiridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_orangeflare_sapphireiridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_orangeflare_violetiridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_polishedwhite_24kiridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_polishedwhite_clear.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_polishedwhite_fireiridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_polishedwhite_gray.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_polishedwhite_jadeiridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_polishedwhite_rubyiridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_polishedwhite_sapphireiridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_polishedwhite_violetiridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_polishedwhite_positiverediridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_redline_24kiridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_redline_clear.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_redline_fireiridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_redline_gray.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_redline_jadeiridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_redline_positiverediridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_redline_rubyiridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_redline_sapphireiridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_redline_violetiridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_skyblue_24kiridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_skyblue_clear.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_skyblue_fireiridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_skyblue_gray.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_skyblue_jadeiridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_skyblue_positiverediridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_skyblue_rubyiridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_skyblue_sapphireiridium.jpg");
            AddProductPicture(productCustomFlakSunglasses, "product/CustomFlak_skyblue_violetiridium.jpg");

            productCustomFlakSunglasses.ProductCategories.Add(new ProductCategory { Category = categories["Sunglasses"], DisplayOrder = 1 });

            productCustomFlakSunglasses.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Oakley"], DisplayOrder = 1 });

            #endregion product Custom Flak Sunglasses


            #endregion category sunglasses

            #region category apple

            #region product iphone plus

            var productIphoneplus = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "iPhone Plus",
                IsEsd = false,
                ShortDescription = "iPhone 7 dramatically improves the most important aspects of the iPhone experience. It introduces advanced new camera systems. The best performance and battery life ever in an iPhone. Immersive stereo speakers. The brightest, most colorful iPhone display. Splash and water resistance.1 And it looks every bit as powerful as it is. This is iPhone 7.",
                FullDescription = "",
                Sku = "P-2001",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "iPhone Plus",
                Price = 878M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 9,
                StockQuantity = 10000,
                DisplayStockAvailability = true,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                IsFreeShipping = true,
                DeliveryTimeId = thirdDeliveryTime.Id
            };

            AddProductPicture(productIphoneplus, "product/iphone-plus_all_colors.jpg");
            AddProductPicture(productIphoneplus, "product/iphoneplus_1.jpg", "iphone-plus-default");
            AddProductPicture(productIphoneplus, "product/iphone-plus_red.jpg");
            AddProductPicture(productIphoneplus, "product/iphone-plus_silver.jpg");
            AddProductPicture(productIphoneplus, "product/iphone-plus_black.jpg");
            AddProductPicture(productIphoneplus, "product/iphone-plus_rose.jpg");
            AddProductPicture(productIphoneplus, "product/iphone-plus_gold.jpg");

            productIphoneplus.ProductCategories.Add(new ProductCategory { Category = categories["Apple"], DisplayOrder = 1 });

            productIphoneplus.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // offer type -> Permanent low price
                SpecificationAttributeOption = specAttributes[22].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 2)
            });

            productIphoneplus.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // storage capacity -> 64gb
                SpecificationAttributeOption = specAttributes[27].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 2)
            });
            productIphoneplus.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // storage capacity -> 128gb
                SpecificationAttributeOption = specAttributes[27].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 3)
            });
            productIphoneplus.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // operating system -> ios
                SpecificationAttributeOption = specAttributes[5].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 9)
            });

            #endregion product iphone plus

            #region product Watch Series 2

            var productWatchSeries2 = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Visibility = ProductVisibility.Hidden,
                Name = "Watch Series 2",
                IsEsd = false,
                ShortDescription = "Live a better day. Built-in GPS. Water resistance to 50 meters.1 A lightning-fast dual‑core processor. And a display that’s two times brighter than before. Full of features that help you stay active, motivated, and connected, Apple Watch Series 2 is the perfect partner for a healthy life.",
                FullDescription = "",
                Sku = "P-2002",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Watch Series 2",
                Price = 299M,
                OldPrice = 399M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = thirdDeliveryTime.Id
            };

            AddProductPicture(productWatchSeries2, "product/watchseries2_1.jpg");
            AddProductPicture(productWatchSeries2, "product/watchseries2_2.jpg");

            productWatchSeries2.ProductCategories.Add(new ProductCategory { Category = categories["Apple"], DisplayOrder = 1 });

            productWatchSeries2.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Apple"], DisplayOrder = 1 });

            productWatchSeries2.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // offer type -> offer of the day
                SpecificationAttributeOption = specAttributes[22].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 6)
            });

            productWatchSeries2.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // storage capacity -> 32gb
                SpecificationAttributeOption = specAttributes[27].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });

            productWatchSeries2.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // operating system -> ios
                SpecificationAttributeOption = specAttributes[5].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 9)
            });

            #endregion product Watch Series 2

            #region product Airpods

            var productAirpods = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "AirPods",
                IsEsd = false,
                ShortDescription = "Wireless. Effortless. Magical. Just take them out and they’re ready to use with all your devices. Put them in your ears and they connect instantly. Speak into them and your voice sounds clear. Introducing AirPods. Simplicity and technology, together like never before. The result is completely magical.",
                FullDescription = "",
                Sku = "P-2003",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "AirPods",
                Price = 999M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = thirdDeliveryTime.Id
            };

            AddProductPicture(productAirpods, "product/airpods_white.jpg", "airpods-white");
            AddProductPicture(productAirpods, "product/airpods_turquoise.jpg", "airpods-turquoise");
            AddProductPicture(productAirpods, "product/airpods_lightblue.jpg", "airpods-lightblue");
            AddProductPicture(productAirpods, "product/airpods_rose.jpg", "airpods-rose");
            AddProductPicture(productAirpods, "product/airpods_gold.jpg", "airpods-gold");
            AddProductPicture(productAirpods, "product/airpods_mint.jpg", "airpods-mint");

            productAirpods.ProductCategories.Add(new ProductCategory { Category = categories["Apple"], DisplayOrder = 1 });

            productAirpods.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Apple"], DisplayOrder = 7 });

            #endregion product Airpods

            #region product Ultimate Apple Pro Hipster Bundle

            var productAppleProHipsterBundle = new Product
            {
                ProductType = ProductType.BundledProduct,
                Name = "Ultimate Apple Pro Hipster Bundle",
                IsEsd = false,
                ShortDescription = "Save with this set 5%!",
                FullDescription = "As an Apple fan and hipster, it is your basic need to always have the latest Apple products. So you do not have to spend four times a year in front of the Apple Store, simply subscribe to the Ultimate Apple Pro Hipster Set in the year subscription!",
                Sku = "P-2005-Bundle",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                ShowOnHomePage = true,
                MetaTitle = "Ultimate Apple Pro Hipster Bundle",
                Price = 2371M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = thirdDeliveryTime.Id,
                BundleTitleText = "Bundle includes",
                BundlePerItemPricing = true,
                BundlePerItemShoppingCart = true
            };

            AddProductPicture(productAppleProHipsterBundle, "product/ultimate-apple-pro-hipster-bundle.jpg");
            AddProductPicture(productAppleProHipsterBundle, "product/airpods_white.jpg", "bundle-airpods-white");
            AddProductPicture(productAppleProHipsterBundle, "product/watchseries2_2.jpg", "bundle-watchseries");
            AddProductPicture(productAppleProHipsterBundle, "product/iphoneplus_2.jpg", "bundle-iphoneplus");
            AddProductPicture(productAppleProHipsterBundle, "category/apple.png", "bundle-apple");

            productAppleProHipsterBundle.ProductCategories.Add(new ProductCategory { Category = categories["Apple"], DisplayOrder = 1 });

            productAppleProHipsterBundle.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Apple"], DisplayOrder = 1 });

            #endregion product Ultimate Apple Pro Hipster Bundle

            #region product 9,7 iPad

            var product97ipad = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "9,7' iPad",
                IsEsd = false,
                ShortDescription = "Flat-out fun. Learn, play, surf, create. iPad gives you the incredible display, performance, and apps to do what you love to do. Anywhere. Easily. Magically.",
                FullDescription = "<ul>  <li>9,7' Retina Display mit True Tone und</li>  <li>A9X Chip der dritten Generation mit 64-Bit Desktoparchitektur</li>  <li>Touch ID Fingerabdrucksensor</li>  <li>12 Megapixel iSight Kamera mit 4K Video</li>  <li>5 Megapixel FaceTime HD Kamera</li>  <li>802.11ac WLAN mit MIMO</li>  <li>Bis zu 10 Stunden Batterielaufzeit***</li>  <li>4-Lautsprecher-Audio</li></ul>",
                Sku = "P-2004",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                ShowOnHomePage = true,
                MetaTitle = "9,7' iPad",
                Price = 319.00M,
                OldPrice = 349.00M,
                SpecialPrice = 299.00M,
                SpecialPriceStartDateTimeUtc = new DateTime(2017, 5, 1, 0, 0, 0),
                SpecialPriceEndDateTimeUtc = specialPriceEndDate,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = thirdDeliveryTime.Id
            };

            AddProductPicture(product97ipad, "product/ipad_1.jpg");
            AddProductPicture(product97ipad, "product/ipad_2.jpg");
            AddProductPicture(product97ipad, "product/97-ipad-yellow.jpg");
            AddProductPicture(product97ipad, "product/97-ipad-turquoise.jpg");
            AddProductPicture(product97ipad, "product/97-ipad-lightblue.jpg");
            AddProductPicture(product97ipad, "product/97-ipad-purple.jpg");
            AddProductPicture(product97ipad, "product/97-ipad-mint.jpg");
            AddProductPicture(product97ipad, "product/97-ipad-rose.jpg");
            AddProductPicture(product97ipad, "product/97-ipad-spacegray.jpg");
            AddProductPicture(product97ipad, "product/97-ipad-gold.jpg");
            AddProductPicture(product97ipad, "product/97-ipad-silver.jpg");

            product97ipad.ProductCategories.Add(new ProductCategory { Category = categories["Apple"], DisplayOrder = 1 });

            product97ipad.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Apple"], DisplayOrder = 1 });

            product97ipad.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // offer type -> promotion
                SpecificationAttributeOption = specAttributes[22].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 3)
            });

            product97ipad.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // storage capacity -> 64gb
                SpecificationAttributeOption = specAttributes[27].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 2)
            });
            product97ipad.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // storage capacity -> 128gb
                SpecificationAttributeOption = specAttributes[27].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 3)
            });
            product97ipad.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // operating system -> ios
                SpecificationAttributeOption = specAttributes[5].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 9)
            });

            #endregion product 9,7 iPad

            #endregion category apple

            #region category Gift Cards

            #region product10GiftCard

            var product10GiftCard = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "$10 Virtual Gift Card",
                IsEsd = true,
                ShortDescription = "$10 Gift Card. Gift Cards must be redeemed through our site Web site toward the purchase of eligible products.",
                FullDescription = "<p>Gift Cards must be redeemed through our site Web site toward the purchase of eligible products. Purchases are deducted from the GiftCard balance. Any unused balance will be placed in the recipient's GiftCard account when redeemed. If an order exceeds the amount of the GiftCard, the balance must be paid with a credit card or other available payment method.</p>",
                Sku = "P-1000",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "$10 Virtual Gift Card",
                Price = 10M,
                IsGiftCard = true,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayOrder = 1
            };

            AddProductPicture(product10GiftCard, "product/gift_card_10.png");
            product10GiftCard.ProductCategories.Add(new ProductCategory { Category = categories["Gift Cards"], DisplayOrder = 1 });

            #endregion product10GiftCard

            #region product25GiftCard

            var product25GiftCard = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "$25 Virtual Gift Card",
                IsEsd = true,
                ShortDescription = "$25 Gift Card. Gift Cards must be redeemed through our site Web site toward the purchase of eligible products.",
                FullDescription = "<p>Gift Cards must be redeemed through our site Web site toward the purchase of eligible products. Purchases are deducted from the GiftCard balance. Any unused balance will be placed in the recipient's GiftCard account when redeemed. If an order exceeds the amount of the GiftCard, the balance must be paid with a credit card or other available payment method.</p>",
                Sku = "P-1001",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "$25 Virtual Gift Card",
                Price = 25M,
                IsGiftCard = true,
                GiftCardType = GiftCardType.Virtual,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayOrder = 2
            };

            AddProductPicture(product25GiftCard, "product/gift_card_25.png");
            product25GiftCard.ProductCategories.Add(new ProductCategory { Category = categories["Gift Cards"], DisplayOrder = 1 });

            #endregion product25GiftCard

            #region product50GiftCard

            var product50GiftCard = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "$50 Virtual Gift Card",
                IsEsd = true,
                ShortDescription = "$50 Gift Card. Gift Cards must be redeemed through our site Web site toward the purchase of eligible products.",
                FullDescription = "<p>Gift Cards must be redeemed through our site Web site toward the purchase of eligible products. Purchases are deducted from the GiftCard balance. Any unused balance will be placed in the recipient's GiftCard account when redeemed. If an order exceeds the amount of the GiftCard, the balance must be paid with a credit card or other available payment method.</p>",
                Sku = "P-1002",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "$50 Virtual Gift Card",
                Price = 50M,
                IsGiftCard = true,
                GiftCardType = GiftCardType.Virtual,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayOrder = 3
            };

            AddProductPicture(product50GiftCard, "product/gift_card_50.png");
            product50GiftCard.ProductCategories.Add(new ProductCategory { Category = categories["Gift Cards"], DisplayOrder = 1 });

            #endregion product50GiftCard

            #region product100GiftCard

            var product100GiftCard = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "$100 Virtual Gift Card",
                IsEsd = true,
                ShortDescription = "$100 Gift Card. Gift Cards must be redeemed through our site Web site toward the purchase of eligible products.",
                FullDescription = "<p>Gift Cards must be redeemed through our site Web site toward the purchase of eligible products. Purchases are deducted from the GiftCard balance. Any unused balance will be placed in the recipient's GiftCard account when redeemed. If an order exceeds the amount of the GiftCard, the balance must be paid with a credit card or other available payment method.</p>",
                Sku = "P-10033",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "$100 Virtual Gift Card",
                Price = 100M,
                IsGiftCard = true,
                GiftCardType = GiftCardType.Virtual,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayOrder = 4,
            };

            AddProductPicture(product100GiftCard, "product/gift_card_100.png");
            product100GiftCard.ProductCategories.Add(new ProductCategory { Category = categories["Gift Cards"], DisplayOrder = 1 });

            #endregion product100GiftCard

            #endregion category Gift Cards

            #region category books

            #region productBooksUberMan

            var productBooksUberMan = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Überman: The novel",
                ShortDescription = "(Hardcover)",
                FullDescription = "<p>From idiots to riches - and back ... Ever since it with my Greek financial advisors were no more delicious cookies to meetings, I should have known something. Was the last cookie it when I bought a Romanian forest funds and leveraged discount certificates on lean hogs - which is sort of a more stringent bet that the price of lean hogs will remain stable, and that's nothing special because it is also available for cattle and cotton and fat pig. Again and again and I joked Kosmas Nikiforos Sarantakos. About all the part-time seer who tremblingly put for fear the euro crisis gold coins under the salami slices of their frozen pizzas And then came the day that revealed to me in almost Sarantakos fraudulent casualness that my plan had not worked out really. 'Why all of a sudden> my plan', 'I heard myself asking yet, but it was in the garage I realized what that really meant minus 211.2 percent in my portfolio report: personal bankruptcy, gutter and Drug Addiction with subsequent loss of the incisors . Not even the study of my friend, I would still be able to finance. The only way out was to me as quickly as secretly again to draw from this unspeakable Greek shit - I had to be Überman! By far the bekloppteste story about 'idiot' Simon Peter! »Tommy Jaud – Deutschlands witzigste Seite.« Alex Dengler, Bild am Sonntag</p>",
                Sku = "P-1003",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Überman: The novel",
                Price = 16.99M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true
            };

            AddProductPicture(productBooksUberMan, "product/0000932_uberman-der-roman.jpeg");
            productBooksUberMan.ProductCategories.Add(new ProductCategory { Category = categories["SPIEGEL-Bestseller"], DisplayOrder = 1 });

            productBooksUberMan.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                SpecificationAttributeOption = specAttributes[13].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });
            productBooksUberMan.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                SpecificationAttributeOption = specAttributes[14].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 7)
            });
            productBooksUberMan.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                SpecificationAttributeOption = specAttributes[12].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });

            #endregion productBooksUberMan

            #region productBooksGefangeneDesHimmels

            var productBooksGefangeneDesHimmels = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "The Prisoner of Heaven: A Novel",
                ShortDescription = "(Hardcover)",
                FullDescription = "<p>By Shadow of the Wind and The Angel's Game, the new large-Barcelona novel by Carlos Ruiz Zafón. - Barcelona, Christmas 1957th The bookseller Daniel Sempere and his friend Fermín be drawn again into a great adventure. In the continuation of his international success with Carlos Ruiz Zafón takes the reader on a fascinating journey into his Barcelona. Creepy and fascinating, with incredible suction power and humor, the novel, the story of Fermin, who 'rose from the dead, and the key to the future is.' Fermin's life story linking the threads of The Shadow of the Wind with those from The Angel's Game. A masterful puzzle that keeps the reader around the world in thrall. </p> <p> Product Hardcover: 416 pages Publisher: S. Fischer Verlag; 1 edition (October 25, 2012) Language: German ISBN-10: 3,100,954,025 ISBN-13: 978-3100954022 Original title: El prisionero del cielo Size and / or weight: 21.4 x 13.6 cm x 4.4 </p>",
                ProductTemplateId = productTemplate.Id,
                Sku = "P-1004",
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "The Prisoner of Heaven: A Novel",
                Price = 22.99M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = firstDeliveryTime.Id
            };

            AddProductPicture(productBooksGefangeneDesHimmels, "product/0000935_der-gefangene-des-himmels-roman_300.jpeg");
            productBooksGefangeneDesHimmels.ProductCategories.Add(new ProductCategory { Category = categories["SPIEGEL-Bestseller"], DisplayOrder = 1 });

            productBooksGefangeneDesHimmels.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Edition -> bound
                SpecificationAttributeOption = specAttributes[13].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });

            productBooksGefangeneDesHimmels.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Category -> bound
                SpecificationAttributeOption = specAttributes[14].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 7)
            });
            productBooksGefangeneDesHimmels.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Language -> German
                SpecificationAttributeOption = specAttributes[12].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });

            #endregion productBooksGefangeneDesHimmels

            #region productBooksBestGrillingRecipes

            var productBooksBestGrillingRecipes = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Best Grilling Recipes",
                ShortDescription = "More Than 100 Regional Favorites Tested and Perfected for the Outdoor Cook (Hardcover)",
                FullDescription = "<p> Take a winding cross-country trip and you'll discover barbecue shacks with offerings like tender-smoky Baltimore pit beef and saucy St. Louis pork steaks. To bring you the best of these hidden gems, along with all the classics, the editors of Cook's Country magazine scoured the country, then tested and perfected their favorites. HEre traditions large and small are brought into the backyard, from Hawaii's rotisserie favorite, the golden-hued Huli Huli Chicken, to fall-off-the-bone Chicago Barbecued Ribs. In Kansas City, they're all about the sauce, and for our saucy Kansas City Sticky Ribs, we found a surprise ingredient-root beer. We also tackle all the best sides. </p> <p> Not sure where or how to start? This cookbook kicks off with an easy-to-follow primer that will get newcomers all fired up. Whether you want to entertain a crowd or just want to learn to make perfect burgers, Best Grilling Recipes shows you the way. </p>",
                ProductTemplateId = productTemplate.Id,
                Sku = "P-1005",
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Best Grilling Recipes",
                Price = 27.00M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = firstDeliveryTime.Id
            };

            AddProductPicture(productBooksBestGrillingRecipes, "product/bestgrillingrecipes.jpg");
            productBooksBestGrillingRecipes.ProductCategories.Add(new ProductCategory { Category = categories["Cook and enjoy"], DisplayOrder = 1 });

            productBooksBestGrillingRecipes.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Edition -> bound
                SpecificationAttributeOption = specAttributes[13].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });
            productBooksBestGrillingRecipes.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Category -> cook & bake
                SpecificationAttributeOption = specAttributes[14].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 8)
            });
            productBooksBestGrillingRecipes.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Language -> German
                SpecificationAttributeOption = specAttributes[12].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 2)
            });

            #endregion productBooksBestGrillingRecipes

            #region productBooksCookingForTwo

            var productBooksCookingForTwo = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Cooking for Two",
                ShortDescription = "More Than 200 Foolproof Recipes for Weeknights and Special Occasions (Hardcover)",
                FullDescription = "<p>In Cooking for Two, the test kitchen's goal was to take traditional recipes and cut them down to size to serve just twowith tailored cooking techniques and smart shopping tips that will cut down on wasted food and wasted money. Great lasagna starts to lose its luster when you're eating the leftovers for the fourth day in a row. While it may seem obvious that a recipe for four can simply be halved to work, our testing has proved that this is not always the case; cooking with smaller amounts of ingredients often requires different preparation techniques, cooking time, temperature, and the proportion of ingredients. This was especially true as we worked on scaled-down desserts; baking is an unforgiving science in which any changes in recipe amounts often called for changes in baking times and temperatures. </p> <p> Hardcover: 352 pages<br> Publisher: America's Test Kitchen (May 2009)<br> Language: English<br> ISBN-10: 1933615435<br> ISBN-13: 978-1933615431<br> </p>",
                ProductTemplateId = productTemplate.Id,
                Sku = "P-1006",
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Cooking for Two",
                Price = 27.00M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = secondDeliveryTime.Id
            };

            AddProductPicture(productBooksCookingForTwo, "product/cookingfortwo.jpg");
            productBooksCookingForTwo.ProductCategories.Add(new ProductCategory { Category = categories["Cook and enjoy"], DisplayOrder = 1 });

            productBooksCookingForTwo.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Edition -> bound
                SpecificationAttributeOption = specAttributes[13].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });
            productBooksCookingForTwo.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Category -> cook & bake
                SpecificationAttributeOption = specAttributes[14].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 8)
            });
            productBooksCookingForTwo.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Language -> German
                SpecificationAttributeOption = specAttributes[12].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 2)
            });

            #endregion productBooksCookingForTwo

            #region productBooksAutosDerSuperlative

            var productBooksAutosDerSuperlative = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Car of superlatives: the strongest, the first, the most beautiful, the fastest",
                ShortDescription = "Hardcover",
                FullDescription = "<p> For some, the car is only a useful means of transportation. For everyone else, there are 'cars - The Ultimate Guide' of art-connoisseur Michael Doerflinger. With authentic images, all important data and a lot of information can be presented to the fastest, most innovative, the strongest, the most unusual and the most successful examples of automotive history. A comprehensive manual for the specific reference and extensive browsing. </p>",
                Sku = "P-1007",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Car of superlatives",
                Price = 14.95M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = thirdDeliveryTime.Id
            };

            AddProductPicture(productBooksAutosDerSuperlative, "product/0000944_autos-der-superlative-die-starksten-die-ersten-die-schonsten-die-schnellsten.jpeg");
            productBooksAutosDerSuperlative.ProductCategories.Add(new ProductCategory { Category = categories["Books"], DisplayOrder = 1 });

            productBooksAutosDerSuperlative.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Edition -> bound
                SpecificationAttributeOption = specAttributes[13].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });
            productBooksAutosDerSuperlative.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Category -> cars
                SpecificationAttributeOption = specAttributes[14].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 6)
            });
            productBooksAutosDerSuperlative.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Language -> German
                SpecificationAttributeOption = specAttributes[12].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });

            #endregion productBooksAutosDerSuperlative

            #region productBooksBildatlasMotorraeder

            var productBooksBildatlasMotorraeder = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Picture Atlas Motorcycles: With more than 350 brilliant images",
                ShortDescription = "Hardcover",
                FullDescription = "<p> Motorcycles are like no other means of transportation for the great dream of freedom and adventure. This richly illustrated atlas image portrayed in brilliant color photographs and informative text, the most famous bikes of the world's motorcycle history. From the primitive steam engine under the saddle of the late 19th Century up to the hugely powerful, equipped with the latest electronics and computer technology superbikes of today he is an impressive picture of the development and fabrication of noble and fast-paced motorcycles. The myth of the motorcycle is just as much investigated as a motorcycle as a modern lifestyle product of our time. Country-specific, company-historical background information and interesting stories and History about the people who preceded drove one of the seminal inventions of recent centuries and evolved, make this comprehensive illustrated book an incomparable reference for any motorcycle enthusiast and technology enthusiasts. </p> <p> • Extensive history of the legendary models of all major motorcycle manufacturers worldwide<br> • With more than 350 brilliant color photographs and fascinating background information relating<br> • With informative drawings, stunning detail shots and explanatory info-boxes<br> </p> <p> content • 1817 1913: The beginning of a success story<br> • 1914 1945: mass mobility<br> • 1946 1990: Battle for the World Market<br> • In 1991: The modern motorcycle<br> • motorcycle cult object: From Transportation to Lifestyle<br> </p>",
                Sku = "P-1008",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Picture Atlas Motorcycles",
                Price = 14.99M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = firstDeliveryTime.Id
            };

            AddProductPicture(productBooksBildatlasMotorraeder, "product/0000942_bildatlas-motorrader-mit-mehr-als-350-brillanten-abbildungen.jpeg");
            productBooksBildatlasMotorraeder.ProductCategories.Add(new ProductCategory { Category = categories["Books"], DisplayOrder = 1 });

            productBooksBildatlasMotorraeder.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Edition -> bound
                SpecificationAttributeOption = specAttributes[13].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });
            productBooksBildatlasMotorraeder.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Category -> non-fiction
                SpecificationAttributeOption = specAttributes[14].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 9)
            });
            productBooksBildatlasMotorraeder.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Language -> German
                SpecificationAttributeOption = specAttributes[12].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });

            #endregion productBooksBildatlasMotorraeder

            #region productBooksAutoBuch

            var productBooksAutoBuch = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "The Car Book. The great history with over 1200 models",
                ShortDescription = "Hardcover",
                FullDescription = "<p> Makes, models, milestones<br> The car - for some, a utensil for other expression of lifestyle, cult object and passion. Few inventions have changed their lives as well as the good of the automobile 125 years ago - one more reason for this extensive chronicle. The car-book brings the history of the automobile to life. It presents more than 1200 important models - Karl Benz 'Motorwagen about legendary cult car to advanced hybrid vehicles. It explains the milestones in engine technology and portrays the big brands and their designers. Characteristics from small cars to limousines and send racing each era invite you to browse and discover. The most comprehensive and bestbebildert illustrated book on the market - it would be any car lover! </p> <p> Hardcover: 360 pages<br> Publisher: Dorling Kindersley Publishing (September 27, 2012)<br> Language: German<br> ISBN-10: 3,831,022,062<br> ISBN-13: 978-3831022069<br> Size and / or weight: 30.6 x 25.8 x 2.8 cm<br> </p>",
                Sku = "P-1009",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "The Car Book",
                Price = 29.95M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = firstDeliveryTime.Id
            };

            AddProductPicture(productBooksAutoBuch, "product/0000947_das-auto-buch-die-grose-chronik-mit-uber-1200-modellen_300.jpeg");
            productBooksAutoBuch.ProductCategories.Add(new ProductCategory { Category = categories["Books"], DisplayOrder = 1 });

            productBooksAutoBuch.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Edition -> bound
                SpecificationAttributeOption = specAttributes[13].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });
            productBooksAutoBuch.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Category -> non-fiction
                SpecificationAttributeOption = specAttributes[14].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 9)
            });
            productBooksAutoBuch.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Language -> German
                SpecificationAttributeOption = specAttributes[12].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });

            #endregion productBooksAutoBuch

            #region productBooksFastCars

            var productBooksFastCars = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Fast Cars, Image Calendar 2013",
                ShortDescription = "spiral bound",
                FullDescription = "<p> Large Size: 48.5 x 34 cm.<br> This impressive picture calendar with silver ring binding thrilled with impressive photographs of exclusive sports cars. Who understands cars not only as a pure commercial vehicles, will find the most sought-after status symbols at all: fast cars are effectively set to the razor sharp and vivid photos in scene and convey freedom, speed, strength and the highest technical perfection. Starting with the 450-horsepower Maserati GranTurismo MC Stradale on the stylish, luxurious Aston Martin Virage Volante accompany up to the produced only in small numbers Mosler Photon MT900S the fast racer with style and elegance through the months. </p> <p> Besides the calendar draws another picture to look at interesting details. There are the essential information on any sports car in the English language. After this year, the high-quality photos are framed an eye-catcher on the wall of every lover of fast cars. Even as a gift this beautiful years companion is wonderfully suited. 12 calendar pages, neutral and discreet held calendar. Printed on paper from sustainable forests. For lovers of luxury vintage cars also available in ALPHA EDITION: the large format image Classic Cars Calendar 2013: ISBN 9,783,840,733,376th </p> <p> Spiral-bound: 14 pages<br> Publisher: Alpha Edition (June 1, 2012)<br> Language: German<br> ISBN-10: 3,840,733,383<br> ISBN-13: 978-3840733383<br> Size and / or weight: 48.8 x 34.2 x 0.6 cm<br> </p>",
                Sku = "P-1010",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Fast Cars",
                Price = 16.95M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = firstDeliveryTime.Id
            };

            AddProductPicture(productBooksFastCars, "product/0000946_fast-cars-bildkalender-2013_300.jpeg");
            productBooksFastCars.ProductCategories.Add(new ProductCategory { Category = categories["Books"], DisplayOrder = 1 });

            productBooksFastCars.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Edition -> bound
                SpecificationAttributeOption = specAttributes[13].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });
            productBooksFastCars.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Category -> cars
                SpecificationAttributeOption = specAttributes[14].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 6)
            });
            productBooksFastCars.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Language -> German
                SpecificationAttributeOption = specAttributes[12].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });

            #endregion productBooksFastCars

            #region productBooksMotorradAbenteuer

            var productBooksMotorradAbenteuer = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Motorcycle Adventures: Riding for travel enduros",
                ShortDescription = "Hardcover",
                FullDescription = "<p> Modern travel enduro bikes are ideal for adventure travel. Their technique is complex, their weight considerably. The driving behavior changes depending on the load and distance. </p> <p> Before the tour starts, you should definitely attend a training course. This superbly illustrated book presents practical means of many informative series photos the right off-road driving in mud and sand, gravel and rock with and without luggage. In addition to the driving course full of information and tips on choosing the right motorcycle for travel planning and practical issues may be on the way. </p>",
                Sku = "P-1011",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Motorcycle Adventures",
                Price = 24.90M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = secondDeliveryTime.Id
            };

            AddProductPicture(productBooksMotorradAbenteuer, "product/0000943_motorrad-abenteuer-fahrtechnik-fur-reise-enduros.jpeg");
            productBooksMotorradAbenteuer.ProductCategories.Add(new ProductCategory { Category = categories["Books"], DisplayOrder = 1 });

            productBooksMotorradAbenteuer.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Edition -> bound
                SpecificationAttributeOption = specAttributes[13].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });
            productBooksMotorradAbenteuer.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Category -> cars
                SpecificationAttributeOption = specAttributes[14].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 9)
            });
            productBooksMotorradAbenteuer.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Language -> German
                SpecificationAttributeOption = specAttributes[12].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });

            #endregion productBooksMotorradAbenteuer

            #endregion category books

            #region Instant Download Music / Digital Products

            #region product Books Stone of the Wise

            var productBooksStoneOfTheWise = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Ebook 'Stone of the Wise' in 'Lorem ipsum'",
                IsEsd = true,
                ShortDescription = "E-Book, 465 pages",
                FullDescription = "",
                Sku = "P-6001",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Ebook 'Stone of the Wise' in 'Lorem ipsum'",
                Price = 9.90M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsDownload = true,
                HasSampleDownload = true
            };

            AddProductPicture(productBooksStoneOfTheWise, "product/stone_of_wisdom.jpg");
            productBooksStoneOfTheWise.ProductCategories.Add(new ProductCategory { Category = categories["Digital Products"], DisplayOrder = 1 });

            productBooksStoneOfTheWise.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Edition -> bound
                SpecificationAttributeOption = specAttributes[13].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });
            productBooksStoneOfTheWise.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Category -> cars
                SpecificationAttributeOption = specAttributes[14].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 6)
            });
            productBooksStoneOfTheWise.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Language -> German
                SpecificationAttributeOption = specAttributes[12].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });

            #endregion product Books Stone of the Wise


            #region Antonio Vivaldi: then spring

            var productInstantDownloadVivaldi = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Antonio Vivaldi: spring",
                IsEsd = true,
                ShortDescription = "MP3, 320 kbit/s",
                FullDescription = "<p>Antonio Vivaldi: Spring</p> <p>Antonio Lucio Vivaldi (March 4, 1678 in Venice, &dagger; 28 July 1741 in Vienna) was a Venetian composer and violinist in the Baroque.</p> <p>The Four Seasons (Le quattro stagioni Italian) is perhaps the most famous works of Antonio Vivaldi. It's four violin concertos with extra-musical programs, each portraying a concert season. This is the individual concerts one - probably written by Vivaldi himself - Sonnet preceded by consecutive letters in front of the lines and in the appropriate places in the score arrange the verbal description of the music.</p> <p>Vivaldi had previously always been experimenting with non-musical programs, which often reflected in his tracks, the exact interpretation of the individual points score is unusual for him. His experience as a virtuoso violinist allowed him access to particularly effective playing techniques, as an opera composer, he had developed a strong sense of effects, both of which benefitted from him.</p> <p>As the title suggests, especially to imitate natural phenomena - gentle winds, severe storms and thunderstorms are elements that are common to all four concerts. There are also various birds and even a dog, further human activities such as hunting, a barn dance, ice skating, including stumbling and falling to the heavy sleep of a drunkard.</p> <p>The work dates from 1725 and is available in two print editions, which appeared more or less simultaneously published in Amsterdam and Paris.</p>",
                Sku = "P-1016",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Antonio Vivaldi: spring",
                Price = 1.99M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsDownload = true,
                HasSampleDownload = true
            };

            AddProductPicture(productInstantDownloadVivaldi, "product/vivaldi.jpg");
            productInstantDownloadVivaldi.ProductCategories.Add(new ProductCategory { Category = categories["Digital Products"], DisplayOrder = 1 });

            productInstantDownloadVivaldi.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // mp3 quality > 320 kbit/S
                SpecificationAttributeOption = specAttributes[18].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 3)
            });
            productInstantDownloadVivaldi.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 2,
                // genre > classic
                SpecificationAttributeOption = specAttributes[19].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 6)
            });

            #endregion Antonio Vivildi: then spring

            #region Beethoven für Elise

            var productInstantDownloadBeethoven = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Ludwig van Beethoven: For Elise",
                IsEsd = true,
                ShortDescription = "Ludwig van Beethoven's most popular compositions",
                FullDescription = "<p> The score was not published until 1867, 40 years after the composer's death in 1827. The discoverer of the piece, Ludwig Nohl, affirmed that the original autographed manuscript, now lost, was dated 27 April 1810.[4] The version of \"Für Elise\" we hear today is an earlier version that was transcribed by Ludwig Nohl. There is a later version, with drastic changes to the accompaniment which was transcribed from a later manuscript by Barry Cooper. The most notable difference is in the first theme, the left-hand arpeggios are delayed by a 16th note beat. There are a few extra bars in the transitional section into the B section; and finally, the rising A minor arpeggio figure is moved later into the piece. The tempo marking Poco Moto is believed to have been on the manuscript that Ludwig Nohl transcribed (now lost). The later version includes the marking Molto Grazioso. It is believed that Beethoven intended to add the piece to a cycle of bagatelles.[citation needed] </p> <p> Therese Malfatti, widely believed to be the dedicatee of \"Für Elise\" The pianist and musicologist Luca Chiantore (es) argued in his thesis and his 2010 book Beethoven al piano that Beethoven might not have been the person who gave the piece the form that we know today. Chiantore suggested that the original signed manuscript, upon which Ludwig Nohl claimed to base his transcription, may never have existed.[5] On the other hand, the musicologist Barry Cooper stated, in a 1984 essay in The Musical Times, that one of two surviving sketches closely resembles the published version.[6] </p>",
                Sku = "P-1017",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Ludwig van Beethoven: Für Elise",
                ShowOnHomePage = true,
                Price = 1.89M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsDownload = true,
                HasSampleDownload = true
            };

            AddProductPicture(productInstantDownloadBeethoven, "product/Beethoven.jpg");
            productInstantDownloadBeethoven.ProductCategories.Add(new ProductCategory { Category = categories["Digital Products"], DisplayOrder = 1 });

            productInstantDownloadBeethoven.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // mp3 quality > 320 kbit/S
                SpecificationAttributeOption = specAttributes[18].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 3)
            });
            productInstantDownloadBeethoven.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 2,
                // genre > classic
                SpecificationAttributeOption = specAttributes[19].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 6)
            });

            #endregion Beethoven für Elise

            #endregion Instant Download Music

            #region watches

            #region productTRANSOCEANCHRONOGRAPH

            var productTRANSOCEANCHRONOGRAPH = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "TRANSOCEAN CHRONOGRAPH",
                ShortDescription = "The Transocean Chronograph interprets the factual aesthetics of classic chronographs of the 1950s and 1960s in a decidedly contemporary style.",
                FullDescription = "<p>The Transocean Chronograph interprets the factual aesthetics of classic chronographs of the 1950s and 1960s in a decidedly contemporary style. The high-performance caliber 01, designed and manufactured entirely in the Breitling studios, works in its form, which is reduced to the essentials. </p> <p> </p> <table style='width: 425px;'>   <tbody>     <tr>       <td style='width: 185px;'>Caliber       </td>       <td style='width: 237px;'>Breitling 01 (Manufactory caliber)       </td>     </tr>     <tr>       <td style='width: 185px;'>Movement       </td>       <td style='width: 237px;'>Mechanically, Automatic       </td>     </tr>     <tr>       <td style='width: 185px;'>Power reserve       </td>       <td style='width: 237px;'>Min. 70 hour       </td>     </tr>     <tr>       <td style='width: 185px;'>Chronograph       </td>       <td style='width: 237px;'>1/4-Seconds, 30 Minutes, 12 Hours       </td>     </tr>     <tr>       <td style='width: 185px;'>Half vibrations       </td>       <td style='width: 237px;'>28 800 a/h       </td>     </tr>     <tr>       <td style='width: 185px;'>Rubies       </td>       <td style='width: 237px;'>47 Rubies       </td>     </tr>     <tr>       <td style='width: 185px;'>Calendar       </td>       <td style='width: 237px;'>Window       </td>     </tr>   </tbody> </table> ",
                Sku = "P-9001",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "TRANSOCEAN CHRONOGRAPH",
                ShowOnHomePage = true,
                Price = 24110.00M,
                OldPrice = 26230.00M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = thirdDeliveryTime.Id
            };

            AddProductPicture(productTRANSOCEANCHRONOGRAPH, "product/transocean-chronograph.jpg");
            productTRANSOCEANCHRONOGRAPH.ProductCategories.Add(new ProductCategory { Category = categories["Watches"], DisplayOrder = 1 });

            productTRANSOCEANCHRONOGRAPH.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Breitling"], DisplayOrder = 1 });

            productTRANSOCEANCHRONOGRAPH.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // offer > promotion
                SpecificationAttributeOption = specAttributes[22].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 3)
            });
            productTRANSOCEANCHRONOGRAPH.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 2,
                // manufacturer > Breitling
                SpecificationAttributeOption = specAttributes[20].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 18)
            });
            productTRANSOCEANCHRONOGRAPH.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // housing > steel
                SpecificationAttributeOption = specAttributes[8].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });
            productTRANSOCEANCHRONOGRAPH.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // material -> leather
                SpecificationAttributeOption = specAttributes[8].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 5)
            });
            productTRANSOCEANCHRONOGRAPH.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // Gender -> gentlemen
                SpecificationAttributeOption = specAttributes[7].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });
            productTRANSOCEANCHRONOGRAPH.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // movement -> mechanical, self winding
                SpecificationAttributeOption = specAttributes[9].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });
            productTRANSOCEANCHRONOGRAPH.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // diameter -> 44mm
                SpecificationAttributeOption = specAttributes[24].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 3)
            });
            productTRANSOCEANCHRONOGRAPH.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // closure -> folding clasp
                SpecificationAttributeOption = specAttributes[25].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 2)
            });

            #endregion productTRANSOCEANCHRONOGRAPH

            #region productTissotT-TouchExpertSolar

            var productTissotTTouchExpertSolar = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Tissot T-Touch Expert Solar",
                ShortDescription = "The beam of the Tissot T-Touch Expert Solar on the dial ensures that the Super-LumiNova®-coated indexes and hands illuminate in the dark, and on the other hand, charges the battery of the watch. This model is a force package in every respect.",
                FullDescription = "<p>The T-Touch Expert Solar is an important new model in the Tissot range. </p> <p>Tissot’s pioneering spirit is what led to the creation of tactile watches in 1999. </p> <p>Today, it is the first to present a touch-screen watch powered by solar energy, confirming its position as leader in tactile technology in watchmaking. </p> <p>Extremely well designed, it showcases clean lines in both sports and timeless pieces. </p> <p>Powered by solar energy with 25 features including weather forecasting, altimeter, second time zone and a compass it is the perfect travel companion. </p> ",
                Sku = "P-9002",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Tissot T-Touch Expert Solar",
                ShowOnHomePage = true,
                Price = 969.00M,
                OldPrice = 990.00M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = thirdDeliveryTime.Id
            };

            AddProductPicture(productTissotTTouchExpertSolar, "product/tissot-t-touch-expert-solar.jpg");
            AddProductPicture(productTissotTTouchExpertSolar, "product/tissot-t-touch-expert-solar-t091_2.jpg");

            productTissotTTouchExpertSolar.ProductCategories.Add(new ProductCategory { Category = categories["Watches"], DisplayOrder = 1 });

            productTissotTTouchExpertSolar.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Tissot"], DisplayOrder = 1 });

            productTissotTTouchExpertSolar.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // offer > best price
                SpecificationAttributeOption = specAttributes[22].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 8)
            });
            productTissotTTouchExpertSolar.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 2,
                // manufacturer > Tissot
                SpecificationAttributeOption = specAttributes[20].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 17)
            });
            productTissotTTouchExpertSolar.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // housing > steel
                SpecificationAttributeOption = specAttributes[8].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });
            productTissotTTouchExpertSolar.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // material -> silicone
                SpecificationAttributeOption = specAttributes[8].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 7)
            });
            productTissotTTouchExpertSolar.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // Gender -> gentlemen
                SpecificationAttributeOption = specAttributes[7].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });
            productTissotTTouchExpertSolar.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // movement -> Automatic, self-winding
                SpecificationAttributeOption = specAttributes[9].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });
            productTissotTTouchExpertSolar.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // diameter -> 44mm
                SpecificationAttributeOption = specAttributes[24].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 3)
            });
            productTissotTTouchExpertSolar.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // closure -> thorn close
                SpecificationAttributeOption = specAttributes[25].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 3)
            });

            #endregion productTissotT-TouchExpertSolar

            #region productSeikoSRPA49K1

            var productSeikoSRPA49K1 = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Seiko Mechanical Automatic SRPA49K1",
                ShortDescription = "Seiko Mechanical Automatic SRPA49K1",
                FullDescription = "<p><strong>Seiko 5 Sports Automatic Watch SRPA49K1 SRPA49</strong> </p> <ul>   <li>Unidirectional Rotating Bezel</li>   <li>Day And Date Display</li>   <li>See Through Case Back</li>   <li>100M Water Resistance</li>   <li>Stainless Steel Case</li>   <li>Automatic Movement</li>   <li>24 Jewels</li>   <li>Caliber: 4R36</li> </ul> ",
                Sku = "P-9003",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Seiko Mechanical Automatic SRPA49K1",
                ShowOnHomePage = true,
                Price = 269.00M,
                OldPrice = 329.00M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = thirdDeliveryTime.Id
            };

            AddProductPicture(productSeikoSRPA49K1, "product/SeikoSRPA49K1.jpg");
            productSeikoSRPA49K1.ProductCategories.Add(new ProductCategory { Category = categories["Watches"], DisplayOrder = 1 });

            productSeikoSRPA49K1.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Seiko"], DisplayOrder = 1 });

            productSeikoSRPA49K1.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // housing > steel
                SpecificationAttributeOption = specAttributes[8].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });
            productSeikoSRPA49K1.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // material -> stainless steel
                SpecificationAttributeOption = specAttributes[8].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });
            productSeikoSRPA49K1.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 2,
                // manufacturer > Seiko
                SpecificationAttributeOption = specAttributes[20].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 16)
            });
            productSeikoSRPA49K1.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // Gender -> gentlemen
                SpecificationAttributeOption = specAttributes[7].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });
            productSeikoSRPA49K1.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // movement -> quarz
                SpecificationAttributeOption = specAttributes[9].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });
            productSeikoSRPA49K1.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // closure -> folding clasp
                SpecificationAttributeOption = specAttributes[25].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 2)
            });
            productSeikoSRPA49K1.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // diameter -> 44mm
                SpecificationAttributeOption = specAttributes[24].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 3)
            });

            #endregion productSeikoSRPA49K1 


            #region productWatchesCertinaDSPodiumBigSize

            var productWatchesCertinaDSPodiumBigSize = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Name = "Certina DS Podium Big Size",
                ShortDescription = "C001.617.26.037.00",
                FullDescription = "<p>Since 1888, Certina has maintained an enviable reputation for its excellent watches and reliable movements. From the time of its integration into the SMH (today's Swatch Group) in the early 1980s, every Certina has been equipped with a high-quality ETA movement.</p><p>In a quartz watch movement, high-frequency oscillations are generated in a tiny synthetic crystal, then divided down electronically to provide the extreme accuracy of the Certina internal clock. A battery supplies the necessary energy.</p><p>The quartz movement is sometimes equipped with an end-of-life (EOL) indicator. When the seconds hand begins moving in four-second increments, the battery should be replaced within two weeks.</p><p>An automatic watch movement is driven by a rotor. Arm and wrist movements spin the rotor, which in turn winds the main spring. Energy is continuously produced, eliminating the need for a battery. The rate precision therefore depends on a rigorous manufacturing process and the original calibration, as well as the lifestyle of the user.</p><p>Most automatic movements are driven by an offset rotor. To earn the title of chronometer, a watch must be equipped with a movement that has obtained an official rate certificate from the COSC (Contrôle Officiel Suisse des Chronomètres). To obtain this, precision tests in different positions and at different temperatures must be carried out. These tests take place over a 15-day period. Thermocompensated means that the effective temperature inside the watch is measured and taken into account when improving precision. This allows fluctuations in the rate precision of a normal quartz watch due to temperature variations to be reduced by several seconds a week. The precision is 20 times more accurate than on a normal quartz watch, i.e. +/- 10 seconds per year (0.07 seconds/day).</p>",
                Sku = "P-9004",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Certina DS Podium Big Size",
                ShowOnHomePage = true,
                Price = 479.00M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = thirdDeliveryTime.Id
            };

            AddProductPicture(productWatchesCertinaDSPodiumBigSize, "product/certina_ds_podium_big.png");
            productWatchesCertinaDSPodiumBigSize.ProductCategories.Add(new ProductCategory { Category = categories["Watches"], DisplayOrder = 1 });

            productWatchesCertinaDSPodiumBigSize.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Certina"], DisplayOrder = 1 });

            productWatchesCertinaDSPodiumBigSize.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // housing > steel
                SpecificationAttributeOption = specAttributes[8].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });
            productWatchesCertinaDSPodiumBigSize.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // material -> leather
                SpecificationAttributeOption = specAttributes[8].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 5)
            });
            productWatchesCertinaDSPodiumBigSize.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 2,
                // manufacturer > Certina
                SpecificationAttributeOption = specAttributes[20].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 14)
            });
            productWatchesCertinaDSPodiumBigSize.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // Gender -> gentlemen
                SpecificationAttributeOption = specAttributes[7].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 1)
            });
            productWatchesCertinaDSPodiumBigSize.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // movement -> quarz
                SpecificationAttributeOption = specAttributes[9].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 3)
            });
            productWatchesCertinaDSPodiumBigSize.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // closure -> folding clasp
                SpecificationAttributeOption = specAttributes[25].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 2)
            });
            productWatchesCertinaDSPodiumBigSize.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // diameter -> 40mm
                SpecificationAttributeOption = specAttributes[24].SpecificationAttributeOptions.FirstOrDefault(x => x.DisplayOrder == 2)
            });

            #endregion productWatchesCertinaDSPodiumBigSize

            #endregion watches                      

            #region gaming

            var manuSony = _ctx.Set<Manufacturer>().First(c => c.Name == "Sony");
            var manuMicrosoft = _ctx.Set<Manufacturer>().First(c => c.Name == "Microsoft");
            var manuEASports = _ctx.Set<Manufacturer>().First(c => c.Name == "EA Sports");
            var manuUbisoft = _ctx.Set<Manufacturer>().First(c => c.Name == "Ubisoft");
            var categoryGaming = _ctx.Set<Category>().First(c => c.Alias == "Gaming");
            var categoryGamingAccessories = _ctx.Set<Category>().First(c => c.Alias == "Gaming Accessories");
            var categoryGamingGames = _ctx.Set<Category>().First(c => c.Alias == "Games");
            var manuWarnerHomme = _ctx.Set<Manufacturer>().First(c => c.Name == "Warner Home Video Games");

            #region bundlePs3AssassinCreed

            var productPs3 = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Sku = "Sony-PS399000",
                Name = "Playstation 4 Pro",
                ShortDescription = "The Sony PlayStation 4 Pro is the multi media console for next-generation digital home entertainment. It offers the Blu-ray technology, which enables you to enjoy movies in high definition.",
                FullDescription = "<ul><li>PowerPC-base Core @5.2GHz</li><li>1 VMX vector unit per core</li><li>512KB L2 cache</li><li>7 x SPE @5.2GHz</li><li>7 x 128b 128 SIMD GPRs</li><li>7 x 256MB SRAM for SPE</li><li>* 1 of 8 SPEs reserved for redundancy total floating point performance: 218 GFLOPS</li><li> 1.8 TFLOPS floating point performance</li><li>Full HD (up to 1080p) x 2 channels</li><li>Multi-way programmable parallel floating point shader pipelines</li><li>GPU: RSX @550MHz</li><li>256MB XDR Main RAM @3.2GHz</li><li>256MB GDDR3 VRAM @700MHz</li><li>Sound: Dolby 5.1ch, DTS, LPCM, etc. (Cell-base processing)</li><li>Wi-Fi: IEEE 802.11 b/g</li><li>USB: Front x 4, Rear x 2 (USB2.0)</li><li>Memory Stick: standard/Duo, PRO x 1</li></ul>",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                //MetaTitle = "Playstation 4 Super Slim",
                MetaTitle = "Playstation 4 Pro",
                Price = 189.00M,
                OldPrice = 199.99M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = firstDeliveryTime.Id
            };

            AddProductPicture(productPs3, "product/ps4_w_controller.jpg", "ps4-w-controller");
            AddProductPicture(productPs3, "product/ps4_wo_controller.jpg", "ps4-wo-single");

            productPs3.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Sony"], DisplayOrder = 1 });
            productPs3.ProductCategories.Add(new ProductCategory { Category = categories["Gaming"], DisplayOrder = 4 });

            var productDualshock4Controller = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Sku = "Sony-PS399004",
                Name = "DUALSHOCK 4 Wireless Controller",
                ShortDescription = "Revolutionary. Intuitive. Precise. A revolutionary controller for a new era of gaming, the DualShock 4 Wireless Controller features familiar PlayStation controls and innovative new additions, such as a touch pad, light bar, and more.",
                FullDescription = "<ul>  <li>Precision Controller for PlayStation 4: The feel, shape, and sensitivity of the DualShock 4’s analog sticks and trigger buttons have been enhanced to offer players absolute control for all games</li>  <li>Sharing at your Fingertips: The addition of the Share button makes sharing your greatest gaming moments as easy as a push of a button. Upload gameplay videos and screenshots directly from your system or live-stream your gameplay, all without disturbing the game in progress.</li>  <li>New ways to Play: Revolutionary features like the touch pad, integrated light bar, and built-in speaker offer exciting new ways to experience and interact with your games and its 3.5mm audio jack offers a practical personal audio solution for gamers who want to listen to their games in private.</li>  <li>Charge Efficiently: The DualShock 4 Wireless Controller can easily be recharged by plugging it into your PlayStation 4 system, even when on standby, or with any standard charger with a micro-USB port.</li></ul>",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "DUALSHOCK 4 Wireless Controller",
                Price = 54.90M,
                OldPrice = 59.90M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = firstDeliveryTime.Id
            };

            AddProductPicture(productDualshock4Controller, "product/dualshock4.jpg");
            productDualshock4Controller.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Sony"], DisplayOrder = 1 });
            productDualshock4Controller.ProductCategories.Add(new ProductCategory { Category = categories["Gaming Accessories"], DisplayOrder = 1 });


            var productMinecraft = new Product
            {
                ProductType = ProductType.SimpleProduct,
                //Sku = "Ubi-acreed3",
                Sku = "PD-Minecraft4ps4",
                Name = "Minecraft - Playstation 4 Edition",
                ShortDescription = "Third-person action-adventure title set.",
                FullDescription = "<p><strong>Build! Craft! Explore! </strong></p><p>The critically acclaimed Minecraft comes to PlayStation 4, offering bigger worlds and greater draw distance than the PS3 and PS Vita editions.</p><p>Create your own world, then, build, explore and conquer. When night falls the monsters appear, so be sure to build a shelter before they arrive.</p><p>The world is only limited by your imagination! Bigger worlds and greater draw distance than PS3 and PS Vita Editions Includes all features from the PS3 version Import your PS3 and PS Vita worlds to the PS4 Editition.</p>",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                //MetaTitle = "Assassin's Creed III",
                MetaTitle = "Minecraft - Playstation 4 Edition",

                Price = 49.90M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = firstDeliveryTime.Id
            };

            AddProductPicture(productMinecraft, "product/minecraft.jpg");
            productMinecraft.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Sony"], DisplayOrder = 1 });
            productMinecraft.ProductCategories.Add(new ProductCategory { Category = categories["Games"], DisplayOrder = 4 });


            var productBundlePs3AssassinCreed = new Product
            {
                ProductType = ProductType.BundledProduct,
                Sku = "Sony-PS399105",
                Name = "PlayStation 4 Minecraft Bundle",
                ShortDescription = "100GB PlayStation®4 system, 2 × DUALSHOCK®4 wireless controller and Minecraft for PS4 Edition.",
                FullDescription =
                    "<ul><li><h4>Processor</h4><ul><li>Processor Technology : Cell Broadband Engine™</li></ul></li><li><h4>General</h4><ul><li>Communication : Ethernet (10BASE-T, 100BASE-TX, 1000BASE-T IEEE 802.11 b/g Wi-Fi<br tabindex=\"0\">Bluetooth 2.0 (EDR)</li><li>Inputs and Outputs : USB 2.0 X 2</li></ul></li><li><h4>Graphics</h4><ul><li>Graphics Processor : RSX</li></ul></li><li><h4>Memory</h4><ul><li>Internal Memory : 256MB XDR Main RAM<br>256MB GDDR3 VRAM</li></ul></li><li><h4>Power</h4><ul><li>Power Consumption (in Operation) : Approximately 250 watts</li></ul></li><li><h4>Storage</h4><ul><li>Storage Capacity : 2.5' Serial ATA (500GB)</li></ul></li><li><h4>Video</h4><ul><li>Resolution : 480i, 480p, 720p, 1080i, 1080p (24p/60p)</li></ul></li><li><h4>Weights and Measurements</h4><ul><li>Dimensions (Approx.) : Approximately 11.42\" (W) x 2.56\" (H) x 11.42\" (D) (290mm x 65mm x 290mm)</li><li>Weight (Approx.) : Approximately 7.055 lbs (3.2 kg)</li></ul></li></ul>",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "PlayStation 4 Minecraft Bundle",
                Price = 269.97M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = firstDeliveryTime.Id,
                ShowOnHomePage = true,
                BundleTitleText = "Bundle includes",
                BundlePerItemPricing = true,
                BundlePerItemShoppingCart = true
            };

            AddProductPicture(productBundlePs3AssassinCreed, "product/ps4_bundle_minecraft.jpg");
            productBundlePs3AssassinCreed.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Sony"], DisplayOrder = 1 });
            productBundlePs3AssassinCreed.ProductCategories.Add(new ProductCategory { Category = categories["Gaming"], DisplayOrder = 1 });

            #endregion bundlePs3AssassinCreed

            #region bundlePs4

            var productPs4 = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Sku = "Sony-PS410034",
                //Sku = "PS410037",

                Name = "PlayStation 4",
                ShortDescription = "The best place to play. Working with some of the most creative minds in the industry, PlayStation®4 delivers breathtaking and unique gaming experiences.",
                FullDescription = "<p><h4>The power to perform.</h4><div>PlayStation®4 was designed from the ground up to ensure that game creators can unleash their imaginations to develop the very best games and deliver new play experiences never before possible. With ultra-fast customized processors and 8GB of high-performance unified system memory, PS4™ is the home to games with rich, high-fidelity graphics and deeply immersive experiences that shatter expectations.</div></p><p><ul><li><h4>Processor</h4><ul><li>Processor Technology : Low power x86-64 AMD 'Jaguar', 8 cores</li></ul></li><li><h4>Software</h4><ul><li>Processor : Single-chip custom processor</li></ul></li><li><h4>Display</h4><ul><li>Display Technology : HDMI<br />Digital Output (optical)</li></ul></li><li><h4>General</h4><ul><li>Ethernet ports x speed : Ethernet (10BASE-T, 100BASE-TX, 1000BASE-T); IEEE 802.11 b/g/n; Bluetooth® 2.1 (EDR)</li><li>Hard disk : Built-in</li></ul></li><li><h4>General Specifications</h4><ul><li>Video : BD 6xCAV<br />DVD 8xCAV</li></ul></li><li><h4>Graphics</h4><ul><li>Graphics Processor : 1.84 TFLOPS, AMD Radeon™ Graphics Core Next engine</li></ul></li><li><h4>Interface</h4><ul><li>I/O Port : Super-Speed USB (USB 3.0), AUX</li></ul></li><li><h4>Memory</h4><ul><li>Internal Memory : GDDR5 8GB</li></ul></li></ul></p>",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "PlayStation 4",
                Price = 399.99M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 3,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = firstDeliveryTime.Id
            };

            AddProductPicture(productPs4, "product/sony_ps4.png", "sony-ps4");
            AddProductPicture(productPs4, "product/sony_dualshock4_wirelesscontroller.png");

            productPs4.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Sony"], DisplayOrder = 1 });
            productPs4.ProductCategories.Add(new ProductCategory { Category = categories["Gaming"], DisplayOrder = 5 });


            var productPs4Camera = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Sku = "Sony-PS410040",
                Name = "PlayStation 4 Camera",
                ShortDescription = "Play, challenge and share your epic gaming moments with PlayStation®Camera and your PS4™. Multiplayer is enhanced through immediate, crystal clear audio and picture-in-picture video sharing.",
                FullDescription = "<ul><li>When combined with the DualShock 4 Wireless Controller's light bar, the evolutionary 3D depth-sensing technology in the PlayStation Camera allows it to precisely track a player's position in the room.</li><li>From navigational voice commands to facial recognition, the PlayStation Camera adds incredible innovation to your gaming.</li><li>Automatically integrate a picture-in-picture video of yourself during gameplay broadcasts, and challenge your friends during play.</li><li>Never leave a friend hanging or miss a chance to taunt your opponents with voice chat that keeps the conversation going, whether it's between rounds, between games, or just while kicking back.</li></ul>",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "PlayStation 4 Camera",
                Price = 59.99M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = firstDeliveryTime.Id
            };

            AddProductPicture(productPs4Camera, "product/sony_ps4_camera.png");
            productPs4Camera.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Sony"], DisplayOrder = 1 });
            productPs4Camera.ProductCategories.Add(new ProductCategory { Category = categories["Gaming Accessories"], DisplayOrder = 3 });


            var productBundlePs4 = new Product
            {
                ProductType = ProductType.BundledProduct,
                Sku = "Sony-PS410099",
                Name = "PlayStation 4 Bundle",
                ShortDescription = "PlayStation®4 system, DUALSHOCK®4 wireless controller and PS4 camera.",
                FullDescription =
                    "<p><h4>The best place to play</h4><div>PlayStation 4 is the best place to play with dynamic, connected gaming, powerful graphics and speed, intelligent personalization, deeply integrated social capabilities, and innovative second-screen features. Combining unparalleled content, immersive gaming experiences, all of your favorite digital entertainment apps, and PlayStation exclusives, PS4 centers on gamers, enabling them to play when, where and how they want. PS4 enables the greatest game developers in the world to unlock their creativity and push the boundaries of play through a system that is tuned specifically to their needs.</div></p><p><h4>Gamer focused, developer inspired</h4><div>The PS4 system focuses on the gamer, ensuring that the very best games and the most immersive experiences are possible on the platform. The PS4 system enables the greatest game developers in the world to unlock their creativity and push the boundaries of play through a system that is tuned specifically to their needs. The PS4 system is centered around a powerful custom chip that contains eight x86-64 cores and a state of the art 1.84 TFLOPS graphics processor with 8 GB of ultra-fast GDDR5 unified system memory, easing game creation and increasing the richness of content achievable on the platform. The end result is new games with rich, high-fidelity graphics and deeply immersive experiences.</div></p><p><h4>Personalized, curated content</h4><div>The PS4 system has the ability to learn about your preferences. It will learn your likes and dislikes, allowing you to discover content pre-loaded and ready to go on your console in your favorite game genres or by your favorite creators. Players also can look over game-related information shared by friends, view friends’ gameplay with ease, or obtain information about recommended content, including games, TV shows and movies.</div></p><p><h4>New DUALSHOCK controller</h4><div>DUALSHOCK 4 features new innovations to deliver more immersive gaming experiences, including a highly sensitive six-axis sensor as well as a touch pad located on the top of the controller, which offers gamers completely new ways to play and interact with games.</div></p><p><h4>Shared game experiences</h4><div>Engage in endless personal challenges with your community and share your epic triumphs with the press of a button. Simply hit the SHARE button on the controller, scan through the last few minutes of gameplay, tag it and return to the game—the video uploads as you play. The PS4 system also enhances social spectating by enabling you to broadcast your gameplay in real-time.</div></p><p><h4>Remote play</h4><div>Remote Play on the PS4 system fully unlocks the PlayStation Vita system’s potential, making it the ultimate companion device. With the PS Vita system, gamers will be able to seamlessly play a range of PS4 titles on the beautiful 5-inch display over Wi-Fi access points in a local area network.</div></p><p><h4>PlayStation app</h4><div>The PlayStation App will enable iPhone, iPad, and Android-based smartphones and tablets to become second screens for the PS4 system. Once installed on these devices, players can view in-game items, purchase PS4 games and download them directly to the console at home, or remotely watch the gameplay of other gamers playing on their devices.</div></p><p><h4>PlayStation Plus</h4><div>Built to bring games and gamers together and fuel the next generation of gaming, PlayStation Plus helps you discover a world of exceptional gaming experiences. PlayStation Plus is a membership service that takes your gaming experience to the next level. Each month members receive an Instant Game Collection of top rated blockbuster and innovative Indie games, which they can download direct to their console.</div></p>",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "PlayStation 4 Bundle",
                Price = 429.99M,
                OldPrice = 449.99M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = firstDeliveryTime.Id,
                BundleTitleText = "Bundle includes"
            };

            AddProductPicture(productBundlePs4, "product/sony_ps4_bundle.png");
            productBundlePs4.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Sony"], DisplayOrder = 1 });
            productBundlePs4.ProductCategories.Add(new ProductCategory { Category = categories["Gaming"], DisplayOrder = 2 });

            #endregion bundlePs4

            #region groupAccessories

            var productGroupAccessories = new Product
            {
                ProductType = ProductType.GroupedProduct,
                Sku = "Sony-GroupAccessories",
                Name = "Accessories for unlimited gaming experience",
                ShortDescription = "The future of gaming is now with dynamic, connected gaming, powerful graphics and speed, intelligent personalization, deeply integrated social capabilities, and innovative second-screen features. The brilliant culmination of the most creative minds in the industry, PlayStation®4 delivers a unique gaming environment that will take your breath away.",
                FullDescription = "<ul><li>Immerse yourself in a new world of gameplay with powerful graphics and speed.</li><li>Eliminate lengthy load times of saved games with Suspend mode.</li><li>Immediately play digital titles without waiting for them to finish downloading thanks to background downloading and updating capability.</li><li>Instantly share images and videos of your favorite gameplay moments on Facebook with the SHARE button on the DUALSHOCK®4 controller.</li><li>Broadcast while you play in real-time through Ustream.</li></ul>",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Accessories for unlimited gaming experience",
                Price = 0.0M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 3,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                ShowOnHomePage = true
            };

            AddProductPicture(productGroupAccessories, "category/gaming_accessories.png");
            productGroupAccessories.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Sony"], DisplayOrder = 1 });
            productGroupAccessories.ProductCategories.Add(new ProductCategory { Category = categories["Gaming"], DisplayOrder = 3 });

            #endregion groupAccessories

            #region Ps3PlusOneGame

            var productPrinceOfPersia = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Sku = "Ubi-princepersia",
                Name = "Prince of Persia \"The Forgotten Sands\"",
                ShortDescription = "Play the epic story of the heroic Prince as he fights and outwits his enemies in order to save his kingdom.",
                FullDescription = "<p>This game marks the return to the Prince of Persia® Sands of Time storyline. Prince of Persia: The Forgotten Sands™ will feature many of the fan-favorite elements from the original series as well as new gameplay innovations that gamers have come to expect from Prince of Persia.</p><p>Experience the story, setting, and gameplay in this return to the Sands of Time universe as we follow the original Prince of Persia through a new untold chapter.</p><p>Created by Ubisoft Montreal, the team that brought you various Prince of Persia® and Assassin’s Creed® games, Prince of Persia The Forgotten Sands™ has been over 2 years in the making.</p>",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Prince of Persia",
                Price = 39.90M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = firstDeliveryTime.Id
            };

            AddProductPicture(productPrinceOfPersia, "product/princeofpersia.jpg");
            productPrinceOfPersia.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Ubisoft"], DisplayOrder = 1 });
            productPrinceOfPersia.ProductCategories.Add(new ProductCategory { Category = categories["Games"], DisplayOrder = 2 });

            #endregion Ps3PlusOneGame

            #region Horizon Zero Down

            var productHorizonZeroDown = new Product
            {
                ProductType = ProductType.SimpleProduct,
                //Sku = "Ubi-princepersia",
                Sku = "PD-ZeroDown4PS4",
                Name = "Horizon Zero Dawn - PlayStation 4",
                ShortDescription = "Experience A Vibrant, Lush World Inhabited By Mysterious Mechanized Creatures",
                FullDescription = "<ul>  <li>A Lush Post-Apocalyptic World – How have machines dominated this world, and what is their purpose? What happened to the civilization here before? Scour every corner of a realm filled with ancient relics and mysterious buildings in order to uncover your past and unearth the many secrets of a forgotten land.</li>  <li></li>  <li>Nature and Machines Collide – Horizon Zero Dawn juxtaposes two contrasting elements, taking a vibrant world rich with beautiful nature and filling it with awe-inspiring highly advanced technology. This marriage creates a dynamic combination for both exploration and gameplay.</li>  <li>Defy Overwhelming Odds – The foundation of combat in Horizon Zero Dawn is built upon the speed and cunning of Aloy versus the raw strength and size of the machines. In order to overcome a much larger and technologically superior enemy, Aloy must use every ounce of her knowledge, intelligence, and agility to survive each encounter.</li>  <li>Cutting Edge Open World Tech – Stunningly detailed forests, imposing mountains, and atmospheric ruins of a bygone civilization meld together in a landscape that is alive with changing weather systems and a full day/night cycle.</li></ul>",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Horizon Zero Dawn - PlayStation 4",
                Price = 69.90M,
                OldPrice = 79.90M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = firstDeliveryTime.Id
            };

            AddProductPicture(productHorizonZeroDown, "product/horizon.jpg");
            productHorizonZeroDown.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Sony"], DisplayOrder = 1 });
            productHorizonZeroDown.ProductCategories.Add(new ProductCategory { Category = categories["Games"], DisplayOrder = 2 });

            #endregion Horizon Zero Down

            #region Fifa 17

            var productFifa17 = new Product
            {
                ProductType = ProductType.SimpleProduct,
                //Sku = "Ubi-princepersia",
                Sku = "PD-Fifa17",
                Name = "FIFA 17 - PlayStation 4",
                ShortDescription = "Powered by Frostbite",
                FullDescription = "<ul>  <li>Powered by Frostbite: One of the industry’s leading game engines, Frostbite delivers authentic, true-to-life action, takes players to new football worlds, and introduces fans to characters full of depth and emotion in FIFA 17.</li>  <li>The Journey: For the first time ever in FIFA, live your story on and off the pitch as the Premier League’s next rising star, Alex Hunter. Play on any club in the premier league, for authentic managers and alongside some of the best players on the planet. Experience brand new worlds in FIFA 17, all while navigating your way through the emotional highs and lows of The Journey.</li>  <li>Own Every Moment: Complete innovation in the way players think and move, physically interact with opponents, and execute in attack puts you in complete control of every moment on the pitch.</li></ul>",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "FIFA 17 - PlayStation 4",
                Price = 79.90M,
                OldPrice = 89.90M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = firstDeliveryTime.Id
            };

            AddProductPicture(productFifa17, "product/fifa17.jpg");
            productFifa17.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["EA Sports"], DisplayOrder = 1 });
            productFifa17.ProductCategories.Add(new ProductCategory { Category = categories["Games"], DisplayOrder = 2 });

            #endregion Fifa 17

            #region Lego Worlds

            var productLegoWorlds = new Product
            {
                ProductType = ProductType.SimpleProduct,
                Sku = "Gaming-Lego-001",
                Name = "LEGO Worlds - PlayStation 4",
                ShortDescription = "Experience a galaxy of Worlds made entirely from LEGO bricks.",
                FullDescription = "<ul>  <li>Experience a galaxy of Worlds made entirely from LEGO bricks.</li>  <li>LEGO Worlds is an open environment of procedurally-generated Worlds made entirely of LEGO bricks which you can freely manipulate and dynamically populate with LEGO models.</li>  <li>Create anything you can imagine one brick at a time, or use large-scale landscaping tools to create vast mountain ranges and dot your world with tropical islands.</li>  <li>Explore using helicopters, dragons, motorbikes or even gorillas and unlock treasures that enhance your gameplay.</li>  <li>Watch your creations come to life through characters and creatures that interact with you and each other in unexpected ways.</li></ul><p></p>",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "LEGO Worlds - PlayStation 4",
                Price = 29.90M,
                OldPrice = 34.90M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTimeId = firstDeliveryTime.Id
            };

            AddProductPicture(productLegoWorlds, "product/legoworlds.jpg");
            productLegoWorlds.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manufacturers["Warner Home Video Games"], DisplayOrder = 1 });
            productLegoWorlds.ProductCategories.Add(new ProductCategory { Category = categories["Games"], DisplayOrder = 3 });

            #endregion Lego Worlds

            //#region XBox One S

            //var productXBoxOneS = new Product
            //{
            //    ProductType = ProductType.SimpleProduct,
            //    Sku = "Microsoft-xbox1s",
            //    Name = "Xbox One S 500 GB Konsole",
            //    ShortDescription = "Genieße über 100 Spiele, die es nur für die Konsole gibt, sowie eine ständig größer werdende Bibliothek an Xbox 360-Spielen auf der Xbox One S im neuen Design – der einzigen Konsole mit 4K Ultra HD Blu-ray, 4K-Videostreaming und HDR. Streame deine Lieblingsfilme und -sendungen in atemberaubendem 4K Ultra HD. Spiele Blockbuster wie Gears of War 4 und Battlefield 1 mit Freunden auf Xbox Live, dem schnellsten und zuverlässigsten Gaming-Netzwerk.",
            //    FullDescription = "<ul><li>Die ultimativen Spiele und 4K-Entertainment-System.</li> <li><b>40 % kompaktere Konsole<b> <br/> Lasse dich nicht von der Größe täuschen. Mit integriertem Netzteil und bis zu 2 TB Speicherplatz ist die Xbox One S die fortschrittlichste Xbox überhaupt.</li><li><b>Der beste Controller - jetzt noch besser</b> <br/> Der neue Xbox Wireless Controller bietet ein schlankes, optimiertes Design, texturierte Grip - Fläche und Bluetooth zum Spielen auf Windows 10 Geräten. Genieße individuelle Tastenbelegung und verbesserte drahtlose Reichweite und stecke jeden kompatiblen Kopfhörer mit der 3, 5 mm Stereo - Headset - Buchse ein.</li></ul>",
            //    ProductTemplateId = productTemplate.Id,
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    MetaTitle = "Xbox One S",
            //    Price = 279.99M,
            //    OldPrice = 279.99M,
            //    ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    IsShipEnabled = true,
            //    DeliveryTimeId = firstDeliveryTime.Id
            //};

            //AddProductPicture(productXBoxOneS, "product/xbox_one_s_1.png", "microsoft-xbox-one-s-1");
            //productXBoxOneS.ProductManufacturers.Add(new ProductManufacturer { Manufacturer = manuMicrosoft, DisplayOrder = 1 });
            //productXBoxOneS.ProductCategories.Add(new ProductCategory { Category = categoryGaming, DisplayOrder = 1 });

            //#endregion

            #endregion gaming

            var entities = new List<Product>
            {
                productTRANSOCEANCHRONOGRAPH, productTissotTTouchExpertSolar, productSeikoSRPA49K1,
                productTitleistSM6TourChrome, productTitleistProV1x, productGBBEpicSubZeroDriver,
                productSupremeGolfball, productBooksStoneOfTheWise, productNikeStrikeFootball, productNikeEvoPowerBall,
                productTorfabrikOfficialGameBall, productAdidasTangoSalaBall, productAllCourtBasketball, productEvolutionHighSchoolGameBasketball, productRayBanTopBar,
                productOriginalWayfarer, productCustomFlakSunglasses, productRadarEVPrizmSportsSunglasses, productAppleProHipsterBundle, product97ipad, productAirpods,
                productIphoneplus,productWatchSeries2,product10GiftCard, product25GiftCard, product50GiftCard,product100GiftCard, productBooksUberMan, productBooksGefangeneDesHimmels,
                productBooksBestGrillingRecipes, productBooksCookingForTwo, productBooksAutosDerSuperlative, productBooksBildatlasMotorraeder,
                productBooksAutoBuch, productBooksFastCars, productBooksMotorradAbenteuer,
                productInstantDownloadVivaldi, productInstantDownloadBeethoven, productWatchesCertinaDSPodiumBigSize,
                productPs3, productMinecraft, productBundlePs3AssassinCreed,
                productPs4, productDualshock4Controller, productPs4Camera, productBundlePs4,
                productGroupAccessories, productPrinceOfPersia, productLegoWorlds, productHorizonZeroDown, productFifa17, productJackWolfsKinKanukaPoint
            };

            entities.AddRange(GetFashionProducts(specAttributes));
            entities.AddRange(GetFurnitureProducts(specAttributes));

            this.Alter(entities);
            return entities;
        }

        public IList<ProductBundleItem> ProductBundleItems()
        {
            var utcNow = DateTime.UtcNow;

            #region apple bundles
            var bundleAppleProHipster = _ctx.Set<Product>().First(x => x.Sku == "P-2005-Bundle");

            var bundleItemIproductIphoneplus = new ProductBundleItem()
            {
                BundleProduct = bundleAppleProHipster,
                Product = _ctx.Set<Product>().First(x => x.Sku == "P-2001"),
                Quantity = 1,
                Discount = 40.0M,
                Visible = true,
                Published = true,
                DisplayOrder = 1
            };

            var bundleItemProductWatchSeries2 = new ProductBundleItem()
            {
                BundleProduct = bundleAppleProHipster,
                Product = _ctx.Set<Product>().First(x => x.Sku == "P-2002"),
                Quantity = 2,
                Discount = 30.0M,
                Visible = true,
                Published = true,
                DisplayOrder = 2
            };

            var bundleItemproductAirpods = new ProductBundleItem()
            {
                BundleProduct = bundleAppleProHipster,
                Product = _ctx.Set<Product>().First(x => x.Sku == "P-2003"),
                Quantity = 1,
                Discount = 30.0M,
                Visible = true,
                Published = true,
                DisplayOrder = 3
            };

            var bundleItemproductIpad = new ProductBundleItem()
            {
                BundleProduct = bundleAppleProHipster,
                Product = _ctx.Set<Product>().First(x => x.Sku == "P-2004"),
                Quantity = 1,
                Discount = 30.0M,
                Visible = true,
                Published = true,
                DisplayOrder = 3
            };

            #endregion apple bundles

            #region gaming

            //var bundlePs3AssassinCreed = _ctx.Set<Product>().First(x => x.Sku == "Sony-PS399105");
            var bundlePs4Minecraft = _ctx.Set<Product>().First(x => x.Sku == "Sony-PS399105");

            var bundleItemPs4Minecraft1 = new ProductBundleItem()
            {
                BundleProduct = bundlePs4Minecraft,
                Product = _ctx.Set<Product>().First(x => x.Sku == "Sony-PS399000"),
                Quantity = 1,
                Discount = 20.0M,
                Visible = true,
                Published = true,
                DisplayOrder = 1
            };

            var bundleItemPs4Minecraft2 = new ProductBundleItem()
            {
                BundleProduct = bundlePs4Minecraft,
                Product = _ctx.Set<Product>().First(x => x.Sku == "Sony-PS399004"),
                Quantity = 2,
                Discount = 30.0M,
                Visible = true,
                Published = true,
                DisplayOrder = 2
            };

            var bundleItemPs4Minecraft3 = new ProductBundleItem()
            {
                BundleProduct = bundlePs4Minecraft,
                Product = _ctx.Set<Product>().First(x => x.Sku == "PD-Minecraft4ps4"),
                Quantity = 1,
                Discount = 20.0M,
                Visible = true,
                Published = true,
                DisplayOrder = 3
            };


            var bundlePs4 = _ctx.Set<Product>().First(x => x.Sku == "Sony-PS410099");

            var bundleItemPs41 = new ProductBundleItem
            {
                BundleProduct = bundlePs4,
                Product = _ctx.Set<Product>().First(x => x.Sku == "Sony-PS410034"),
                Quantity = 1,
                Visible = true,
                Published = true,
                DisplayOrder = 1
            };

            var bundleItemPs42 = new ProductBundleItem
            {
                BundleProduct = bundlePs4,
                //Product = _ctx.Set<Product>().First(x => x.Sku == "Sony-PS410037"),
                Product = _ctx.Set<Product>().First(x => x.Sku == "Sony-PS399004"),
                Quantity = 1,
                Visible = true,
                Published = true,
                DisplayOrder = 2
            };

            var bundleItemPs43 = new ProductBundleItem
            {
                BundleProduct = bundlePs4,
                Product = _ctx.Set<Product>().First(x => x.Sku == "Sony-PS410040"),
                Quantity = 1,
                Visible = true,
                Published = true,
                DisplayOrder = 3
            };

            #endregion gaming

            var entities = new List<ProductBundleItem>
            {
                bundleItemPs4Minecraft1, bundleItemPs4Minecraft2, bundleItemPs4Minecraft3,
                bundleItemPs41, bundleItemPs42, bundleItemPs43,bundleItemIproductIphoneplus, bundleItemProductWatchSeries2,bundleItemproductAirpods,bundleItemproductIpad
            };

            this.Alter(entities);
            return entities;
        }

        public void AddDownloads(IList<Product> savedProducts)
        {
            // Sample downloads.
            var sampleDownloadSkus = new List<string> { "P-1017", "P-1016", "P-6001" };
            var sampleDownloadProducts = savedProducts
                .Where(x => sampleDownloadSkus.Contains(x.Sku))
                .ToDictionary(x => x.Sku);

            var now = DateTime.UtcNow;

            foreach (var product in sampleDownloadProducts.Values)
            {
                if (product.Sku.IsCaseInsensitiveEqual("P-1017"))
                {
                    var buffer = File.ReadAllBytes(_sampleImagesPath + "download/beethoven-fur-elise.mp3");
                    product.SampleDownload = new Download
                    {
                        EntityId = product.Id,
                        EntityName = nameof(Product),
                        DownloadGuid = Guid.NewGuid(),
                        MediaFile = new MediaFile
                        {
                            Name = "beethoven-fur-elise.mp3",
                            MediaType = "audio",
                            MimeType = "audio/mp3",
                            Extension = "mp3",
                            Size = buffer.Length,
                            UpdatedOnUtc = now,
                            CreatedOnUtc = now,
                            MediaStorage = new MediaStorage { Data = buffer }
                        },
                        UpdatedOnUtc = now
                    };
                }
                else if (product.Sku.IsCaseInsensitiveEqual("P-1016"))
                {
                    var buffer = File.ReadAllBytes(_sampleImagesPath + "download/vivaldi-four-seasons-spring.mp3");
                    product.SampleDownload = new Download
                    {
                        EntityId = product.Id,
                        EntityName = nameof(Product),
                        DownloadGuid = Guid.NewGuid(),
                        MediaFile = new MediaFile
                        {
                            Name = "vivaldi-four-seasons-spring.mp3",
                            MediaType = "audio",
                            MimeType = "audio/mp3",
                            Extension = "mp3",
                            Size = buffer.Length,
                            UpdatedOnUtc = now,
                            CreatedOnUtc = now,
                            MediaStorage = new MediaStorage { Data = buffer }
                        },
                        UpdatedOnUtc = DateTime.UtcNow
                    };
                }
                else if (product.Sku.IsCaseInsensitiveEqual("P-6001"))
                {
                    var buffer = File.ReadAllBytes(_sampleImagesPath + "download/Stone_of_the_wise_preview.pdf");
                    product.SampleDownload = new Download
                    {
                        EntityId = product.Id,
                        EntityName = nameof(Product),
                        DownloadGuid = Guid.NewGuid(),
                        MediaFile = new MediaFile
                        {
                            Name = "Stone_of_the_wise_preview.pdf",
                            MediaType = "document",
                            MimeType = "application/pdf",
                            Extension = "pdf",
                            Size = buffer.Length,
                            UpdatedOnUtc = now,
                            CreatedOnUtc = now,
                            MediaStorage = new MediaStorage { Data = buffer }
                        },
                        UpdatedOnUtc = DateTime.UtcNow
                    };
                }
            }

            _ctx.SaveChanges();
        }

        public void AssignGroupedProducts(IList<Product> savedProducts)
        {
            int productGamingAccessoriesId = savedProducts.First(x => x.Sku == "Sony-GroupAccessories").Id;
            var gamingAccessoriesSkus = new List<string>() { "Sony-PS399004", "PD-Minecraft4ps4", "Sony-PS410037", "Sony-PS410040" };

            savedProducts
                .Where(x => gamingAccessoriesSkus.Contains(x.Sku))
                .ToList()
                .Each(x =>
                {
                    x.ParentGroupedProductId = productGamingAccessoriesId;

                    //_ctx.Set<Product>().Attach(x);
                    //_ctx.Entry(x).State = System.Data.Entity.EntityState.Modified;
                });

            _ctx.SaveChanges();
        }
    }
}
