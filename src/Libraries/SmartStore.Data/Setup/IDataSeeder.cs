using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace SmartStore.Data.Setup
{
	public interface IDataSeeder<TContext> where TContext : DbContext
	{
		/// <summary>
		/// Seeds data
		/// </summary>
		void Seed(TContext context);
	}
}
