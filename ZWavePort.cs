/*
 * ZSharp - C# Z-Wave Implementation
 * HiØ 2011
 * H11D13 / iAutomation@Home
 * Author: thomrand
 */

using System;
using System.IO.Ports;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.Concurrent;


namespace ZSharp
{
	/// <summary>
	/// A class that deals with low level serial communications with a Z-Wave USB Controller.
	/// </summary>
	internal class ZWavePort
	{
        public class UnsubscribedMessageEventArgs : EventArgs
        {
            public ZWaveMessage Message;
            public UnsubscribedMessageEventArgs(ZWaveMessage message) : base()
            {
                this.Message = message;
            }
        }

        public event EventHandler UnsubscribedMessageEvent;
        public void FireUnsubscribedMessageEvent(ZWaveMessage message)
        {
            if (this.UnsubscribedMessageEvent != null)
                this.UnsubscribedMessageEvent(this, new UnsubscribedMessageEventArgs(message));
        }
        
        private SerialPort _sp;
		private Thread _runner;
        private Object _queueLock = new Object();
        private byte[] buff2 = new byte[1024];

        private ConcurrentQueue<ZWaveJob> JobQueue = new ConcurrentQueue<ZWaveJob>();

		/// <summary>
		/// Create and initialize a new communication port.
		/// </summary>
		public ZWavePort()
		{
			this._sp = new SerialPort();
            _sp.DataReceived += _sp_DataReceived;
			this._sp.Parity = Parity.None;
			this._sp.BaudRate = 115200;
			this._sp.Handshake = Handshake.None;
			this._sp.StopBits = StopBits.One;
			this._sp.DtrEnable = true;
			this._sp.RtsEnable = true;
			this._sp.NewLine = Environment.NewLine;
			
			this._runner = new Thread(new ThreadStart(Run));
		}

        void _sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var sp = sender as SerialPort;
            DebugLogger.Logger.Trace(string.Format("Event Type {0}, Bytes: {1}", e.EventType.ToString(), sp.BytesToRead));


        }
		
		/// <summary>
		/// Open the port.
		/// </summary>
		public bool Open()
		{
            DebugLogger.Logger.Trace("Opening port: " + this._sp.PortName);
            if (!this._sp.IsOpen)
            {
                for (int i = 5; i > 0; i--)
                {
                    String port = "COM" + i;
                    this._sp.PortName = port;

                    try
                    {
                        this._sp.Open();
                    }
                    catch (Exception e)
                    {
                        DebugLogger.Logger.Error(string.Format("ZWave controller not found at port: {0}\n{1}", this._sp.PortName, e.ToString()));
                    }

                    if (this._sp.IsOpen)
                        break;
                }

                if (this._sp.IsOpen)
                {
                    DebugLogger.Logger.Trace("Found ZWave controller at port: " + this._sp.PortName);
                    this._runner.Start();
                    return true;
                }
                else
                {
                    DebugLogger.Logger.Error("ZWave controller not found");
                    return false;
                }
            }
            else
            {
                return true;
            }
		}

        public void Close()
        {
            this._sp.Close();
            this._sp.Dispose();
        }
		
		/// <summary>
		///
		/// </summary>
        private void Run()
        {
            byte[] buf = new byte[1024];
            ZWaveJob _currentJob = null;

            while (this._sp.IsOpen)
            {             

                //We work on the current job until its done. Only dequeue the next job after that
                if (_currentJob == null || _currentJob.IsDone)
                {
                    if (_currentJob != null && _currentJob.IsDone)
                        DebugLogger.Logger.Trace("Job IsDone = True, Dequeueing Job");

                    if (JobQueue.TryDequeue(out _currentJob))
                    {
                        if (_currentJob.IsDone)
                        {
                            continue;
                        }
                    }
                }

                if (_currentJob != null)
                    DebugLogger.Logger.Trace("CurrentJob = \n{0}", _currentJob.ToString());

                // Check for incoming messages
                int btr = this._sp.BytesToRead;
                if (btr > 0)
                {
                    // Read first byte
                    this._sp.Read(buf, 0, 1);
                    switch (buf[0])
                    {
                        case ZWaveProtocol.SOF:

                            // Read the length byte
                            this._sp.Read(buf, 1, 1);
                            byte len = buf[1];

                            // Read rest of the frame
                            this._sp.Read(buf, 2, len);
                            byte[] message = Utils.ByteSubstring(buf, 0, (len + 2));
                            DebugLogger.Logger.Trace("Received: " + Utils.ByteArrayToString(message));

                            // Verify checksum
                            if (message[(message.Length - 1)] == CalculateChecksum(Utils.ByteSubstring(message, 0, (message.Length - 1))))
                            {
                                ZWaveMessage zMessage = new ZWaveMessage(message);

                                if (_currentJob == null)
                                {
                                    // Incoming response?
                                    this.FireUnsubscribedMessageEvent(zMessage);
                                    DebugLogger.Logger.Trace("*** Incoming response");
                                }
                                else
                                {
                                    if (_currentJob.AwaitACK)
                                    {
                                        // We wanted an ACK instead. Resend...
                                        _currentJob.AwaitACK = false;
                                        _currentJob.AwaitResponse = false;
                                        _currentJob.Resend = true;
                                    }
                                    else
                                    {
                                        _currentJob.AddResponse(zMessage);
                                        this.FireUnsubscribedMessageEvent(zMessage);
                                    }
                                }

                                // Send ACK - Checksum is correct
                                this._sp.Write(new byte[] { ZWaveProtocol.ACK }, 0, 1);
                                DebugLogger.Logger.Trace("Sent: ACK");
                            }
                            else
                            {
                                // Send NAK
                                this._sp.Write(new byte[] { ZWaveProtocol.NAK }, 0, 1);
                                DebugLogger.Logger.Trace("Sent: NAK");
                            }

                            break;
                        case ZWaveProtocol.CAN:
                            DebugLogger.Logger.Trace("Received: CAN");
                            break;
                        case ZWaveProtocol.NAK:
                            DebugLogger.Logger.Trace("Received: NAK");
                            _currentJob.AwaitACK = false;
                            _currentJob.JobStarted = false;
                            break;
                        case ZWaveProtocol.ACK:
                            DebugLogger.Logger.Trace("Received: ACK");
                            if (_currentJob != null)
                            {
                                if (_currentJob.AwaitACK && !_currentJob.AwaitResponse)
                                {
                                    _currentJob.AwaitResponse = true;
                                    _currentJob.AwaitACK = false;
                                }
                            }
                            break;
                        default:
                            DebugLogger.Logger.Trace("Critical error. Out of frame flow.");
                            break;
                    }
                }
                else
                {
                    if (_currentJob != null)
                    {
                        if (_currentJob.SendCount >= 3)
                        {
                            _currentJob.CancelJob();
                        }

                        if ((!_currentJob.JobStarted && !_currentJob.IsDone) || _currentJob.Resend)
                        {
                            ZWaveMessage msg = _currentJob.Request;
                            if (msg != null)
                            {
                                DebugLogger.Logger.Trace(string.Format("Sending Message:{0}", msg.ToString()));
                                this._sp.Write(msg.Message, 0, msg.Message.Length);
                                _currentJob.Start();
                                _currentJob.Resend = false;
                                _currentJob.AwaitACK = true;
                                _currentJob.SendCount++;                                
                            }
                        }
                    }
                }

                Thread.Sleep(100);
            }
        }
		
		/// <summary>
		/// 
		/// </summary>
        public void EnqueueJob(ZWaveJob job)
        {
            this.JobQueue.Enqueue(job);
        }

		public static byte CalculateChecksum(byte[] message)
		{
			byte chksum = 0xff;
			for(int i = 1; i < message.Length; i++)
			{
				chksum ^= (byte)message[i];
			}
			return chksum;
		}
	}
}
