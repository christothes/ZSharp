using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace ZSharp
{
    static class DebugLogger
    {
        static DebugLogger()
        {
        }

        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    }
}
