/*
 * ZSharp - C# Z-Wave Implementation
 * HiØ 2011
 * H11D13 / iAutomation@Home
 * Author: thomrand
 */

using System;
using ZSharp.Nodes;
using System.Collections.Generic;
using System.Timers;

namespace ZSharp
{
	/// <summary>
	/// Description of ZWaveJob.
	/// </summary>
	internal class ZWaveJob
	{
        // Define our events
        public event EventHandler ResponseReceived;
        private void FireResponseReceivedEvent()
        {
            if (this.ResponseReceived != null)
                this.ResponseReceived(this, null);
        }

        public event EventHandler JobCanceled;
        private void FireJobCanceledEvent()
        {
            if (this.JobCanceled != null)
                this.JobCanceled(this, null);
        }

        private ZWaveMessage _request;
        public ZWaveMessage Request
        {
            get { return this._request; }
            set { this._request = value; }
        }

        private Queue<ZWaveMessage> _response = new Queue<ZWaveMessage>();
        public ZWaveMessage GetResponse()
        {
            lock (this._responseLock)
            {
                if (this._response.Count > 0) return this._response.Dequeue();
                else return null;
            }
        }

        public void AddResponse(ZWaveMessage message)
        {
            this.RemoveTimeout();
            lock (this._responseLock) { this._response.Enqueue(message); }
            this.FireResponseReceivedEvent();
        }

        public void CancelJob()
        {
            DebugLogger.Logger.Trace("*** Canceled");
            
            this.MarkDone();
            this._awaitACK = false;
            this._awaitResponse = false;
            this.FireJobCanceledEvent();
        }

        public void TriggerResend()
        {
            DebugLogger.Logger.Trace("*** Trigger resend");
            
            this._awaitACK = false;
            this._awaitResponse = false;
            this.Resend = true;
        }

        private Timer _timeout;
        public void SetTimeout(int interval)
        {
            this._timeout = new Timer(interval);
            this._timeout.Elapsed += Timeout;
            this._timeout.Start();
        }

        public void RemoveTimeout()
        {
            if (this._timeout != null)
            {
                this._timeout.Elapsed -= Timeout;
                this._timeout.Dispose();
                this._timeout = null;
            }
        }

        private void Timeout(object sender, EventArgs e)
        {
            this.TriggerResend();
        }

        // Messaging control-switch
        private bool _awaitACK = false;
        public bool AwaitACK
        {
            get { return this._awaitACK; }
            set { this._awaitACK = value; }
        }

        private bool _awaitResponse = false;
        public bool AwaitResponse
        {
            get { return this._awaitResponse; }
            set { this._awaitResponse = value; }
        }

        private Object _responseLock = new Object();
        public int SendCount = 0;
        
        public bool Resend = false;

        public bool IsDone = false;
        public void MarkDone()
        {
            this.IsDone = true;
            this.RemoveTimeout();
        }

        public bool JobStarted = false;
        public void Start()
        {
            this.JobStarted = true;
        }

        public ZWaveJob() { }
    }
}
