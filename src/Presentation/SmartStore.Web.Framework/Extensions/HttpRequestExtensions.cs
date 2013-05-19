using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Diagnostics;

namespace SmartStore
{
	/// <remarks>codehint: sm-add</remarks>
	public static class HttpRequestExtension
	{
		public static Stream ToFileStream(this HttpRequestBase request, out string fileName, out string contentType, string paramName = "qqfile") {
			fileName = contentType = "";
			Stream stream = null;

            if (request[paramName].HasValue())
            {
                stream = request.InputStream;
                fileName = request[paramName];
            }
            else
            {
                if (request.Files.Count > 0)
                {
                    stream = request.Files[0].InputStream;
                    contentType = request.Files[0].ContentType;
                    fileName = Path.GetFileName(request.Files[0].FileName);
                }
            }

            //string ext = Path.GetExtension(fileName);
            //if (ext.HasValue())
            //{
            //    ext = ext.ToLowerInvariant();
            //}

            if (contentType.IsNullOrEmpty())
            {
                //contentType = ext.ExtensionToMimeType();
                contentType = SmartStore.Core.IO.MimeTypes.MapNameToMimeType(fileName);
            }

			return stream;
		}
	}	// class
}
