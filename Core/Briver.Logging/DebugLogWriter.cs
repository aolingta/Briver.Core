using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Briver.Logging
{
    class DebugLogWriter : ILogWriter
    {
        public void Write(IReadOnlyCollection<LogEntry> entries)
        {
            if (!Debugger.IsAttached) return;

            foreach (var entry in entries)
            {
                var writer = new StringBuilder();
                writer.AppendLine($"时间：{entry.Time:HH:mm:ss.fff}");
                writer.AppendLine($"级别：{entry.Level}");
                writer.AppendLine($"位置：{entry.FilePath}@{entry.MemberName}#{entry.LineNumber}");
                writer.AppendLine($"消息：{entry.Message}");
                if (!string.IsNullOrEmpty(entry.Content))
                {
                    writer.AppendLine(entry.Content);
                }
                Debug.Write(writer.ToString());
            }
        }
    }
}
