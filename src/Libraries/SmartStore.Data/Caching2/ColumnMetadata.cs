using System;

namespace SmartStore.Data.Caching2
{
	public class ColumnMetadata
	{
		public string Name { get; set; }
		public string DataTypeName { get; set; }
		public Type DataType { get; set; }
	}
}
