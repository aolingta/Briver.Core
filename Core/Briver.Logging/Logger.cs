using Briver.Framework;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Briver.Logging
{
    /// <summary>
    /// 日志
    /// </summary>
    public static class Logger
    {
        private static Thread _thread;
        private static readonly LogLevel _minLevel = LogLevel.Info;
        private static readonly IEnumerable<ILogWriter> _writers;

        private static ManualResetEvent _event = new ManualResetEvent(false);
        private static CancellationTokenSource _cts = new CancellationTokenSource();
        private static readonly BlockingCollection<LogEntry> _entries = new BlockingCollection<LogEntry>();

        static Logger()
        {
            var config = SystemContext.Configuration
                .GetSection(nameof(Logger))?.Get<Configuration.Logger>();
            if (config != null)
            {
                _minLevel = config.MinLevel;
            }

            _writers = SystemContext.GetExports<ILogWriter>();

            _thread = new Thread(Flush)
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };
            _thread.Start();

            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            _cts.Cancel();
            _event.WaitOne();

            if (_entries.Count > 0)
            {
                Output(_entries.ToArray());
            }
        }

        private static void Flush()
        {
            var token = _cts.Token;
            var list = new List<LogEntry>();

            while (!token.IsCancellationRequested)
            {
                list.Clear();
                try
                {
                    if (_entries.TryTake(out LogEntry entry, -1, token))//先阻塞式拿一次
                    {
                        list.Add(entry);
                        while (_entries.TryTake(out entry, 100, token))//再连续拿多次，最多等待100毫秒
                        {
                            list.Add(entry);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                }

                Output(list);
            }

            _event.Set();
        }

        private static void Output(IReadOnlyCollection<LogEntry> entries)
        {
            foreach (var writer in _writers)
            {
                try { writer.Write(entries); } catch (Exception) { }
            }
        }

        /// <summary>
        /// 是否接受指定的日志级别
        /// </summary>
        /// <param name="level">日志级别</param>
        public static bool Adopt(LogLevel level)
        {
            return _minLevel <= level;
        }

        /// <summary>
        /// 记录错误级别的日志
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="content">详细内容</param>
        /// <param name="filePath">源码的文件路径（由编译器自动填充，请勿手工赋值）</param>
        /// <param name="memberName">源码的成员名称（由编译器自动填充，请勿手工赋值）</param>
        /// <param name="lineNumber">源码的行号（由编译器自动填充，请勿手工赋值）</param>
        public static void Fatal(string message, string content = null, [CallerFilePath]string filePath = null, [CallerMemberName]string memberName = null, [CallerLineNumber]int? lineNumber = null)
        {
            if (Adopt(LogLevel.Fatal))
            {
                _entries.Add(new LogEntry(LogLevel.Fatal, message, content, filePath, memberName, lineNumber));
            }
        }

        /// <summary>
        /// 记录错误级别的日志
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="content">详细内容</param>
        /// <param name="filePath">源码的文件路径（由编译器自动填充，请勿手工赋值）</param>
        /// <param name="memberName">源码的成员名称（由编译器自动填充，请勿手工赋值）</param>
        /// <param name="lineNumber">源码的行号（由编译器自动填充，请勿手工赋值）</param>
        public static void Error(string message, string content = null, [CallerFilePath]string filePath = null, [CallerMemberName]string memberName = null, [CallerLineNumber]int? lineNumber = null)
        {
            if (Adopt(LogLevel.Error))
            {
                _entries.Add(new LogEntry(LogLevel.Error, message, content, filePath, memberName, lineNumber));
            }
        }

        /// <summary>
        /// 记录警告级别的日志
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="content">详细内容</param>
        /// <param name="filePath">源码的文件路径（由编译器自动填充，请勿手工赋值）</param>
        /// <param name="memberName">源码的成员名称（由编译器自动填充，请勿手工赋值）</param>
        /// <param name="lineNumber">源码的行号（由编译器自动填充，请勿手工赋值）</param>
        public static void Warn(string message, string content = null, [CallerFilePath]string filePath = null, [CallerMemberName]string memberName = null, [CallerLineNumber]int? lineNumber = null)
        {
            if (Adopt(LogLevel.Warn))
            {
                _entries.Add(new LogEntry(LogLevel.Warn, message, content, filePath, memberName, lineNumber));
            }
        }

        /// <summary>
        /// 记录普通级别的日志
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="content">详细内容</param>
        /// <param name="filePath">源码的文件路径（由编译器自动填充，请勿手工赋值）</param>
        /// <param name="memberName">源码的成员名称（由编译器自动填充，请勿手工赋值）</param>
        /// <param name="lineNumber">源码的行号（由编译器自动填充，请勿手工赋值）</param>
        public static void Info(string message, string content = null, [CallerFilePath]string filePath = null, [CallerMemberName]string memberName = null, [CallerLineNumber]int? lineNumber = null)
        {
            if (Adopt(LogLevel.Info))
            {
                _entries.Add(new LogEntry(LogLevel.Info, message, content, filePath, memberName, lineNumber));
            }
        }

        /// <summary>
        /// 记录调试级别的日志
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="content">详细内容</param>
        /// <param name="filePath">源码的文件路径（由编译器自动填充，请勿手工赋值）</param>
        /// <param name="memberName">源码的成员名称（由编译器自动填充，请勿手工赋值）</param>
        /// <param name="lineNumber">源码的行号（由编译器自动填充，请勿手工赋值）</param>
        public static void Debug(string message, string content = null, [CallerFilePath]string filePath = null, [CallerMemberName]string memberName = null, [CallerLineNumber]int? lineNumber = null)
        {
            if (Adopt(LogLevel.Debug))
            {
                _entries.Add(new LogEntry(LogLevel.Debug, message, content, filePath, memberName, lineNumber));
            }
        }

        /// <summary>
        /// 记录跟踪级别的日志
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="content">详细内容</param>
        /// <param name="filePath">源码的文件路径（由编译器自动填充，请勿手工赋值）</param>
        /// <param name="memberName">源码的成员名称（由编译器自动填充，请勿手工赋值）</param>
        /// <param name="lineNumber">源码的行号（由编译器自动填充，请勿手工赋值）</param>
        public static void Trace(string message, string content = null, [CallerFilePath]string filePath = null, [CallerMemberName]string memberName = null, [CallerLineNumber]int? lineNumber = null)
        {
            if (Adopt(LogLevel.Trace))
            {
                _entries.Add(new LogEntry(LogLevel.Trace, message, content, filePath, memberName, lineNumber));
            }
        }

    }


}
