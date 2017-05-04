﻿using FluentValidation.TestHelper;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Web.Models.Common;
using SmartStore.Web.Models.Customer;
using SmartStore.Web.Validators.Common;
using NUnit.Framework;

namespace SmartStore.Web.MVC.Tests.Public.Validators.Common
{
    [TestFixture]
    public class AddressValidatorTests : BaseValidatorTests
    {
        [Test]
        public void Should_have_error_when_email_is_null_or_empty()
        {
            var validator = new AddressValidator(_localizationService,
                new AddressSettings());

            var model = new AddressModel();
            model.Email = null;
            validator.ShouldHaveValidationErrorFor(x => x.Email, model);
            model.Email = "";
            validator.ShouldHaveValidationErrorFor(x => x.Email, model);
        }
        [Test]
        public void Should_have_error_when_email_is_wrong_format()
        {
            var validator = new AddressValidator(_localizationService,
                new AddressSettings());

            var model = new AddressModel();
            model.Email = "adminexample.com";
            validator.ShouldHaveValidationErrorFor(x => x.Email, model);
        }
        [Test]
        public void Should_not_have_error_when_email_is_correct_format()
        {
            var validator = new AddressValidator(_localizationService,
                new AddressSettings());

            var model = new AddressModel();
            model.Email = "admin@example.com";
            validator.ShouldNotHaveValidationErrorFor(x => x.Email, model);
        }

        [Test]
        public void Should_have_error_when_email_doesnt_match()
        {
            var validator = new AddressValidator(_localizationService,
                new AddressSettings());

            var model = new AddressModel();
            model.Email = "admin@example.com";
            model.EmailMatch = "amin@example.com";
            validator.ShouldHaveValidationErrorFor(x => x.EmailMatch, model);
        }

        [Test]
        public void Should_not_have_error_when_email_match()
        {
            var validator = new AddressValidator(_localizationService,
                new AddressSettings());

            var model = new AddressModel();
            model.Email = "admin@example.com";
            model.EmailMatch = "admin@example.com";
            validator.ShouldNotHaveValidationErrorFor(x => x.EmailMatch, model);
        }

        [Test]
        public void Should_have_error_when_firstName_is_null_or_empty()
        {
            var validator = new AddressValidator(_localizationService,
                new AddressSettings());

            var model = new AddressModel();
            model.FirstName = null;
            validator.ShouldHaveValidationErrorFor(x => x.FirstName, model);
            model.FirstName = "";
            validator.ShouldHaveValidationErrorFor(x => x.FirstName, model);
        }
        [Test]
        public void Should_not_have_error_when_firstName_is_specified()
        {
            var validator = new AddressValidator(_localizationService,
                new AddressSettings());

            var model = new AddressModel();
            model.FirstName = "John";
            validator.ShouldNotHaveValidationErrorFor(x => x.FirstName, model);
        }

        [Test]
        public void Should_have_error_when_lastName_is_null_or_empty()
        {
            var validator = new AddressValidator(_localizationService,
                new AddressSettings());

            var model = new AddressModel();
            model.LastName = null;
            validator.ShouldHaveValidationErrorFor(x => x.LastName, model);
            model.LastName = "";
            validator.ShouldHaveValidationErrorFor(x => x.LastName, model);
        }
        [Test]
        public void Should_not_have_error_when_lastName_is_specified()
        {
            var validator = new AddressValidator(_localizationService,
                new AddressSettings());

            var model = new AddressModel();
            model.LastName = "Smith";
            validator.ShouldNotHaveValidationErrorFor(x => x.LastName, model);
        }

        [Test]
        public void Should_have_error_when_company_is_null_or_empty_based_on_required_setting()
        {
            var model = new AddressModel();

            //required
            var validator = new AddressValidator(_localizationService,
                new AddressSettings()
                {
                    CompanyEnabled = true,
                    CompanyRequired = true
                });
            model.Company = null;
            validator.ShouldHaveValidationErrorFor(x => x.Company, model);
            model.Company = "";
            validator.ShouldHaveValidationErrorFor(x => x.Company, model);


            //not required
            validator = new AddressValidator(_localizationService,
                new AddressSettings()
                {
                    CompanyEnabled = true,
                    CompanyRequired = false
                });
            model.Company = null;
            validator.ShouldNotHaveValidationErrorFor(x => x.Company, model);
            model.Company = "";
            validator.ShouldNotHaveValidationErrorFor(x => x.Company, model);
        }
        [Test]
        public void Should_not_have_error_when_company_is_specified()
        {
            var validator = new AddressValidator(_localizationService,
                new AddressSettings()
                {
                    CompanyEnabled = true
                });

            var model = new AddressModel();
            model.Company = "Company";
            validator.ShouldNotHaveValidationErrorFor(x => x.Company, model);
        }

        [Test]
        public void Should_have_error_when_streetaddress_is_null_or_empty_based_on_required_setting()
        {
            var model = new AddressModel();

            //required
            var validator = new AddressValidator(_localizationService,
                new AddressSettings()
                {
                    StreetAddressEnabled = true,
                    StreetAddressRequired = true
                });
            model.Address1 = null;
            validator.ShouldHaveValidationErrorFor(x => x.Address1, model);
            model.Address1 = "";
            validator.ShouldHaveValidationErrorFor(x => x.Address1, model);

            //not required
            validator = new AddressValidator(_localizationService,
                new AddressSettings()
                {
                    StreetAddressEnabled = true,
                    StreetAddressRequired = false
                });
            model.Address1 = null;
            validator.ShouldNotHaveValidationErrorFor(x => x.Address1, model);
            model.Address1 = "";
            validator.ShouldNotHaveValidationErrorFor(x => x.Address1, model);
        }
        [Test]
        public void Should_not_have_error_when_streetaddress_is_specified()
        {
            var validator = new AddressValidator(_localizationService,
                new AddressSettings()
                {
                    StreetAddressEnabled = true
                });

            var model = new AddressModel();
            model.Address1 = "Street address";
            validator.ShouldNotHaveValidationErrorFor(x => x.Address1, model);
        }

        [Test]
        public void Should_have_error_when_streetaddress2_is_null_or_empty_based_on_required_setting()
        {
            var model = new AddressModel();

            //required
            var validator = new AddressValidator(_localizationService,
                new AddressSettings()
                {
                    StreetAddress2Enabled = true,
                    StreetAddress2Required = true
                });
            model.Address2 = null;
            validator.ShouldHaveValidationErrorFor(x => x.Address2, model);
            model.Address2 = "";
            validator.ShouldHaveValidationErrorFor(x => x.Address2, model);

            //not required
            validator = new AddressValidator(_localizationService,
                new AddressSettings()
                {
                    StreetAddress2Enabled = true,
                    StreetAddress2Required = false
                });
            model.Address2 = null;
            validator.ShouldNotHaveValidationErrorFor(x => x.Address2, model);
            model.Address2 = "";
            validator.ShouldNotHaveValidationErrorFor(x => x.Address2, model);
        }
        [Test]
        public void Should_not_have_error_when_streetaddress2_is_specified()
        {
            var validator = new AddressValidator(_localizationService,
                new AddressSettings()
                {
                    StreetAddress2Enabled = true
                });

            var model = new AddressModel();
            model.Address2 = "Street address 2";
            validator.ShouldNotHaveValidationErrorFor(x => x.Address2, model);
        }

        [Test]
        public void Should_have_error_when_zippostalcode_is_null_or_empty_based_on_required_setting()
        {
            var model = new AddressModel();

            //required
            var validator = new AddressValidator(_localizationService,
                new AddressSettings()
                {
                    ZipPostalCodeEnabled = true,
                    ZipPostalCodeRequired = true
                });
            model.ZipPostalCode = null;
            validator.ShouldHaveValidationErrorFor(x => x.ZipPostalCode, model);
            model.ZipPostalCode = "";
            validator.ShouldHaveValidationErrorFor(x => x.ZipPostalCode, model);


            //not required
            validator = new AddressValidator(_localizationService,
                new AddressSettings()
                {
                    ZipPostalCodeEnabled = true,
                    ZipPostalCodeRequired = false
                });
            model.ZipPostalCode = null;
            validator.ShouldNotHaveValidationErrorFor(x => x.ZipPostalCode, model);
            model.ZipPostalCode = "";
            validator.ShouldNotHaveValidationErrorFor(x => x.ZipPostalCode, model);
        }
        [Test]
        public void Should_not_have_error_when_zippostalcode_is_specified()
        {
            var validator = new AddressValidator(_localizationService,
                new AddressSettings()
                {
                    StreetAddress2Enabled = true
                });

            var model = new AddressModel();
            model.ZipPostalCode = "zip";
            validator.ShouldNotHaveValidationErrorFor(x => x.ZipPostalCode, model);
        }

        [Test]
        public void Should_have_error_when_city_is_null_or_empty_based_on_required_setting()
        {
            var model = new AddressModel();

            //required
            var validator = new AddressValidator(_localizationService,
                new AddressSettings()
                {
                    CityEnabled = true,
                    CityRequired = true
                });
            model.City = null;
            validator.ShouldHaveValidationErrorFor(x => x.City, model);
            model.City = "";
            validator.ShouldHaveValidationErrorFor(x => x.City, model);


            //not required
            validator = new AddressValidator(_localizationService,
                new AddressSettings()
                {
                    CityEnabled = true,
                    CityRequired = false
                });
            model.City = null;
            validator.ShouldNotHaveValidationErrorFor(x => x.City, model);
            model.City = "";
            validator.ShouldNotHaveValidationErrorFor(x => x.City, model);
        }
        [Test]
        public void Should_not_have_error_when_city_is_specified()
        {
            var validator = new AddressValidator(_localizationService,
                new AddressSettings()
                {
                    CityEnabled = true
                });

            var model = new AddressModel();
            model.City = "City";
            validator.ShouldNotHaveValidationErrorFor(x => x.City, model);
        }

        [Test]
        public void Should_have_error_when_phone_is_null_or_empty_based_on_required_setting()
        {
            var model = new AddressModel();

            //required
            var validator = new AddressValidator(_localizationService,
                new AddressSettings()
                {
                    PhoneEnabled = true,
                    PhoneRequired = true
                });
            model.PhoneNumber = null;
            validator.ShouldHaveValidationErrorFor(x => x.PhoneNumber, model);
            model.PhoneNumber = "";
            validator.ShouldHaveValidationErrorFor(x => x.PhoneNumber, model);

            //not required
            validator = new AddressValidator(_localizationService,
                new AddressSettings()
                {
                    PhoneEnabled = true,
                    PhoneRequired = false
                });
            model.PhoneNumber = null;
            validator.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber, model);
            model.PhoneNumber = "";
            validator.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber, model);
        }
        [Test]
        public void Should_not_have_error_when_phone_is_specified()
        {
            var validator = new AddressValidator(_localizationService,
                new AddressSettings()
                {
                    PhoneEnabled = true
                });

            var model = new AddressModel();
            model.PhoneNumber = "Phone";
            validator.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber, model);
        }

        [Test]
        public void Should_have_error_when_fax_is_null_or_empty_based_on_required_setting()
        {
            var model = new AddressModel();

            //required
            var validator = new AddressValidator(_localizationService,
                new AddressSettings()
                {
                    FaxEnabled = true,
                    FaxRequired = true
                });
            model.FaxNumber = null;
            validator.ShouldHaveValidationErrorFor(x => x.FaxNumber, model);
            model.FaxNumber = "";
            validator.ShouldHaveValidationErrorFor(x => x.FaxNumber, model);


            //not required
            validator = new AddressValidator(_localizationService,
                new AddressSettings()
                {
                    FaxEnabled = true,
                    FaxRequired = false
                });
            model.FaxNumber = null;
            validator.ShouldNotHaveValidationErrorFor(x => x.FaxNumber, model);
            model.FaxNumber = "";
            validator.ShouldNotHaveValidationErrorFor(x => x.FaxNumber, model);
        }
        [Test]
        public void Should_not_have_error_when_fax_is_specified()
        {
            var validator = new AddressValidator(_localizationService,
                new AddressSettings()
                {
                    FaxEnabled = true
                });

            var model = new AddressModel();
            model.FaxNumber = "Fax";
            validator.ShouldNotHaveValidationErrorFor(x => x.FaxNumber, model);
        }
    }
}
