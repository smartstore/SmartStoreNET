using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using Microsoft.Win32;

namespace SmartStore.Core.IO
{ 
    public static class MimeTypes
    {
		private static readonly ConcurrentDictionary<string, string> _mimeMap = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public static string MapNameToMimeType(string fileNameOrExtension)
        {
            return MimeMapping.GetMimeMapping(fileNameOrExtension);
        }

        /// <summary>
        /// Returns the (dotless) extension for a mime type
        /// </summary>
        /// <param name="mimeType">The mime type</param>
        /// <returns>The corresponding file extension (without dot)</returns>
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        public static string MapMimeTypeToExtension(string mimeType)
        {
            if (mimeType.IsEmpty())
                return null;

			return _mimeMap.GetOrAdd(mimeType, k => {
				string result;

				try
				{
					using (var key = Registry.ClassesRoot.OpenSubKey(@"MIME\Database\Content Type\" + mimeType, false))
					{
						object value = key != null ? key.GetValue("Extension", null) : null;
						result = value != null ? value.ToString().Trim('.') : null;
					}
				}
				catch
				{
					string[] parts = mimeType.Split('/');
					result = parts[parts.Length - 1];
					switch (result)
					{
						case "pjpeg":
							result = "jpg";
							break;
						case "x-png":
							result = "png";
							break;
						case "x-icon":
							result = "ico";
							break;
					}
				}

				return result;
			});
        }
    }
}
