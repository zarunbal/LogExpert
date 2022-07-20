namespace LogExpert.Interface
{
    /// <summary>
    /// Interface which can register at the LogWindow to be informed of pressing ESC.
    /// Used e.g. for cancelling a filter.
    /// </summary>
    public interface BackgroundProcessCancelHandler
    {
        #region Public methods

        /// <summary>
        /// Called when ESC was pressed.
        /// </summary>
        void EscapePressed();

        #endregion
    }
}