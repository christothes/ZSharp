using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using ZSharp;
using ZSharp.Nodes;

namespace ZSharpCon
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static ZWave zw;
        private static AutoResetEvent exitEvent = new AutoResetEvent(false);
        private static string[] argsCache = null;

        static void Main(string[] args)
        {
            argsCache = args;
            DoInit();
            exitEvent.WaitOne();
        }

        private static void ParseArgs(string[] args)
        {
            // -{Object}:NODEID {ACTION}
            //ex. -SWITCH:02 ON
            //ex. -SWITCH:03 OFF

            List<SwitchCommand> swCmdList = new List<SwitchCommand>();

            const string SWITCH = "-SWITCH";
            const string QUIT = "-Q";

            bool doQuit = false;
            Command curCommand = null;

            bool objectFound = false;

            for (int i = 0; i < args.Length; i++)
            {
                if (!objectFound)
                {
                    if (args[i].StartsWith(SWITCH))
                    {
                        Console.WriteLine("ParseArgs: Found SWITCH Object.");
                        objectFound = true;
                        int colonIndex = args[i].LastIndexOf(':');
                        if (colonIndex < args[i].Length)
                        {
                            var nodeIdStr = args[i].Substring(colonIndex + 1);
                            int nodeID = 0;
                            if (Int32.TryParse(nodeIdStr, out nodeID))
                            {
                                Console.WriteLine(string.Format("ParseArgs: SWITCH NodeID:{0}.", nodeID));
                                curCommand = new SwitchCommand(TargetDevice.SWITCH, (byte)nodeID);
                            }
                        }
                    }
                    else if (args[i] == QUIT)
                    {
                        doQuit = true;
                    }

                }
                else
                {
                    objectFound = false;
                    if (curCommand != null)
                    {
                        SwitchCommand sc = curCommand as SwitchCommand;
                        if (sc != null)
                        {
                            switch (args[i])
                            {
                                case "ON":
                                    Console.WriteLine(string.Format("ParseArgs: SWITCH ACTION: {0}.", "ON"));
                                    sc.Action = SwitchAction.ON;
                                    break;
                                case "OFF":
                                    Console.WriteLine(string.Format("ParseArgs: SWITCH ACTION: {0}.", "OFF"));
                                    sc.Action = SwitchAction.OFF;
                                    break;
                                default:
                                    sc.Action = SwitchAction.OFF;
                                    break;
                            }
                            swCmdList.Add(sc);
                        }
                    }
                }
            }

            foreach (var cmd in swCmdList)
            {
                DoSwitchAction(cmd);
                Thread.Sleep(1000);
            }

            if (doQuit)
            {
                zw.ShutdownGracefully();
                exitEvent.Set();
            }
        }

        private static void DoInit()
        {
            zw = new ZWave();
            zw.ZWaveInitializedEvent += zw_ZWaveInitializedEvent;
            zw.ZWaveReadyEvent += zw_ZWaveReadyEvent;
            zw.Initialize();
        }

        private static void RunLoop()
        {
            ParseArgs(argsCache);

            ConsoleKeyInfo key;
            key = Console.ReadKey();
            while (true)
            {
                key = Console.ReadKey();

                switch (key.Key)
                {
                    case ConsoleKey.Escape:
                        zw.ShutdownGracefully();
                        exitEvent.Set();
                        return;
                    case ConsoleKey.R:
                        Console.WriteLine("Reset Command");
                        zw.Controller.Reset();
                        break;
                    case ConsoleKey.Insert:
                        Console.WriteLine("AddNewNodeStart Command");
                        Console.WriteLine("Provide a name for this node");
                        var nodeName = Console.ReadLine();
                        zw.Controller.AddNewNodeStart();
                        break;
               
                    case ConsoleKey.End:
                        Console.WriteLine("AddNewNodeEnd Command");
                        zw.Controller.AddNewNodeStop();
                        break;

                    case ConsoleKey.Spacebar:
                        foreach (var nodeEntry in zw.Controller.Nodes)
                        {
                            var node = nodeEntry.Value;
                            var sw = node as SwitchBinary;
                            ToggleSwitch(sw);
                        }
                        break;

                    case ConsoleKey.D2:

                        if (zw.Controller.Nodes.ContainsKey(2))
                            ToggleSwitch(zw.Controller.Nodes[2] as SwitchBinary);
                        break;

                    case ConsoleKey.D3:

                        if (zw.Controller.Nodes.ContainsKey(3))
                            ToggleSwitch(zw.Controller.Nodes[3] as SwitchBinary);
                        break;

                    case ConsoleKey.D4:

                        if (zw.Controller.Nodes.ContainsKey(4))
                            ToggleSwitch(zw.Controller.Nodes[4] as SwitchBinary);
                        break;

                    case ConsoleKey.D5:

                        if (zw.Controller.Nodes.ContainsKey(5))
                            ToggleSwitch(zw.Controller.Nodes[5] as SwitchBinary);
                        break;
                    default:
                        break;
                }
            }
        }

        private static void DoSwitchAction(SwitchCommand action)
        {
            SwitchBinary switchB;
            if (zw.Controller.Nodes.ContainsKey(action.NodeID))
            {
                switchB = zw.Controller.Nodes[action.NodeID] as SwitchBinary;

                switch (action.Action)
                {
                    case SwitchAction.ON:
                        Console.WriteLine(string.Format("DoSwitchAction: SWITCH ID:{0} Turn: {1}.", action.NodeID, "ON"));
                        switchB.On();
                        break;
                    case SwitchAction.OFF:
                        Console.WriteLine(string.Format("DoSwitchAction: SWITCH ID:{0} Turn: {1}.", action.NodeID, "OFF"));
                        switchB.Off();
                        break;
                    default:
                        break;
                }
            }
        }

        private static byte ToggleSwitch(SwitchBinary sw)
        {
            if (sw != null)
            {
                Console.WriteLine(string.Format("Switch in state: {0}", sw.State.ToString()));
                if (sw.State == 0xff)
                {
                    Console.WriteLine("And then there was darkness...");
                    sw.Off();
                }
                else
                {
                    Console.WriteLine("Let there be light!");
                    sw.On();
                }
            }
            return sw.State;
        }

        static void zw_ZWaveInitializedEvent(object sender, EventArgs e)
        {
            Logger.Trace("");
        }

        static void zw_ZWaveReadyEvent(object sender, EventArgs e)
        {
            Logger.Trace("READY!");
            Task.Factory.StartNew(() =>
            {
                RunLoop();
            });
        }


        private class SwitchCommand : Command
        {
            public SwitchAction Action { get; set; }

            public SwitchCommand(TargetDevice t, byte nodeid)
                : base(t, nodeid)
            {

            }
        }

        private class Command
        {
            public TargetDevice target { get; private set; }
            public byte NodeID { get; private set; }

            public Command(TargetDevice t, byte nodeid)
            {
                target = t;
                NodeID = nodeid;
            }
        }

        private enum TargetDevice
        {
            SWITCH,
            CONTROLLER
        }

        private enum SwitchAction
        {
            ON,
            OFF
        }
    }

    
}
