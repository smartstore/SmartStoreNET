using System.Collections.Generic;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Data.Setup
{
    public abstract partial class InvariantSeedData
    {
        public IList<SpecificationAttribute> SpecificationAttributes()
        {
            #region sa1 CPU-Manufacturer

            var sa1 = new SpecificationAttribute
            {
                Name = "CPU-Manufacturer",
                DisplayOrder = 1,
            };
            sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "AMD",
                DisplayOrder = 1,
            });
            sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Intel",
                DisplayOrder = 2,
            });
            sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "ARM",
                DisplayOrder = 3,
            });
            sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Samsung",
                DisplayOrder = 4,
            });
            sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Apple",
                DisplayOrder = 5,
            });

            #endregion sa1 CPU-Manufacturer

            #region sa2 color

            var sa2 = new SpecificationAttribute
            {
                Name = "Color",
                DisplayOrder = 2,
            };
            sa2.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "white",
                DisplayOrder = 1,
            });
            sa2.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "black",
                DisplayOrder = 2,
            });
            sa2.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "beige",
                DisplayOrder = 3,
            });
            sa2.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "red",
                DisplayOrder = 4,
            });
            sa2.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "blue",
                DisplayOrder = 5,
            });
            sa2.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "green",
                DisplayOrder = 6,
            });
            sa2.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "yellow",
                DisplayOrder = 7,
            });

            #endregion sa2 color

            #region sa3 harddisk capacity

            var sa3 = new SpecificationAttribute
            {
                Name = "Harddisk capacity",
                DisplayOrder = 3,
            };
            sa3.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "250 GB",
                DisplayOrder = 1,
            });
            sa3.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "500 GB",
                DisplayOrder = 2,
            });
            sa3.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "750 GB",
                DisplayOrder = 3,
            });
            sa3.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "1000 GB",
                DisplayOrder = 4,
            });
            sa3.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "1500 GB",
                DisplayOrder = 5,
            });

            #endregion sa3 harddisk capacity

            #region sa4 ram

            var sa4 = new SpecificationAttribute
            {
                Name = "RAM",
                DisplayOrder = 4,
            };
            sa4.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "4 GB",
                DisplayOrder = 1,
            });
            sa4.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "8 GB",
                DisplayOrder = 2,
            });
            sa4.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "13 GB",
                DisplayOrder = 3,
            });
            sa4.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "2 GB",
                DisplayOrder = 4,
            });
            sa4.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "1500 GB",
                DisplayOrder = 5,
            });

            #endregion sa4 ram

            #region sa5 Operating System

            var sa5 = new SpecificationAttribute
            {
                Name = "Operating System",
                DisplayOrder = 5,
            };
            sa5.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Windows 7 32 Bit",
                DisplayOrder = 1,
            });
            sa5.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Windows 7 64 Bit",
                DisplayOrder = 2,
            });
            sa5.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Windows 8 32 Bit",
                DisplayOrder = 3,
            });
            sa5.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Windows 8 64 Bit",
                DisplayOrder = 4,
            });
            sa5.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Linux",
                DisplayOrder = 5,
            });
            sa5.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Mac OS",
                DisplayOrder = 6,
            });
            sa5.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Android 2",
                DisplayOrder = 7,
            });
            sa5.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Android 4",
                DisplayOrder = 8,
            });
            sa5.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "iOS",
                DisplayOrder = 9,
            });

            #endregion sa5 Operating System

            #region sa6 ports

            var sa6 = new SpecificationAttribute
            {
                Name = "Ports",
                DisplayOrder = 6,
            };
            sa6.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "USB 2.0",
                DisplayOrder = 1,
            });
            sa6.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "USB 3.0",
                DisplayOrder = 2,
            });
            sa6.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Firewire",
                DisplayOrder = 3,
            });
            sa6.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "HDMI",
                DisplayOrder = 4,
            });
            sa6.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "DVI",
                DisplayOrder = 5,
            });
            sa6.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "VGA",
                DisplayOrder = 6,
            });
            sa6.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Ethernet/RJ45",
                DisplayOrder = 7,
            });

            #endregion sa6 ports

            #region sa7 Gender

            var sa7 = new SpecificationAttribute
            {
                Name = "Gender",
                DisplayOrder = 7,
            };
            sa7.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "gentlemen",
                DisplayOrder = 1,
            });
            sa7.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "ladies",
                DisplayOrder = 2,
            });
            sa7.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "unisex",
                DisplayOrder = 3,
            });

            #endregion sa7 Gender

            #region sa8 material

            var sa8 = new SpecificationAttribute
            {
                Name = "Material",
                DisplayOrder = 8,
            };
            sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "stainless steel",
                DisplayOrder = 1,
            });
            sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "titanium",
                DisplayOrder = 2,
            });
            sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "plastic",
                DisplayOrder = 3,
            });
            sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "aluminium",
                DisplayOrder = 4,
            });

            sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "leather",
                DisplayOrder = 5,
            });

            sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "nylon",
                DisplayOrder = 6,
            });

            sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "silicone",
                DisplayOrder = 7,
            });

            sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "ceramic",
                DisplayOrder = 8,
            });

            sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
            {
                Name = "cotton",
                DisplayOrder = 9,
            });
            sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
            {
                Name = "100% organic cotton",
                DisplayOrder = 10,
            });
            sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
            {
                Name = "polyamide",
                DisplayOrder = 11,
            });
            sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
            {
                Name = "rubber",
                DisplayOrder = 12,
            });
            sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
            {
                Name = "wood",
                DisplayOrder = 13,
            });
            sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
            {
                Name = "glass",
                DisplayOrder = 14,
            });
            sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
            {
                Name = "spandex",
                DisplayOrder = 15,
            });
            sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
            {
                Name = "polyester",
                DisplayOrder = 16,
            });

            #endregion sa8 material

            #region sa9 movement

            var sa9 = new SpecificationAttribute
            {
                Name = "Movement",
                DisplayOrder = 9,
            };
            sa9.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "mechanical, self winding",
                DisplayOrder = 1,
            });
            sa9.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "mechanical",
                DisplayOrder = 2,
            });
            sa9.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "quarz, battery operated",
                DisplayOrder = 3,
            });

            #endregion sa9 movement

            #region sa10 clasp

            var sa10 = new SpecificationAttribute
            {
                Name = "Clasp",
                DisplayOrder = 10,
            };
            sa10.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "glidelock",
                DisplayOrder = 1,
            });
            sa10.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "folding clasp",
                DisplayOrder = 2,
            });
            sa10.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "thorn close",
                DisplayOrder = 3,
            });

            #endregion sa10 clasp

            #region sa11 window material

            var sa11 = new SpecificationAttribute
            {
                Name = "Window material",
                DisplayOrder = 11,
            };
            sa11.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "mineral",
                DisplayOrder = 1,
            });
            sa11.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "sapphire",
                DisplayOrder = 2,
            });

            #endregion sa11 window material

            #region sa12 language

            var sa12 = new SpecificationAttribute
            {
                Name = "Language",
                DisplayOrder = 12,
            };
            sa12.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "german",
                DisplayOrder = 1,
            });
            sa12.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "english",
                DisplayOrder = 2,
            });
            sa12.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "french",
                DisplayOrder = 3,
            });
            sa12.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "italian",
                DisplayOrder = 4,
            });

            #endregion sa12 language

            #region sa13 edition

            var sa13 = new SpecificationAttribute
            {
                Name = "Edition",
                DisplayOrder = 13,
            };
            sa13.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "bound",
                DisplayOrder = 1,
            });
            sa13.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "paperback",
                DisplayOrder = 2,
            });

            #endregion sa13 edition

            #region sa14 category

            var sa14 = new SpecificationAttribute
            {
                Name = "Category",
                DisplayOrder = 14,
            };
            sa14.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "adventure",
                DisplayOrder = 1,
            });
            sa14.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "fantasy & science fiction",
                DisplayOrder = 2,
            });
            sa14.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "history",
                DisplayOrder = 3,
            });
            sa14.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "internet & computer",
                DisplayOrder = 4,
            });
            sa14.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "thriller",
                DisplayOrder = 5,
            });
            sa14.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "cars",
                DisplayOrder = 6,
            });
            sa14.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "novel",
                DisplayOrder = 7,
            });
            sa14.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "cook and bake",
                DisplayOrder = 8,
            });
            sa14.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "non-fiction",
                DisplayOrder = 9,
            });

            #endregion sa14 category

            #region sa15 Computer-type

            var sa15 = new SpecificationAttribute
            {
                Name = "Computer-type",
                DisplayOrder = 15,
            };
            sa15.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "desktop",
                DisplayOrder = 1,
            });
            sa15.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "all-in-one",
                DisplayOrder = 2,
            });
            sa15.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "laptop",
                DisplayOrder = 3,
            });
            sa15.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "tablet",
                DisplayOrder = 4,
            });

            #endregion sa15 Computer-type

            #region sa16 type of mass-storage

            var sa16 = new SpecificationAttribute
            {
                Name = "Type of mass-storage",
                DisplayOrder = 16,
            };
            sa16.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "HDD",
                DisplayOrder = 1,
            });
            sa16.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "SSD",
                DisplayOrder = 2,
            });
            sa16.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Flash",
                DisplayOrder = 3,
            });

            #endregion sa16 type of mass-storage

            #region sa17 Size (ext. HDD)

            var sa17 = new SpecificationAttribute
            {
                Name = "Size (ext. HDD)",
                DisplayOrder = 17,
            };
            sa17.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "3.5",
                DisplayOrder = 1,
            });
            sa17.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "2.5",
                DisplayOrder = 2,
            });

            #endregion sa17 Size (ext. HDD)

            #region sa18 MP3 quality

            var sa18 = new SpecificationAttribute
            {
                Name = "MP3 quality",
                DisplayOrder = 18,
            };
            sa18.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "128 kbit/s",
                DisplayOrder = 1,
            });
            sa18.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "256 kbit/s",
                DisplayOrder = 2,
            });
            sa18.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "320 kbit/s",
                DisplayOrder = 3,
            });

            #endregion sa18 MP3 quality

            #region sa19 music genre

            var sa19 = new SpecificationAttribute
            {
                Name = "Music genre",
                DisplayOrder = 19,
            };
            sa19.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "blues",
                DisplayOrder = 1,
            });
            sa19.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "jazz",
                DisplayOrder = 2,
            });
            sa19.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "disco",
                DisplayOrder = 3,
            });
            sa19.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "pop",
                DisplayOrder = 4,
            });
            sa19.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "funk",
                DisplayOrder = 5,
            });
            sa19.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "classical",
                DisplayOrder = 6,
            });
            sa19.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "R&B",
                DisplayOrder = 7,
            });

            #endregion sa19 music genre

            #region sa20 manufacturer

            var sa20 = new SpecificationAttribute
            {
                Name = "Manufacturer",
                DisplayOrder = 20,
            };
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Apple",
                DisplayOrder = 1,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Samsung",
                DisplayOrder = 2,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "HTC",
                DisplayOrder = 3,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "LG",
                DisplayOrder = 4,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Motorola",
                DisplayOrder = 5,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Nokia",
                DisplayOrder = 6,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Sony",
                DisplayOrder = 7,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Blackberry",
                DisplayOrder = 8,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Microsoft",
                DisplayOrder = 9,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "HP",
                DisplayOrder = 10,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Canon",
                DisplayOrder = 11,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Casio",
                DisplayOrder = 12,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Panasonic",
                DisplayOrder = 13,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Certina",
                DisplayOrder = 14,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Festina",
                DisplayOrder = 15,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Seiko",
                DisplayOrder = 16,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Tissot",
                DisplayOrder = 17,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Breitling",
                DisplayOrder = 18,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Adidas",
                DisplayOrder = 19,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Nike",
                DisplayOrder = 20,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Puma",
                DisplayOrder = 21,
            });

            #endregion sa20 manufacturer

            #region sa21 Watches for whom

            var sa21 = new SpecificationAttribute
            {
                Name = "For whom",
                DisplayOrder = 21,
            };
            sa21.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "For him",
                DisplayOrder = 1,
            });
            sa21.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "For her",
                DisplayOrder = 2,
            });

            #endregion sa11 Watches for whom

            #region sa22 Offer

            var sa22 = new SpecificationAttribute
            {
                Name = "Offer",
                DisplayOrder = 22,
            };

            sa22.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Clearance",
                DisplayOrder = 1,
            });

            sa22.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Permanent low price",
                DisplayOrder = 2,
            });

            sa22.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Promotion",
                DisplayOrder = 3,
            });

            sa22.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Reduced price",
                DisplayOrder = 4,
            });

            sa22.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Special Buy",
                DisplayOrder = 5,
            });

            sa22.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Offer of the day",
                DisplayOrder = 6,
            });

            sa22.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Weekly offer",
                DisplayOrder = 7,
            });

            sa22.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Best Price",
                DisplayOrder = 8,
            });

            #endregion sa22 Offer

            #region sa23 Size

            var sa23 = new SpecificationAttribute
            {
                Name = "Size",
                DisplayOrder = 23,
            };

            sa23.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "XS",
                DisplayOrder = 1,
            });

            sa23.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "S",
                DisplayOrder = 2,
            });

            sa23.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "M",
                DisplayOrder = 3,
            });

            sa23.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "L",
                DisplayOrder = 4,
            });

            sa23.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "XL",
                DisplayOrder = 5,
            });

            sa23.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "XXL",
                DisplayOrder = 6,
            });


            #endregion sa23 Size

            #region sa24 diameter

            var sa24 = new SpecificationAttribute
            {
                Name = "Diameter",
                DisplayOrder = 24,
            };

            sa24.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "38mm",
                DisplayOrder = 1,
            });

            sa24.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "40mm",
                DisplayOrder = 2,
            });

            sa24.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "44mm",
                DisplayOrder = 3,
            });

            #endregion sa24 diameter

            #region sa25 closure

            var sa25 = new SpecificationAttribute
            {
                Name = "Closure",
                DisplayOrder = 25,
            };

            sa25.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "snap closure",
                DisplayOrder = 1,
            });

            sa25.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "folding clasp",
                DisplayOrder = 2,
            });

            sa25.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "thorn close",
                DisplayOrder = 3,
            });

            #endregion sa25 closure

            #region sa26 facial shape

            var sa26 = new SpecificationAttribute
            {
                Name = "Facial shape",
                DisplayOrder = 26,
            };

            sa26.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "oval",
                DisplayOrder = 1,
            });

            sa26.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "round",
                DisplayOrder = 2,
            });

            sa26.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "heart shaped",
                DisplayOrder = 3,
            });

            sa26.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "angular",
                DisplayOrder = 4,
            });

            #endregion sa26 facial shape

            #region sa27 storage capacity

            var sa27 = new SpecificationAttribute
            {
                Name = "Storage capacity",
                DisplayOrder = 27,
            };

            sa27.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "32 GB",
                DisplayOrder = 1,
            });

            sa27.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "64 GB",
                DisplayOrder = 2,
            });

            sa27.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "128 GB",
                DisplayOrder = 3,
            });

            #endregion sa27 facial shape

            #region sa28 Dial window material type

            var sa28 = new SpecificationAttribute
            {
                Name = "Dial window material type",
                DisplayOrder = 28,
            };

            sa28.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Mineral",
                DisplayOrder = 1,
            });

            sa28.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Sapphire",
                DisplayOrder = 2,
            });

            #endregion sa28 Dial window material type

            var entities = new List<SpecificationAttribute>
            {
                sa1,sa2,sa3,sa4,sa5,sa6,sa7,sa8,sa9,sa10,sa11,sa12,sa13,sa14,sa15,sa16,sa17,sa18,sa19,sa20,sa21,sa22,sa23,sa24,sa25,sa26,sa27,sa28
            };

            this.Alter(entities);
            return entities;
        }
    }
}
