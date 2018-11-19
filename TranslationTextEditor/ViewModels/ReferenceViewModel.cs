using PkmnAdvanceTranslation.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PkmnAdvanceTranslation.ViewModels
{
    public class ReferenceViewModel : ViewModelBase
    {
        private TextReference _reference;
        public ReferenceViewModel(TextReference reference)
        {            
            _reference = reference;
        }

        public Boolean UseReference
        {
            get
            {
                return _reference.Repoint;
            }
            set
            {
                if (value == _reference.Repoint)
                    return;

                _reference.Repoint = value;
                OnPropertyChanged("UseReference");
            }
        }

        public String Address
        {
            get
            {
                return _reference.Address.ToString("X6");
            }
        }
    }
}
