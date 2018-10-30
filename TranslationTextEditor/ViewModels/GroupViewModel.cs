using PkmnAdvanceTranslation.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PkmnAdvanceTranslation.ViewModels
{
    public class GroupViewModel : ViewModelBase
    {
        public GroupViewModel(String title, String value)
        {
            Title = title;
            Value = value;
        }

        public String Value { get; set; }
    }
}
