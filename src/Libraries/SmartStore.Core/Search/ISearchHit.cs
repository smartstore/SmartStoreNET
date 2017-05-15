using System;
using System.Collections.Generic;

namespace SmartStore.Core.Search
{
	public interface ISearchHit
	{
		int EntityId { get; }
		float Score { get; }

		int GetInt(string name);
		double GetDouble(string name);
		bool GetBoolean(string name);
		string GetString(string name);
		string GetString(string name, string languageSeoCode);
		DateTime GetDateTime(string name);

		IEnumerable<string> GetStoredFieldNames();
	}
}
