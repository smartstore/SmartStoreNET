using System;

namespace SmartStore.Core.Search
{
	public interface ISearchBits
	{
		ISearchBits And(ISearchBits other);
		ISearchBits Or(ISearchBits other);
		ISearchBits Xor(ISearchBits other);
		long Count();
	}
}
