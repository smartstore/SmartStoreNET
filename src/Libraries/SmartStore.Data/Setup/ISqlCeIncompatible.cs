using System;

namespace SmartStore.Data.Setup
{
	/// <summary>
	/// Marker interface implemented by migration classes to indicate that
	/// it contains seeds incompatible with Sql Server Compact.
	/// When any pending migration implements this interface and the current data provider
	/// is 'sqlce', the affected migrations will not be executed and an exception will be thrown.
	/// </summary>
	public interface ISqlCeIncompatible
	{
	}
}
