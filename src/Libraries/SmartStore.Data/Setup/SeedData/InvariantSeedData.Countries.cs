using System.Collections.Generic;
using SmartStore.Core.Domain.Directory;

namespace SmartStore.Data.Setup
{
    public abstract partial class InvariantSeedData
    {
        public IList<Country> Countries()
        {
            var cUsa = new Country
            {
                Name = "United States",
                AllowsBilling = true,
                AllowsShipping = true,
                TwoLetterIsoCode = "US",
                ThreeLetterIsoCode = "USA",
                NumericIsoCode = 840,
                SubjectToVat = false,
                DisplayOrder = 1,
                Published = true,
            };

            #region US Regions

            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "AA (Armed Forces Americas)",
                Abbreviation = "AA",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "AE (Armed Forces Europe)",
                Abbreviation = "AE",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Alabama",
                Abbreviation = "AL",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Alaska",
                Abbreviation = "AK",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "American Samoa",
                Abbreviation = "AS",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "AP (Armed Forces Pacific)",
                Abbreviation = "AP",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Arizona",
                Abbreviation = "AZ",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Arkansas",
                Abbreviation = "AR",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "California",
                Abbreviation = "CA",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Colorado",
                Abbreviation = "CO",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Connecticut",
                Abbreviation = "CT",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Delaware",
                Abbreviation = "DE",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "District of Columbia",
                Abbreviation = "DC",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Federated States of Micronesia",
                Abbreviation = "FM",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Florida",
                Abbreviation = "FL",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Georgia",
                Abbreviation = "GA",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Guam",
                Abbreviation = "GU",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Hawaii",
                Abbreviation = "HI",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Idaho",
                Abbreviation = "ID",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Illinois",
                Abbreviation = "IL",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Indiana",
                Abbreviation = "IN",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Iowa",
                Abbreviation = "IA",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Kansas",
                Abbreviation = "KS",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Kentucky",
                Abbreviation = "KY",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Louisiana",
                Abbreviation = "LA",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Maine",
                Abbreviation = "ME",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Marshall Islands",
                Abbreviation = "MH",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Maryland",
                Abbreviation = "MD",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Massachusetts",
                Abbreviation = "MA",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Michigan",
                Abbreviation = "MI",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Minnesota",
                Abbreviation = "MN",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Mississippi",
                Abbreviation = "MS",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Missouri",
                Abbreviation = "MO",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Montana",
                Abbreviation = "MT",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Nebraska",
                Abbreviation = "NE",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Nevada",
                Abbreviation = "NV",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "New Hampshire",
                Abbreviation = "NH",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "New Jersey",
                Abbreviation = "NJ",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "New Mexico",
                Abbreviation = "NM",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "New York",
                Abbreviation = "NY",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "North Carolina",
                Abbreviation = "NC",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "North Dakota",
                Abbreviation = "ND",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Northern Mariana Islands",
                Abbreviation = "MP",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Ohio",
                Abbreviation = "OH",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Oklahoma",
                Abbreviation = "OK",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Oregon",
                Abbreviation = "OR",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Palau",
                Abbreviation = "PW",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Pennsylvania",
                Abbreviation = "PA",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Puerto Rico",
                Abbreviation = "PR",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Rhode Island",
                Abbreviation = "RI",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "South Carolina",
                Abbreviation = "SC",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "South Dakota",
                Abbreviation = "SD",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Tennessee",
                Abbreviation = "TN",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Texas",
                Abbreviation = "TX",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Utah",
                Abbreviation = "UT",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Vermont",
                Abbreviation = "VT",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Virgin Islands",
                Abbreviation = "VI",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Virginia",
                Abbreviation = "VA",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Washington",
                Abbreviation = "WA",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "West Virginia",
                Abbreviation = "WV",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Wisconsin",
                Abbreviation = "WI",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Wyoming",
                Abbreviation = "WY",
                Published = true,
                DisplayOrder = 1,
            });

            #endregion

            var cCanada = new Country
            {
                Name = "Canada",
                AllowsBilling = true,
                AllowsShipping = true,
                TwoLetterIsoCode = "CA",
                ThreeLetterIsoCode = "CAN",
                NumericIsoCode = 124,
                SubjectToVat = false,
                DisplayOrder = 2,
                Published = true,
            };

            #region CA Regions

            cCanada.StateProvinces.Add(new StateProvince()
            {
                Name = "Alberta",
                Abbreviation = "AB",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince()
            {
                Name = "British Columbia",
                Abbreviation = "BC",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince()
            {
                Name = "Manitoba",
                Abbreviation = "MB",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince()
            {
                Name = "New Brunswick",
                Abbreviation = "NB",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince()
            {
                Name = "Newfoundland and Labrador",
                Abbreviation = "NL",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince()
            {
                Name = "Northwest Territories",
                Abbreviation = "NT",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince()
            {
                Name = "Nova Scotia",
                Abbreviation = "NS",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince()
            {
                Name = "Nunavut",
                Abbreviation = "NU",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince()
            {
                Name = "Ontario",
                Abbreviation = "ON",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince()
            {
                Name = "Prince Edward Island",
                Abbreviation = "PE",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince()
            {
                Name = "Quebec",
                Abbreviation = "QC",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince()
            {
                Name = "Saskatchewan",
                Abbreviation = "SK",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince()
            {
                Name = "Yukon Territory",
                Abbreviation = "YU",
                Published = true,
                DisplayOrder = 1,
            });
            #endregion

            var entities = new List<Country>()
            {
                new Country()
                {
                    Name = "Germany",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "DE",
                    ThreeLetterIsoCode = "DEU",
                    NumericIsoCode = 276,
                    SubjectToVat = true,
                    DisplayOrder = -10,
                    Published = true
                },

                new Country
                {
                    Name = "Austria",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AT",
                    ThreeLetterIsoCode = "AUT",
                    NumericIsoCode = 40,
                    SubjectToVat = true,
                    DisplayOrder = -5,
                    Published = true
                },
                new Country
                {
                    Name = "Switzerland",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CH",
                    ThreeLetterIsoCode = "CHE",
                    NumericIsoCode = 756,
                    SubjectToVat = false,
                    DisplayOrder = -1,
                    Published = true
                },
                cUsa,
                cCanada,

				//other countries
				new Country
                {
                    Name = "Argentina",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AR",
                    ThreeLetterIsoCode = "ARG",
                    NumericIsoCode = 32,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Armenia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AM",
                    ThreeLetterIsoCode = "ARM",
                    NumericIsoCode = 51,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Aruba",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AW",
                    ThreeLetterIsoCode = "ABW",
                    NumericIsoCode = 533,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Australia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AU",
                    ThreeLetterIsoCode = "AUS",
                    NumericIsoCode = 36,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Azerbaijan",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AZ",
                    ThreeLetterIsoCode = "AZE",
                    NumericIsoCode = 31,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Bahamas",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BS",
                    ThreeLetterIsoCode = "BHS",
                    NumericIsoCode = 44,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Bangladesh",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BD",
                    ThreeLetterIsoCode = "BGD",
                    NumericIsoCode = 50,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Belarus",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BY",
                    ThreeLetterIsoCode = "BLR",
                    NumericIsoCode = 112,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Belgium",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BE",
                    ThreeLetterIsoCode = "BEL",
                    NumericIsoCode = 56,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Belize",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BZ",
                    ThreeLetterIsoCode = "BLZ",
                    NumericIsoCode = 84,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Bermuda",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BM",
                    ThreeLetterIsoCode = "BMU",
                    NumericIsoCode = 60,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Bolivia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BO",
                    ThreeLetterIsoCode = "BOL",
                    NumericIsoCode = 68,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Bosnia and Herzegowina",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BA",
                    ThreeLetterIsoCode = "BIH",
                    NumericIsoCode = 70,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Brazil",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BR",
                    ThreeLetterIsoCode = "BRA",
                    NumericIsoCode = 76,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Bulgaria",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BG",
                    ThreeLetterIsoCode = "BGR",
                    NumericIsoCode = 100,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Cayman Islands",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "KY",
                    ThreeLetterIsoCode = "CYM",
                    NumericIsoCode = 136,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Chile",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CL",
                    ThreeLetterIsoCode = "CHL",
                    NumericIsoCode = 152,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "China",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CN",
                    ThreeLetterIsoCode = "CHN",
                    NumericIsoCode = 156,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Colombia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CO",
                    ThreeLetterIsoCode = "COL",
                    NumericIsoCode = 170,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Costa Rica",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CR",
                    ThreeLetterIsoCode = "CRI",
                    NumericIsoCode = 188,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Croatia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "HR",
                    ThreeLetterIsoCode = "HRV",
                    NumericIsoCode = 191,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Cuba",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CU",
                    ThreeLetterIsoCode = "CUB",
                    NumericIsoCode = 192,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Cyprus",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CY",
                    ThreeLetterIsoCode = "CYP",
                    NumericIsoCode = 196,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Czech Republic",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CZ",
                    ThreeLetterIsoCode = "CZE",
                    NumericIsoCode = 203,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Denmark",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "DK",
                    ThreeLetterIsoCode = "DNK",
                    NumericIsoCode = 208,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Dominican Republic",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "DO",
                    ThreeLetterIsoCode = "DOM",
                    NumericIsoCode = 214,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Ecuador",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "EC",
                    ThreeLetterIsoCode = "ECU",
                    NumericIsoCode = 218,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Egypt",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "EG",
                    ThreeLetterIsoCode = "EGY",
                    NumericIsoCode = 818,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Finland",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "FI",
                    ThreeLetterIsoCode = "FIN",
                    NumericIsoCode = 246,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "France",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "FR",
                    ThreeLetterIsoCode = "FRA",
                    NumericIsoCode = 250,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Georgia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GE",
                    ThreeLetterIsoCode = "GEO",
                    NumericIsoCode = 268,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Gibraltar",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GI",
                    ThreeLetterIsoCode = "GIB",
                    NumericIsoCode = 292,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Greece",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GR",
                    ThreeLetterIsoCode = "GRC",
                    NumericIsoCode = 300,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Guatemala",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GT",
                    ThreeLetterIsoCode = "GTM",
                    NumericIsoCode = 320,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Hong Kong",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "HK",
                    ThreeLetterIsoCode = "HKG",
                    NumericIsoCode = 344,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Hungary",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "HU",
                    ThreeLetterIsoCode = "HUN",
                    NumericIsoCode = 348,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "India",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "IN",
                    ThreeLetterIsoCode = "IND",
                    NumericIsoCode = 356,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Indonesia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "ID",
                    ThreeLetterIsoCode = "IDN",
                    NumericIsoCode = 360,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Ireland",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "IE",
                    ThreeLetterIsoCode = "IRL",
                    NumericIsoCode = 372,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Israel",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "IL",
                    ThreeLetterIsoCode = "ISR",
                    NumericIsoCode = 376,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Italy",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "IT",
                    ThreeLetterIsoCode = "ITA",
                    NumericIsoCode = 380,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Jamaica",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "JM",
                    ThreeLetterIsoCode = "JAM",
                    NumericIsoCode = 388,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Japan",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "JP",
                    ThreeLetterIsoCode = "JPN",
                    NumericIsoCode = 392,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Jordan",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "JO",
                    ThreeLetterIsoCode = "JOR",
                    NumericIsoCode = 400,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Kazakhstan",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "KZ",
                    ThreeLetterIsoCode = "KAZ",
                    NumericIsoCode = 398,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Korea, Democratic People's Republic of",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "KP",
                    ThreeLetterIsoCode = "PRK",
                    NumericIsoCode = 408,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Kuwait",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "KW",
                    ThreeLetterIsoCode = "KWT",
                    NumericIsoCode = 414,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Malaysia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MY",
                    ThreeLetterIsoCode = "MYS",
                    NumericIsoCode = 458,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Mexico",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MX",
                    ThreeLetterIsoCode = "MEX",
                    NumericIsoCode = 484,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Netherlands",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "NL",
                    ThreeLetterIsoCode = "NLD",
                    NumericIsoCode = 528,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "New Zealand",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "NZ",
                    ThreeLetterIsoCode = "NZL",
                    NumericIsoCode = 554,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Norway",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "NO",
                    ThreeLetterIsoCode = "NOR",
                    NumericIsoCode = 578,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Pakistan",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "PK",
                    ThreeLetterIsoCode = "PAK",
                    NumericIsoCode = 586,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Paraguay",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "PY",
                    ThreeLetterIsoCode = "PRY",
                    NumericIsoCode = 600,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Peru",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "PE",
                    ThreeLetterIsoCode = "PER",
                    NumericIsoCode = 604,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Philippines",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "PH",
                    ThreeLetterIsoCode = "PHL",
                    NumericIsoCode = 608,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Poland",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "PL",
                    ThreeLetterIsoCode = "POL",
                    NumericIsoCode = 616,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Portugal",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "PT",
                    ThreeLetterIsoCode = "PRT",
                    NumericIsoCode = 620,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Puerto Rico",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "PR",
                    ThreeLetterIsoCode = "PRI",
                    NumericIsoCode = 630,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Qatar",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "QA",
                    ThreeLetterIsoCode = "QAT",
                    NumericIsoCode = 634,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Romania",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "RO",
                    ThreeLetterIsoCode = "ROM",
                    NumericIsoCode = 642,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Russia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "RU",
                    ThreeLetterIsoCode = "RUS",
                    NumericIsoCode = 643,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Saudi Arabia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SA",
                    ThreeLetterIsoCode = "SAU",
                    NumericIsoCode = 682,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Singapore",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SG",
                    ThreeLetterIsoCode = "SGP",
                    NumericIsoCode = 702,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Slovakia (Slovak Republic)",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SK",
                    ThreeLetterIsoCode = "SVK",
                    NumericIsoCode = 703,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Slovenia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SI",
                    ThreeLetterIsoCode = "SVN",
                    NumericIsoCode = 705,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "South Africa",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "ZA",
                    ThreeLetterIsoCode = "ZAF",
                    NumericIsoCode = 710,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Spain",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "ES",
                    ThreeLetterIsoCode = "ESP",
                    NumericIsoCode = 724,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Sweden",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SE",
                    ThreeLetterIsoCode = "SWE",
                    NumericIsoCode = 752,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Taiwan",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TW",
                    ThreeLetterIsoCode = "TWN",
                    NumericIsoCode = 158,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Thailand",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TH",
                    ThreeLetterIsoCode = "THA",
                    NumericIsoCode = 764,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Turkey",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TR",
                    ThreeLetterIsoCode = "TUR",
                    NumericIsoCode = 792,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Ukraine",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "UA",
                    ThreeLetterIsoCode = "UKR",
                    NumericIsoCode = 804,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "United Arab Emirates",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AE",
                    ThreeLetterIsoCode = "ARE",
                    NumericIsoCode = 784,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "United Kingdom",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GB",
                    ThreeLetterIsoCode = "GBR",
                    NumericIsoCode = 826,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "United States minor outlying islands",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "UM",
                    ThreeLetterIsoCode = "UMI",
                    NumericIsoCode = 581,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Uruguay",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "UY",
                    ThreeLetterIsoCode = "URY",
                    NumericIsoCode = 858,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Uzbekistan",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "UZ",
                    ThreeLetterIsoCode = "UZB",
                    NumericIsoCode = 860,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Venezuela",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "VE",
                    ThreeLetterIsoCode = "VEN",
                    NumericIsoCode = 862,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Serbia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "RS",
                    ThreeLetterIsoCode = "SRB",
                    NumericIsoCode = 688,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Afghanistan",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AF",
                    ThreeLetterIsoCode = "AFG",
                    NumericIsoCode = 4,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Albania",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AL",
                    ThreeLetterIsoCode = "ALB",
                    NumericIsoCode = 8,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Algeria",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "DZ",
                    ThreeLetterIsoCode = "DZA",
                    NumericIsoCode = 12,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "American Samoa",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AS",
                    ThreeLetterIsoCode = "ASM",
                    NumericIsoCode = 16,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Andorra",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AD",
                    ThreeLetterIsoCode = "AND",
                    NumericIsoCode = 20,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Angola",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AO",
                    ThreeLetterIsoCode = "AGO",
                    NumericIsoCode = 24,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Anguilla",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AI",
                    ThreeLetterIsoCode = "AIA",
                    NumericIsoCode = 660,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Antarctica",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AQ",
                    ThreeLetterIsoCode = "ATA",
                    NumericIsoCode = 10,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Antigua and Barbuda",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AG",
                    ThreeLetterIsoCode = "ATG",
                    NumericIsoCode = 28,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Bahrain",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BH",
                    ThreeLetterIsoCode = "BHR",
                    NumericIsoCode = 48,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Barbados",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BB",
                    ThreeLetterIsoCode = "BRB",
                    NumericIsoCode = 52,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Benin",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BJ",
                    ThreeLetterIsoCode = "BEN",
                    NumericIsoCode = 204,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Bhutan",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BT",
                    ThreeLetterIsoCode = "BTN",
                    NumericIsoCode = 64,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Botswana",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BW",
                    ThreeLetterIsoCode = "BWA",
                    NumericIsoCode = 72,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Bouvet Island",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BV",
                    ThreeLetterIsoCode = "BVT",
                    NumericIsoCode = 74,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "British Indian Ocean Territory",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "IO",
                    ThreeLetterIsoCode = "IOT",
                    NumericIsoCode = 86,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Brunei Darussalam",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BN",
                    ThreeLetterIsoCode = "BRN",
                    NumericIsoCode = 96,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Burkina Faso",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BF",
                    ThreeLetterIsoCode = "BFA",
                    NumericIsoCode = 854,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Burundi",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "BI",
                    ThreeLetterIsoCode = "BDI",
                    NumericIsoCode = 108,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Cambodia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "KH",
                    ThreeLetterIsoCode = "KHM",
                    NumericIsoCode = 116,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Cameroon",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CM",
                    ThreeLetterIsoCode = "CMR",
                    NumericIsoCode = 120,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Cape Verde",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CV",
                    ThreeLetterIsoCode = "CPV",
                    NumericIsoCode = 132,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Central African Republic",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CF",
                    ThreeLetterIsoCode = "CAF",
                    NumericIsoCode = 140,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Chad",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TD",
                    ThreeLetterIsoCode = "TCD",
                    NumericIsoCode = 148,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Christmas Island",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CX",
                    ThreeLetterIsoCode = "CXR",
                    NumericIsoCode = 162,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Cocos (Keeling) Islands",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CC",
                    ThreeLetterIsoCode = "CCK",
                    NumericIsoCode = 166,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Comoros",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "KM",
                    ThreeLetterIsoCode = "COM",
                    NumericIsoCode = 174,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Congo",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CG",
                    ThreeLetterIsoCode = "COG",
                    NumericIsoCode = 178,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Cook Islands",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CK",
                    ThreeLetterIsoCode = "COK",
                    NumericIsoCode = 184,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Cote D'Ivoire",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "CI",
                    ThreeLetterIsoCode = "CIV",
                    NumericIsoCode = 384,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Djibouti",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "DJ",
                    ThreeLetterIsoCode = "DJI",
                    NumericIsoCode = 262,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Dominica",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "DM",
                    ThreeLetterIsoCode = "DMA",
                    NumericIsoCode = 212,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "El Salvador",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SV",
                    ThreeLetterIsoCode = "SLV",
                    NumericIsoCode = 222,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Equatorial Guinea",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GQ",
                    ThreeLetterIsoCode = "GNQ",
                    NumericIsoCode = 226,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Eritrea",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "ER",
                    ThreeLetterIsoCode = "ERI",
                    NumericIsoCode = 232,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Estonia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "EE",
                    ThreeLetterIsoCode = "EST",
                    NumericIsoCode = 233,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Ethiopia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "ET",
                    ThreeLetterIsoCode = "ETH",
                    NumericIsoCode = 231,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Falkland Islands (Malvinas)",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "FK",
                    ThreeLetterIsoCode = "FLK",
                    NumericIsoCode = 238,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Faroe Islands",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "FO",
                    ThreeLetterIsoCode = "FRO",
                    NumericIsoCode = 234,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Fiji",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "FJ",
                    ThreeLetterIsoCode = "FJI",
                    NumericIsoCode = 242,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "French Guiana",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GF",
                    ThreeLetterIsoCode = "GUF",
                    NumericIsoCode = 254,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "French Polynesia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "PF",
                    ThreeLetterIsoCode = "PYF",
                    NumericIsoCode = 258,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "French Southern Territories",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TF",
                    ThreeLetterIsoCode = "ATF",
                    NumericIsoCode = 260,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Gabon",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GA",
                    ThreeLetterIsoCode = "GAB",
                    NumericIsoCode = 266,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Gambia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GM",
                    ThreeLetterIsoCode = "GMB",
                    NumericIsoCode = 270,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Ghana",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GH",
                    ThreeLetterIsoCode = "GHA",
                    NumericIsoCode = 288,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Greenland",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GL",
                    ThreeLetterIsoCode = "GRL",
                    NumericIsoCode = 304,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Grenada",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GD",
                    ThreeLetterIsoCode = "GRD",
                    NumericIsoCode = 308,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Guadeloupe",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GP",
                    ThreeLetterIsoCode = "GLP",
                    NumericIsoCode = 312,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Guam",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GU",
                    ThreeLetterIsoCode = "GUM",
                    NumericIsoCode = 316,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Guinea",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GN",
                    ThreeLetterIsoCode = "GIN",
                    NumericIsoCode = 324,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Guinea-bissau",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GW",
                    ThreeLetterIsoCode = "GNB",
                    NumericIsoCode = 624,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Guyana",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GY",
                    ThreeLetterIsoCode = "GUY",
                    NumericIsoCode = 328,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Haiti",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "HT",
                    ThreeLetterIsoCode = "HTI",
                    NumericIsoCode = 332,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Heard and Mc Donald Islands",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "HM",
                    ThreeLetterIsoCode = "HMD",
                    NumericIsoCode = 334,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Honduras",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "HN",
                    ThreeLetterIsoCode = "HND",
                    NumericIsoCode = 340,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Iceland",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "IS",
                    ThreeLetterIsoCode = "ISL",
                    NumericIsoCode = 352,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Iran (Islamic Republic of)",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "IR",
                    ThreeLetterIsoCode = "IRN",
                    NumericIsoCode = 364,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Iraq",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "IQ",
                    ThreeLetterIsoCode = "IRQ",
                    NumericIsoCode = 368,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Kenya",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "KE",
                    ThreeLetterIsoCode = "KEN",
                    NumericIsoCode = 404,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Kiribati",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "KI",
                    ThreeLetterIsoCode = "KIR",
                    NumericIsoCode = 296,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Korea",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "KR",
                    ThreeLetterIsoCode = "KOR",
                    NumericIsoCode = 410,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Kyrgyzstan",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "KG",
                    ThreeLetterIsoCode = "KGZ",
                    NumericIsoCode = 417,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Lao People's Democratic Republic",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "LA",
                    ThreeLetterIsoCode = "LAO",
                    NumericIsoCode = 418,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Latvia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "LV",
                    ThreeLetterIsoCode = "LVA",
                    NumericIsoCode = 428,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Lebanon",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "LB",
                    ThreeLetterIsoCode = "LBN",
                    NumericIsoCode = 422,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Lesotho",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "LS",
                    ThreeLetterIsoCode = "LSO",
                    NumericIsoCode = 426,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Liberia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "LR",
                    ThreeLetterIsoCode = "LBR",
                    NumericIsoCode = 430,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Libyan Arab Jamahiriya",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "LY",
                    ThreeLetterIsoCode = "LBY",
                    NumericIsoCode = 434,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Liechtenstein",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "LI",
                    ThreeLetterIsoCode = "LIE",
                    NumericIsoCode = 438,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Lithuania",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "LT",
                    ThreeLetterIsoCode = "LTU",
                    NumericIsoCode = 440,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Luxembourg",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "LU",
                    ThreeLetterIsoCode = "LUX",
                    NumericIsoCode = 442,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Macau",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MO",
                    ThreeLetterIsoCode = "MAC",
                    NumericIsoCode = 446,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Macedonia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MK",
                    ThreeLetterIsoCode = "MKD",
                    NumericIsoCode = 807,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Madagascar",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MG",
                    ThreeLetterIsoCode = "MDG",
                    NumericIsoCode = 450,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Malawi",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MW",
                    ThreeLetterIsoCode = "MWI",
                    NumericIsoCode = 454,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Maldives",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MV",
                    ThreeLetterIsoCode = "MDV",
                    NumericIsoCode = 462,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Mali",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "ML",
                    ThreeLetterIsoCode = "MLI",
                    NumericIsoCode = 466,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Malta",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MT",
                    ThreeLetterIsoCode = "MLT",
                    NumericIsoCode = 470,
                    SubjectToVat = true,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Marshall Islands",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MH",
                    ThreeLetterIsoCode = "MHL",
                    NumericIsoCode = 584,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Martinique",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MQ",
                    ThreeLetterIsoCode = "MTQ",
                    NumericIsoCode = 474,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Mauritania",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MR",
                    ThreeLetterIsoCode = "MRT",
                    NumericIsoCode = 478,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Mauritius",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MU",
                    ThreeLetterIsoCode = "MUS",
                    NumericIsoCode = 480,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Mayotte",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "YT",
                    ThreeLetterIsoCode = "MYT",
                    NumericIsoCode = 175,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Micronesia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "FM",
                    ThreeLetterIsoCode = "FSM",
                    NumericIsoCode = 583,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Moldova",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MD",
                    ThreeLetterIsoCode = "MDA",
                    NumericIsoCode = 498,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Monaco",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MC",
                    ThreeLetterIsoCode = "MCO",
                    NumericIsoCode = 492,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Mongolia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MN",
                    ThreeLetterIsoCode = "MNG",
                    NumericIsoCode = 496,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Montenegro",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "ME",
                    ThreeLetterIsoCode = "MNE",
                    NumericIsoCode = 499,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Montserrat",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MS",
                    ThreeLetterIsoCode = "MSR",
                    NumericIsoCode = 500,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Morocco",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MA",
                    ThreeLetterIsoCode = "MAR",
                    NumericIsoCode = 504,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Mozambique",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MZ",
                    ThreeLetterIsoCode = "MOZ",
                    NumericIsoCode = 508,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Myanmar",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MM",
                    ThreeLetterIsoCode = "MMR",
                    NumericIsoCode = 104,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Namibia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "NA",
                    ThreeLetterIsoCode = "NAM",
                    NumericIsoCode = 516,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Nauru",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "NR",
                    ThreeLetterIsoCode = "NRU",
                    NumericIsoCode = 520,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Nepal",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "NP",
                    ThreeLetterIsoCode = "NPL",
                    NumericIsoCode = 524,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Netherlands Antilles",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "AN",
                    ThreeLetterIsoCode = "ANT",
                    NumericIsoCode = 530,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "New Caledonia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "NC",
                    ThreeLetterIsoCode = "NCL",
                    NumericIsoCode = 540,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Nicaragua",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "NI",
                    ThreeLetterIsoCode = "NIC",
                    NumericIsoCode = 558,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Niger",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "NE",
                    ThreeLetterIsoCode = "NER",
                    NumericIsoCode = 562,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Nigeria",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "NG",
                    ThreeLetterIsoCode = "NGA",
                    NumericIsoCode = 566,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Niue",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "NU",
                    ThreeLetterIsoCode = "NIU",
                    NumericIsoCode = 570,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Norfolk Island",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "NF",
                    ThreeLetterIsoCode = "NFK",
                    NumericIsoCode = 574,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Northern Mariana Islands",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "MP",
                    ThreeLetterIsoCode = "MNP",
                    NumericIsoCode = 580,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Oman",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "OM",
                    ThreeLetterIsoCode = "OMN",
                    NumericIsoCode = 512,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Palau",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "PW",
                    ThreeLetterIsoCode = "PLW",
                    NumericIsoCode = 585,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Panama",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "PA",
                    ThreeLetterIsoCode = "PAN",
                    NumericIsoCode = 591,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Papua New Guinea",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "PG",
                    ThreeLetterIsoCode = "PNG",
                    NumericIsoCode = 598,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Pitcairn",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "PN",
                    ThreeLetterIsoCode = "PCN",
                    NumericIsoCode = 612,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Reunion",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "RE",
                    ThreeLetterIsoCode = "REU",
                    NumericIsoCode = 638,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Rwanda",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "RW",
                    ThreeLetterIsoCode = "RWA",
                    NumericIsoCode = 646,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Saint Kitts and Nevis",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "KN",
                    ThreeLetterIsoCode = "KNA",
                    NumericIsoCode = 659,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Saint Lucia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "LC",
                    ThreeLetterIsoCode = "LCA",
                    NumericIsoCode = 662,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Saint Vincent and the Grenadines",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "VC",
                    ThreeLetterIsoCode = "VCT",
                    NumericIsoCode = 670,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Samoa",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "WS",
                    ThreeLetterIsoCode = "WSM",
                    NumericIsoCode = 882,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "San Marino",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SM",
                    ThreeLetterIsoCode = "SMR",
                    NumericIsoCode = 674,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Sao Tome and Principe",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "ST",
                    ThreeLetterIsoCode = "STP",
                    NumericIsoCode = 678,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Senegal",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SN",
                    ThreeLetterIsoCode = "SEN",
                    NumericIsoCode = 686,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Seychelles",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SC",
                    ThreeLetterIsoCode = "SYC",
                    NumericIsoCode = 690,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Sierra Leone",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SL",
                    ThreeLetterIsoCode = "SLE",
                    NumericIsoCode = 694,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Solomon Islands",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SB",
                    ThreeLetterIsoCode = "SLB",
                    NumericIsoCode = 90,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Somalia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SO",
                    ThreeLetterIsoCode = "SOM",
                    NumericIsoCode = 706,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "South Georgia & South Sandwich Islands",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "GS",
                    ThreeLetterIsoCode = "SGS",
                    NumericIsoCode = 239,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Sri Lanka",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "LK",
                    ThreeLetterIsoCode = "LKA",
                    NumericIsoCode = 144,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "St. Helena",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SH",
                    ThreeLetterIsoCode = "SHN",
                    NumericIsoCode = 654,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "St. Pierre and Miquelon",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "PM",
                    ThreeLetterIsoCode = "SPM",
                    NumericIsoCode = 666,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Sudan",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SD",
                    ThreeLetterIsoCode = "SDN",
                    NumericIsoCode = 736,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Suriname",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SR",
                    ThreeLetterIsoCode = "SUR",
                    NumericIsoCode = 740,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Svalbard and Jan Mayen Islands",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SJ",
                    ThreeLetterIsoCode = "SJM",
                    NumericIsoCode = 744,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Swaziland",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SZ",
                    ThreeLetterIsoCode = "SWZ",
                    NumericIsoCode = 748,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Syrian Arab Republic",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "SY",
                    ThreeLetterIsoCode = "SYR",
                    NumericIsoCode = 760,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Tajikistan",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TJ",
                    ThreeLetterIsoCode = "TJK",
                    NumericIsoCode = 762,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Tanzania",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TZ",
                    ThreeLetterIsoCode = "TZA",
                    NumericIsoCode = 834,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Togo",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TG",
                    ThreeLetterIsoCode = "TGO",
                    NumericIsoCode = 768,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Tokelau",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TK",
                    ThreeLetterIsoCode = "TKL",
                    NumericIsoCode = 772,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Tonga",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TO",
                    ThreeLetterIsoCode = "TON",
                    NumericIsoCode = 776,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Trinidad and Tobago",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TT",
                    ThreeLetterIsoCode = "TTO",
                    NumericIsoCode = 780,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Tunisia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TN",
                    ThreeLetterIsoCode = "TUN",
                    NumericIsoCode = 788,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Turkmenistan",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TM",
                    ThreeLetterIsoCode = "TKM",
                    NumericIsoCode = 795,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Turks and Caicos Islands",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TC",
                    ThreeLetterIsoCode = "TCA",
                    NumericIsoCode = 796,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Tuvalu",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "TV",
                    ThreeLetterIsoCode = "TUV",
                    NumericIsoCode = 798,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Uganda",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "UG",
                    ThreeLetterIsoCode = "UGA",
                    NumericIsoCode = 800,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Vanuatu",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "VU",
                    ThreeLetterIsoCode = "VUT",
                    NumericIsoCode = 548,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Vatican City State (Holy See)",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "VA",
                    ThreeLetterIsoCode = "VAT",
                    NumericIsoCode = 336,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Viet Nam",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "VN",
                    ThreeLetterIsoCode = "VNM",
                    NumericIsoCode = 704,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Virgin Islands (British)",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "VG",
                    ThreeLetterIsoCode = "VGB",
                    NumericIsoCode = 92,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Virgin Islands (U.S.)",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "VI",
                    ThreeLetterIsoCode = "VIR",
                    NumericIsoCode = 850,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Wallis and Futuna Islands",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "WF",
                    ThreeLetterIsoCode = "WLF",
                    NumericIsoCode = 876,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Western Sahara",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "EH",
                    ThreeLetterIsoCode = "ESH",
                    NumericIsoCode = 732,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Yemen",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "YE",
                    ThreeLetterIsoCode = "YEM",
                    NumericIsoCode = 887,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Zambia",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "ZM",
                    ThreeLetterIsoCode = "ZMB",
                    NumericIsoCode = 894,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
                new Country
                {
                    Name = "Zimbabwe",
                    AllowsBilling = true,
                    AllowsShipping = true,
                    TwoLetterIsoCode = "ZW",
                    ThreeLetterIsoCode = "ZWE",
                    NumericIsoCode = 716,
                    SubjectToVat = false,
                    DisplayOrder = 100,
                    Published = true
                },
            };
            this.Alter(entities);
            return entities;
        }
    }
}
