using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZSharp;
using ZSharp.Nodes;
using System.Threading.Tasks;

namespace ZSharp.WebAPI
{
    public class ZWaveConfig
    {
        private static ZWave zw;

        private static void DoInit()
        {
            zw = new ZWave();
            zw.ZWaveInitializedEvent += zw_ZWaveInitializedEvent;
            zw.ZWaveReadyEvent += zw_ZWaveReadyEvent;
            zw.Initialize();
        }

        private static byte ToggleSwitch(SwitchBinary sw)
        {
            if (sw != null)
            {
                DebugLogger.Logger.Trace(string.Format("Switch in state: {0}", sw.State.ToString()));
                if (sw.State == 0xff)
                {
                    DebugLogger.Logger.Trace("And then there was darkness...");
                    sw.Off();
                }
                else
                {
                    DebugLogger.Logger.Trace("Let there be light!");
                    sw.On();
                }
            }
            return sw.State;
        }

        static void zw_ZWaveInitializedEvent(object sender, EventArgs e)
        {
            DebugLogger.Logger.Trace("");
        }

        static void zw_ZWaveReadyEvent(object sender, EventArgs e)
        {
            DebugLogger.Logger.Trace("READY!");
            Task.Factory.StartNew(() =>
            {
                //RunLoop();
            });
        }
    }
}