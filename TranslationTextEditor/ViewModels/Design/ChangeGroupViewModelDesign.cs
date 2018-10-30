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
            Groups.Add("Group 1");
            Groups.Add("Group 2");
            Groups.Add("Group 3");
            Groups.Add("Group 4");
        }
    }
}
