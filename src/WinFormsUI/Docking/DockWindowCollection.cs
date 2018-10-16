using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WeifenLuo.WinFormsUI.Docking
{
    public class DockWindowCollection : ReadOnlyCollection<DockWindow>
    {
        #region Ctor

        internal DockWindowCollection(DockPanel dockPanel)
            : base(new List<DockWindow>())
        {
            Items.Add(new DockWindow(dockPanel, DockState.Document));
            Items.Add(new DockWindow(dockPanel, DockState.DockLeft));
            Items.Add(new DockWindow(dockPanel, DockState.DockRight));
            Items.Add(new DockWindow(dockPanel, DockState.DockTop));
            Items.Add(new DockWindow(dockPanel, DockState.DockBottom));
        }

        #endregion

        #region Properties / Indexers

        public DockWindow this[DockState dockState]
        {
            get
            {
                if (dockState == DockState.Document)
                {
                    return Items[0];
                }

                if (dockState == DockState.DockLeft || dockState == DockState.DockLeftAutoHide)
                {
                    return Items[1];
                }

                if (dockState == DockState.DockRight || dockState == DockState.DockRightAutoHide)
                {
                    return Items[2];
                }

                if (dockState == DockState.DockTop || dockState == DockState.DockTopAutoHide)
                {
                    return Items[3];
                }

                if (dockState == DockState.DockBottom || dockState == DockState.DockBottomAutoHide)
                {
                    return Items[4];
                }

                throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
    }
}
