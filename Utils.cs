using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                buffer2[i] = buffer[i];
            }
            return buffer2;
        }

        internal static string ByteArrayToString(byte[] message)
        {
            string s = string.Empty;
            for (int i = 0; i < message.Length; i++)
            {
                s += message[i];
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
            throw new NotImplementedException();
        }
    }
}
