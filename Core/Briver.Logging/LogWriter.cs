using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Briver.Framework;
using Microsoft.Extensions.Configuration;

namespace Briver.Logging
{
    public interface ILogWriter : IComposition
    {
        void Write(IReadOnlyCollection<LogEntry> entries);
    }


}
