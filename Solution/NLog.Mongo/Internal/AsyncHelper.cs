namespace NLog.Mongo.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    /// <summary>
    /// Хелпер для выполнения асинхронных задач синхронно
    /// </summary>
    internal static class AsyncHelper
    {
        /// <summary>
        ///     Выполняет задачу синхронно
        /// </summary>
        /// <param name="task">Делегат, возвращающий задачу</param>
        public static void RunSync([NotNull]Func<Task> task)
        {
            var oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            try
            {
                SynchronizationContext.SetSynchronizationContext(synch);
                synch.Post(async _ =>
                {
                    try
                    {
                        var running = task();
                        if (running == null)
                        {
                            throw new NullReferenceException("task");
                        }
                        await running;
                    }
                    catch (Exception e)
                    {
                        synch.InnerException = e;
                        throw;
                    }
                    finally
                    {
                        synch.EndMessageLoop();
                    }
                }, null);
                synch.BeginMessageLoop();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(oldContext);
            }
        }


        /// <summary>
        ///     Выполняет задачу синхронно
        /// </summary>
        /// <typeparam name="T">Тип объекта, который возвращает задача</typeparam>
        /// <param name="task">задача</param>
        /// <returns>Объект, который вернула задача</returns>
        public static T RunSync<T>([NotNull]Func<Task<T>> task)
        {
            var oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            try
            {
                SynchronizationContext.SetSynchronizationContext(synch);
                var ret = default(T);
                synch.Post(async _ =>
                {
                    try
                    {
                        var running = task();
                        if (running == null)
                        {
                            throw new NullReferenceException("task");
                        }
                        ret = await running;
                    }
                    catch (Exception e)
                    {
                        synch.InnerException = e;
                        throw;
                    }
                    finally
                    {
                        synch.EndMessageLoop();
                    }
                }, null);
                synch.BeginMessageLoop();
                return ret;
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(oldContext);
            }
        }

        private class ExclusiveSynchronizationContext : SynchronizationContext
        {
            [NotNull]
            private readonly Queue<Tuple<SendOrPostCallback, object>> _items =
                    new Queue<Tuple<SendOrPostCallback, object>>();

            [NotNull]
            private readonly AutoResetEvent _workItemsWaiting = new AutoResetEvent(false);

            private bool _done;
            public Exception InnerException { private get; set; }

            public override void Send(SendOrPostCallback d, object state)
            {
                throw new NotSupportedException("We cannot send to our same thread");
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                lock (_items)
                {
                    _items.Enqueue(Tuple.Create(d, state));
                }
                _workItemsWaiting.Set();
            }

            public void EndMessageLoop()
            {
                Post(_ => _done = true, null);
            }

            public void BeginMessageLoop()
            {
                while (!_done)
                {
                    Tuple<SendOrPostCallback, object> task = null;
                    lock (_items)
                    {
                        if (_items.Count > 0)
                        {
                            task = _items.Dequeue();
                        }
                    }
                    if (task?.Item1 != null)
                    {
                        task.Item1(task.Item2);
                        if (InnerException != null)
                        {
                            throw new AggregateException("AsyncHelper.Run method threw an exception.", InnerException);
                        }
                    }
                    else
                    {
                        _workItemsWaiting.WaitOne();
                    }
                }
            }

            public override SynchronizationContext CreateCopy()
            {
                return this;
            }
        }
    }
}