using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web.Hosting;
using SmartStore.Core.Data;
using SmartStore.Utilities;

namespace SmartStore.Core.IO
{
    public class DirectoryHasher
    {
        private static readonly string _appRootPath;
        private static readonly string _defaultStoragePath;

        private readonly string _path;
        private readonly string _searchPattern;
        private readonly bool _deep;
        private readonly string _storagePath;

        private int? _lastHash;
        private int? _currentHash;
        private string _lookupKey;

        static DirectoryHasher()
        {
            _appRootPath = HostingEnvironment.ApplicationPhysicalPath;
            _defaultStoragePath = Path.Combine(CommonHelper.MapPath(DataSettings.Current.TenantPath), "Hash");
            Directory.CreateDirectory(_defaultStoragePath);
        }

        public DirectoryHasher(string path)
            : this(path, "*", false, null)
        {
        }

        public DirectoryHasher(string path, string searchPattern, bool deep = false, string storagePath = null)
        {
            Guard.NotEmpty(path, nameof(path));

            if (!path.StartsWith(_appRootPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Path must be contained within current app root path '{_appRootPath}'", nameof(path));
            }

            _path = path;
            _searchPattern = searchPattern ?? "*";
            _deep = deep;
            _storagePath = storagePath ?? _defaultStoragePath;
        }

        public bool HasChanged => LastHash != CurrentHash;

        public int? LastHash
        {
            get
            {
                if (_lastHash == null)
                {
                    _lastHash = ReadLastHash();
                }

                return _lastHash == -1 ? (int?)null : _lastHash.Value;
            }
        }

        public int CurrentHash => (_currentHash ?? (_currentHash = ComputeHash())).Value;

        public string LookupKey => _lookupKey ?? (_lookupKey = BuildLookupKey());

        public void Refresh()
        {
            _currentHash = null;
        }

        //public void Reset()
        //{
        //    _lastHash = -1;

        //    var path = Path.Combine(_storagePath, LookupKey + ".hash");
        //    if (File.Exists(path))
        //    {
        //        File.Delete(path);
        //    }
        //}

        public void Persist()
        {
            if (LastHash == CurrentHash)
                return;

            var path = Path.Combine(_storagePath, LookupKey + ".hash");
            File.WriteAllText(path, CurrentHash.ToString(CultureInfo.InvariantCulture), Encoding.UTF8);
            _lastHash = CurrentHash;
        }

        protected virtual int ComputeHash()
        {
            var hash = 0;
            var di = new DirectoryInfo(_path);

            if (di.Exists)
            {
                var files = di.GetFiles(_searchPattern, _deep ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                if (files.Length > 0)
                {
                    // Calculate current hash
                    var hashCombiner = new HashCodeCombiner();
                    foreach (var file in files)
                    {
                        hashCombiner.Add(file);
                    }

                    hash = hashCombiner.CombinedHash;
                }
            }

            return hash;
        }

        protected virtual int ReadLastHash()
        {
            var path = Path.Combine(_storagePath, LookupKey + ".hash");
            var hash = File.Exists(path)
                ? ConvertHash(File.ReadAllText(path, Encoding.UTF8))
                : -1;

            return hash;
        }

        protected virtual string BuildLookupKey()
        {
            var key = PathHelper.MakeRelativePath(_appRootPath, _path, "_").ToLower();

            if (_deep)
                key += "_d";

            if (_searchPattern.HasValue() && _searchPattern != "*")
                key += "_" + _searchPattern.ToLower().ToValidFileName("x");

            return key.Trim(new char[] { '/', '\\' });
        }

        private static int ConvertHash(string val)
        {
            if (Int32.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out var outVal))
            {
                return outVal;
            }

            return 0;
        }
    }
}
