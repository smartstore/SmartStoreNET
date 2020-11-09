using System;
using System.Collections.Specialized;
using NUnit.Framework;
using SmartStore.Web.Framework.Filters;

namespace SmartStore.Web.MVC.Tests.Framework.Controllers
{
    [TestFixture]
    public class FormValueRequiredTests
    {
        private NameValueCollection _form;

        [SetUp]
        public void SetUp()
        {
            _form = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
            _form["Submit.First"] = "val";
            _form["Submit.Second"] = "val";
            _form["Submit.Third"] = "val";
            _form["Cancel.First"] = "val";
            _form["Cancel.Second"] = "val";
            _form["Cancel.Third"] = "val";
            _form["val1"] = "val";
            _form["VAL2"] = "val";
            _form["vAl3"] = "val";
        }

        [Test]
        public void Can_Match_Required_Equals_Any()
        {
            var values = new string[] { "NOTAVAIL", "VAL1" };
            var attr = new FormValueRequiredAttribute(FormValueRequirement.Equal, FormValueRequirementRule.MatchAny, values);
            Assert.IsTrue(attr.IsValidForRequest(_form));

            values = new string[] { "NOTAVAIL", "NOTAVAIL2" };
            attr = new FormValueRequiredAttribute(FormValueRequirement.Equal, FormValueRequirementRule.MatchAny, values);
            Assert.IsFalse(attr.IsValidForRequest(_form));
        }

        [Test]
        public void Can_Match_Required_Equals_All()
        {
            var values = new string[] { "val2", "VAL1", "Cancel.First" };
            var attr = new FormValueRequiredAttribute(FormValueRequirement.Equal, FormValueRequirementRule.MatchAll, values);
            Assert.IsTrue(attr.IsValidForRequest(_form));

            values = new string[] { "val2", "VAL1", "NOTAVAIL", "Cancel.First" };
            attr = new FormValueRequiredAttribute(FormValueRequirement.Equal, FormValueRequirementRule.MatchAll, values);
            Assert.IsFalse(attr.IsValidForRequest(_form));
        }

        [Test]
        public void Can_Match_Required_StartsWith_Any()
        {
            var values = new string[] { "NOTAVAIL", "VAL1", "SuBmit." };
            var attr = new FormValueRequiredAttribute(FormValueRequirement.StartsWith, FormValueRequirementRule.MatchAny, values);
            Assert.IsTrue(attr.IsValidForRequest(_form));

            values = new string[] { "NOTAVAIL", "NOTAVAIL2" };
            attr = new FormValueRequiredAttribute(FormValueRequirement.StartsWith, FormValueRequirementRule.MatchAny, values);
            Assert.IsFalse(attr.IsValidForRequest(_form));
        }

        [Test]
        public void Can_Match_Required_StartsWith_All()
        {
            var values = new string[] { "SUBMIT", "Cancel.", "VAL" };
            var attr = new FormValueRequiredAttribute(FormValueRequirement.StartsWith, FormValueRequirementRule.MatchAll, values);
            Assert.IsTrue(attr.IsValidForRequest(_form));

            values = new string[] { "SUBMIT", "Cancel.", "VAL", "notavail" };
            attr = new FormValueRequiredAttribute(FormValueRequirement.StartsWith, FormValueRequirementRule.MatchAll, values);
            Assert.IsFalse(attr.IsValidForRequest(_form));
        }

        [Test]
        public void Can_Match_Absent()
        {
            var values = new string[] { "NOTAVAIL", "VAL1" };
            var attr = new FormValueAbsentAttribute(FormValueRequirement.Equal, FormValueRequirementRule.MatchAny, values);
            Assert.IsTrue(attr.IsValidForRequest(_form));

            attr = new FormValueAbsentAttribute(FormValueRequirement.Equal, FormValueRequirementRule.MatchAll, values);
            Assert.IsFalse(attr.IsValidForRequest(_form));
        }

    }
}
