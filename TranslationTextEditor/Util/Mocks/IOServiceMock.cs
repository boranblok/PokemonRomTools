using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PkmnAdvanceTranslation.Util.Mocks
{
    public class IOServiceMock : IOService
    {
        public string OpenFileDialog(String defaultPath, String title)
        {
            return defaultPath;
        }
    }
}
