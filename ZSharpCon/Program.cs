﻿using System;
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

        static void Main(string[] args)
        {
            DoInit();
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
            ConsoleKeyInfo key;
            key = Console.ReadKey();
            while (true)
            {
                key = Console.ReadKey();

                switch (key.Key)
                {
                    case ConsoleKey.Escape:
                        zw.ShutdownGracefully();
                        return;
                    case ConsoleKey.R:
                        Console.WriteLine("Reset Command");
                        zw.Controller.Reset();
                        break;
                    case ConsoleKey.Insert:
                        Console.WriteLine("AddNewNodeStart Command");
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
                        }
                        break;
                    default:
                        break;
                }
            }
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
    }
}