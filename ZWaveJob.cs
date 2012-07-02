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
using System.Collections.Concurrent;

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
            DebugLogger.Logger.Trace("");
            Utils.SafeEventFire(this, null, ResponseReceived);
        }

        public event EventHandler JobCanceled;
        private void FireJobCanceledEvent()
        {
            DebugLogger.Logger.Trace("");
            Utils.SafeEventFire(this, null, JobCanceled);
        }

        public event EventHandler JobFinished;
        private void FireJobFinishedEvent()
        {
            DebugLogger.Logger.Trace("");
            Utils.SafeEventFire(this, null, JobFinished);
        }

        public event EventHandler ResendRequired;
        private void FireResendRequiredEvent()
        {
            DebugLogger.Logger.Trace("");
            Utils.SafeEventFire(this, null, ResendRequired);
        }

        private ZWaveMessage _request;
        public ZWaveMessage Request
        {
            get { return this._request; }
            set { this._request = value; }
        }

        private ConcurrentQueue<ZWaveMessage> _response = new ConcurrentQueue<ZWaveMessage>();
        public ZWaveMessage GetResponse()
        {
            ZWaveMessage result;
            if (_response.TryDequeue(out result))
                return result;
            else return null;
        }

        public void AddResponse(ZWaveMessage message)
        {
            this.RemoveTimeout();
            this._response.Enqueue(message);
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
            FireResendRequiredEvent();
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

        public int SendCount = 0;
        
        public bool Resend = false;

        public bool IsDone = false;
        public void MarkDone()
        {
            this.IsDone = true;
            this.RemoveTimeout();
            this.FireJobFinishedEvent();
        }

        public bool JobStarted = false;
        public void Start()
        {
            this.JobStarted = true;
        }

        public override string ToString()
        {
            return string.Format(@"
===============================
Zwave Job
===============================
AwaitACK:       {0}
AwaitRespnse:   {1}
IsDone:         {2}
JobStarted:     {3}
Resend:         {4}
SendCount:      {5}
Current RQ:     {6}
Responses:      {7}",
                this.AwaitACK,
                this.AwaitResponse,
                this.IsDone,
                this.JobStarted,
                this.Resend,
                this.SendCount,
                this.Request != null ? this.Request.Function_s : "N/A",
                this._response.Count);
        }
    }
}
