using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZSharp;

namespace ZSharp.Test
{
    [TestClass]
    public class ZwaveTest
    {
        [TestMethod]
        public void DummyTestForPlayingWithZwave()
        {
            ZWave zw = new ZWave();
            zw.Initialize();

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
