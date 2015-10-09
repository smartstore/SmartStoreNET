using System;

namespace SmartStore
{
	public interface ITransient
	{
		bool IsTransient { get; set; }
	}
}
