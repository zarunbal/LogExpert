using System;
using System.Threading;

namespace LogExpert
{
    /// <summary>
    ///     This class receives Trigger calls and sends an event as soons as no more input triggers calls
    ///     are received for a given time.
    /// </summary>
    internal class DelayedTrigger
    {
        #region Delegates

        public delegate void SignalEventHandler(object sender, EventArgs e);

        #endregion

        #region Private Fields

        private readonly Thread thread;
        private readonly EventWaitHandle timerEvent = new ManualResetEvent(false);
        private readonly int waitTime;
        private readonly EventWaitHandle wakeupEvent = new ManualResetEvent(false);
        private bool shouldCancel;

        #endregion

        #region Public Events

        public event SignalEventHandler Signal;

        #endregion

        #region Ctor

        public DelayedTrigger(int waitTimeMs)
        {
            waitTime = waitTimeMs;
            thread = new Thread(worker);
            thread.IsBackground = true;
            thread.Start();
        }

        #endregion

        #region Public Methods

        public void Stop()
        {
            shouldCancel = true;
            wakeupEvent.Set();
        }

        public void Trigger()
        {
            timerEvent.Set();
            wakeupEvent.Set();
        }

        public void TriggerImmediate()
        {
            OnSignal();
        }

        #endregion

        #region Event handling Methods

        protected void OnSignal()
        {
            if (Signal != null)
            {
                Signal(this, new EventArgs());
            }
        }

        #endregion

        #region Private Methods

        private void worker()
        {
            while (!shouldCancel)
            {
                wakeupEvent.WaitOne();
                if (shouldCancel)
                {
                    return;
                }

                wakeupEvent.Reset();

                while (!shouldCancel)
                {
                    bool signaled = timerEvent.WaitOne(waitTime, true);
                    timerEvent.Reset();
                    if (!signaled)
                    {
                        break;
                    }
                }

                // timeout with no new Trigger -> send event
                if (!shouldCancel)
                {
                    OnSignal();
                }
            }
        }

        #endregion
    }
}
