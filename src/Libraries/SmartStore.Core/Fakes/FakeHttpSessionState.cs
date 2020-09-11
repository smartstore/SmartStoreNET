using System.Collections;
using System.Collections.Specialized;
using System.Web;
using System.Web.SessionState;

namespace SmartStore.Core.Fakes
{
    public class FakeHttpSessionState : HttpSessionStateBase
    {
        private readonly SessionStateItemCollection _sessionItems;

        public FakeHttpSessionState(SessionStateItemCollection sessionItems)
        {
            _sessionItems = sessionItems;
        }

        public override int Count => _sessionItems.Count;

        public override NameObjectCollectionBase.KeysCollection Keys => _sessionItems.Keys;

        public override object this[string name]
        {
            get => _sessionItems[name];
            set => _sessionItems[name] = value;
        }

        public bool Exists(string key)
        {
            return _sessionItems[key] != null;
        }

        public override object this[int index]
        {
            get => _sessionItems[index];
            set => _sessionItems[index] = value;
        }

        public override void Add(string name, object value)
        {
            _sessionItems[name] = value;
        }

        public override IEnumerator GetEnumerator()
        {
            return _sessionItems.GetEnumerator();
        }

        public override void Remove(string name)
        {
            _sessionItems.Remove(name);
        }
    }
}