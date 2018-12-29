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
    /// 切面拦截的绑定
    /// </summary>
    public enum AspectBinder
    {
        /// <summary>
        /// 调用方法
        /// </summary>
        InvokeMethod,

        /// <summary>
        /// 获取属性
        /// </summary>
        GetProperty,

        /// <summary>
        /// 设置属性
        /// </summary>
        SetProperty
    }
    /// <summary>
    /// 切面绑定
    /// </summary>
    public abstract class AspectBinding
    {
        /// <summary>
        /// 成员名称
        /// </summary>
        public abstract string Name { get; }

        public abstract AspectBinder Binder { get; }
    }

    /// <summary>
    /// 方法调用绑定
    /// </summary>
    public sealed class InvokeMethodBinding : AspectBinding
    {
        /// <summary>
        /// 绑定的方法
        /// </summary>
        public MethodInfo Method { get; }

        /// <summary>
        /// 成员名称
        /// </summary>
        public override string Name
        {
            get { return this.Method.Name; }
        }

        public override AspectBinder Binder
        {
            get { return AspectBinder.InvokeMethod; }
        }

        internal InvokeMethodBinding(MethodInfo method)
        {
            this.Method = method;
        }

    }

    /// <summary>
    /// 属性获取绑定
    /// </summary>
    public sealed class GetPropertyBinding : AspectBinding
    {
        /// <summary>
        /// 绑定的属性
        /// </summary>
        public PropertyInfo Property { get; }

        public override string Name
        {
            get { return this.Property.Name; }
        }

        public override AspectBinder Binder
        {
            get { return AspectBinder.GetProperty; }
        }


        internal GetPropertyBinding(PropertyInfo property)
        {
            this.Property = property;
        }
    }

    /// <summary>
    /// 属性设置绑定
    /// </summary>
    public sealed class SetPropertyBinding : AspectBinding
    {
        /// <summary>
        /// 绑定的属性
        /// </summary>
        public PropertyInfo Property { get; }

        public override string Name
        {
            get { return this.Property.Name; }
        }

        public override AspectBinder Binder
        {
            get { return AspectBinder.SetProperty; }
        }

        internal SetPropertyBinding(PropertyInfo property)
        {
            this.Property = property;
        }
    }
}
