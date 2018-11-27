using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Briver.Framework;

namespace Briver.Aspect
{
    internal static class StaticMethods
    {
        public static void CheckMethodSignature(MethodInfo method, DynamicMetaObject[] args)
        {
            var parameters = method.GetParameters();
            if (parameters.Length != args.Length)
            {
                throw new ArgumentException($"方法{method}期望{parameters.Length}个参数，但是实际提供了{args.Length}个参数值");
            }
        }

        /// <summary>
        /// 如果生成表达式失败，则抛出异常
        /// </summary>
        /// <param name="target"></param>
        public static void ThrowIfBindFailed(DynamicMetaObject target)
        {
            if (target.Expression.NodeType == ExpressionType.Throw)
            {
                Expression.Lambda<Action>(target.Expression).Compile()();
            }
        }

        /// <summary>
        /// 获取方法调用的表达式
        /// </summary>
        /// <param name="meta">元数据</param>
        /// <returns></returns>
        public static MethodCallExpression RetrieveMethodCall(DynamicMetaObject meta)
        {
            Expression method = null;
            while (method == null || method.NodeType != ExpressionType.Call)
            {
                switch (meta.Expression.NodeType)
                {
                    case ExpressionType.Call:
                        return (MethodCallExpression)meta.Expression;
                    case ExpressionType.Block:
                        var block = (BlockExpression)meta.Expression;
                        return (MethodCallExpression)block.Expressions.First(exp => exp.NodeType == ExpressionType.Call);
                    case ExpressionType.Convert:
                        method = ((UnaryExpression)meta.Expression).Operand;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            return (MethodCallExpression)method;
        }

        /// <summary>
        /// 获取所有拦截器（由于对每个动态方法调用都只执行一次，因此不需要进行缓存之类的处理）
        /// </summary>
        /// <param name="targets"></param>
        /// <returns></returns>
        public static IInterception[] RetrieveInterceptions(params ICustomAttributeProvider[] targets)
        {
            var interceptions = SystemContext.GetExports<IComposableInterception>().Cast<IInterception>();
            foreach (var member in targets)
            {
                interceptions = interceptions.Union(member.GetCustomAttributes(true).OfType<IInterception>());
            }
            return interceptions.OrderByDescending(it => it.Priority).ToArray();
        }

    }
}
