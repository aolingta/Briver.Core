using System;
using System.IO;
using System.Text;

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

        /// <summary>
        /// 输出到文本流
        /// </summary>
        /// <param name="writer">文本流</param>
        public void Output(TextWriter writer)
        {
            if (writer == null) { return; }

            writer.WriteLine($"时间：{this.Time:HH:mm:ss.fff}");
            writer.WriteLine($"级别：{this.Level}");
            writer.WriteLine($"位置：{this.FilePath}@{this.MemberName}#{this.LineNumber}");
            writer.WriteLine($"消息：{this.Message}");
            if (!string.IsNullOrEmpty(this.Content))
            {
                writer.WriteLine(this.Content);
            }
            writer.WriteLine();
        }

        public override string ToString()
        {
            using (var writer = new StringWriter())
            {
                this.Output(writer);
                return writer.ToString();
            }
        }
    }
}