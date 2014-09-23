using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	/// <summary>
	/// Interface which can register at the LogWindow to be informed of pressing ESC.
	/// Used e.g. for cancelling a filter.
	/// </summary>
	public interface BackgroundProcessCancelHandler
	{
		/// <summary>
		/// Called when ESC was pressed.
		/// </summary>
		void EscapePressed();
	}
}