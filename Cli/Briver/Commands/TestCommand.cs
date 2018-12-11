using Briver.Framework;
using Briver.Logging;
using Briver.Runtime;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Briver.Commands
{
    public class TestCommand : Command
    {
        public TestCommand()
        {
        }

        private CommandLineApplication _app;

        protected override void Initialize(CommandLineApplication app)
        {
            _app = app;
        }

        protected override int Execute()
        {
            _app.ShowHelp();
            return 0;
        }


    }

    internal class EventCommand : Command
    {
        private class TestEventArgs : EventArgs { }

        public override Type Parent => typeof(TestCommand);

        private EventBus.SubscriptionToken _token;

        public EventCommand()
        {
            _token = EventBus.Subscribe<TestEventArgs>((o, e) => { });
        }

        private CommandOption _times;
        protected override void Initialize(CommandLineApplication app)
        {
            _times = app.Option("--times", "执行次数", CommandOptionType.SingleValue);
        }

        protected override int Execute()
        {
            if (!int.TryParse(_times.Value(), out var times))
            {
                times = 1;
            }

            var watch = Stopwatch.StartNew();
            Parallel.For(0, times, ii =>
            {
                EventBus.Publish(null, new TestEventArgs());
            });
            watch.Stop();
            var message = $"处理{times}次消息，用时{watch.Elapsed.TotalMilliseconds}毫秒";
            Console.WriteLine(message);
            Logger.Warn(message);
            EventBus.Unsubscribe(_token);
            return 0;
        }
    }

    internal class PipeCommand : Command
    {
        public override Type Parent => typeof(TestCommand);

        private CommandLineApplication _app;
        protected override void Initialize(CommandLineApplication app)
        {
            _app = app;
        }
        protected override int Execute()
        {
            TestPipe().GetAwaiter().GetResult();
            return 0;
        }

        private async Task TestPipe()
        {
            var pipe = new Pipe();
            await WriteAsync(pipe.Writer);
            pipe.Writer.Complete();
            await ReadAsync(pipe.Reader);
        }

        private async Task WriteAsync(PipeWriter writer)
        {
            var memoey = writer.GetMemory(100);
            Console.Write("请输入：");
            var text = Console.ReadLine();
            var bytes = Encoding.UTF8.GetBytes(text, memoey.Span);
            writer.Advance(bytes);
            await writer.FlushAsync();
        }

        private async Task ReadAsync(PipeReader reader)
        {
            while (true)
            {
                var result = await reader.ReadAsync();
                var buffer = result.Buffer;
                if (buffer.IsEmpty && result.IsCompleted)
                {
                    break;
                }
                foreach (var memory in buffer)
                {
                    var text = Encoding.UTF8.GetString(memory.Span);
                    Console.WriteLine($"已接收：{text}");
                }
                reader.AdvanceTo(buffer.End);
            }
        }

    }
}
