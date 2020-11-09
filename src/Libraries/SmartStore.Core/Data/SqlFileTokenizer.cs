using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using SmartStore.Utilities;
using SmartStore.Utilities.ObjectPools;

namespace SmartStore.Core.Data
{
    public class SqlFileTokenizer
    {
        public SqlFileTokenizer(string fileName, Assembly assembly = null, string location = null)
        {
            Guard.NotEmpty(fileName, nameof(fileName));

            this.FileName = fileName;
            this.Assembly = assembly;
            this.Location = location;
        }

        public string FileName
        {
            get;
            private set;
        }

        public Assembly Assembly
        {
            get;
            private set;
        }

        public string Location
        {
            get;
            private set;
        }

        public IEnumerable<string> Tokenize()
        {
            if (this.Assembly == null)
            {
                this.Assembly = Assembly.GetExecutingAssembly();
            }

            using (var reader = ReadSqlFile())
            {
                string statement;
                while ((statement = ReadNextSqlStatement(reader)) != null)
                {
                    yield return statement.EmptyNull();
                }
            }
        }

        protected virtual StreamReader ReadSqlFile()
        {

            var fileName = this.FileName;

            if (fileName.StartsWith("~") || fileName.StartsWith("/"))
            {
                string path = CommonHelper.MapPath(fileName);
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException("Sql file '{0}' not found".FormatInvariant(this.FileName));
                }

                return new StreamReader(File.OpenRead(path));
            }

            // SQL file is obviously an embedded resource
            var assembly = this.Assembly;
            var asmName = assembly.FullName.Substring(0, assembly.FullName.IndexOf(','));
            var location = this.Location ?? asmName + ".Sql";
            var name = String.Format("{0}.{1}", location, fileName);

            try
            {
                var stream = assembly.GetManifestResourceStream(name);
                return new StreamReader(stream);
            }
            catch (Exception ex)
            {
                throw new FileLoadException("Error while loading embedded sql resource '{0}'".FormatInvariant(name), ex);
            }
        }

        private string ReadNextSqlStatement(TextReader reader)
        {
            var psb = PooledStringBuilder.Rent();
            var sb = (StringBuilder)psb;

            string lineOfText;

            while (true)
            {
                lineOfText = reader.ReadLine();
                if (lineOfText == null)
                {
                    if (sb.Length > 0)
                        return sb.ToString();
                    else
                        return null;
                }

                if (lineOfText.TrimEnd().ToUpper() == "GO")
                    break;

                sb.Append(lineOfText + Environment.NewLine);
            }

            return psb.ToStringAndReturn();
        }
    }
}
