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
	public class DelayedTrigger
	{
		private int waitTime = 0;
		private readonly EventWaitHandle _wakeupEvent = new ManualResetEvent(false);
		private readonly EventWaitHandle _timerEvent = new ManualResetEvent(false);
		private readonly Thread _thread = null;
		private bool _shouldCancel = false;

		public DelayedTrigger(int waitTimeMs)
		{
			waitTime = waitTimeMs;
			_thread = new Thread(new ThreadStart(Worker));
			_thread.IsBackground = true;
			_thread.Start();
		}

		private void Worker()
		{
			while (!_shouldCancel)
			{
				_wakeupEvent.WaitOne();
				if (_shouldCancel)
					return;
				_wakeupEvent.Reset();

				while (!_shouldCancel)
				{
					bool signaled = _timerEvent.WaitOne(waitTime, true);
					_timerEvent.Reset();
					if (!signaled)
					{
						break;
					}
				}
				// timeout with no new Trigger -> send event
				if (!_shouldCancel)
				{
					OnSignal();
				}
			}
		}

		public void Trigger()
		{
			_timerEvent.Set();
			_wakeupEvent.Set();
		}

		public void TriggerImmediate()
		{
			OnSignal();
		}

		public void Stop()
		{
			_shouldCancel = true;
			_wakeupEvent.Set();
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