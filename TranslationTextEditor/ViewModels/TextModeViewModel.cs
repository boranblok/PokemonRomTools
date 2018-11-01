using PkmnAdvanceTranslation.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PkmnAdvanceTranslation.ViewModels
{
    public class TextModeViewModel : ViewModelBase
    {
        public TextModeViewModel(String title, TextMode textMode)
        {
            Title = title;
            TextMode = textMode;
        }

        public TextMode TextMode { get; private set; }
    }
}
