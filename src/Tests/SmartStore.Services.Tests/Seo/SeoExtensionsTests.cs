using System;
using NUnit.Framework;
using SmartStore.Tests;
using SmartStore.Utilities;

namespace SmartStore.Services.Tests.Seo
{
    [TestFixture]
    public class SeoExtensionsTests
    {
        [Test]
        public void Should_return_lowercase()
        {
            SeoHelper.GetSeName("tEsT", false, false).ShouldEqual("test");
        }

        [Test]
        public void Should_allow_all_latin_chars()
        {
            SeoHelper.GetSeName("abcdefghijklmnopqrstuvwxyz1234567890", false, false).ShouldEqual("abcdefghijklmnopqrstuvwxyz1234567890");
        }

        [Test]
        public void Should_remove_illegal_chars()
        {
            SeoHelper.GetSeName("test!@#$%^&*()+<>?", false, false).ShouldEqual("test");
        }

        [Test]
        public void Should_replace_space_with_dash()
        {
            SeoHelper.GetSeName("test test", false, false).ShouldEqual("test-test");
            SeoHelper.GetSeName("test     test", false, false).ShouldEqual("test-test");
        }

        [Test]
        public void Can_convert_non_western_chars()
        {
            //german letters with diacritics
            SeoHelper.GetSeName("testäöü", true, false).ShouldEqual("testaou");
            SeoHelper.GetSeName("testäöü", false, false).ShouldEqual("test");

            var charConversions = string.Join(Environment.NewLine, new string[] { "ä;ae", "ö;oe", "ü;ue" });

            SeoHelper.GetSeName("testäöü", false, false, charConversions).ShouldEqual("testaeoeue");

            SeoHelper.ResetUserSeoCharacterTable();
        }

        [Test]
        public void Can_allow_unicode_chars()
        {
            //russian letters
            SeoHelper.GetSeName("testтест", false, true).ShouldEqual("testтест");
            SeoHelper.GetSeName("testтест", false, false).ShouldEqual("test");
        }
    }
}



