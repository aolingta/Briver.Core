using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Briver.Runtime
{
    /// <summary>
    /// 终结器的扩展
    /// </summary>
    public static class FinalizerExtension
    {
        private class Container
        {
            private ConcurrentBag<Action> _actions = new ConcurrentBag<Action>();

            public Container()
            {
            }

            ~Container()
            {
                if (!Environment.HasShutdownStarted)
                {
                    ThreadPool.QueueUserWorkItem(OnFinalize, _actions.ToArray());
                }
            }

            public void AddCallback(Action callback)
            {
                _actions.Add(callback);
            }

            private static void OnFinalize(object state)
            {
                var actions = (Action[])state;
                foreach (var callback in actions)
                {
                    try { callback(); } catch { }
                }
            }
        }



        private static ConditionalWeakTable<object, Container> _table = new ConditionalWeakTable<object, Container>();

        private static Container CreateContainer(object item) => new Container();

        /// <summary>
        /// 注册对象被垃圾回收时的回调方法
        /// </summary>
        /// <param name="this">当前对象</param>
        /// <param name="callback">要执行的回调方法（注意：此方法将在新线程上执行）</param>
        public static void RegisterFinalizeCallback<T>(this T @this, Action callback) where T : class
        {
            if (@this != null && callback != null)
            {
                _table.GetValue(@this, CreateContainer).AddCallback(callback);
            }
        }
    }
}
