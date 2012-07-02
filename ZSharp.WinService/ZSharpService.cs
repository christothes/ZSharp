using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ZSharp.WinService
{
    public partial class ZSharpService : ServiceBase
    {
        private ZWave zw;

        public ZSharpService()
        {
            DebugLogger.Logger.Trace("");
            InitializeComponent();
            this.AutoLog = true;
            this.CanShutdown = true;
            this.ServiceName = "ZSharp Service";            
        }

        protected override void OnStart(string[] args)
        {
            DebugLogger.Logger.Trace("");
            zw = new ZWave();
            zw.ZWaveInitializedEvent += zw_ZWaveInitializedEvent;
            zw.ZWaveReadyEvent += zw_ZWaveReadyEvent;
            zw.Initialize();
        }

        protected override void OnStop()
        {
            DebugLogger.Logger.Trace("");
            zw.ShutdownGracefully();
        }

        protected override void OnShutdown()
        {
            DebugLogger.Logger.Trace("");
            zw.ShutdownGracefully();
            base.OnShutdown();
        }

        #region EventHandlers

        static void zw_ZWaveInitializedEvent(object sender, EventArgs e)
        {
            DebugLogger.Logger.Trace("");
        }

        static void zw_ZWaveReadyEvent(object sender, EventArgs e)
        {
            DebugLogger.Logger.Trace("READY!");
        }

        #endregion
    }
}
