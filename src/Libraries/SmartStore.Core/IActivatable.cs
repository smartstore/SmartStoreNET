using System;

namespace SmartStore.Core
{
	public interface IActivatable
	{
		bool IsActive { get; }
	}
}
