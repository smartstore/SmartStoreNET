using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.IO;
using System.Dynamic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Net;


///////////////////////////////////////////////////////////////////////
// Needs JSon.Net (Newtonsoft.Json.dll) from http://json.codeplex.com/
//////////////////////////////////////////////////////////////////////

namespace SmartStore.Core.Utilities
{
    /// <summary>
    /// Creates a dynamic object
    /// Methods that can be used on arrays: foreach, ToArray(), ToList(), Count, Length
    /// </summary>
    public class JsonObject : DynamicObject, IEnumerable, IEnumerator
    {
        object _object;

        JsonObject(object jObject)
        {
            this._object = jObject;
        }

        public static dynamic GetDynamicJsonObject(byte[] buf)
        {
            return GetDynamicJsonObject(buf, Encoding.UTF8);
        }

        public static dynamic GetDynamicJsonObject(byte[] buf, Encoding encoding)
        {
            return GetDynamicJsonObject(encoding.GetString(buf));
        }

        public static dynamic GetDynamicJsonObject(string json)
        {
            object o = JsonConvert.DeserializeObject(json);
            return new JsonObject(o);
        }

        internal static dynamic GetDynamicJsonObject(JObject jObj)
        {
            return new JsonObject(jObj);
        }

        public object this[string s]
        {
            get
            {
                JObject jObject = _object as JObject;
                object obj = jObject.SelectToken(s);
                if (obj == null)
                    return true;

                if (obj is JValue)
                    return GetValue(obj);
                else
                    return new JsonObject(obj);
            }
        }

        public object this[int i]
        {
            get
            {
                if (!(_object is JArray))
                    return null;

                object obj = (_object as JArray)[i];
                if (obj is JValue)
                {
                    return GetValue(obj);
                }
                return new JsonObject(obj);
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;

            if (_object is JArray)
            {
                JArray jArray = _object as JArray;
                switch (binder.Name)
                {
                    case "Length":
                    case "Count":
                        result = jArray.Count;
                        break;
                    case "ToList":
                        result = (Func<List<string>>)(() => jArray.Values().Select(x => x.ToString()).ToList());
                        break;
                    case "ToArray":
                        result = (Func<string[]>)(() => jArray.Values().Select(x => x.ToString()).ToArray());
                        break;
                }

                return true;
            }

            JObject jObject = _object as JObject;
            object obj = jObject.SelectToken(binder.Name);
            if (obj == null)
                return true;

            if (obj is JValue)
                result = GetValue(obj);
            else
                result = new JsonObject(obj);

            return true;
        }

        object GetValue(object obj)
        {
            string val = ((JValue)obj).ToString();

            int resInt;
            double resDouble;
            DateTime resDateTime;

            if (int.TryParse(val, out resInt))
                return resInt;
            if (DateTime.TryParse(val, out resDateTime))
                return resDateTime;
            if (double.TryParse(val, out resDouble))
                return resDouble;

            return val;
        }

        public override string ToString()
        {
            return _object.ToString();
        }

        int _index = -1;

        public IEnumerator GetEnumerator()
        {
            _index = -1;
            return this;
        }

        public object Current
        {
            get
            {
                if (!(_object is JArray))
                    return null;
                object obj = (_object as JArray)[_index];
                if (obj is JValue)
                    return GetValue(obj);
                return new JsonObject(obj);
            }
        }

        public bool MoveNext()
        {
            if (!(_object is JArray))
                return false;
            _index++;
            return _index < (_object as JArray).Count;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }

    public class XmlObject
    {
        public static dynamic GetDynamicJsonObject(string xmlString)
        {
            var xmlDoc = XDocument.Load(new StringReader(xmlString));
            return JsonObject.GetDynamicJsonObject(XmlToJObject(xmlDoc.Root));
        }

        public static dynamic GetDynamicJsonObject(Stream xmlStream)
        {
            var xmlDoc = XDocument.Load(xmlStream);
            return JsonObject.GetDynamicJsonObject(XmlToJObject(xmlDoc.Root));
        }

        static JObject XmlToJObject(XElement node)
        {
            JObject jObj = new JObject();
            foreach (var attr in node.Attributes())
            {
                jObj.Add(attr.Name.LocalName, attr.Value);
            }

            foreach (var childs in node.Elements().GroupBy(x => x.Name.LocalName))
            {
                string name = childs.ElementAt(0).Name.LocalName;
                if (childs.Count() > 1)
                {
                    JArray jArray = new JArray();
                    foreach (var child in childs)
                    {
                        jArray.Add(XmlToJObject(child));
                    }
                    jObj.Add(name, jArray);
                }
                else
                {
                    jObj.Add(name, XmlToJObject(childs.ElementAt(0)));
                }
            }

            node.Elements().Remove();
            if (!String.IsNullOrEmpty(node.Value))
            {
                string name = "Value";
                while (jObj[name] != null)
                    name = "_" + name;
                jObj.Add(name, node.Value);
            }

            return jObj;
        }
    }

    public static class JsonExtensions
    {
        public static JsonObject GetDynamicJsonObject(this Uri uri)
        {
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = System.Text.Encoding.UTF8;
                wc.Headers["User-Agent"] = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727; .NET4.0C; .NET4.0E)";
                return JsonObject.GetDynamicJsonObject(wc.DownloadString(uri.ToString()));
            }
        }
    }
}



