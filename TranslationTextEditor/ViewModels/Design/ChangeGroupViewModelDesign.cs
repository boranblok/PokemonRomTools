using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PkmnAdvanceTranslation.ViewModels.Design
{
    public class ChangeGroupViewModelDesign : ChangeGroupViewModel
    {
        public ChangeGroupViewModelDesign()
        {
            Groups.Add(new GroupViewModel("Group 1", "Group 1"));
            Groups.Add(new GroupViewModel("Group 2", "Group 2"));
            Groups.Add(new GroupViewModel("Group 3", "Group 3"));
            Groups.Add(new GroupViewModel("Group 4", "Group 4"));
        }
    }
}
