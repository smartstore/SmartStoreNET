using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SmartStore.Core
{
    public static class ProcessExtensions
    {
        public static async Task<int> RunAsync(this Process process, CancellationToken cancellationToken = default(CancellationToken))
        {
            Guard.NotNull(process, nameof(process));

            process.Start();
            return await WaitForExitAsync(process, cancellationToken);
        }

        public static async Task<int> WaitForExitAsync(this Process process, CancellationToken cancellationToken = default(CancellationToken))
        {
            Guard.NotNull(process, nameof(process));

            process.EnableRaisingEvents = true;

            var tcs = new TaskCompletionSource<int>();

            process.Exited += (sender, args) =>
            {
                tcs.TrySetResult(process.ExitCode);
            };

            if (process.HasExited)
            {
                return process.ExitCode;
            }

            using (var registration = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken)))
            {
                return await tcs.Task.ConfigureAwait(false);
            }
        }
    }
}
