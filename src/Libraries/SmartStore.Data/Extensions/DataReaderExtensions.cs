//Contributor: Rick Strahl - http://codepaste.net/qqcf4x

using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using SmartStore.ComponentModel;

namespace SmartStore.Data
{
    public static class DataReaderExtensions
    {
        public static object GetValue(this IDataReader reader, string columnName)
        {
            try
            {
                if (reader != null && !reader.IsClosed && columnName.HasValue())
                {
                    int ordinal = reader.GetOrdinal(columnName);
                    return reader.GetValue(ordinal);
                }
            }
            catch (Exception ex)
            {
                ex.Dump();
            }

            return null;
        }

        public static IEnumerable<T> MapSequence<T>(this IDataReader reader, params string[] fieldsToSkip)
            where T : new()
        {
            Guard.NotNull(reader, nameof(reader));

            while (reader.Read())
            {
                yield return reader.Map<T>(fieldsToSkip);
            }
        }

        public static IEnumerable<dynamic> MapSequence(this IDataReader reader, params string[] fieldsToSkip)
        {
            Guard.NotNull(reader, nameof(reader));

            while (reader.Read())
            {
                yield return reader.Map(fieldsToSkip);
            }
        }

        /// <summary>
        /// Populates an object from a single DataReader row using
        /// fast reflection routines by matching the DataReader fields to a public property on
        /// the object passed in. Unmatched properties are left unchanged.
        /// 
        /// You need to pass in a data reader located on the active row you want
        /// to serialize.
        /// </summary>
        /// <param name="reader">Instance of the DataReader to read data from. Should be located on the correct record (Read() should have been called on it before calling this method)</param>
        /// <param name="fieldsToSkip">An array of reader field names to ignore</param>
        public static T Map<T>(this IDataReader reader, params string[] fieldsToSkip)
            where T : new()
        {
            if (reader.IsClosed)
                throw new InvalidOperationException("Data reader cannot be used because it's already closed");

            var instance = new T();
            MapObject(reader, instance, fieldsToSkip);
            return instance;
        }

        public static dynamic Map(this IDataReader reader, params string[] fieldsToSkip)
        {
            if (reader.IsClosed)
                throw new InvalidOperationException("Data reader cannot be used because it's already closed");

            dynamic instance = new ExpandoObject();
            MapDictionary(reader, instance, fieldsToSkip);
            return instance;
        }

        public static void Map(this IDataReader reader, object instance, params string[] fieldsToSkip)
        {
            Guard.NotNull(instance, "instance");

            if (reader.IsClosed)
                throw new InvalidOperationException("Data reader cannot be used because it's already closed");

            var dict = instance as IDictionary<string, object>;

            if (dict != null)
            {
                MapDictionary(reader, dict, fieldsToSkip);
            }
            else
            {
                MapObject(reader, instance, fieldsToSkip);
            }
        }

        private static void MapObject(IDataReader reader, object instance, params string[] fieldsToSkip)
        {
            var fastProperties = FastProperty.GetProperties(instance.GetType());

            if (fastProperties.Count == 0)
                return;

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string name = reader.GetName(i);
                if (fastProperties.ContainsKey(name))
                {
                    var fastProp = fastProperties[name];

                    if (fieldsToSkip.Contains(name))
                        continue;

                    if ((fastProp != null) && fastProp.Property.CanWrite)
                    {
                        var dbValue = reader.GetValue(i);
                        fastProp.SetValue(instance, (dbValue == DBNull.Value) ? null : dbValue);
                    }
                }
            }
        }

        private static void MapDictionary(IDataReader reader, IDictionary<string, object> instance, params string[] fieldsToSkip)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                string name = reader.GetName(i);

                if (fieldsToSkip.Contains(name))
                    continue;

                var dbValue = reader.GetValue(i);
                instance[name] = (dbValue == DBNull.Value) ? null : dbValue;
            }
        }
    }
}
