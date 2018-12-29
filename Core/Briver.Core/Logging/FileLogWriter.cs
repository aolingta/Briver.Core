using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Briver.Framework;
using Microsoft.Extensions.Configuration;

namespace Briver.Logging
{
    internal class FileLogWriter : ILogWriter
    {
        public class Config
        {
            public bool Enabled { get; set; } = true;
            public string FilePrefix { get; set; }
            public string OutputDir { get; set; }
        }

        private readonly bool _enabled = true;
        private readonly string _prefix = "";
        private readonly string _outputDir;

        public FileLogWriter()
        {
            var config = SystemContext.Configuration.GetSection(nameof(FileLogWriter))?.Get<Config>();
            if (config != null)
            {
                _enabled = config.Enabled;
                _prefix = config.FilePrefix;
                _outputDir = config.OutputDir;
            }
            if (string.IsNullOrEmpty(_outputDir))
            {
                _outputDir = Path.Combine(SystemContext.Application.WorkDirectory, "Log");
            }
            else if (!Path.IsPathRooted(_outputDir))
            {
                _outputDir = Path.Combine(SystemContext.Application.WorkDirectory, _outputDir);
            }
        }

        public void Write(IReadOnlyCollection<LogEntry> entries)
        {
            if (!_enabled) { return; }
            if (entries == null || entries.Count == 0) { return; }

            Directory.CreateDirectory(_outputDir);
            foreach (var group in entries.GroupBy(it => it.Time.Date))
            {
                var fileName = Path.Combine(_outputDir, $"{_prefix}{group.Key:yyyyMMdd}.log");
                using (var writer = new StreamWriter(fileName, true, Encoding.UTF8))
                {
                    foreach (var entry in group)
                    {
                        writer.WriteLine($"时间：{entry.Time:HH:mm:ss.fff}");
                        writer.WriteLine($"级别：{entry.Level}");
                        writer.WriteLine($"位置：{entry.FilePath}@{entry.MemberName}#{entry.LineNumber}");
                        writer.WriteLine($"消息：{entry.Message}");
                        if (!string.IsNullOrEmpty(entry.Content))
                        {
                            writer.WriteLine(entry.Content);
                        }
                        writer.WriteLine();
                    }
                    writer.Flush();
                }
            }
        }
    }
}
