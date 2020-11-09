using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SmartStore.Services.Directory;

namespace SmartStore.Services.Tests.Directory
{
    [TestFixture]
    public class GeoCountryLookupTests
    {
        private IDictionary<string, LookupCountryResponse> _addresses;

        [SetUp]
        public void SetUp()
        {
            try
            {
                _addresses = new Dictionary<string, LookupCountryResponse>
                {
                    ["0:0:0:0:0:ffff:b0eb:6304"] = new LookupCountryResponse { IsoCode = "TR", Name = "Turkey" },
                    ["104.221.132.123"] = new LookupCountryResponse { IsoCode = "US", Name = "United States" },
                    ["14.232.208.88"] = new LookupCountryResponse { IsoCode = "VN", Name = "Vietnam" },
                    ["88.199.164.142"] = new LookupCountryResponse { IsoCode = "PL", Name = "Poland", IsInEu = true },
                    ["186.10.251.74"] = new LookupCountryResponse { IsoCode = "CL", Name = "Chile" },
                    ["131.196.141.56"] = new LookupCountryResponse { IsoCode = "MX", Name = "Mexico" },
                    ["1.179.245.146"] = new LookupCountryResponse { IsoCode = "TH", Name = "Thailand" },
                    ["85.62.10.84"] = new LookupCountryResponse { IsoCode = "ES", Name = "Spain", IsInEu = true },
                    ["89.111.105.72"] = new LookupCountryResponse { IsoCode = "CZ", Name = "Czechia", IsInEu = true },
                    ["46.254.246.123"] = new LookupCountryResponse { IsoCode = "RU", Name = "Russia" },
                    ["177.87.79.90"] = new LookupCountryResponse { IsoCode = "BR", Name = "Brazil" },
                    ["176.235.99.4"] = new LookupCountryResponse { IsoCode = "TR", Name = "Turkey" },
                    ["185.216.213.237"] = new LookupCountryResponse { IsoCode = "DE", Name = "Germany", IsInEu = true },
                    ["41.60.232.2"] = new LookupCountryResponse { IsoCode = "KE", Name = "Kenya" },
                    ["88.157.176.94"] = new LookupCountryResponse { IsoCode = "PT", Name = "Portugal", IsInEu = true },
                    ["193.62.7.52"] = new LookupCountryResponse { IsoCode = "GB", Name = "United Kingdom", IsInEu = false }
                };
            }
            catch (FileNotFoundException)
            {
                Assert.Ignore("This unit test can only be executed from the Test Explorer.");
            }
        }

        [Test]
        public void CanLookup()
        {
            using (var geoCountryLookup = new GeoCountryLookup())
            {
                if (_addresses?.Any() ?? false)
                {
                    foreach (var kvp in _addresses)
                    {
                        var ip = kvp.Key;
                        var expect = kvp.Value;

                        var response = geoCountryLookup.LookupCountry(ip);

                        Assert.AreEqual(expect.IsoCode, response.IsoCode, response.Name);
                        Assert.AreEqual(expect.Name, response.Name, response.Name);
                        Assert.AreEqual(expect.IsInEu, response.IsInEu, response.Name);
                    }
                }
            }
        }
    }
}
