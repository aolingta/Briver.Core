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
                using (var writer = new StringWriter())
                {
                    foreach (var entry in entries)
                    {
                        entry.Output(writer);
                        writer.WriteLine();
                    }
                    Trace.Write(writer.ToString());
                }
            }
        }
    }
}
