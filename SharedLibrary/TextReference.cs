using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PkmnAdvanceTranslation
{
    public class TextReference
    {
        public Int32 Address { get; set; }
        public Boolean Repoint { get; set; }

        public override string ToString()
        {
            return (Repoint ? "1" : "0") + "," + Address.ToString("X6");
        }
    }
}
