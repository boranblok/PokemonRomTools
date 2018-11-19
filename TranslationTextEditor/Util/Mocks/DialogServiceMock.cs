using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PkmnAdvanceTranslation.Util.Mocks
{
    public class DialogServiceMock : IDialogService
    {
        public DialogViewModelBase DialogViewModel { get; set; }
    }
}
