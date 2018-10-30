using PkmnAdvanceTranslation.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PkmnAdvanceTranslation.ViewModels
{
    public class ContainsViewModel : ViewModelBase
    {
        public ContainsViewModel(String title, ContainFilterMode filterMode)
        {
            Title = title;
            FilterMode = filterMode;
        }

        public ContainFilterMode FilterMode { get; private set; }
    }
}
