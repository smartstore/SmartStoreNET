using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Core.Tests.Fakes
{
    public class FakeTypeFinder : ITypeFinder
    {
        public Assembly[] Assemblies { get; set; }
        public Type[] Types { get; set; }

        public FakeTypeFinder(Assembly assembly, params Type[] types)
        {
            Assemblies = new[] { assembly };
            this.Types = types;
        }
        public FakeTypeFinder(params Type[] types)
        {
            Assemblies = new Assembly[0];
            this.Types = types;
        }
        public FakeTypeFinder(params Assembly[] assemblies)
        {
            this.Assemblies = assemblies;
        }

        public IEnumerable<Assembly> GetAssemblies(bool ignoreInactivePlugins = false)
        {
            return Assemblies;
        }

        public IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true)
        {
            return (from t in Types
                    where !t.IsInterface && assignTypeFrom.IsAssignableFrom(t) && (onlyConcreteClasses ? (t.IsClass && !t.IsAbstract) : true)
                    select t).ToList();
        }

    }
}
