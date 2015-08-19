using System;
using SmartStore.Core.Domain.DataExchange;

namespace SmartStore.Services.DataExchange
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class ExportProjectionSupportAttribute : Attribute
	{
		public ExportProjectionSupportAttribute(params ExportProjectionSupport[] types)
		{
			Types = types;
		}

		public ExportProjectionSupport[] Types { get; set; }
	}
}
