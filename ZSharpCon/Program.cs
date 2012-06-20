using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using ZSharp;

namespace ZSharpCon
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static ZWave zw;

        static void Main(string[] args)
        {
            DoInit();
        }

        private static void DoInit()
        {
            zw = new ZWave();
            zw.ZWaveInitializedEvent += zw_ZWaveInitializedEvent;
            zw.Initialize();
            zw.ZWaveReadyEvent += zw_ZWaveReadyEvent;

            while (true)
            {
                //hack loop just to wait for events.
                Thread.Sleep(1000);
            }
        }

        static void zw_ZWaveInitializedEvent(object sender, EventArgs e)
        {
            Logger.Trace("");
            zw.Controller.AddNewNodeStart();
        }

        static void zw_ZWaveReadyEvent(object sender, EventArgs e)
        {
            Logger.Trace("READY!");
            
            
        }
    }
}
