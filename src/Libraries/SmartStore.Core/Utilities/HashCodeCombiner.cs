using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;

/*
	Copied over from Microsoft.Framework.Internal
*/

namespace SmartStore.Utilities
{
	public struct HashCodeCombiner
	{
		private long _combinedHash64;

		public int CombinedHash
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get { return _combinedHash64.GetHashCode(); }
		}

		public string CombinedHashString
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get { return _combinedHash64.GetHashCode().ToString("x", CultureInfo.InvariantCulture); }
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private HashCodeCombiner(long seed)
		{
			_combinedHash64 = seed;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashCodeCombiner Add(IEnumerable e)
		{
			if (e == null)
			{
				Add(0);
			}
			else
			{
				var count = 0;
				foreach (object o in e)
				{
					Add(o);
					count++;
				}
				Add(count);
			}
			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator int (HashCodeCombiner self)
		{
			return self.CombinedHash;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashCodeCombiner Add(int i)
		{
			_combinedHash64 = ((_combinedHash64 << 5) + _combinedHash64) ^ i;
			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashCodeCombiner Add(string s)
		{
			var hashCode = (s != null) ? s.GetHashCode() : 0;
			return Add(hashCode);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashCodeCombiner Add(object o)
		{
			var hashCode = (o != null) ? o.GetHashCode() : 0;
			return Add(hashCode);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashCodeCombiner Add(DateTime d)
		{
			return Add(d.GetHashCode());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashCodeCombiner Add<TValue>(TValue value, IEqualityComparer<TValue> comparer)
		{
			var hashCode = value != null ? comparer.GetHashCode(value) : 0;
			return Add(hashCode);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashCodeCombiner Add(FileSystemInfo fi, bool deep = true)
		{
			Guard.NotNull(fi, nameof(fi));

			if (!fi.Exists)
				return this;

			Add(fi.FullName.ToLower());
			Add(fi.CreationTimeUtc);
			Add(fi.LastWriteTimeUtc);

			var file = fi as FileInfo;
			if (file != null)
			{
				Add(file.Length.GetHashCode());
			}

			var dir = fi as DirectoryInfo;
			if (dir != null)
			{
				foreach (var f in dir.GetFiles())
				{
					Add(f);
				}
				if (deep)
				{
					foreach (var s in dir.GetDirectories())
					{
						Add(s);
					}
				}
			}

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static HashCodeCombiner Start()
		{
			return new HashCodeCombiner(0x1505L);
		}
	}
}
