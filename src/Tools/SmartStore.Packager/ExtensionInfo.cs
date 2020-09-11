using System;

namespace SmartStore.Packager
{
    public class ExtensionInfo : Tuple<string, string>
    {
        public ExtensionInfo(string path, string name) : base(path, name)
        {
        }

        public string Path => base.Item1;
        public string Name => base.Item2;
    }
}
