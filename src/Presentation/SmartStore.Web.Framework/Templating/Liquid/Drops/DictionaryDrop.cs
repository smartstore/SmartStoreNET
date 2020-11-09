using System.Collections.Generic;

namespace SmartStore.Templating.Liquid
{
    internal class DictionaryDrop : SafeDropBase
    {
        private readonly IDictionary<string, object> _inner;

        public DictionaryDrop(IDictionary<string, object> data)
        {
            Guard.NotNull(data, nameof(data));

            _inner = data;
        }

        public override bool ContainsKey(object key)
        {
            return (key is string s)
                ? _inner.ContainsKey(s)
                : false;
        }

        protected override object InvokeMember(string name)
        {
            return _inner.Get(name);
        }

        public override object GetWrappedObject()
        {
            return _inner;
        }
    }
}
