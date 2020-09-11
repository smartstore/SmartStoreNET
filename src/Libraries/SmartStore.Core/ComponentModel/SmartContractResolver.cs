using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace SmartStore.ComponentModel
{
    public class SmartContractResolver : DefaultContractResolver
    {
        protected override IValueProvider CreateMemberValueProvider(MemberInfo member)
        {
            if (member is PropertyInfo pi)
            {
                return new FastPropertyValueProvider(pi);
            }

            return base.CreateMemberValueProvider(member);
        }

        public static SmartContractResolver Instance { get; } = new SmartContractResolver();
    }
}
