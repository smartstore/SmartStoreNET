using System.Collections.Specialized;

namespace SmartStore.WebApi.Client.Models
{
    public class ApiFileParameter
    {
        public ApiFileParameter(byte[] data)
            : this(data, null)
        {
        }

        public ApiFileParameter(byte[] data, string filename)
            : this(data, filename, null)
        {
        }

        public ApiFileParameter(byte[] data, string filename, string contenttype)
        {
            Data = data;
            FileName = filename;
            ContentType = contenttype;
            Parameters = new NameValueCollection();
        }

        public byte[] Data { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }

        public NameValueCollection Parameters { get; set; }
    }
}
