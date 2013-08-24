using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;

namespace Samba.Presentation.Common.ModelBase
{
    public abstract class ViewModelBase : ObservableObject
    {
        [Browsable(false)]
        public string HeaderInfo
        {
            get { return GetHeaderInfo(); }
        }

        protected abstract string GetHeaderInfo();

        protected void SetActiveView(IEnumerable<VisibleViewModelBase> views, VisibleViewModelBase wm)
        {
            ICollectionView collectionView = CollectionViewSource.GetDefaultView(views);
            if (collectionView != null && collectionView.Contains(wm))
                collectionView.MoveCurrentTo(wm);
        }

        protected VisibleViewModelBase GetActiveView(IEnumerable<VisibleViewModelBase> views)
        {
            ICollectionView collectionView = CollectionViewSource.GetDefaultView(views);
            return collectionView.CurrentItem as VisibleViewModelBase;
        }

#if DEBUG
        ~ViewModelBase()
        {
            string msg = string.Format("{0} ({1}) ({2}) Finalized", GetType().Name, HeaderInfo, GetHashCode());
            System.Diagnostics.Debug.WriteLine(msg);
        }
#endif

    }
}
