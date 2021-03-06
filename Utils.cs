﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZSharp
{
    public static class Utils
    {
        // Methods
        internal static byte[] ByteSubstring(byte[] buffer, int startIndex, int length)
        {
            byte[] buffer2 = new byte[length];
            for (int i = 0; i < length; i++)
            {
                buffer2[i] = buffer[i + startIndex];
            }
            return buffer2;
        }

        internal static string ByteArrayToString(byte[] message)
        {
            string s = string.Empty;
            for (int i = 0; i < message.Length; i++)
            {
                s += message[i].ToString("X2") + " ";
            }
            
            return s;
        }

        internal enum VersionType
        {
            API,
            SDK
        }

        internal static string VersionToString(byte[] p, VersionType versionType)
        {
            string s = string.Empty;
            foreach (var b in p)
            {
                s += b;
            }
            return s;
        }

        internal static string GenericTypeToString(byte genericType)
        {
            switch (genericType)
            {
                case 0x01:
                    return "GENERIC_CONTROLLER";
                case 0x02:
                    return "STATIC_CONTROLLER";
                case 0x10:
                    return "SWITCH_BINARY";
                case 0x11:
                    return "SWITCH_MULTILEVEL";
                case 0x20:
                    return "SENSOR_BINARY";
                case 0x21:
                    return "SENSOR_MULTILEVEL";
                case 0x31:
                    return "METER";
                default:
                    return "Unknown";
            }
        }

        public static void SafeEventFire(object sender, EventArgs e, EventHandler evt)
        {
            var tmpEvt = Interlocked.CompareExchange(ref evt, null, null);
            if (tmpEvt != null)
                tmpEvt(sender, e);
        }
    }
}
