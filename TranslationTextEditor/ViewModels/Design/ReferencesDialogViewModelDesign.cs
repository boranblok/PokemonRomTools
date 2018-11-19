using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PkmnAdvanceTranslation.ViewModels.Design
{
    public class ReferencesDialogViewModelDesign : ReferencesDialogViewModel
    {
        public ReferencesDialogViewModelDesign() : base()
        {
            References.Add(new ReferenceViewModel(new TextReference() { Repoint = false, Address = 0xAABBCC }));
            References.Add(new ReferenceViewModel(new TextReference() { Repoint = true, Address = 0x112233 }));
            References.Add(new ReferenceViewModel(new TextReference() { Repoint = false, Address = 0xAA11DD }));
            References.Add(new ReferenceViewModel(new TextReference() { Repoint = true, Address = 0xAA44FF }));
            References.Add(new ReferenceViewModel(new TextReference() { Repoint = false, Address = 0xCCAABB }));
        }
    }
}
