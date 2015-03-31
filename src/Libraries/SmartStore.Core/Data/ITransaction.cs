using System;

namespace SmartStore.Core.Data
{
	public interface ITransaction : IDisposable
	{
		void Commit();
		void Rollback();
	}
}
