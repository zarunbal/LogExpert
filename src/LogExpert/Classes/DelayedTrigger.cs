using System;
using System.Threading;

namespace LogExpert.Classes
{
    /// <summary>
    /// This class receives Trigger calls and sends an event as soons as no more input triggers calls
    /// are received for a given time.
    /// </summary>
    internal class DelayedTrigger
    {
        #region Fields

        private readonly Thread thread = null;
        private readonly EventWaitHandle timerEvent = new ManualResetEvent(false);
        private readonly EventWaitHandle wakeupEvent = new ManualResetEvent(false);
        private bool shouldCancel = false;
        private readonly int waitTime = 0;

        #endregion

        #region cTor

        public DelayedTrigger(int waitTimeMs)
        {
            this.waitTime = waitTimeMs;
            this.thread = new Thread(new ThreadStart(worker));
            this.thread.IsBackground = true;
            this.thread.Start();
        }

        #endregion

        #region Delegates

        public delegate void SignalEventHandler(object sender, EventArgs e);

        #endregion

        #region Events

        public event SignalEventHandler Signal;

        #endregion

        #region Public methods

        public void Trigger()
        {
            this.timerEvent.Set();
            this.wakeupEvent.Set();
        }

        public void TriggerImmediate()
        {
            OnSignal();
        }

        public void Stop()
        {
            this.shouldCancel = true;
            this.wakeupEvent.Set();
        }

        #endregion

        #region Private Methods

        private void worker()
        {
            while (!this.shouldCancel)
            {
                this.wakeupEvent.WaitOne();
                if (this.shouldCancel)
                {
                    return;
                }
                this.wakeupEvent.Reset();

                while (!this.shouldCancel)
                {
                    bool signaled = this.timerEvent.WaitOne(this.waitTime, true);
                    this.timerEvent.Reset();
                    if (!signaled)
                    {
                        break;
                    }
                }
                // timeout with no new Trigger -> send event
                if (!this.shouldCancel)
                {
                    OnSignal();
                }
            }
        }

        #endregion

        protected void OnSignal()
        {
            if (Signal != null)
            {
                Signal(this, new EventArgs());
            }
        }
    }
}