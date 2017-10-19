using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LogExpert
{
	/// <summary>
	/// This class receives Trigger calls and sends an event as soons as no more input triggers calls
	/// are received for a given time.
	/// </summary>
	class DelayedTrigger
	{
		private int waitTime = 0;
		private readonly EventWaitHandle wakeupEvent = new ManualResetEvent(false);
		private readonly EventWaitHandle timerEvent = new ManualResetEvent(false);
		private readonly Thread thread = null;
		private bool shouldCancel = false;

		public DelayedTrigger(int waitTimeMs)
		{
			this.waitTime = waitTimeMs;
			this.thread = new Thread(new ThreadStart(worker));
			this.thread.IsBackground = true;
			this.thread.Start();
		}

		private void worker()
		{
			while (!this.shouldCancel)
			{
				this.wakeupEvent.WaitOne();
				if (this.shouldCancel)
					return;
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

		public delegate void SignalEventHandler(object sender, EventArgs e);
		public event SignalEventHandler Signal;
		protected void OnSignal()
		{
			if (Signal != null)
			{
				Signal(this, new EventArgs());
			}
		}
	}
}
