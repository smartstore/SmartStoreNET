using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Infrastructure;

namespace SmartStore.ComponentModel
{
    public static class MapperFactory
    {
        private static IDictionary<TypePair, Type> _mapperTypes = null;
        private readonly static object _lock = new object();
        
        private static void EnsureInitialized()
        {
            if (_mapperTypes == null)
            {
                lock (_lock)
                {
                    if (_mapperTypes == null)
                    {
                        _mapperTypes = new Dictionary<TypePair, Type>();

                        var typeFinder = EngineContext.Current.Resolve<ITypeFinder>();
                        var mapperTypes = typeFinder.FindClassesOfType(typeof(IMapper<,>));
                        
                        foreach (var type in mapperTypes)
                        {
                            foreach (var intf in type.GetInterfaces())
                            {
                                intf.IsSubClass(typeof(IMapper<,>), out var impl);
                                var genericArguments = impl.GetGenericArguments();
                                var typePair = new TypePair(genericArguments[0], genericArguments[1]);
                                _mapperTypes.Add(typePair, type);
                            }
                        }
                    }
                }
            }
        }

        public static IMapper<TFrom, TTo> GetMapper<TFrom, TTo>()
        {
            EnsureInitialized();

            var key = new TypePair(typeof(TFrom), typeof(TTo));

            var implType = _mapperTypes.Get(key);
            if (implType != null)
            {
                var instance = EngineContext.Current.ContainerManager.ResolveUnregistered(implType);
                return (IMapper<TFrom, TTo>)instance;
            }

            return null;
        }

        class TypePair : Tuple<Type, Type>
        {
            public TypePair(Type fromType, Type toType)
                : base(fromType, toType)
            {
            }

            public Type FromType { get => base.Item1; }
            public Type ToType { get => base.Item2; }
        }
    }
}
