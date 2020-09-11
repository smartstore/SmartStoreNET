using System;
using System.Security.Principal;

namespace SmartStore.Core.Fakes
{
    public class FakeIdentity : IIdentity
    {
        private readonly string _name;

        public FakeIdentity(string userName)
        {
            _name = userName;
        }

        public string AuthenticationType => throw new NotImplementedException();

        public bool IsAuthenticated => !String.IsNullOrEmpty(_name);

        public string Name => _name;

    }
}