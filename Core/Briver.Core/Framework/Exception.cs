using System;

namespace Briver.Framework
{
    /// <summary>
    /// 异常级别
    /// </summary>
    public enum ExceptionLevel
    {
        /// <summary>
        /// 轻微的
        /// </summary>
        Slight = 0,

        /// <summary>
        /// 严重的
        /// </summary>
        Serious = 1,

        /// <summary>
        /// 致命的
        /// </summary>
        Fatal = 2,
    }


    /// <summary>
    /// 业务框架异常
    /// </summary>
    public class FrameworkException : Exception
    {
        /// <summary>
        /// 异常级别
        /// </summary>
        public ExceptionLevel Level { get; }

        private static string BuildMessage(ExceptionLevel level, string message)
        {
            return $"发生{level}异常，{message}";
        }
        /// <summary>
        /// 业务框架异常
        /// </summary>
        /// <param name="level">异常级别</param>
        /// <param name="message">详细的内容</param>
        public FrameworkException(ExceptionLevel level, string message)
            : base(BuildMessage(level, message))
        {
            this.Level = level;
        }

        /// <summary>
        /// 业务框架异常
        /// </summary>
        /// <param name="level">异常级别</param>
        /// <param name="message">详细的内容</param>
        /// <param name="exception">内部异常</param>
        public FrameworkException(ExceptionLevel level, string message, Exception exception)
            : base(BuildMessage(level, message), exception)
        {
            this.Level = level;
        }

    }

    /// <summary>
    /// 初始化失败的异常
    /// </summary>
    public sealed class InitializeFailedException : FrameworkException
    {
        /// <summary>
        /// 初始化失败的异常
        /// </summary>
        /// <param name="message">异常的消息</param>
        public InitializeFailedException(string message) : base(ExceptionLevel.Fatal, message) { }

    }


}
