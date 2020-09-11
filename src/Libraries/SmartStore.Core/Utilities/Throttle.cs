using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace SmartStore.Core.Utilities
{
    public static class Throttle
    {
        private readonly static ConcurrentDictionary<string, CheckEntry> _checks = new ConcurrentDictionary<string, CheckEntry>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Performs a throttled check.
        /// </summary>
        /// <param name="key">Identifier for the check process</param>
        /// <param name="interval">Interval between actual checks</param>
        /// <param name="check">The check factory</param>
        /// <returns>Check result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Check(string key, TimeSpan interval, Func<bool> check)
        {
            return Check(key, interval, false, check);
        }

        /// <summary>
        /// Performs a throttled check.
        /// </summary>
        /// <param name="key">Identifier for the check process</param>
        /// <param name="interval">Interval between actual checks</param>
        /// <param name="recheckWhenFalse"></param>
        /// <param name="check">The check factory</param>
        /// <returns>Check result</returns>
        public static bool Check(string key, TimeSpan interval, bool recheckWhenFalse, Func<bool> check)
        {
            Guard.NotEmpty(key, nameof(key));
            Guard.NotNull(check, nameof(check));

            bool added = false;
            var now = DateTime.UtcNow;

            var entry = _checks.GetOrAdd(key, x =>
            {
                added = true;
                return new CheckEntry { Value = check(), NextCheckUtc = (now + interval) };
            });

            if (added)
            {
                return entry.Value;
            }

            var ok = entry.Value;
            var isOverdue = (!ok && recheckWhenFalse) || (now > entry.NextCheckUtc);

            if (isOverdue)
            {
                // Check is overdue: recheck
                ok = check();
                _checks.TryUpdate(key, new CheckEntry { Value = ok, NextCheckUtc = (now + interval) }, entry);
            }

            return ok;
        }

        class CheckEntry
        {
            public bool Value { get; set; }
            public DateTime NextCheckUtc { get; set; }
        }
    }
}
