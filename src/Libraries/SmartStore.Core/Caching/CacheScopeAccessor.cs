using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Utilities;

namespace SmartStore.Core.Caching
{
    public class CacheScopeAccessor : ICacheScopeAccessor
    {
        private readonly Stack<CacheScope> _stack = new Stack<CacheScope>();

        public CacheScopeAccessor()
        {
        }

        public CacheScope Current
        {
            get
            {
                _stack.TryPeek(out var result);
                return result;
            }
        }

        public bool HasScope(string key)
        {
            return _stack.Any(x => x.Key.IsCaseInsensitiveEqual(key));
        }

        public void PropagateKey(string key)
        {
            var current = Current;
            if (current != null)
            {
                current.AddDependency(key);
            }
        }

        public IDisposable BeginScope(string key, bool independent = false)
        {
            if (Current?.Key == key)
            {
                // Void if parent scope has the same key.
                return ActionDisposable.Empty;
            }

            _stack.Push(new CacheScope(key));

            var action = new ActionDisposable(() =>
            {
                _stack.TryPop(out _);

                if (!independent)
                {
                    PropagateKey(key);
                }
            });

            return action;
        }
    }
}
