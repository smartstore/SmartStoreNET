using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace SmartStore
{
    public abstract class DisposableObject : IDisposable
    {
        private bool _isDisposed;

        public virtual bool IsDisposed
        {
            [DebuggerStepThrough]
            get => _isDisposed;
        }

        [DebuggerStepThrough]
        protected void CheckDisposed()
        {
            if (_isDisposed)
            {
                throw Error.ObjectDisposed(GetType().FullName);
            }
        }

        [DebuggerStepThrough]
        protected void CheckDisposed(string errorMessage)
        {
            if (_isDisposed)
            {
                throw Error.ObjectDisposed(GetType().FullName, errorMessage);
            }
        }

        [DebuggerStepThrough]
        public void Dispose()
        {
            if (!_isDisposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        protected void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                OnDispose(disposing);
            }
            _isDisposed = true;
        }

        protected abstract void OnDispose(bool disposing);

        protected static void DisposeEnumerable(IEnumerable enumerable)
        {
            if (enumerable != null)
            {
                foreach (object obj2 in enumerable)
                {
                    DisposeMember(obj2);
                }
                DisposeMember(enumerable);
            }
        }

        protected static void DisposeDictionary<K, V>(IDictionary<K, V> dictionary)
        {
            if (dictionary != null)
            {
                foreach (KeyValuePair<K, V> pair in dictionary)
                {
                    DisposeMember(pair.Value);
                }
                DisposeMember(dictionary);
            }
        }

        protected static void DisposeDictionary(IDictionary dictionary)
        {
            if (dictionary != null)
            {
                foreach (IDictionaryEnumerator pair in dictionary)
                {
                    DisposeMember(pair.Value);
                }
                DisposeMember(dictionary);
            }
        }

        protected static void DisposeMember(object member)
        {
            IDisposable disposable = member as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        ~DisposableObject()
        {
            Dispose(false);
        }
    }
}
