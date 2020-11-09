using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace SmartStore.Utilities.ObjectPools
{
    public sealed class PooledStringBuilder : IPooledObject
    {
        public readonly StringBuilder Builder;
        private readonly ObjectPool<PooledStringBuilder> _pool;

        private PooledStringBuilder(ObjectPool<PooledStringBuilder> pool, int? capacity, int? maxCapacity)
        {
            Debug.Assert(pool != null);

            InitialCapacity = capacity ?? DefaultInitialCapacity;
            MaxCapacity = maxCapacity ?? DefaultMaxCapacity;
            Builder = new StringBuilder(InitialCapacity);

            _pool = pool;
        }

        #region Static

        /// <summary>
        /// The default initial capacity of builder.
        /// </summary>
        public const int DefaultInitialCapacity = 100;

        /// <summary>
        /// The default maximum capacity of builder.
        /// </summary>
        public const int DefaultMaxCapacity = 4 * 1024;

        private static readonly ObjectPool<PooledStringBuilder> _defaultPool = CreatePool();

        public static ObjectPool<PooledStringBuilder> CreatePool(int? capacity = null, int? maxCapacity = null, int? poolSize = null)
        {
            ObjectPool<PooledStringBuilder> pool = null;
            pool = new ObjectPool<PooledStringBuilder>(() => new PooledStringBuilder(pool, capacity, maxCapacity), poolSize);
            return pool;
        }

        public static PooledStringBuilder Rent(string value = null)
        {
            var builder = _defaultPool.Rent();
            Debug.Assert(builder.Builder.Length == 0);

            if (!string.IsNullOrEmpty(value))
            {
                builder.Builder.Append(value);
            }

            return builder;
        }

        #endregion

        #region Instance

        public int InitialCapacity { get; private set; }

        public int MaxCapacity { get; private set; }

        public int Length => this.Builder.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringBuilder Append(string value)
        {
            return this.Builder.Append(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringBuilder Append(object value)
        {
            return this.Builder.Append(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringBuilder AppendLine()
        {
            return this.Builder.AppendLine();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringBuilder AppendFormat(string format, object arg0)
        {
            return this.Builder.AppendFormat(format, arg0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringBuilder AppendFormat(string format, params object[] args)
        {
            return this.Builder.AppendFormat(format, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringBuilder AppendLine(string value)
        {
            return this.Builder.AppendLine(value);
        }

        public bool Return()
        {
            var builder = this.Builder;

            // Do not return builders that are too large.
            if (builder.Capacity <= MaxCapacity)
            {
                builder.Clear();
                _pool.Return(this);
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return this.Builder.ToString();
        }

        public string ToStringAndReturn()
        {
            var result = this.Builder.ToString();
            this.Return();

            return result;
        }

        public string ToStringAndReturn(int startIndex, int length)
        {
            var result = this.Builder.ToString(startIndex, length);
            this.Return();

            return result;
        }

        public static implicit operator StringBuilder(PooledStringBuilder obj)
        {
            return obj.Builder;
        }

        #endregion
    }
}
