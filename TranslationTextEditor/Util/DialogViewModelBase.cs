using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace PkmnAdvanceTranslation.Util
{
    public abstract class DialogViewModelBase : ViewModelBase
    {
        private Boolean _ShowDialog;
        public Boolean ShowDialog
        {
            get { return _ShowDialog; }
            set
            {
                if (value == _ShowDialog)
                    return;

                _ShowDialog = value;
                OnPropertyChanged("ShowDialog");
            }
        }


        public RelayCommand CloseDialogCommand
        {
            get
            {
                return new RelayCommand(param => CloseDialog());
            }
        }

        private void CloseDialog()
        {
            ShowDialog = false;
        }

        public virtual ICommand OnCloseDialogCommand
        {
            get
            {
                return null;
            }
        }

        public virtual Boolean CanCloseDialog()
        {
            return true;
        }
    }
}
