using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Briver.Logging
{
    internal class DebugLogWriter : ILogWriter
    {
        public void Write(IReadOnlyCollection<LogEntry> entries)
        {
            if (Debugger.IsAttached)
            {
                foreach (var entry in entries)
                {
                    using (var writer = new StringWriter())
                    {
                        entry.Output(writer);
                        Trace.Write(writer.ToString());
                    }
                }
            }
        }
    }
}
