using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Briver.Aspect
{
    internal static class Constant
    {
        internal static readonly BindingFlags _binding_flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        internal static readonly Type _type_context = typeof(AspectContext);
        internal static readonly ConstructorInfo _ctor_context = _type_context.GetConstructor(_binding_flags, null, new Type[] {
            typeof(object),
            typeof(MethodInfo),
            typeof(AspectBinding),
            typeof(object[]),
            typeof(IReadOnlyList<IInterception>)
        }, null);

        internal static readonly MethodInfo _method_proceed = _type_context.GetMethod(nameof(AspectContext.Proceed), _binding_flags);

        internal static readonly PropertyInfo _property_target = _type_context.GetProperty(nameof(AspectContext.Target));
        internal static readonly PropertyInfo _property_method = _type_context.GetProperty(nameof(AspectContext.Method));
        internal static readonly PropertyInfo _property_binding = _type_context.GetProperty(nameof(AspectContext.Binding));
        internal static readonly PropertyInfo _property_arguments = _type_context.GetProperty(nameof(AspectContext.Arguments));
        internal static readonly PropertyInfo _property_result = _type_context.GetProperty(nameof(AspectContext.Result));
        internal static readonly PropertyInfo _property_interceptions = _type_context.GetProperty(nameof(AspectContext.Interceptions));
        internal static readonly PropertyInfo _property_execution = _type_context.GetProperty(nameof(AspectContext.Execution), _binding_flags);
    }
}
