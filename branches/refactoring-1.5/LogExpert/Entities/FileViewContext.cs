using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	public class FileViewContext
	{
		internal FileViewContext(ILogPaintContext logPaintContext, ILogView logView)
		{
			this.LogPaintContext = logPaintContext;
			this.LogView = logView;
		}

		public ILogPaintContext LogPaintContext { get; private set; }

		public ILogView LogView { get; private set; }
	}
}