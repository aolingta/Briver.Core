namespace Briver.Aspect
{
    /// <summary>
    /// 用于切面编程的扩展方法
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// 启用切面
        /// </summary>
        /// <typeparam name="T">对象的类型</typeparam>
        /// <param name="this">当前对象</param>
        /// <returns></returns>
        public static dynamic Aspect<T>(this T @this) where T : class
        {
            return new AspectDynamic<T>(@this);
        }

    }
}
