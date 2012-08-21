using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NLog;

namespace ZSharp.WebAPI
{
    public class DebugLogger
    {
        static DebugLogger()
        {
        }

        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    }
}