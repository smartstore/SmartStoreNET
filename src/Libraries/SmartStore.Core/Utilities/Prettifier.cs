using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace SmartStore.Utilities
{

    public static class Prettifier
    {

        public static string BytesToString(long bytes)
        {
            double result = bytes;
            double dsize = bytes;
            string unit;

            if (bytes < 1024)
            {
                unit = "B";
                result = dsize;

            }
            else if (bytes < Math.Pow(1024, 2))
            {
                unit = "KB";
                result = dsize / 1024;
            }
            else if (bytes < Math.Pow(1024, 3))
            {
                unit = "MB";
                result = dsize / Math.Pow(1024, 2);
            }
            else
            {
                unit = "GB";
                result = dsize / Math.Pow(1024, 3);
            }

            return "{0:F} {1}".FormatCurrent(result, unit);
        }

        public static string SecondsToString(double seconds)
        {
            try
            {
                int secsTemp = Convert.ToInt32(seconds);
                string label = "sek.";
                int remainder = 0;
                string remainderLabel = @"";

                if (secsTemp > 59)
                {
                    remainder = secsTemp % 60;
                    secsTemp /= 60;
                    label = "min.";
                    remainderLabel = @"sek.";
                }

                if (secsTemp > 59)
                {
                    remainder = secsTemp % 60;
                    secsTemp /= 60;
                    label = (secsTemp == 1) ? "Stunde" : "Stunden";
                    remainderLabel = "min.";
                }

                if (remainder == 0)
                {
                    return string.Format("{0:#,##0.#} {1}", secsTemp, label);
                }
                else
                {
                    return string.Format("{0:#,##0} {1} {2} {3}", secsTemp, label, remainder, remainderLabel);
                }
            }
            catch
            {
                return @"(-)";
            }

        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToPrettyUrl(string value)
        {
            return value; // TODO
        }

        public static string PrettifyXML(string xml)
        {
            if (xml.IsEmpty() || xml.IsWhiteSpace())
                return xml;
            
            // first read the xml ignoring whitespace
            using (var xmlReader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings { IgnoreWhitespace = true, CheckCharacters = false }))
            {
                // then write it out with indentation
                var sb = new StringBuilder();
                using (XmlWriter writer = XmlWriter.Create(sb, new XmlWriterSettings { Indent = true, IndentChars = "\t", CheckCharacters = false }))
                {
                    writer.WriteNode(xmlReader, true);
                }

                var result = sb.ToString();
                return result;
            }

        }

    }

}
