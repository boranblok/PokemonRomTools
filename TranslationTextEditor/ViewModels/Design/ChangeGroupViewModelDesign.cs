using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PkmnAdvanceTranslation.ViewModels.Design
{
    public class ChangeGroupViewModelDesign : ChangeGroupViewModel
    {
        public ChangeGroupViewModelDesign() : base(new ObservableCollection<String>())
        {
            Groups.Add("Group 1");
            Groups.Add("Group 2");
            Groups.Add("Group 3");
            Groups.Add("Group 4");
        }
    }
}
