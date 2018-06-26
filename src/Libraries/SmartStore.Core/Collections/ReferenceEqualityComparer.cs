using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SmartStore.ComponentModel
{
	public sealed class ReferenceEqualityComparer : IEqualityComparer, IEqualityComparer<object>
	{
		public static ReferenceEqualityComparer Default { get; } = new ReferenceEqualityComparer();

		public new bool Equals(object x, object y) => ReferenceEquals(x, y);
		public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
	}
}
