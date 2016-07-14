
using System;

namespace SmartStore.Core.IO
{
    public interface IFolder 
    {
        string Path { get; }
        string Name { get; }
		long Size { get; }
		DateTime LastUpdated { get; }
		IFolder Parent { get; }
	}
}