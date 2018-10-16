using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WeifenLuo.WinFormsUI.Docking
{
    public class FloatWindowCollection : ReadOnlyCollection<FloatWindow>
    {
        #region Ctor

        internal FloatWindowCollection()
            : base(new List<FloatWindow>())
        {
        }

        #endregion

        internal int Add(FloatWindow fw)
        {
            if (Items.Contains(fw))
            {
                return Items.IndexOf(fw);
            }

            Items.Add(fw);
            return Count - 1;
        }

        internal void Dispose()
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                this[i].Close();
            }
        }

        internal void Remove(FloatWindow fw)
        {
            Items.Remove(fw);
        }

        internal void BringWindowToFront(FloatWindow fw)
        {
            Items.Remove(fw);
            Items.Add(fw);
        }
    }
}
