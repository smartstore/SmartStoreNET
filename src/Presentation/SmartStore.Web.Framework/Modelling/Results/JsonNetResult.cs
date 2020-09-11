using System;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SmartStore.ComponentModel;
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
            Guard.NotNull(dateTimeHelper, nameof(dateTimeHelper));

            _dateTimeHelper = dateTimeHelper;
            _settings = settings;
        }

        public static JsonSerializerSettings CreateDefaultSerializerSettings()
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = SmartContractResolver.Instance,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,

                // We cannot ignore null. Client template of several Telerik grids would fail.
                // NullValueHandling = NullValueHandling.Ignore,

                // Limit the object graph we'll consume to a fixed depth. This prevents stackoverflow exceptions
                // from deserialization errors that might occur from deeply nested objects.
                MaxDepth = 32
            };

            return settings;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            Guard.NotNull(context, nameof(context));

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

            var serializerSettings = _settings ?? CreateDefaultSerializerSettings();

            if (_settings == null)
            {
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
                Guard.NotNull(innerConverter, nameof(innerConverter));

                _dateTimeHelper = dateTimeHelper;
                _innerConverter = innerConverter;
            }

            public override bool CanConvert(Type objectType)
            {
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
                    var d = (DateTime)value;
                    if (d.Kind == DateTimeKind.Unspecified)
                    {
                        // when DateTime kind is "Unspecified", it was very likely converted from UTC to 
                        // SERVER*s preferred local time before (with DateTimeHelper.ConvertToUserTime()).
                        // While this works fine during server-time rendering, it can lead to wrong UTC offsets
                        // on the client (e.g. in AJAX mode Grids, where rendering is performed locally with JSON data).
                        // The issue occurs when the client's time zone is not the same as "CurrentTimeZone" (configured in the backend).
                        // To fix it, we have to convert the date back to UTC kind, but with the SERVER PREFERRED TIMEZONE
                        // in order to calculate with the correct UTC offset. Then it's up to the client to display the date
                        // in the CLIENT's time zone. Which is not perfect of course, because the same date would be displayed in the 
                        // "CurrentTimeZone" if rendered on server.
                        // But: it fixes the issue and is way better than converting all AJAXable dates to strings on the server.
                        value = _dateTimeHelper.ConvertToUtcTime(d, _dateTimeHelper.CurrentTimeZone);
                    }
                }

                _innerConverter.WriteJson(writer, value, serializer);
            }
        }
    }
}
