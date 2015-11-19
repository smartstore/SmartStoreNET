using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Utilities;
using SmartStore.Services.DataExchange.Csv;
using SmartStore.Services.DataExchange.Excel;
using SmartStore.Services.DataExchange.Import;
using SmartStore.Tests;
using SmartStore.Utilities;

namespace SmartStore.Services.Tests.DataExchange
{
	[TestFixture]
	public class DataReaderTests
	{
		[Test]
		public void CsvReaderThrowsIfNotInitialized()
		{
			using (var csv = new CsvDataReader(new StreamReader("D:\\products-10000.csv")))
			{
				ExceptionAssert.Throws<InvalidOperationException>(() => { var value = csv["Name"]; });
			}
		}

		[Test]
		public void ExcelReaderThrowsIfNotInitialized()
		{
			using (var excel = new ExcelDataReader(new FileStream("D:\\products-10000.xlsx", FileMode.Open, FileAccess.Read), true))
			{
				ExceptionAssert.Throws<InvalidOperationException>(() => { var value = excel["Name"]; });
			}
		}

		[Test]
		public void DataReaderPerfTest()
		{
			int cycles = 1;

			////Chronometer.Measure(cycles, "ReadCsv", i => ReadCsv());
			//Chronometer.Measure(cycles, "ReadExcel", i => ReadExcel());

			Chronometer.Measure(cycles, "CreateDataTableFromReader", i => CreateDataTableFromReader());
		}

		private void CreateDataTableFromReader()
		{
			IDataTable dataTable;

			using (var csv = new CsvDataReader(new StreamReader("D:\\csvwriter-1-7-0001-elmarshopinfocsvfeed.csv")))
			{
				dataTable = LightweightDataTable.FromDataReader(csv);
				//foreach (var r in dataTable.Rows)
				//{
				//	ObjectDumper.ToConsole(r);
				//}
			}

			//using (var fileStream = new FileStream("D:\\products-10000.xlsx", FileMode.Open, FileAccess.Read))
			//{
			//	using (var excel = new ExcelDataReader(fileStream, true))
			//	{
			//		dataTable = LightweightDataTable.FromDataReader(excel);
			//	}
			//}

			Console.WriteLine("TotalRows: " + dataTable.Rows.Count);

			dynamic lastRow = dataTable.Rows.Last();
			var row = (IDataRow)lastRow;

			//ObjectDumper.ToConsole(row["AllowCustomerReviews"].Convert<bool>());
			ObjectDumper.ToConsole(lastRow);
			ObjectDumper.ToConsole(dataTable.Columns);
		}

		private void ReadCsv()
		{
			var memBefore = Process.GetCurrentProcess().PrivateMemorySize64;
			long memDuring = 0;
			int rows = 0;

			using (var fileStream = new FileStream("D:\\products-10000.csv", FileMode.Open, FileAccess.Read))
			{
				// determine total row count
				using (var csv = new CsvDataReader(fileStream.ToStreamReader(true)))
				{
					while (csv.Read())
					{
						rows++;
					}
				}

				// actually read rows
				fileStream.Seek(0, SeekOrigin.Begin);
				using (var csv = new CsvDataReader(fileStream.ToStreamReader(true)))
				{
					var reader = csv as IDataReader;

					ObjectDumper.ToConsole(reader.GetSchemaTable());

					var fields = csv.GetFieldHeaders().Reverse().ToArray();

					while (csv.Read())
					{
						ReadRow(csv, fields, System.Convert.ToInt16(csv.CurrentRowIndex));
					}
					memDuring = Process.GetCurrentProcess().PrivateMemorySize64;
				}
			}

			var memAfter = Process.GetCurrentProcess().PrivateMemorySize64;

			Console.WriteLine("Read Lines: {0}, MemBefore: {1}, MemDuring: {2}, MemAfter: {3}",
				rows,
				Prettifier.BytesToString(memBefore),
				Prettifier.BytesToString(memDuring),
				Prettifier.BytesToString(memAfter));
		}

		private void ReadExcel()
		{
			var memBefore = Process.GetCurrentProcess().PrivateMemorySize64;
			long memDuring = 0;
			int rows = 0;

			using (var fileStream = new FileStream("D:\\products-100.xlsx", FileMode.Open, FileAccess.Read))
			{
				using (var excel = new ExcelDataReader(fileStream, true))
				{
					var reader = excel as IDataReader;
					rows = excel.TotalRows;

					ObjectDumper.ToConsole(reader.GetSchemaTable());

					var fields = excel.GetColumnHeaders().Reverse().ToArray();

					while (excel.Read())
					{
						ReadRow(excel, fields, excel.CurrentRowIndex);
					}
					memDuring = Process.GetCurrentProcess().PrivateMemorySize64;
				}
			}

			var memAfter = Process.GetCurrentProcess().PrivateMemorySize64;

			Console.WriteLine("Read Lines: {0}, MemBefore: {1}, MemDuring: {2}, MemAfter: {3}",
				rows,
				Prettifier.BytesToString(memBefore),
				Prettifier.BytesToString(memDuring),
				Prettifier.BytesToString(memAfter));
		}

		private void ReadRow(IDataReader reader, string[] fields, int row)
		{
			foreach (var field in fields)
			{
				var value = reader[field].Convert<string>();
			}
		}
	}

}



