using System.Resources;

namespace WeifenLuo.WinFormsUI.Docking
{
    internal static class ResourceHelper
    {
        #region Static/Constants

        private static ResourceManager _resourceManager;

        #endregion

        #region Properties / Indexers

        private static ResourceManager ResourceManager
        {
            get
            {
                if (_resourceManager == null)
                {
                    _resourceManager = new ResourceManager("WeifenLuo.WinFormsUI.Docking.Strings",
                        typeof(ResourceHelper).Assembly);
                }

                return _resourceManager;
            }
        }

        #endregion

        #region Public Methods

        public static string GetString(string name)
        {
            return ResourceManager.GetString(name);
        }

        #endregion
    }
}
