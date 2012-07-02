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
            Utils.SafeEventFire(this, new UnsubscribedMessageEventArgs(message), UnsubscribedMessageEvent);
        }
        
        private SerialPort _sp;
        private byte[] buff2 = new byte[1024];

        private ConcurrentQueue<ZWaveJob> _jobQueue = new ConcurrentQueue<ZWaveJob>();

        /// <summary>
        /// The current ZwaveJob. This is just held as the message on the top of the _jobQueue. The current job is only dequeued once it is complete.
        /// </summary>
        private ZWaveJob CurrentJob
        {
            get
            {
                ZWaveJob curJob = null;
                if (_jobQueue.TryPeek(out curJob))
                {
                    return curJob;
                }
                else return null;
            }
        }

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
			
			//this._runner = new Thread(new ThreadStart(Run));
		}

        /// <summary>
        /// Handles the event and dumps all bytes to be read to a buffer to be handled by DataReceivedHandler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var sp = sender as SerialPort;
            var cnt = sp.BytesToRead;
            DebugLogger.Logger.Trace(string.Format("Event Type {0}, Bytes: {1}", e.EventType.ToString(), cnt));

            if (cnt > 0)
            {
                byte[] ba = new byte[cnt];
                sp.Read(ba, 0, cnt);
                DataReceivedHandler(ba);
            }
        }

        /// <summary>
        /// Handler when data is received on the port
        /// </summary>
        /// <param name="buf">buffer received on the port</param>
        void DataReceivedHandler(byte[] buf)
        {            
            var job = this.CurrentJob;

            switch (buf[ZWaveProtocol.MessageBufferOffsets.ResponseType])
            {
                case ZWaveProtocol.SOF:
                    DebugLogger.Logger.Trace("Received: SOF");
                    ProcessMessage(new ArraySegment<byte>(buf), job);
                    break;
                case ZWaveProtocol.CAN:
                    DebugLogger.Logger.Trace("Received: CAN");
                    ResendCurrentJob(resendReason.ReceivedCAN);
                    break;
                case ZWaveProtocol.NAK:
                    DebugLogger.Logger.Trace("Received: NAK");
                    ResendCurrentJob(resendReason.ReceivedNAK);
                    break;
                case ZWaveProtocol.ACK:
                    DebugLogger.Logger.Trace("Received: ACK");
                    HandleACKForCurrentJob();
                    if(buf.Length > 1 && buf[ZWaveProtocol.MessageBufferOffsets.ResponseType + 1] == ZWaveProtocol.SOF)
                        ProcessMessage(new ArraySegment<byte>(buf, 1, buf.Length - 1), job);
                    break;
                default:
                    DebugLogger.Logger.Trace("Critical error. Out of frame flow.");
                    break;
            }
        }

        /// <summary>
        /// Message processing
        /// </summary>
        /// <param name="bufSeg">An ArraySegment for the buffer to be processed</param>
        /// <param name="job">The ZwaveJob to be processed</param>
        private void ProcessMessage(ArraySegment<byte> bufSeg, ZWaveJob job)
        {
            // Read the length byte
            byte len = bufSeg.Array[ZWaveProtocol.MessageBufferOffsets.MessageLength + bufSeg.Offset];

            // Read rest of the frame
            byte[] message = Utils.ByteSubstring(bufSeg.Array,
                bufSeg.Offset,
                bufSeg.Count);
            DebugLogger.Logger.Trace("Received: " + Utils.ByteArrayToString(message));

            ZWaveMessage zMessage = null;
            try
            {
                zMessage = new ZWaveMessage(message);
                //Checksum is correct
                SendACKToPort();
            }
            catch (MessageChecksumInvalidException ex)
            {
                DebugLogger.Logger.Error("Message Checksum invalid. Sending NAK.\nMessage: {0}", Utils.ByteArrayToString(message));
                SendNAKToPort();
                return;
            }
            if (job == null)
            {
                // Incoming response?
                DebugLogger.Logger.Trace("*** Incoming response");
                this.FireUnsubscribedMessageEvent(zMessage);
            }
            else
            {
                if (job.AwaitACK)
                {
                    // We wanted an ACK instead. Resend...
                    ResendCurrentJob(resendReason.ExpectingACK);
                }
                else
                {
                    job.AddResponse(zMessage);
                    this.FireUnsubscribedMessageEvent(zMessage);
                }
            }
        }

        //Send a NAK (Negative Acknowledgement) to the port
        private void SendNAKToPort()
        {
            DebugLogger.Logger.Trace("Sending: NAK");
            this._sp.Write(new byte[] { ZWaveProtocol.NAK }, 0, 1);            
        }

        /// <summary>
        /// Send an ACK to the port
        /// </summary>
        private void SendACKToPort()
        {
            DebugLogger.Logger.Trace("Sending: ACK");
            this._sp.Write(new byte[] { ZWaveProtocol.ACK }, 0, 1);
        }

        /// <summary>
        /// Set appropriate job fields when an ACK is received
        /// </summary>
        private void HandleACKForCurrentJob()
        {
            var job = this.CurrentJob;
            if (job != null)
            {
                if (job.AwaitACK && !job.AwaitResponse)
                {
                    job.AwaitResponse = true;
                    job.AwaitACK = false;
                }
            }
        }

        /// <summary>
        /// Resends the CurrentJob to the port
        /// </summary>
        /// <param name="reason">The reason the job is to be resent. This affects which flags are set on the job object</param>
        private void ResendCurrentJob(resendReason reason)
        {            
            var job = this.CurrentJob;
            DebugLogger.Logger.Trace(string.Format("{0}", job.ToString()));

            switch (reason)
            {
                case resendReason.ExpectingACK:
                    job.AwaitResponse = false;
                    break;
                case resendReason.ReceivedNAK:
                case resendReason.ReceivedCAN:
                    job.AwaitACK = false;
                    job.JobStarted = false;
                    break;
                default:
                    break;
            }
            SendJob(job);
        }

        /// <summary>
        /// Sends a job to the port. If the job Sendcount is >= 3 the job will be canceled
        /// </summary>
        /// <param name="job">ZwaveJob to be sent</param>
        private void SendJob(ZWaveJob job)
        {
            DebugLogger.Logger.Trace(string.Format("{0}", job.ToString()));
            if (job.SendCount >= 3)
            {
                job.CancelJob();
            }
            else
            {
                ZWaveMessage msg = job.Request;
                if (msg != null)
                {
                    DebugLogger.Logger.Trace(string.Format("Sending Message:{0}", msg.ToString()));
                    job.Start();
                    job.Resend = false;
                    job.AwaitACK = true;
                    job.SendCount++;
                    this._sp.Write(msg.Message, 0, msg.Message.Length);
                }
            }
        }

        /// <summary>
        /// Handles processing of the next job on the queue
        /// </summary>
        private void ProcessJobQueue()
        {
            var job = this.CurrentJob;
            DebugLogger.Logger.Trace(string.Format("{0}", job.ToString()));
            if (job == null || job.IsDone)
            {
                if (_jobQueue.TryDequeue(out job))
                {
                    DebugLogger.Logger.Trace(string.Format("Current Job null or Complete. Dequeueing."));
                    job.JobFinished -= job_JobFinished;
                    job.ResendRequired -= job_ResendRequired;
                    job = this.CurrentJob;
                    DebugLogger.Logger.Trace(string.Format("Current Job: {0}", job != null ? job.ToString() : "null"));                    
                }
                else
                {
                    DebugLogger.Logger.Error("Error DeQueuing finished CurrentJob");
                }
            }

            if (job != null)
            {
                job.JobFinished += job_JobFinished;
                job.ResendRequired += job_ResendRequired;
                SendJob(job);
            }
        }

        /// <summary>
        /// Event handler for job resend
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void job_ResendRequired(object sender, EventArgs e)
        {
            var job = sender as ZWaveJob;
            SendJob(job);
        }

        /// <summary>
        /// Event handler when job IsDone is set
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void job_JobFinished(object sender, EventArgs e)
        {           
            ProcessJobQueue();
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
                    //this._runner.Start();
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

        /// <summary>
        /// Close the port
        /// </summary>
        public void Close()
        {
            this._sp.Close();
            this._sp.Dispose();
        }
		
		/// <summary>
		/// Enqueue a new job to be sent by the controller
		/// </summary>
        public void EnqueueJob(ZWaveJob job)
        {
            this._jobQueue.Enqueue(job);
            if (job == CurrentJob)
            {
                //Process the new job
                ProcessJobQueue();
            }
        }		

        private enum resendReason
        {
            ExpectingACK,
            ReceivedNAK,
            ReceivedCAN
        }
	}
}
