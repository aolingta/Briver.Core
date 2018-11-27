using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Briver.Framework;
using System.Threading.Tasks;

namespace Briver.Runtime
{
    /// <summary>
    /// <para>事件总线，主要用于界面之间的事件通知，内部通过SynchronizationContext确保最终调度到UI线程执行。</para>
    /// <para>由于采用弱引用来存储订阅事件的方法，不需要主动取消事件订阅，但是其垃圾回收时间不确定，通过主动取消订阅可以避免这种不确定的情况。</para>
    /// </summary>
    public static class EventBus
    {

        /// <summary>
        /// 委托的引用
        /// </summary>
        private class DelegateReference
        {
            /// <summary>
            /// 令牌
            /// </summary>
            public Guid Token { get; }

            /// <summary>
            /// 订阅者的弱引用
            /// </summary>
            public WeakReference<object> Target { get; set; }

            /// <summary>
            /// 事件的处理方法
            /// </summary>
            public MethodInfo Method { get; set; }

            /// <summary>
            /// 同步上下文环境，避免发生跨线程调用异常
            /// </summary>
            public SynchronizationContext Context { get; private set; }

            /// <summary>
            /// 委托的引用
            /// </summary>
            /// <param name="delegate">事件的处理方法</param>
            public DelegateReference(Delegate @delegate)
            {
                var token = Guid.NewGuid();

                this.Token = token;
                this.Method = @delegate.Method;
                this.Context = SynchronizationContext.Current;

                if (!this.Method.IsStatic)
                {
                    var target = @delegate.Target;
                    this.Target = new WeakReference<object>(target);
                    target.RegisterFinalizeCallback(() => Unsubscribe(token));
                }
            }

            /// <summary>
            /// 获取弱引用指向的对象，如果对象有效（未被垃圾回收，或事件处理方法为静态方法），则返回true，否则返回false
            /// </summary>
            /// <param name="target">弱引用指向的对象</param>
            /// <returns></returns>
            public bool TryGetTarget(out object target)
            {
                target = null;
                if (this.Method.IsStatic)
                {
                    return true;
                }
                return this.Target.TryGetTarget(out target);
            }

            /// <summary>
            /// 在弱引用指向的对象上调用事件处理方法
            /// </summary>
            /// <param name="target">弱引用指向的对象（如果事件处理方法为静态方法，则为null）</param>
            /// <param name="arguments">参数数组</param>
            public void RaiseEvent(object target, object[] arguments)
            {
                if (this.Context != null)
                {
                    this.Context.Send(state =>
                    {
                        var context = (EventContext)state;
                        //直接通过反射引发事件，比创建委托再调用要快一些
                        context.Method.Invoke(context.Target, context.Arguments);

                    }, new EventContext(target, this.Method, arguments));
                }
                else
                {
                    this.Method.Invoke(target, arguments);
                }
            }
        }

        /// <summary>
        /// 同步上下文调用参数
        /// </summary>
        private class EventContext
        {
            /// <summary>
            /// 目标对象
            /// </summary>
            public object Target { get; }

            /// <summary>
            /// 事件处理方法
            /// </summary>
            public MethodInfo Method { get; }

            /// <summary>
            /// 事件状态
            /// </summary>
            public object[] Arguments { get; }

            /// <summary>
            /// 同步上下文调用参数
            /// </summary>
            /// <param name="target">目标对象</param>
            /// <param name="method">事件处理方法</param>
            /// <param name="arguments">参数数组</param>
            public EventContext(object target, MethodInfo method, object[] arguments)
            {
                this.Target = target;
                this.Method = method;
                this.Arguments = arguments;
            }
        }

        /// <summary>
        /// 事件处理函数容器
        /// </summary>
        private interface IEventContainer
        {
            /// <summary>
            /// 订阅者数量
            /// </summary>
            int Count { get; }

            /// <summary>
            /// 事件参数的类型
            /// </summary>
            Type Type { get; }

            /// <summary>
            /// 添加事件处理函数
            /// </summary>
            /// <param name="handler">事件处理函数</param>
            /// <returns>返回代表此函数的令牌，后面移除此函数时要用到</returns>
            Guid Add(object handler);

            /// <summary>
            /// 移除事件处理函数
            /// </summary>
            /// <param name="token">代表此函数的主键</param>
            void Remove(Guid token);

            /// <summary>
            /// 引发事件
            /// </summary>
            /// <param name="arguments">参数数组</param>
            void Raise(object[] arguments);

        }

        /// <summary>
        /// 事件处理函数容器的泛型实现
        /// </summary>
        /// <typeparam name="T">事件参数的类型</typeparam>
        private class EventContainer<T> : IEventContainer where T : EventArgs
        {
            /// <summary>
            /// 订阅了此事件的所有函数的委托引用
            /// </summary>
            private List<DelegateReference> _delegates = new List<DelegateReference>();

            public int Count
            {
                get
                {
                    lock (_delegates)
                    {
                        return _delegates.Count;
                    }
                }
            }

            public Type Type
            {
                get { return typeof(T); }
            }

            public Guid Add(object handler)
            {
                var @delegate = new DelegateReference((EventHandler<T>)handler);
                lock (_delegates)
                {
                    _delegates.Add(@delegate);
                }
                return @delegate.Token;
            }

            public void Remove(Guid token)
            {
                lock (_delegates)
                {
                    _delegates.RemoveAll(it => it.Token == token);
                    if (_delegates.Count == 0)
                    {
                        _containers.TryRemove(Type, out var container);
                    }
                }
            }

            public void Raise(object[] arguments)
            {
                DelegateReference[] array;
                lock (_delegates)
                {
                    array = _delegates.ToArray();
                }

                foreach (var @delegate in array)
                {
                    if (@delegate.TryGetTarget(out object target))
                    {
                        @delegate.RaiseEvent(target, arguments);
                    }
                    else
                    {
                        this.Remove(@delegate.Token);
                    }
                }
            }

        }


        /// <summary>
        /// 事件参数类型与事件容器的映射
        /// </summary>
        private static ConcurrentDictionary<Type, IEventContainer> _containers = new ConcurrentDictionary<Type, IEventContainer>();

        /// <summary>
        /// 事件订阅令牌与事件容器的映射
        /// </summary>
        private static ConcurrentDictionary<Guid, IEventContainer> _subscriptions = new ConcurrentDictionary<Guid, IEventContainer>();

        /// <summary>
        /// <para>订阅事件</para>
        /// <para>由于内部采用弱引用来存储订阅事件的方法，不需要主动取消事件订阅，但是其垃圾回收时间不确定，通过主动取消订阅可以避免这种不确定的情况。</para>
        /// </summary>
        /// <typeparam name="T">事件参数类型</typeparam>
        /// <param name="handler">事件处理函数</param>
        /// <returns></returns>
        public static Guid Subscribe<T>(EventHandler<T> handler) where T : EventArgs
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }
            var container = _containers.GetOrAdd(typeof(T), type => new EventContainer<T>());
            var token = container.Add(handler);
            _subscriptions.TryAdd(token, container);
            return token;
        }

        /// <summary>
        /// 取消订阅事件
        /// </summary>
        /// <param name="token">在订阅时返回的令牌</param>
        public static void Unsubscribe(Guid token)
        {
            if (_subscriptions.TryRemove(token, out var container))
            {
                container.Remove(token);
            }
        }

        /// <summary>
        /// 向事件总线发布特定类型参数的事件
        /// </summary>
        /// <typeparam name="T">事件参数类型</typeparam>
        /// <param name="sender">事件的发送者</param>
        /// <param name="e">事件的参数</param>
        public static void Publish<T>(object sender, T e) where T : EventArgs
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            if (_containers.TryGetValue(typeof(T), out var container))
            {
                container.Raise(new object[] { sender, e });
            }
        }

    }
}
