using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PkmnAdvanceTranslation.Util.Mocks
{
    public class LineLengthServiceMock : ILineLengthService
    {
        public Double MeasureStringWidth(String candidate)
        {
            return candidate.Length * 5.5;
        }
    }
}
