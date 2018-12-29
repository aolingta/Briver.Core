using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Briver.Aspect.Constant;
using static Briver.Aspect.StaticMethods;

namespace Briver.Aspect
{
    /// <summary>
    /// 具体实现拦截的动态代理对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class AspectDynamic<T> : IDynamicMetaObjectProvider where T : class
    {
        /// <summary>
        /// 目标对象
        /// </summary>
        public T Target { get; }

        public AspectDynamic(T target)
        {
            this.Target = target ?? throw new ArgumentNullException(nameof(target));
        }

        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new MetaObject(this, parameter);
        }

        /// <summary>
        /// 创建动态代理，为方法或属性实现面向切面的调用拦截
        /// </summary>
        internal class MetaObject : DynamicMetaObject
        {
            internal MetaObject(AspectDynamic<T> dynamic, Expression expression)
                : base(expression, BindingRestrictions.Empty, dynamic.Target)
            {
            }

            /// <summary>
            /// 重写方法调用绑定，实现切面拦截功能
            /// </summary>
            /// <param name="binder"></param>
            /// <param name="args"></param>
            /// <returns></returns>
            public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
            {
                var target = base.BindInvokeMember(binder, args);
                ThrowIfBindFailed(target);

                var method = RetrieveMethodCall(target).Method;
                CheckMethodSignature(method, args);

                var interceptions = RetrieveInterceptions(this.LimitType, method);
                return this.Build(target, args, new InvokeMethodBinding(method), method, interceptions);
            }

            /// <summary>
            /// 重写方法调用绑定，实现切面拦截功能
            /// </summary>
            /// <param name="binder"></param>
            /// <returns></returns>
            public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
            {
                var target = base.BindGetMember(binder);
                ThrowIfBindFailed(target);

                var property = typeof(T).GetProperty(binder.Name, _binding_flags);
                var method = property.GetGetMethod(true);

                var interceptions = RetrieveInterceptions(this.LimitType, property, method);
                return this.Build(target, new DynamicMetaObject[0], new GetPropertyBinding(property), method, interceptions);
            }

            /// <summary>
            /// 重写方法调用绑定，实现切面拦截功能
            /// </summary>
            /// <param name="binder"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
            {
                var target = base.BindSetMember(binder, value);
                ThrowIfBindFailed(target);

                var property = typeof(T).GetProperty(binder.Name, _binding_flags);
                var method = property.GetSetMethod(true);

                var interceptions = RetrieveInterceptions(this.LimitType, property, method);
                return this.Build(target, new DynamicMetaObject[] { value }, new SetPropertyBinding(property), method, interceptions);
            }


            private DynamicMetaObject Build(DynamicMetaObject origin, DynamicMetaObject[] arguments, AspectBinding binding, MethodInfo method, IInterception[] interceptions)
            {
                var context = Expression.Parameter(_type_context, "context");

                // 初始赋值
                var expressions = new List<Expression>();
                expressions.Add(Expression.Assign(context, Expression.New(_ctor_context, new Expression[] {
                    Expression.Property(Expression.Convert(this.Expression, typeof(AspectDynamic<T>)), nameof(Target)),
                    Expression.Constant(method),
                    Expression.Constant(binding),
                    Expression.NewArrayInit(typeof(object), arguments.Select(it => Expression.Convert(it.Expression, typeof(object)))),
                    Expression.Constant(interceptions),
                })));

                // 包装方法调用
                var executions = new List<Expression>();
                var parameters = arguments.Select(arg => Expression.Variable(arg.LimitType)).ToArray();
                for (int i = 0; i < arguments.Length; i++)
                {
                    executions.Add(Expression.Assign(parameters[i], Expression.Convert(Expression.ArrayAccess(Expression.Property(context, _property_arguments), Expression.Constant(i)), arguments[i].LimitType)));
                }
                var exp_call = Expression.Call(Expression.Convert(Expression.Property(context, _property_target), this.LimitType), method, parameters);
                if (method.ReturnType == typeof(void))
                {
                    executions.Add(exp_call);
                }
                else
                {
                    executions.Add(Expression.Assign(Expression.Property(context, _property_result), Expression.Convert(exp_call, typeof(object))));
                }
                for (int i = 0; i < arguments.Length; i++)
                {
                    executions.Add(Expression.Assign(Expression.ArrayAccess(Expression.Property(context, _property_arguments), Expression.Constant(i)), Expression.Convert(parameters[i], typeof(object))));
                }
                expressions.Add(Expression.Assign(Expression.Property(context, _property_execution), Expression.Lambda<AspectDelegate>(Expression.Block(parameters, executions))));

                // 发起调用
                expressions.Add(Expression.Call(context, _method_proceed));

                // 处理返回值
                for (int i = 0; i < arguments.Length; i++)
                {
                    if (arguments[i].Expression is ParameterExpression pexp && pexp.IsByRef)
                    {
                        expressions.Add(Expression.Assign(pexp, Expression.Convert(Expression.ArrayAccess(Expression.Property(context, _property_arguments), Expression.Constant(i)), pexp.Type)));
                    }
                }
                expressions.Add(Expression.Property(context, _property_result));

                var block = Expression.Block(new[] { context }, expressions.ToArray());
                return new DynamicMetaObject(block, AmendRestrictions(origin.Restrictions));
            }

            private BindingRestrictions AmendRestrictions(BindingRestrictions restrictions)
            {
                var stack = new Stack<Expression>();
                stack.Push(restrictions.ToExpression());

                var expressions = new List<Expression>();
                while (stack.Count > 0)
                {
                    var current = stack.Pop();
                    if (current is BinaryExpression bexp && bexp.NodeType == ExpressionType.AndAlso)
                    {
                        stack.Push(bexp.Right);
                        stack.Push(bexp.Left);
                    }
                    else
                    {
                        expressions.Add(current);
                    }
                }

                restrictions = BindingRestrictions.Empty;
                for (int i = 0; i < expressions.Count; i++)
                {
                    var current = expressions[i];
                    if (current is TypeBinaryExpression bexp && bexp.Expression == this.Expression && typeof(T).IsAssignableFrom(bexp.TypeOperand))
                    {
                        current = Expression.TypeEqual(Expression.Property(Expression.Convert(this.Expression, typeof(AspectDynamic<T>)), nameof(Target)), this.LimitType);
                    }
                    restrictions = restrictions.Merge(BindingRestrictions.GetExpressionRestriction(current));
                }

                return restrictions;
            }

        }
    }


}
