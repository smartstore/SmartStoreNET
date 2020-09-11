using System.Net.Http.Formatting;
using System.Reflection;
using Newtonsoft.Json.Serialization;
using SmartStore.ComponentModel;

namespace SmartStore.Web.Framework.WebApi
{
    internal partial class WebApiContractResolver : JsonContractResolver
    {
        public WebApiContractResolver(MediaTypeFormatter formatter)
            : base(formatter)
        {
        }

        protected override IValueProvider CreateMemberValueProvider(MemberInfo member)
        {
            if (member is PropertyInfo pi)
            {
                return new FastPropertyValueProvider(pi);
            }

            return base.CreateMemberValueProvider(member);
        }
    }
}
