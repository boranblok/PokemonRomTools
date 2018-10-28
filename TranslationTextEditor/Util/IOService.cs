using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PkmnAdvanceTranslation.Util
{
    public interface IOService
    {
        String OpenFileDialog(String defaultPath, String title);

        String SaveFileDialog(String defaultPath, String defaultName, String title, String defaultEx);
    }
}
