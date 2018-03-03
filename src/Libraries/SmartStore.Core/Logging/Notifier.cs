using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartStore.Core.Localization;

namespace SmartStore.Core.Logging
{
	public interface INotifier
	{
		void Add(NotifyType type, LocalizedString message, bool durable = true);
		ICollection<NotifyEntry> Entries { get; }
	}
	
	public class Notifier : INotifier
	{
		private readonly HashSet<NotifyEntry> _entries = new HashSet<NotifyEntry>();

		public void Add(NotifyType type, LocalizedString message, bool durable = true)
		{
			_entries.Add(new NotifyEntry { Type = type, Message = message, Durable = durable });
		}

		public ICollection<NotifyEntry> Entries
		{
			get { return _entries; }
		}
	}

	public static class INotifierExtension
	{
		public static void Information(this INotifier notifier, LocalizedString message, bool durable = true)
		{
			notifier.Add(NotifyType.Info, message, durable);
		}

		public static void Success(this INotifier notifier, LocalizedString message, bool durable = true)
		{
			notifier.Add(NotifyType.Success, message, durable);
		}

		public static void Warning(this INotifier notifier, LocalizedString message, bool durable = true)
		{
			notifier.Add(NotifyType.Warning, message, durable);
		}

		public static void Error(this INotifier notifier, LocalizedString message, bool durable = true)
		{
			notifier.Add(NotifyType.Error, message, durable);
		}
	}
}
