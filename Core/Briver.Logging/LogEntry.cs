using System;

namespace Briver.Logging
{
    /// <summary>
    /// 日志条目
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// 日志级别
        /// </summary>
        public LogLevel Level { get; }

        /// <summary>
        /// 发生时间
        /// </summary>
        public DateTime Time { get; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 详细信息
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// 记录日志的代码所在的文件
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// 记录日志的代码所在的类的成员名称
        /// </summary>
        public string MemberName { get; }

        /// <summary>
        /// 记录日志的代码所在的文件的行号
        /// </summary>
        public int? LineNumber { get; }

        internal LogEntry(LogLevel level, string message, string content, string filePath, string memberName, int? lineNumber)
        {
            this.Time = DateTime.Now;
            this.Level = level;
            this.Message = message;
            this.Content = content;
            this.FilePath = filePath;
            this.MemberName = memberName;
            this.LineNumber = lineNumber;
        }
    }
}