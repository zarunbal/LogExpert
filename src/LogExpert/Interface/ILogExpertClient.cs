namespace LogExpert
{
    public interface ILogExpertClient
    {
        #region Properties / Indexers

        int Id { get; }

        ILogExpertProxy Proxy { get; }

        #endregion

        #region Public Methods

        void NotifySettingsChanged(ILogExpertProxy server, object cookie);

        void OnSettingsChanged(object cookie);

        #endregion
    }
}
