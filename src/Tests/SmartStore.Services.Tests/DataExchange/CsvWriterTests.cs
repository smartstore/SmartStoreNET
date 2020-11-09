using System.IO;
using System.Text;
using NUnit.Framework;
using SmartStore.Services.DataExchange.Csv;

namespace SmartStore.Services.Tests.DataExchange
{
    [TestFixture]
    public class CsvWriterTests
    {
        [Test]
        public void CanWriteValidCsv()
        {
            var sb = new StringBuilder();
            var cfg = new CsvConfiguration
            {
                QuoteAllFields = false,
                Quote = '"',
                Delimiter = ';',
                Escape = '"',
                TrimValues = true,
                SupportsMultiline = false
            };

            using (var writer = new CsvWriter(new StringWriter(sb), cfg))
            {
                writer.WriteFields(new string[]
                {
                    "1234",
                    " abc ",
                    " def",
                    "ghi ",
                    "j;klmn\"o",
                    "pqrs"
                });

                writer.NextRow();
            }

            string expected = "1234;abc;def;ghi;\"j;klmn\"\"o\";pqrs" + "\r\n";

            Assert.AreEqual(expected, sb.ToString());
        }

        [Test]
        public void CanSpanMultipleLines()
        {
            var sb = new StringBuilder();
            var cfg = new CsvConfiguration
            {
                QuoteAllFields = false,
                Quote = '"',
                Delimiter = ';',
                Escape = '"',
                TrimValues = true,
                SupportsMultiline = true
            };

            using (var writer = new CsvWriter(new StringWriter(sb), cfg))
            {
                writer.WriteFields(new string[]
                {
                    "1234",
                    " abc ",
                    " def",
                    "ghi ",
                    "jk\rl\nmno",
                    "pqrs"
                });

                writer.NextRow();
            }

            string expected = "1234;abc;def;ghi;\"jk\rl\nmno\";pqrs" + "\r\n";

            Assert.AreEqual(expected, sb.ToString());
        }

        [Test]
        public void CanQuoteAllFields()
        {
            var sb = new StringBuilder();
            var cfg = new CsvConfiguration
            {
                QuoteAllFields = true,
                Quote = '\'',
                Delimiter = ';',
                Escape = '"',
                TrimValues = true,
                SupportsMultiline = true
            };

            using (var writer = new CsvWriter(new StringWriter(sb), cfg))
            {
                writer.WriteFields(new string[]
                {
                    "1234",
                    " abc ",
                    " def",
                    "ghi ",
                    "jk\rl\nmno",
                    "pqrs"
                });

                writer.NextRow();
            }

            string expected = "'1234';'abc';'def';'ghi';'jk\rl\nmno';'pqrs'" + "\r\n";

            Assert.AreEqual(expected, sb.ToString());
        }

        [Test]
        public void CanEscapeQuotes()
        {
            var sb = new StringBuilder();
            var cfg = new CsvConfiguration
            {
                QuoteAllFields = true,
                Quote = '\'',
                Delimiter = ';',
                Escape = '\\',
                TrimValues = true,
                SupportsMultiline = true
            };

            using (var writer = new CsvWriter(new StringWriter(sb), cfg))
            {
                writer.WriteFields(new string[]
                {
                    "1234",
                    " abc ",
                    " def",
                    "ghi ",
                    "jk'lmo",
                    "pqrs"
                });

                writer.NextRow();
            }

            string expected = "'1234';'abc';'def';'ghi';'jk\\'lmo';'pqrs'" + "\r\n";

            Assert.AreEqual(expected, sb.ToString());
        }
    }

}



