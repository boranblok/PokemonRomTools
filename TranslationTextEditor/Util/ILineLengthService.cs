using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PkmnAdvanceTranslation.Util
{
    public interface ILineLengthService
    {
        double MeasureStringWidth(String candidate);
    }
}
