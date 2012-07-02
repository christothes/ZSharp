/*
 * ZSharp - C# Z-Wave Implementation
 * HiØ 2011
 * H11D13 / iAutomation@Home
 * Author: thomrand
 */

using System;
using System.Collections.Generic;

namespace ZSharp
{
	/// <summary>
	/// Contains
	/// </summary>
	internal static class ZWaveProtocol
	{
        public static class MessageType
        {
            public const byte REQUEST = 0x00;
            public const byte RESPONSE = 0x01;
            public static string GetStringValue(byte val)
            {
                switch (val)
                {
                    case RESPONSE:
                        return "RESPONSE";
                    case REQUEST:
                        return "REQUEST";
                    default:
                        return "? " + val.ToString("X2");
                }
            }
        }

        public static class CommandClass
        {
            public const byte WAKE_UP = 0x84;

            public const byte SWITCH_BINARY = 0x25;

            public const byte SWITCH_MULTILEVEL = 0x26;

            public const byte METER = 0x32;
            public const byte METER_PULSE = 0x35;
            
            public const byte SENSOR_MULTILEVEL = 0x31;
            
            public const byte MANUFACTURER_SPECIFIC = 0x72;
            
            // FUNCTION ADD_NODE_TO_NETWORK
            public const byte NODE_ANY = 0x01;
            public const byte ADD_NODE_LEARN_READY = 0x01;
            public const byte ADD_NODE_STATUS_FOUND = 0x02;
            public const byte ADD_NODE_STATUS_PROTOCOL_DONE = 0x05;
            public const byte ADD_NODE_STOP = 0x05;
            public const byte ADD_NODE_STATUS_ADDING_SLAVE = 0x03;
            public const byte ADD_NODE_STATUS_DONE = 0x06;
            public const byte ADD_NODE_STATUS_FAILED = 0x07;
            
            // FUNCTION REMOVE_NODE_FROM_NETWORK
            public const byte REMOVE_NODE_STOP = 0x05;

            public static string GetStringValue(byte val)
            {
                switch (val)
                {
                    case WAKE_UP :
                        return "WAKE_UP";
            case SWITCH_BINARY :
                        return "SWITCH_BINARY"; 
            case SWITCH_MULTILEVEL :
                        return "SWITCH_MULTILEVEL"; 
            case METER :
                        return "METER"; 
            case METER_PULSE :
                        return "METER_PULSE"; 
            case SENSOR_MULTILEVEL :
                        return "SENSOR_MULTILEVEL";
            case MANUFACTURER_SPECIFIC:
                        return "MANUFACTURER_SPECIFIC";
            case NODE_ANY:
                        return "NODE_ANY or ADD_NODE_LEARN_READY";
            case ADD_NODE_STATUS_FOUND:
                        return "ADD_NODE_STATUS_FOUND";
            case ADD_NODE_STATUS_PROTOCOL_DONE:
                        return "ADD_NODE_STATUS_PROTOCOL_DONE or ADD_NODE_STOP or REMOVE_NODE_STOP";
            case ADD_NODE_STATUS_ADDING_SLAVE:
                        return "ADD_NODE_STATUS_ADDING_SLAVE";
            case ADD_NODE_STATUS_DONE:
                        return "ADD_NODE_STATUS_DONE";
            case ADD_NODE_STATUS_FAILED:
                        return "ADD_NODE_STATUS_FAILED";
                    default:
                        return "? " + val.ToString("X2");
                }
            }
        }

        public static class Command
        {
            public const byte WAKE_UP_INTERVAL_SET = 0x04;

            // SWITCH BINARY COMMANDCLASS
            public const byte SWITCH_BINARY_SET = 0x01;
            public const byte SWITCH_BINARY_GET = 0x02;
            public const byte SWITCH_BINARY_REPORT = 0x03;

            // SWITCH MULTILEVEL COMMANDCLASS
            public const byte SWITCH_MULTILEVEL_SET = 0x01;
            public const byte SWITCH_MULTILEVEL_GET = 0x02;
            public const byte SWITCH_MULTILEVEL_REPORT = 0x03;

            public const byte METER_REPORT_GET = 0x01;
            public const byte MANUFACTURER_SPECIFIC_GET = 0x04;
            public const byte MANUFACTURER_SPECIFIC_REPORT = 0x05;
        }

        public static class Function
        {
            public const byte SEND_DATA = 0x13;
            public const byte APPLICATION_COMMAND_HANDLER = 0x04;
            public const byte GET_VERSION = 0x15;
            public const byte MEMORY_GET_ID = 0x20;
            public const byte SERIAL_API_GET_CAPABILITIES = 0x07;
            public const byte SERIAL_API_INIT_DATA = 0x02;
            public const byte GET_NODE_PROTOCOL_INFO = 0x41;
            public const byte GET_NODE_CAPABILITIES = 0x60;
            public const byte GET_NODE_CAPABILITIES_RESPONSE = 0x49;
            public const byte GET_SUC_NODE_ID = 0x56;
            public const byte SET_SUC_NODE_ID = 0x54;
            public const byte ENABLE_SUC = 0x52;
            public const byte NODEID_SERVER = 0x01;
            public const byte SET_DEFAULT = 0x42;
            public const byte ADD_NODE_TO_NETWORK = 0x4A;
            public const byte REMOVE_NODE_FROM_NETWORK = 0x4B;
            public const byte REQUEST_NODE_NEIGHBOR_UPDATE = 0x48;

            public static string GetStringValue(byte val)
            {
                switch (val)
                {
                    case SEND_DATA :
                        return "SEND_DATA";
            case APPLICATION_COMMAND_HANDLER :
                        return "APPLICATION_COMMAND_HANDLER";
            case GET_VERSION:
                        return "GET_VERSION";
            case MEMORY_GET_ID:
                        return "MEMORY_GET_ID";
            case SERIAL_API_GET_CAPABILITIES:
                        return "SERIAL_API_GET_CAPABILITIES";
            case SERIAL_API_INIT_DATA:
                        return "SERIAL_API_INIT_DATA";
            case GET_NODE_PROTOCOL_INFO:
                        return "GET_NODE_PROTOCOL_INFO";
            case GET_NODE_CAPABILITIES:
                        return "GET_NODE_CAPABILITIES";
            case GET_NODE_CAPABILITIES_RESPONSE:
                        return "GET_NODE_CAPABILITIES_RESPONSE";
            case GET_SUC_NODE_ID:
                        return "GET_SUC_NODE_ID";
            case SET_SUC_NODE_ID:
                        return "SET_SUC_NODE_ID";
            case ENABLE_SUC:
                        return "ENABLE_SUC";
            case NODEID_SERVER:
                        return "NODEID_SERVER";
            case SET_DEFAULT:
                        return "SET_DEFAULT";
            case ADD_NODE_TO_NETWORK:
                        return "ADD_NODE_TO_NETWORK";
            case REMOVE_NODE_FROM_NETWORK:
                        return "REMOVE_NODE_FROM_NETWORK";
            case REQUEST_NODE_NEIGHBOR_UPDATE:
                        return "REQUEST_NODE_NEIGHBOR_UPDATE";
            default:
                        return "? " + val.ToString("X2");
                }
            }
        }

        public static class ValueConstants
        {
            public const byte ON = 0xFF;
            public const byte OFF = 0x00;
            public const long STATE_POLL_INTERVAL = 5000;
            public const long DATA_POLL_INTERVAL = 600000;
            public const long WAKE_UP_INTERVAL = 600000;
        }

        public static class TransmissonOption
        {
            public const byte ACK = 0x1;
            public const byte AUTO_ROUTE = 0x4;
        }

        public static class Type
        {
            public static class Basic
            {
                public const byte CONTROLLER = 0x01;
                public const byte STATIC_CONTROLLER = 0x02;
                public const byte SLAVE = 0x03;
                public const byte ROUTING_SLAVE = 0x04;
            }

            public static class Generic
            {
                public const byte GENERIC_CONTROLLER = 0x01;
                public const byte STATIC_CONTROLLER = 0x02;
                public const byte SWITCH_BINARY = 0x10;
                public const byte SWITCH_MULTILEVEL = 0x11;
                public const byte SENSOR_BINARY = 0x20;
                public const byte SENSOR_MULTILEVEL = 0x21;
                public const byte METER = 0x31;
            }

            public static class Specific
            {
                public const byte POWER_SWITCH_BINARY = 0x01;
            }
        }

        public static class MessageBufferOffsets{
            public const int ResponseType = 0;
            public const int MessageLength = 1;
            public const int MessageStart = 2;
        }
        

        //Message Response Type Headers
        public const byte SOF = 0x01;
        public const byte ACK = 0x06;
        public const byte NAK = 0x15;
        public const byte CAN = 0x18;
	}
}