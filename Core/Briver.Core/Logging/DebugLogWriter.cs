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
            if (!Debugger.IsAttached)
            {
                return;
            }

            foreach (var entry in entries)
            {
                using (var writer = new StringWriter())
                {
                    writer.WriteLine();
                    entry.Output(writer);
                    writer.WriteLine();
                    Debug.Write(writer.ToString());
                }
            }
        }
    }
}
