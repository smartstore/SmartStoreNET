using System;
using SmartStore.Core.Domain.DataExchange;

namespace SmartStore.Services.DataExchange
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class ExportProjectionFieldAttribute : Attribute
	{
		public ExportProjectionFieldAttribute(params ExportProjectionFieldType[] types)
		{
			Types = types;
		}

		public ExportProjectionFieldType[] Types { get; set; }
	}
}
