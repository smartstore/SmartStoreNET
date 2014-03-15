using System;

namespace SmartStore.Services.Events
{
	[AttributeUsage(AttributeTargets.Class, Inherited=true, AllowMultiple=false)]
	public class AsyncConsumerAttribute : Attribute
	{
	}
}
