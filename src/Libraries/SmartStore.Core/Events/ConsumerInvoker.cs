using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using SmartStore.ComponentModel;
using SmartStore.Core.Async;
using SmartStore.Core.Logging;

namespace SmartStore.Core.Events
{
    public class ConsumerInvoker : IConsumerInvoker
    {
        private readonly IConsumerResolver _resolver;

        public ConsumerInvoker(IConsumerResolver resolver)
        {
            _resolver = resolver;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Invoke<TMessage>(ConsumerDescriptor descriptor, IConsumer consumer, ConsumeContext<TMessage> envelope) where TMessage : class
        {
            var d = descriptor;
            var p = descriptor.WithEnvelope ? (object)envelope : envelope.Message;
            var fastInvoker = FastInvoker.GetInvoker(d.Method);

            if (!d.FireForget && !d.IsAsync)
            {
                // The all synch case
                try
                {
                    InvokeCore();
                }
                catch (Exception ex)
                {
                    HandleException(ex, d);
                }
            }
            else if (!d.FireForget && d.IsAsync)
            {
                //// The awaitable Task case
                //BeginInvoke((Task)InvokeCore(cancelToken: AsyncRunner.AppShutdownCancellationToken), EndInvoke, d);

                // For now we must go with the AsyncRunner, the above call to BeginInvoke (APM > TPL) does not always
                // guarantee that the task is awaited and throws exceptions especially when EF is involved.
                using (var runner = AsyncRunner.Create())
                {
                    try
                    {
                        runner.Run((Task)InvokeCore(cancelToken: AsyncRunner.AppShutdownCancellationToken));
                    }
                    catch (Exception ex)
                    {
                        HandleException(ex, d);
                    }
                }
            }
            else if (d.FireForget && !d.IsAsync)
            {
                // A synch method should be executed async (without awaiting)
                AsyncRunner.Run((c, ct, state) => InvokeCore(c, ct), d)
                    .ContinueWith(t => EndInvoke(t), /*TaskContinuationOptions.OnlyOnFaulted*/ TaskContinuationOptions.None)
                    .ConfigureAwait(false);
            }
            else if (d.FireForget && d.IsAsync)
            {
                // An async (Task) method should be executed without awaiting
                AsyncRunner.Run((c, ct) => (Task)InvokeCore(c, ct), d)
                    .ContinueWith(t => EndInvoke(t), TaskContinuationOptions.OnlyOnFaulted)
                    .ConfigureAwait(false);
            }

            object InvokeCore(IComponentContext c = null, CancellationToken cancelToken = default(CancellationToken))
            {
                if (d.Parameters.Length == 0)
                {
                    // Only one method param: the message!
                    return fastInvoker.Invoke(consumer, p);
                }

                var parameters = new object[d.Parameters.Length + 1];
                parameters[0] = p;

                int i = 0;
                foreach (var obj in ResolveParameters(c, d, cancelToken).ToArray())
                {
                    i++;
                    parameters[i] = obj;
                }

                return fastInvoker.Invoke(consumer, parameters);
            }
        }

        private void HandleException(Exception ex, ConsumerDescriptor descriptor)
        {
            if (ex is AggregateException ae)
            {
                ae.InnerExceptions.Each(x => Logger.Error(x, $"Error invoking event consumer '{descriptor}'."));
            }
            else
            {
                Logger.Error(ex, $"Error invoking event consumer '{descriptor}'.");
            }

            if (!descriptor.FireForget)
            {
                throw ex;
            }
        }

        protected internal virtual IEnumerable<object> ResolveParameters(
            IComponentContext container,
            ConsumerDescriptor descriptor,
            CancellationToken cancelToken)
        {
            foreach (var p in descriptor.Parameters)
            {
                if (p.ParameterType == typeof(CancellationToken))
                {
                    yield return cancelToken;
                }
                else
                {
                    yield return _resolver.ResolveParameter(p, container);
                }
            }
        }

        #region APM > TPL pattern (does not always work stable)

        /// <summary>
        /// Wraps a <see cref="Task"/> into the Begin method of an APM pattern.
        /// </summary>
        /// <param name="task">The task to wrap.</param>
        /// <param name="callback">The callback method passed into the Begin method of the APM pattern.</param>
        /// <param name="descriptor">The state passed into the Begin method of the APM pattern.</param>
        /// <returns>The asynchronous operation, to be returned by the Begin method of the APM pattern.</returns>
        protected IAsyncResult BeginInvoke(Task task, AsyncCallback callback, ConsumerDescriptor descriptor)
        {
            var options = TaskCreationOptions.RunContinuationsAsynchronously;
            var tcs = new TaskCompletionSource<object>(descriptor, options);

            // "_ =" to discard 'async/await' compiler warning
            _ = AwaitCompletionAsync(task, callback, tcs, descriptor);

            return tcs.Task;
        }

        private async Task AwaitCompletionAsync(
            Task task,
            AsyncCallback callback,
            TaskCompletionSource<object> tcs,
            ConsumerDescriptor descriptor)
        {
            try
            {
                await task;
                tcs.TrySetResult(null);
            }
            catch (OperationCanceledException ex)
            {
                tcs.TrySetCanceled(ex.CancellationToken);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
            finally
            {
                callback?.Invoke(tcs.Task);
            }
        }

        protected virtual void EndInvoke(IAsyncResult asyncResult)
        {
            var task = (Task)asyncResult;

            if (task.IsFaulted && task.Exception != null)
            {
                HandleException(task.Exception, (ConsumerDescriptor)asyncResult.AsyncState);
            }
        }

        #endregion
    }
}
