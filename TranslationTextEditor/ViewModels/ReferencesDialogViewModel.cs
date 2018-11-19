using PkmnAdvanceTranslation.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PkmnAdvanceTranslation.ViewModels
{
    public class ReferencesDialogViewModel : DialogViewModelBase
    {

        public ReferencesDialogViewModel()
        {
            Title = "Edit references";

            References = new ObservableCollection<ReferenceViewModel>();
            ReferencesView = CollectionViewSource.GetDefaultView(References);
        }

        public ObservableCollection<ReferenceViewModel> References { get; private set; }
        public ICollectionView ReferencesView { get; private set; }
    }
}
