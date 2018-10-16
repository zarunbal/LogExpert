using System;
using System.ComponentModel;

namespace WeifenLuo.WinFormsUI.Docking
{
    [AttributeUsage(AttributeTargets.All)]
    internal sealed class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        #region Private Fields

        private bool m_initialized;

        #endregion

        #region Ctor

        public LocalizedDescriptionAttribute(string key) : base(key)
        {
        }

        #endregion

        #region Properties / Indexers

        public override string Description
        {
            get
            {
                if (!m_initialized)
                {
                    string key = base.Description;
                    DescriptionValue = ResourceHelper.GetString(key);
                    if (DescriptionValue == null)
                    {
                        DescriptionValue = string.Empty;
                    }

                    m_initialized = true;
                }

                return DescriptionValue;
            }
        }

        #endregion
    }

    [AttributeUsage(AttributeTargets.All)]
    internal sealed class LocalizedCategoryAttribute : CategoryAttribute
    {
        #region Ctor

        public LocalizedCategoryAttribute(string key) : base(key)
        {
        }

        #endregion

        #region Overrides

        protected override string GetLocalizedString(string key)
        {
            return ResourceHelper.GetString(key);
        }

        #endregion
    }
}
