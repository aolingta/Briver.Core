using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Briver.Aspect
{
    /// <summary>
    /// 方法调用的上下文环境
    /// </summary>
    public class AspectContext
    {
        static int _seqNo = 0;
        /// <summary>
        /// 方法调用的唯一序号
        /// </summary>
        public int SeqNo { get; }

        /// <summary>
        /// 方法调用的上下文环境
        /// </summary>
        internal AspectContext(object target, MethodInfo method, AspectBinding binding, object[] arguments, IReadOnlyList<IInterception> interceptions)
        {
            this.SeqNo = Interlocked.Increment(ref _seqNo);
            this.Target = target;
            this.Method = method;
            this.Binding = binding;
            this.Arguments = arguments;
            this.Interceptions = interceptions;
        }

        /// <summary>
        /// 调用的目标对象
        /// </summary>
        public object Target { get; }

        /// <summary>
        /// 调用的方法
        /// </summary>
        public MethodInfo Method { get; }

        /// <summary>
        /// 切面绑定信息
        /// </summary>
        public AspectBinding Binding { get; }

        /// <summary>
        /// 调用的参数值
        /// </summary>
        public object[] Arguments { get; }

        /// <summary>
        /// 拦截器
        /// </summary>
        public IReadOnlyList<IInterception> Interceptions { get; }


        /// <summary>
        /// 执行原始的方法调用
        /// </summary>
        internal AspectDelegate Execution { get; set; }

        /// <summary>
        /// 调用的结果
        /// </summary>
        public object Result { get; set; }

        /// <summary>
        /// 发起调用
        /// </summary>
        internal void Proceed()
        {
            this.BuildMethod()();
        }

        private int index = -1;
        private AspectDelegate BuildMethod()
        {
            Interlocked.Increment(ref index);

            if (index < this.Interceptions.Count)
            {
                return () => this.Interceptions[index].Intercept(this, BuildMethod());
            }
            else if (index == this.Interceptions.Count)
            {
                return this.Execution;
                //return () => { this.Result = this.Method.Invoke(this.Target, this.Arguments); };
            }

            throw new InvalidOperationException($"调用拦截方法的次数超过拦截器的数量");
        }

    }

}
