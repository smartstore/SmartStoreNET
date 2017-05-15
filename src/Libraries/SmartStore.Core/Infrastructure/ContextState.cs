using System;
using System.Runtime.Remoting.Messaging;
using System.Web;

namespace SmartStore.Core.Infrastructure
{
	/// <summary>
	/// Holds some state for the current HttpContext or thread
	/// </summary>
	/// <typeparam name="T">The type of data to store</typeparam>
	public class ContextState<T> where T : class
	{
		private readonly string _name;
		private readonly Func<T> _defaultValue;

		public ContextState(string name)
		{
			_name = name;
		}

		public ContextState(string name, Func<T> defaultValue)
		{
			_name = name;
			_defaultValue = defaultValue;
		}

		public T GetState()
		{
			var key = BuildKey();

			if (HttpContext.Current == null)
			{
				var data = CallContext.GetData(key);
				
				if (data == null)
				{
					if (_defaultValue != null)
					{
						CallContext.SetData(key, data = _defaultValue());
						return data as T;
					}
				}
				
				return data as T;
			}

			if (HttpContext.Current.Items[key] == null)
			{
				HttpContext.Current.Items[key] = _defaultValue?.Invoke();
			}

			return HttpContext.Current.Items[key] as T;
		}

		public void SetState(T state)
		{
			if (HttpContext.Current == null)
			{
				CallContext.SetData(BuildKey(), state);
			}
			else
			{
				HttpContext.Current.Items[BuildKey()] = state;
			}
		}

		public void RemoveState()
		{
			var key = BuildKey();

			if (HttpContext.Current == null)
			{
				CallContext.FreeNamedDataSlot(key);
			}
			else
			{
				if (HttpContext.Current.Items.Contains(key)) 
				{
					HttpContext.Current.Items.Remove(key);
				}
			}
		}

		private string BuildKey()
		{
			return "__ContextState." + _name;
		}
	}
}
