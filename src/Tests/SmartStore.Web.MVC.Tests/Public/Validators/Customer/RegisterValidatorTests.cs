﻿using FluentValidation.TestHelper;
using SmartStore.Core.Domain.Customers;
using SmartStore.Web.Models.Customer;
using SmartStore.Web.Validators.Customer;
using NUnit.Framework;
using SmartStore.Core.Domain.Tax;

namespace SmartStore.Web.MVC.Tests.Public.Validators.Customer
{
    [TestFixture]
    public class RegisterValidatorTests : BaseValidatorTests
    {
        private RegisterValidator _validator;
        private CustomerSettings _customerSettings;
        private TaxSettings _taxSettings = new TaxSettings();
        
        [SetUp]
        public new void Setup()
        {
            _customerSettings = new CustomerSettings();
			_taxSettings = new TaxSettings();
            _validator = new RegisterValidator(_localizationService, _customerSettings, _taxSettings);
        }
        
        [Test]
        public void Should_have_error_when_email_is_null_or_empty()
        {
            var model = new RegisterModel();
            model.Email = null;
            _validator.ShouldHaveValidationErrorFor(x => x.Email, model);
            model.Email = "";
            _validator.ShouldHaveValidationErrorFor(x => x.Email, model);
        }

        [Test]
        public void Should_have_error_when_email_is_wrong_format()
        {
            var model = new RegisterModel();
            model.Email = "adminexample.com";
            _validator.ShouldHaveValidationErrorFor(x => x.Email, model);
        }

        [Test]
        public void Should_not_have_error_when_email_is_correct_format()
        {
            var model = new RegisterModel();
            model.Email = "admin@example.com";
            _validator.ShouldNotHaveValidationErrorFor(x => x.Email, model);
        }

        [Test]
        public void Should_have_error_when_firstName_is_null_or_empty()
        {
            var model = new RegisterModel();
            model.FirstName = null;
            _validator.ShouldHaveValidationErrorFor(x => x.FirstName, model);
            model.FirstName = "";
            _validator.ShouldHaveValidationErrorFor(x => x.FirstName, model);
        }

        [Test]
        public void Should_not_have_error_when_firstName_is_specified()
        {
            var model = new RegisterModel();
            model.FirstName = "John";
            _validator.ShouldNotHaveValidationErrorFor(x => x.FirstName, model);
        }

        [Test]
        public void Should_have_error_when_lastName_is_null_or_empty()
        {
            var model = new RegisterModel();
            model.LastName = null;
            _validator.ShouldHaveValidationErrorFor(x => x.LastName, model);
            model.LastName = "";
            _validator.ShouldHaveValidationErrorFor(x => x.LastName, model);
        }

        [Test]
        public void Should_not_have_error_when_lastName_is_specified()
        {
            var model = new RegisterModel();
            model.LastName = "Smith";
            _validator.ShouldNotHaveValidationErrorFor(x => x.LastName, model);
        }

        [Test]
        public void Should_have_error_when_password_is_null_or_empty()
        {
            var model = new RegisterModel();
            model.Password = null;
            //we know that password should equal confirmation password
            model.ConfirmPassword = model.Password;
            _validator.ShouldHaveValidationErrorFor(x => x.Password, model);
            model.Password = "";
            //we know that password should equal confirmation password
            model.ConfirmPassword = model.Password;
            _validator.ShouldHaveValidationErrorFor(x => x.Password, model);
        }

        [Test]
        public void Should_not_have_error_when_password_is_specified()
        {
            var model = new RegisterModel();
            model.Password = "password";
            //we know that password should equal confirmation password
            model.ConfirmPassword = model.Password;
            _validator.ShouldNotHaveValidationErrorFor(x => x.Password, model);
        }

        [Test]
        public void Should_have_error_when_confirmPassword_is_null_or_empty()
        {
            var model = new RegisterModel();
            model.ConfirmPassword = null;
            _validator.ShouldHaveValidationErrorFor(x => x.ConfirmPassword, model);
            model.ConfirmPassword = "";
            _validator.ShouldHaveValidationErrorFor(x => x.ConfirmPassword, model);
        }

        [Test]
        public void Should_not_have_error_when_confirmPassword_is_specified()
        {
            var model = new RegisterModel();
            model.ConfirmPassword = "some password";
            //we know that new password should equal confirmation password
            model.Password = model.ConfirmPassword;
            _validator.ShouldNotHaveValidationErrorFor(x => x.ConfirmPassword, model);
        }

        [Test]
        public void Should_have_error_when_password_doesnot_equal_confirmationPassword()
        {
            var model = new RegisterModel();
            model.Password = "some password";
            model.ConfirmPassword = "another password";
            _validator.ShouldHaveValidationErrorFor(x => x.ConfirmPassword, model);
        }

        [Test]
        public void Should_not_have_error_when_password_equals_confirmationPassword()
        {
            var model = new RegisterModel();
            model.Password = "some password";
            model.ConfirmPassword = "some password";
            _validator.ShouldNotHaveValidationErrorFor(x => x.Password, model);
        }

        [Test]
        public void Should_validate_password_is_length()
        {
            _customerSettings.PasswordMinLength = 5;
            _validator = new RegisterValidator(_localizationService, _customerSettings, _taxSettings);

            var model = new RegisterModel();
            model.Password = "1234";
            //we know that password should equal confirmation password
            model.ConfirmPassword = model.Password;
            _validator.ShouldHaveValidationErrorFor(x => x.Password, model);
            model.Password = "12345";
            //we know that password should equal confirmation password
            model.ConfirmPassword = model.Password;
            _validator.ShouldNotHaveValidationErrorFor(x => x.Password, model);
        }
    }
}
