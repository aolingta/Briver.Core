using System;
using System.Diagnostics;
using System.Management.Automation;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Briver.Framework;
using Briver.Logging;
using Briver.Runtime;
using static System.Reflection.MethodBase;

namespace Briver.Commands
{
    [Cmdlet(VerbsDiagnostic.Test, "EventCommand")]
    public class TestEvent : Cmdlet, ICommand
    {
        private class TestEventArgs : EventArgs { }

        [Parameter]
        public int Times { get; set; } = 1000;

        Guid _token;
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            _token = EventBus.Subscribe<TestEventArgs>((o, e) => { });
            Console.WriteLine(GetCurrentMethod().ToString());
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
            EventBus.Unsubscribe(_token);
            Console.WriteLine(GetCurrentMethod().ToString());
        }

        CancellationTokenSource _cts = new CancellationTokenSource();
        protected override void ProcessRecord()
        {
            Console.WriteLine(GetCurrentMethod().ToString());
            SystemContext.Initialize(new App());

            var watch = Stopwatch.StartNew();
            Parallel.For(0, Times, (ii, ss) =>
            {
                EventBus.Publish(null, new TestEventArgs());
                if (_cts.IsCancellationRequested) { ss.Stop(); }
            });
            watch.Stop();
            var message = $"处理{Times}次消息，用时{watch.Elapsed.TotalMilliseconds}毫秒";
            Console.WriteLine(message, true);
            Logger.Warn(message);
        }

        protected override void StopProcessing()
        {
            base.StopProcessing();
            Console.WriteLine(GetCurrentMethod().ToString());
            _cts.Cancel();
        }
    }
}
