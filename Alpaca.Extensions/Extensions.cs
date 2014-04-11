using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Alpaca
{
    public static class Extensions
    {
        public static Task<T> TimeoutAfter<T>(this Task<T> task, int millisecondsTimeout)
        {
            if (task.IsCompleted || millisecondsTimeout == Timeout.Infinite)
            {
                return task;
            }

            var tcs = new TaskCompletionSource<T>();

            if (millisecondsTimeout == 0)
            {
                tcs.SetException(new TimeoutException());
                return tcs.Task;
            }

            var timer = new Timer(state =>
            {
                tcs.TrySetException(new TimeoutException());
            }, null, millisecondsTimeout, Timeout.Infinite);

            task.ContinueWith(source =>
            {
                timer.Dispose();
                switch (source.Status)
                {
                    case TaskStatus.Faulted:
                        tcs.TrySetException(source.Exception);
                        break;
                    case TaskStatus.Canceled:
                        tcs.TrySetCanceled();
                        break;
                    case TaskStatus.RanToCompletion:
                        tcs.TrySetResult(source == null ? default(T) : source.Result);
                        break;
                }
            });

            return tcs.Task;
        }
        public static Task TimeoutAfter(this Task task, int millisecondsTimeout)
        {
            if (task.IsCompleted || millisecondsTimeout == Timeout.Infinite)
            {
                return task;
            }

            var tcs = new TaskCompletionSource<object>();

            if (millisecondsTimeout == 0)
            {
                tcs.SetException(new TimeoutException());
                return tcs.Task;
            }

            var timer = new Timer(state =>
            {
                tcs.TrySetException(new TimeoutException());
            }, null, millisecondsTimeout, Timeout.Infinite);

            task.ContinueWith(source =>
            {
                timer.Dispose();
                switch (source.Status)
                {
                    case TaskStatus.Faulted:
                        tcs.TrySetException(source.Exception);
                        break;
                    case TaskStatus.Canceled:
                        tcs.TrySetCanceled();
                        break;
                    case TaskStatus.RanToCompletion:
                        tcs.TrySetResult(null);
                        break;
                }
            });

            return tcs.Task;
        }
        public static void SetResultAsync<T>(this TaskCompletionSource<T> tcs, T data)
        {
            Task.Run(() =>
            {
                tcs.SetResult(data);
            }).ConfigureAwait(false);
        }
        public static string ToShortGuid(this Guid id)
        {
            return Convert.ToBase64String(id.ToByteArray())
            .Replace('/', '_')
            .Replace('+', '-')
            .Replace("=", "");
        }
    }
}
