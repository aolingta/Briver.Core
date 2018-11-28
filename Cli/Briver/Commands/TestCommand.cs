using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Briver.Framework;
using Briver.Logging;
using Briver.Runtime;
using Microsoft.Extensions.CommandLineUtils;

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

        private class EventCommand : Command
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

    }

}
