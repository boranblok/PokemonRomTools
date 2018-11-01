using GalaSoft.MvvmLight.CommandWpf;
using PkmnAdvanceTranslation.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PkmnAdvanceTranslation.ViewModels
{
    public class ChangeTextModeViewModel : DialogViewModelBase
    {
        private Boolean _isSpecialDialog;

        public ChangeTextModeViewModel()
        {
            Title = "Select if lines are special dialog or not.";            
        }

        public Boolean IsSpecialDialog
        {
            get
            {
                return _isSpecialDialog;
            }
            set
            {
                if (value == _isSpecialDialog)
                    return;

                _isSpecialDialog = value;
                OnPropertyChanged("IsSpecialDialog");
            }
        }


        public Boolean Confirmed { get; set; }

        public RelayCommand OkCommand
        {
            get
            {
                return new RelayCommand(Confirm);
            }
        }

        private void Confirm()
        {
            Confirmed = true;
            ShowDialog = false;
        }

        public RelayCommand CancelCommand
        {
            get
            {
                return new RelayCommand(Cancel);
            }
        }

        private void Cancel()
        {
            Confirmed = false;
            ShowDialog = false;
        }
    }
}
