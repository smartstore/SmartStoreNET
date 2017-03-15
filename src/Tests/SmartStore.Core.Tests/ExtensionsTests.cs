﻿using SmartStore.Tests;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using System;

namespace SmartStore.Core.Tests
{
    [TestFixture]
    public class ExtensionsTests
    {
		private Stream GetFileStream(string fileName)
		{
			return typeof(ExtensionsTests).Assembly.GetManifestResourceStream("SmartStore.Core.Tests.Files.{0}".FormatInvariant(fileName));
		}

		[Test]
        public void Can_check_IsNullOrDefault()
        {
            int? x1 = null;
            x1.IsNullOrDefault().ShouldBeTrue();

            int? x2 = 0;
            x2.IsNullOrDefault().ShouldBeTrue();

            int? x3 = 1;
            x3.IsNullOrDefault().ShouldBeFalse();
        }

		[Test]
		public void Can_Strip_Html()
		{
			var html = GetFileStream("testdata.html").AsString();
			var text = html.RemoveHtml().Trim();

			Assert.IsTrue(text.StartsWith("Produktmerkmale"));
			Assert.IsFalse(text.Contains("function()"));
			Assert.IsFalse(text.Contains(".someclass"));
			Assert.IsFalse(text.Contains("This is a comment and should be stripped from result"));
			Assert.IsTrue(text.EndsWith("Technologie:WCDM"));
		}
	}
}



