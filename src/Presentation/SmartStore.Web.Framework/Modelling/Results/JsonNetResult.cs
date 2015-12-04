using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SmartStore.Services.Helpers;

// ReSharper disable CheckNamespace
namespace SmartStore.Web.Framework.Modelling
{
	public class JsonNetResult : JsonResult
	{
		private readonly IDateTimeHelper _dateTimeHelper;
		private readonly JsonSerializerSettings _settings;

		public JsonNetResult(IDateTimeHelper dateTimeHelper)
			: this(dateTimeHelper, null)
		{
		}

		public JsonNetResult(IDateTimeHelper dateTimeHelper, JsonSerializerSettings settings)
		{
			Guard.ArgumentNotNull(() => dateTimeHelper);

			_dateTimeHelper = dateTimeHelper;
			_settings = settings;
		}

		public override void ExecuteResult(ControllerContext context)
		{
			Guard.ArgumentNotNull(() => context);

			if (this.Data == null)
				return;

			if (this.JsonRequestBehavior == JsonRequestBehavior.DenyGet && context.HttpContext.Request.HttpMethod.IsCaseInsensitiveEqual("GET"))
			{
				throw new InvalidOperationException("This request has been blocked because sensitive information could be disclosed to third party web sites when this is used in a GET request.To allow GET requests, set JsonRequestBehavior to AllowGet.");
			}

			var response = context.HttpContext.Response;

			if (this.ContentEncoding != null)
			{
				response.ContentEncoding = this.ContentEncoding;
			}
			
			response.ContentType = this.ContentType.NullEmpty() ?? "application/json";

			var serializerSettings = _settings ?? new JsonSerializerSettings
			{
				MissingMemberHandling = MissingMemberHandling.Ignore,

				// Limit the object graph we'll consume to a fixed depth. This prevents stackoverflow exceptions
				// from deserialization errors that might occur from deeply nested objects.
				MaxDepth = 32,

				// Do not change this setting
				// Setting this to None prevents Json.NET from loading malicious, unsafe, or security-sensitive types
				TypeNameHandling = TypeNameHandling.None
			};

			if (_settings == null)
			{
				serializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;
				serializerSettings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
				
				var utcDateTimeConverter = new UTCDateTimeConverter(_dateTimeHelper, new JavaScriptDateTimeConverter());
				serializerSettings.Converters.Add(utcDateTimeConverter);
			}

			using (var jsonWriter = new JsonTextWriter(response.Output))
			{
				jsonWriter.CloseOutput = false;
				var jsonSerializer = JsonSerializer.Create(serializerSettings);
				jsonSerializer.Serialize(jsonWriter, this.Data);
			}
		}

		class UTCDateTimeConverter : DateTimeConverterBase
		{
			private readonly IDateTimeHelper _dateTimeHelper;
			private readonly DateTimeConverterBase _innerConverter;

			public UTCDateTimeConverter(IDateTimeHelper dateTimeHelper, DateTimeConverterBase innerConverter)
			{
				Guard.ArgumentNotNull(() => innerConverter);

				_dateTimeHelper = dateTimeHelper;
				_innerConverter = innerConverter;
			}

			public override bool CanConvert(Type objectType)
			{
				//System.Diagnostics.Debug.WriteLine(objectType.Name);
				return _innerConverter.CanConvert(objectType);
			}

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
			{
				return _innerConverter.ReadJson(reader, objectType, existingValue, serializer);
			}

			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				if (value is DateTime)
				{
					value = DateTime.SpecifyKind((DateTime)value, DateTimeKind.Unspecified);
					value = new DateTimeOffset((DateTime)value, _dateTimeHelper.CurrentTimeZone.BaseUtcOffset);				
				}

				//var d = value as DateTime?;
				//if (d != null)
				//{
				//	value = DateTime.SpecifyKind(d.Value, DateTimeKind.Local);
				//}

				_innerConverter.WriteJson(writer, value, serializer);
			}
		}
	}
}
