using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using SmartStore.Services.DataExchange.Csv;
using SmartStore.Services.DataExchange.Excel;
using SmartStore.Services.DataExchange.Import;
using SmartStore.Tests;

namespace SmartStore.Services.Tests.DataExchange
{
    [TestFixture]
    public class DataReaderTests
    {
        private Stream GetFileStream(string fileName)
        {
            return typeof(DataReaderTests).Assembly.GetManifestResourceStream("SmartStore.Services.Tests.DataExchange.Files.{0}".FormatInvariant(fileName));
        }

        [Test]
        public void CsvReaderThrowsIfNotInitialized()
        {
            using (var csv = new CsvDataReader(new StreamReader(GetFileStream("testdata.csv"))))
            {
                ExceptionAssert.Throws<InvalidOperationException>(() => { var value = csv["Name"]; });
            }
        }

        [Test]
        public void ExcelReaderThrowsIfNotInitialized()
        {
            using (var excel = new ExcelDataReader(GetFileStream("testdata.xlsx"), true))
            {
                ExceptionAssert.Throws<InvalidOperationException>(() => { var value = excel["Name"]; });
            }
        }

        [Test]
        public void CanReadExcel()
        {
            IDataTable table;

            using (var excel = new ExcelDataReader(GetFileStream("testdata.xlsx"), true))
            {
                table = LightweightDataTable.FromDataReader(excel);
            }

            VerifyDataTable(table);
        }

        [Test]
        public void CanReadCsv()
        {
            IDataTable table;

            using (var csv = new CsvDataReader(new StreamReader(GetFileStream("testdata.csv"), Encoding.UTF8)))
            {
                table = LightweightDataTable.FromDataReader(csv);
            }

            VerifyDataTable(table);
        }

        private void VerifyDataTable(IDataTable table)
        {
            Assert.AreEqual(12, table.Columns.Count, "Columns Count");
            Assert.AreEqual(10, table.Rows.Count, "Rows Count");

            var cols = table.Columns;

            Assert.AreEqual("Id", cols[0].Name);
            Assert.AreEqual("Sku", cols[1].Name);
            Assert.AreEqual("Name", cols[2].Name);
            Assert.AreEqual("Description", cols[3].Name);
            Assert.AreEqual("Bool", cols[4].Name);
            Assert.AreEqual("Date", cols[5].Name);
            Assert.AreEqual("OADate", cols[6].Name);
            Assert.AreEqual("UnixDate", cols[7].Name);
            Assert.AreEqual("Int", cols[8].Name);
            Assert.AreEqual("Double", cols[9].Name);
            Assert.AreEqual("Guid", cols[10].Name);
            Assert.AreEqual("IntList", cols[11].Name);

            var rows = table.Rows;

            rows[3]["Sku"].ShouldEqual("SKU 4");
            rows[1]["Name"].ShouldEqual("äöü");
            rows[7]["Description"].ShouldEqual("Description 8");
            rows[0]["Bool"].Convert<bool>().ShouldBeTrue();
            rows[5]["Bool"].Convert<bool>().ShouldBeTrue();
            rows[6]["Bool"].Convert<bool>().ShouldBeFalse();
            rows[3]["Double"].Convert<double>().ShouldEqual(9999.765);
            rows[0]["OADate"].Convert<DateTime>().ShouldEqual(DateTime.FromOADate(rows[0]["OADate"].Convert<double>()));
            rows[3]["Guid"].Convert<Guid>().ShouldEqual(Guid.Parse("77866957-eec3-4b35-950f-10d1699ac46d"));

            rows[0]["IntList"].Convert<List<int>>().ShouldSequenceEqual(new List<int> { 1, 2, 3, 4 });
            rows[1]["IntList"].Convert<List<short>>().ShouldSequenceEqual(new List<short> { 1, 2, 3, 4 });
            rows[5]["IntList"].Convert<List<double>>().ShouldSequenceEqual(new List<double> { 1, 2, 3, 4 });
        }
    }

}



