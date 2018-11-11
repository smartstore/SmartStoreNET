#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2008 - 2009
 *          http://www.west-wind.com/
 * 
 * Created: 09/08/2008
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 **************************************************************  
*/
#endregion

using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;

namespace SmartStore.ComponentModel
{
    internal static class SerializationUtils
    {
        /// <summary>
        /// Serializes an object instance to a file.
        /// </summary>
        /// <param name="instance">the object instance to serialize</param>
        /// <param name="fileName"></param>
        /// <param name="binarySerialization">determines whether XML serialization or binary serialization is used</param>
        /// <returns></returns>
        public static bool SerializeObject(object instance, string fileName, bool binarySerialization)
        {
            bool retVal = true;

            if (!binarySerialization)
            {
                XmlTextWriter writer = null;
                try
                {
                    XmlSerializer serializer =
                        new XmlSerializer(instance.GetType());

                    // Create an XmlTextWriter using a FileStream.
                    Stream fs = new FileStream(fileName, FileMode.Create);
                    writer = new XmlTextWriter(fs, new UTF8Encoding());
                    writer.Formatting = Formatting.Indented;
                    writer.IndentChar = ' ';
                    writer.Indentation = 3;

                    // Serialize using the XmlTextWriter.
                    serializer.Serialize(writer, instance);
                }
                catch (Exception ex)
                {
                    Debug.Write("SerializeObject failed with : " + ex.Message, "West Wind");
                    retVal = false;
                }
                finally
                {
                    if (writer != null)
                        writer.Close();
                }
            }
            else
            {
                Stream fs = null;
                try
                {
                    BinaryFormatter serializer = new BinaryFormatter();
                    fs = new FileStream(fileName, FileMode.Create);
                    serializer.Serialize(fs, instance);
                }
                catch
                {
                    retVal = false;
                }
                finally
                {
                    if (fs != null)
                        fs.Close();
                }
            }

            return retVal;
        }

        /// <summary>
        /// Overload that supports passing in an XML TextWriter. 
        /// </summary>
        /// <remarks>
        /// Note the Writer is not closed when serialization is complete 
        /// so the caller needs to handle closing.
        /// </remarks>
        /// <param name="instance">object to serialize</param>
        /// <param name="writer">XmlTextWriter instance to write output to</param>       
        /// <param name="throwExceptions">Determines whether false is returned on failure or an exception is thrown</param>
        /// <returns></returns>
        public static bool SerializeObject(object instance, XmlTextWriter writer, bool throwExceptions)
        {
            bool retVal = true;

            try
            {
                XmlSerializer serializer =
                    new XmlSerializer(instance.GetType());

                // Create an XmlTextWriter using a FileStream.
                writer.Formatting = Formatting.Indented;
                writer.IndentChar = ' ';
                writer.Indentation = 3;

                // Serialize using the XmlTextWriter.
                serializer.Serialize(writer, instance);
            }
            catch (Exception ex)
            {
                Debug.Write("SerializeObject failed with : " + ex.GetBaseException().Message + "\r\n" + (ex.InnerException != null ? ex.InnerException.Message : ""), "West Wind");

                if (throwExceptions)
                    throw;

                retVal = false;
            }

            return retVal;
        }


        /// <summary>
        /// Serializes an object into an XML string variable for easy 'manual' serialization
        /// </summary>
        /// <param name="instance">object to serialize</param>
        /// <param name="xmlResultString">resulting XML string passed as an out parameter</param>
        /// <returns>true or false</returns>
        public static bool SerializeObject(object instance, out string xmlResultString)
        {
            return SerializeObject(instance, out xmlResultString, false);
        }

        /// <summary>
        /// Serializes an object into a string variable for easy 'manual' serialization
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="xmlResultString">Out parm that holds resulting XML string</param>
        /// <param name="throwExceptions">If true causes exceptions rather than returning false</param>
        /// <returns></returns>
        public static bool SerializeObject(object instance, out string xmlResultString, bool throwExceptions)
        {
            xmlResultString = string.Empty;
            MemoryStream ms = new MemoryStream();

            XmlTextWriter writer = new XmlTextWriter(ms, new UTF8Encoding());

            if (!SerializeObject(instance, writer, throwExceptions))
            {
                ms.Close();
                return false;
            }

            xmlResultString = Encoding.UTF8.GetString(ms.ToArray(), 0, (int)ms.Length);

            ms.Close();
            writer.Close();

            return true;
        }

	    /// <summary>
	    /// Serializes an object instance to a file.
	    /// </summary>
	    /// <param name="instance">the object instance to serialize</param>
	    /// <param name="resultBuffer"></param>
	    /// <param name="throwExceptions"></param>
	    /// <returns></returns>
	    public static bool SerializeObject(object instance, out byte[] resultBuffer, bool throwExceptions = false)
        {
            bool retVal = true;
		    resultBuffer = null;

			var ms = new MemoryStream();
            try
            {
                var serializer = new BinaryFormatter();
                serializer.Serialize(ms, instance);
				resultBuffer = ms.ToArray();
			}
            catch (Exception ex)
            {
                Debug.Write("SerializeObject failed with : " + ex.GetBaseException().Message, "West Wind");
                retVal = false;

                if (throwExceptions)
                    throw;
            }
            finally
            {
				ms.Close();
            }

            return retVal;
        }

        /// <summary>
        /// Serializes an object to an XML string. Unlike the other SerializeObject overloads
        /// this methods *returns a string* rather than a bool result!
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="throwExceptions">Determines if a failure throws or returns null</param>
        /// <returns>
        /// null on error otherwise the Xml String.         
        /// </returns>
        /// <remarks>
        /// If null is passed in null is also returned so you might want
        /// to check for null before calling this method.
        /// </remarks>
        public static string SerializeObjectToString(object instance, bool throwExceptions = false)
        {
            string xmlResultString = string.Empty;

            if (!SerializeObject(instance, out xmlResultString, throwExceptions))
                return null;

            return xmlResultString;
        }

        public static byte[] SerializeObjectToByteArray(object instance, bool throwExceptions = false)
        {
            byte[] byteResult = null;

            if (!SerializeObject(instance, out byteResult))
                return null;

            return byteResult;
        }



        /// <summary>
        /// Deserializes an object from file and returns a reference.
        /// </summary>
        /// <param name="fileName">name of the file to serialize to</param>
        /// <param name="objectType">The Type of the object. Use typeof(yourobject class)</param>
        /// <param name="binarySerialization">determines whether we use Xml or Binary serialization</param>
        /// <returns>Instance of the deserialized object or null. Must be cast to your object type</returns>
        public static object DeSerializeObject(string fileName, Type objectType, bool binarySerialization)
        {
            return DeSerializeObject(fileName, objectType, binarySerialization, false);
        }

        /// <summary>
        /// Deserializes an object from file and returns a reference.
        /// </summary>
        /// <param name="fileName">name of the file to serialize to</param>
        /// <param name="objectType">The Type of the object. Use typeof(yourobject class)</param>
        /// <param name="binarySerialization">determines whether we use Xml or Binary serialization</param>
        /// <param name="throwExceptions">determines whether failure will throw rather than return null on failure</param>
        /// <returns>Instance of the deserialized object or null. Must be cast to your object type</returns>
        public static object DeSerializeObject(string fileName, Type objectType, bool binarySerialization, bool throwExceptions)
        {
            object instance = null;

            if (!binarySerialization)
            {

                XmlReader reader = null;
                XmlSerializer serializer = null;
                FileStream fs = null;
                try
                {
                    // Create an instance of the XmlSerializer specifying type and namespace.
                    serializer = new XmlSerializer(objectType);

                    // A FileStream is needed to read the XML document.
                    fs = new FileStream(fileName, FileMode.Open);
                    reader = new XmlTextReader(fs);

                    instance = serializer.Deserialize(reader);
                }
                catch (Exception ex)
                {
                    if (throwExceptions)
                        throw;

                    string message = ex.Message;
                    return null;
                }
                finally
                {
                    if (fs != null)
                        fs.Close();

                    if (reader != null)
                        reader.Close();
                }
            }
            else
            {

                BinaryFormatter serializer = null;
                FileStream fs = null;

                try
                {
                    serializer = new BinaryFormatter();
                    fs = new FileStream(fileName, FileMode.Open);
                    instance = serializer.Deserialize(fs);

                }
                catch
                {
                    return null;
                }
                finally
                {
                    if (fs != null)
                        fs.Close();
                }
            }

            return instance;
        }

        /// <summary>
        /// Deserialize an object from an XmlReader object.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public static object DeSerializeObject(XmlReader reader, Type objectType)
        {
            XmlSerializer serializer = new XmlSerializer(objectType);
            object Instance = serializer.Deserialize(reader);
            reader.Close();

            return Instance;
        }

        public static object DeSerializeObject(string xml, Type objectType)
        {
            XmlTextReader reader = new XmlTextReader(xml, XmlNodeType.Document, null);
            return DeSerializeObject(reader, objectType);
        }

        /// <summary>
        /// Deseializes a binary serialized object from  a byte array
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="objectType"></param>
        /// <param name="throwExceptions"></param>
        /// <returns></returns>
        public static object DeSerializeObject(byte[] buffer, Type objectType, bool throwExceptions = false)
        {
            BinaryFormatter serializer = null;
            MemoryStream ms = null;
            object Instance = null;

            try
            {
                serializer = new BinaryFormatter();
                ms = new MemoryStream(buffer);
                Instance = serializer.Deserialize(ms);

            }
            catch
            {
                if (throwExceptions)
                    throw;

                return null;
            }
            finally
            {
                if (ms != null)
                    ms.Close();
            }

            return Instance;
        }


        /// <summary>
        /// Returns a string of all the field value pairs of a given object.
        /// Works only on non-statics.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string ObjectToString(object instance, string separator, ObjectToStringTypes type)
        {
            var fi = instance.GetType().GetFields();

            string output = string.Empty;

            if (type == ObjectToStringTypes.Properties || type == ObjectToStringTypes.PropertiesAndFields)
            {
                foreach (var property in instance.GetType().GetProperties())
                {
                    try
                    {
                        output += property.Name + ":" + property.GetValue(instance, null).ToString() + separator;
                    }
                    catch
                    {
                        output += property.Name + ": n/a" + separator;
                    }
                }
            }

            if (type == ObjectToStringTypes.Fields || type == ObjectToStringTypes.PropertiesAndFields)
            {
                foreach (var field in fi)
                {
                    try
                    {
                        output = output + field.Name + ": " + field.GetValue(instance).ToString() + separator;
                    }
                    catch
                    {
                        output = output + field.Name + ": n/a" + separator;
                    }
                }
            }
            return output;
        }

    }

    public enum ObjectToStringTypes
    {
        Properties,
        PropertiesAndFields,
        Fields
    }
}




