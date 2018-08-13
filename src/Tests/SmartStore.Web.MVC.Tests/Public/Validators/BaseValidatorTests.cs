using NUnit.Framework;
using SmartStore.Core.Localization;

namespace SmartStore.Web.MVC.Tests.Public.Validators
{
    [TestFixture]
    public abstract class BaseValidatorTests
    {
        protected Localizer T = NullLocalizer.Instance;

        [SetUp]
        public void Setup()
        {
        }
    }
}
