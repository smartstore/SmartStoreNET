using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

        public ICollection<NotifyEntry> Entries => _entries;
    }

    public static class INotifierExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Information(this INotifier notifier, LocalizedString message, bool durable = true)
        {
            notifier.Add(NotifyType.Info, message, durable);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Success(this INotifier notifier, LocalizedString message, bool durable = true)
        {
            notifier.Add(NotifyType.Success, message, durable);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(this INotifier notifier, LocalizedString message, bool durable = true)
        {
            notifier.Add(NotifyType.Warning, message, durable);
        }

        public static void Error(this INotifier notifier, Exception exception, bool durable = true)
        {
            if (exception == null)
                return;

            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
            }

            notifier.Add(NotifyType.Error, exception.Message, durable);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(this INotifier notifier, LocalizedString message, bool durable = true)
        {
            notifier.Add(NotifyType.Error, message, durable);
        }
    }
}
