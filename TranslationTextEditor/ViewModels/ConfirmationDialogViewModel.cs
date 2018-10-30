using GalaSoft.MvvmLight.CommandWpf;
using PkmnAdvanceTranslation.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PkmnAdvanceTranslation.ViewModels
{
    public class ConfirmationDialogViewModel : DialogViewModelBase
    {
        public ConfirmationDialogViewModel(String title, String message)
        {
            Message = message;
            Title = title;
        }

        public String Message { get; set; }

        public Boolean Confirmed { get; set; }

        public RelayCommand OkCommand
        {
            get
            {
                return new RelayCommand(Confirm);
            }
        }

        public void Confirm()
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

        public void Cancel()
        {
            Confirmed = false;
            ShowDialog = false;
        }
    }
}
