using System;
using Briver.Framework;

namespace Briver.Aspect
{
    /// <summary>
    /// 切面委托
    /// </summary>
    public delegate void AspectDelegate();

    /// <summary>
    /// 切面拦截器
    /// </summary>
    public interface IInterception
    {
        /// <summary>
        /// 执行的优先级
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// 执行拦截操作
        /// </summary>
        /// <param name="context">上下文环境</param>
        /// <param name="proceed">执行后续的拦截操作</param>
        void Intercept(AspectContext context, AspectDelegate proceed);
    }

    /// <summary>
    /// 支持组合的切面拦截器
    /// </summary>
    internal interface IComposableInterception : IComposition, IInterception
    {
    }

    /// <summary>
    /// 切面拦截器基类
    /// </summary>
    public abstract class Interception : IComposableInterception
    {
        /// <summary>
        /// 优先级
        /// </summary>
        public virtual int Priority => this.GetCompositionMetadata().Priority;

        /// <summary>
        /// 拦截处理
        /// </summary>
        /// <param name="context">切面上下文</param>
        /// <param name="proceed">切面委托</param>
        public abstract void Intercept(AspectContext context, AspectDelegate proceed);
    }

    /// <summary>
    /// 切面拦截器特性基类
    /// </summary>
    public abstract class InterceptionAttribute : Attribute, IInterception
    {
        /// <summary>
        /// 执行的优先级
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 执行拦截操作
        /// </summary>
        /// <param name="context">上下文环境</param>
        /// <param name="proceed">执行后续的拦截操作</param>
        public abstract void Intercept(AspectContext context, AspectDelegate proceed);
    }
}
