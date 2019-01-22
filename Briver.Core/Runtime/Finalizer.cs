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
    public static partial class ExtensionMethods
    {
        private class FinalizationContainer
        {
            private ConcurrentBag<Action> _actions = new ConcurrentBag<Action>();

            public FinalizationContainer()
            {
            }

            ~FinalizationContainer()
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

        private static ConditionalWeakTable<object, FinalizationContainer> _finalizationContainers = new ConditionalWeakTable<object, FinalizationContainer>();

        private static FinalizationContainer CreateFinalizationContainer(object item) => new FinalizationContainer();

        /// <summary>
        /// 注册对象被垃圾回收时的回调方法
        /// </summary>
        /// <param name="this">当前对象</param>
        /// <param name="callback">要执行的回调方法（注意：此方法将在新线程上执行）</param>
        public static void RegisterFinalizeCallback<T>(this T @this, Action callback) where T : class
        {
            if (@this != null && callback != null)
            {
                _finalizationContainers.GetValue(@this, CreateFinalizationContainer).AddCallback(callback);
            }
        }
    }
}
