using System;
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
				Name = "سازنده پردازنده",
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
				Name = "رنگ",
				DisplayOrder = 2,
			};
			sa2.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "سفید",
				DisplayOrder = 1,
			});
			sa2.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "مشکی",
				DisplayOrder = 2,
			});
			sa2.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "بژ",
				DisplayOrder = 3,
			});
			sa2.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "قرمز",
				DisplayOrder = 4,
			});
			sa2.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "آبی",
				DisplayOrder = 5,
			});
			sa2.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "سبز",
				DisplayOrder = 6,
			});
			sa2.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "زرد",
				DisplayOrder = 7,
			});

			#endregion sa2 color

			#region sa3 harddisk capacity

			var sa3 = new SpecificationAttribute
			{
				Name = "ظرفیت هارد دیسک",
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
				Name = "رم",
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
				Name = "سیستم عامل",
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
				Name = "پورت‌ها",
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
				Name = "جنسیت",
				DisplayOrder = 7,
			};
			sa7.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "آقا",
				DisplayOrder = 1,
			});
			sa7.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "خانم",
				DisplayOrder = 2,
			});
			sa7.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "دگر",
				DisplayOrder = 3,
			});

			#endregion sa7 Gender

			#region sa8 material

			var sa8 = new SpecificationAttribute
			{
				Name = "جنس",
				DisplayOrder = 8,
			};
			sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "استیل ضدزنگ",
				DisplayOrder = 1,
			});
			sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "تیتانیوم",
				DisplayOrder = 2,
			});
			sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "پلاستیک",
				DisplayOrder = 3,
			});
			sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "آلومینیوم",
				DisplayOrder = 4,
			});

			sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "چرم",
				DisplayOrder = 5,
			});

			sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "نایلون",
				DisplayOrder = 6,
			});

			sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "سیلیکون",
				DisplayOrder = 7,
			});

			sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "سرامیک",
				DisplayOrder = 8,
			});

			sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
			{
				Name = "پنبه",
				DisplayOrder = 9,
			});
			sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
			{
				Name = "100% پنبه آلی",
				DisplayOrder = 10,
			});
			sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
			{
				Name = "پلی آمید",
				DisplayOrder = 11,
			});
			sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
			{
				Name = "لاستیک",
				DisplayOrder = 12,
			});
			sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
			{
				Name = "چوب",
				DisplayOrder = 13,
			});
			sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
			{
				Name = "شیشه",
				DisplayOrder = 14,
			});
			sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
			{
				Name = "کشی",
				DisplayOrder = 15,
			});
			sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
			{
				Name = "پلیستر",
				DisplayOrder = 16,
			});

			#endregion sa8 material

			#region sa9 movement

			var sa9 = new SpecificationAttribute
			{
				Name = "نحوه حرکت",
				DisplayOrder = 9,
			};
			sa9.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "مکانیکی، خودکار",
				DisplayOrder = 1,
			});
			sa9.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "مکانیکی",
				DisplayOrder = 2,
			});
			sa9.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "کوارتز",
				DisplayOrder = 3,
			});

			#endregion sa9 movement

			#region sa10 clasp

			var sa10 = new SpecificationAttribute
			{
				Name = "قلاب",
				DisplayOrder = 10,
			};
			sa10.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "قفل کشویی",
				DisplayOrder = 1,
			});
			sa10.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "قلاب تاشو",
				DisplayOrder = 2,
			});
			sa10.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "میخی",
				DisplayOrder = 3,
			});

			#endregion sa10 clasp

			#region sa11 window material

			var sa11 = new SpecificationAttribute
			{
				Name = "جنس شیشه",
				DisplayOrder = 11,
			};
			sa11.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "سنگ معدنی",
				DisplayOrder = 1,
			});
			sa11.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "یاقوت",
				DisplayOrder = 2,
			});

			#endregion sa11 window material

			#region sa12 language

			var sa12 = new SpecificationAttribute
			{
				Name = "زبان",
				DisplayOrder = 12,
			};
			sa12.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "آلمانی",
				DisplayOrder = 1,
			});
			sa12.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "انگلیسی",
				DisplayOrder = 2,
			});
			sa12.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "فرانسوی",
				DisplayOrder = 3,
			});
			sa12.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "ایتالیایی",
				DisplayOrder = 4,
			});

			#endregion sa12 language

			#region sa13 edition

			var sa13 = new SpecificationAttribute
			{
				Name = "نسخه",
				DisplayOrder = 13,
			};
			sa13.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "کوچک",
				DisplayOrder = 1,
			});
			sa13.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "شومیز",
				DisplayOrder = 2,
			});

			#endregion sa13 edition

			#region sa14 category

			var sa14 = new SpecificationAttribute
			{
				Name = "دسته بندی",
				DisplayOrder = 14,
			};
			sa14.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "ماجراجویی",
				DisplayOrder = 1,
			});
			sa14.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "تخیلی",
				DisplayOrder = 2,
			});
			sa14.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "تاریخی",
				DisplayOrder = 3,
			});
			sa14.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "اینترنت و رایانه",
				DisplayOrder = 4,
			});
			sa14.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "دلهره‌آور",
				DisplayOrder = 5,
			});
			sa14.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "خودرو",
				DisplayOrder = 6,
			});
			sa14.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "رمان",
				DisplayOrder = 7,
			});
			sa14.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "آشپزی",
				DisplayOrder = 8,
			});
			sa14.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "غیرداستانی",
				DisplayOrder = 9,
			});

			#endregion sa14 category

			#region sa15 Computer-type

			var sa15 = new SpecificationAttribute
			{
				Name = "نوع کامپیوتر",
				DisplayOrder = 15,
			};
			sa15.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "رومیزی",
				DisplayOrder = 1,
			});
			sa15.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "all-in-one",
				DisplayOrder = 2,
			});
			sa15.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "لپتاپ",
				DisplayOrder = 3,
			});
			sa15.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "تبلت",
				DisplayOrder = 4,
			});

			#endregion sa15 Computer-type

			#region sa16 type of mass-storage

			var sa16 = new SpecificationAttribute
			{
				Name = "نوع حافظه ذخیره‌سازی",
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
				Name = "سایز هارددیسک",
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
				Name = "کیفیت صوت",
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
				Name = "نوع موسیقی",
				DisplayOrder = 19,
			};
			sa19.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "رپ",
				DisplayOrder = 1,
			});
			sa19.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "جاز",
				DisplayOrder = 2,
			});
			sa19.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "دیسکو",
				DisplayOrder = 3,
			});
			sa19.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "پاپ",
				DisplayOrder = 4,
			});
			sa19.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "فانکی",
				DisplayOrder = 5,
			});
			sa19.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "کلاسیک",
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
				Name = "سازنده",
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
				Name = "برای چه کسی",
				DisplayOrder = 21,
			};
			sa21.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "آقا",
				DisplayOrder = 1,
			});
			sa21.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "خانم",
				DisplayOrder = 2,
			});

			#endregion sa11 Watches for whom

			#region sa22 Offer

			var sa22 = new SpecificationAttribute
			{
				Name = "پیشنهاد",
				DisplayOrder = 22,
			};

			sa22.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "ترخیص",
				DisplayOrder = 1,
			});

			sa22.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "حداقل قیمت همیشگی",
				DisplayOrder = 2,
			});

			sa22.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "ترفیع",
				DisplayOrder = 3,
			});

			sa22.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "تخفیف خورده",
				DisplayOrder = 4,
			});

			sa22.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "خرید ویژه",
				DisplayOrder = 5,
			});

			sa22.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "پیشنهاد روز",
				DisplayOrder = 6,
			});

			sa22.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "پیشنهاد هفتگی",
				DisplayOrder = 7,
			});

			sa22.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "بهترین قیمت",
				DisplayOrder = 8,
			});

			#endregion sa22 Offer

			#region sa23 Size

			var sa23 = new SpecificationAttribute
			{
				Name = "اندازه",
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
				Name = "طول",
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
				Name = "نحوه بسته شدن",
				DisplayOrder = 25,
			};

			sa25.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "سریع",
				DisplayOrder = 1,
			});

			sa25.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "قلاب تاشو",
				DisplayOrder = 2,
			});

			sa25.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "میخی",
				DisplayOrder = 3,
			});

			#endregion sa25 closure

			#region sa26 facial shape

			var sa26 = new SpecificationAttribute
			{
				Name = "شکل صورت",
				DisplayOrder = 26,
			};

			sa26.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "بیضی",
				DisplayOrder = 1,
			});

			sa26.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "گرد",
				DisplayOrder = 2,
			});

			sa26.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "شکل قلب",
				DisplayOrder = 3,
			});

			sa26.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "زاویه دار",
				DisplayOrder = 4,
			});

			#endregion sa26 facial shape

			#region sa27 storage capacity

			var sa27 = new SpecificationAttribute
			{
				Name = "ظرفیت حاقظه",
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
				Name = "معدنی",
				DisplayOrder = 1,
			});

			sa28.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "یاقوت",
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
