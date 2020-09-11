using System.Collections.Generic;
using NUnit.Framework;
using SmartStore.Services.DataExchange.Import;
using SmartStore.Tests;

namespace SmartStore.Services.Tests.DataExchange
{
    [TestFixture]
    public class ImportDataSegmenterTests
    {
        [Test]
        public void CanResolveColumnIndexNames()
        {
            var columns = new List<IDataColumn>
            {
                new LightweightDataColumn("Attr[Color]", typeof(string)),
                new LightweightDataColumn("Attr[Size]", typeof(string)),
                new LightweightDataColumn("material", typeof(string)),
                new LightweightDataColumn("Name[en]", typeof(string)),
                new LightweightDataColumn("Name[de]", typeof(string)),
                new LightweightDataColumn("Name[fr]", typeof(string)),
                new LightweightDataColumn("name_it", typeof(string)),
            };

            var table = new LightweightDataTable(columns, new List<object[]>());
            var segmenter = new ImportDataSegmenter(table, new ColumnMap());
            segmenter.ColumnMap.AddMapping("Attr[Material]", "material");
            segmenter.ColumnMap.AddMapping("Name[it]", "name_it");

            var attrs = segmenter.GetColumnIndexes("Attr");
            attrs.ShouldSequenceEqual(new string[] { "Material", "Color", "Size" });

            var langs = segmenter.GetColumnIndexes("Name");
            langs.ShouldSequenceEqual(new string[] { "it", "en", "de", "fr" });
        }
    }
}
