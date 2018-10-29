using PkmnAdvanceTranslation.Util.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PkmnAdvanceTranslation.ViewModels.Design
{
    public class MainWindowViewModelDesign : MainWindowViewModel
    {
        public MainWindowViewModelDesign() : base(new IOServiceMock())
        {
            TranslationLines.Add(new TranslationItemViewModel(PointerText.FromString("1C5D4B| 1|100|0|N|D|A|Voor sommigen zijn POKéMON huisdieren.[br]Anderen gebruiken ze om te vechten[nb2]Ikzelf[...][nb2]Ik bestudeer POKéMON als beroep.[nb2]")));
            TranslationLines.Add(new TranslationItemViewModel(PointerText.FromString("18D8FE| 0|120|0|N|D|A|[FC]ÉÁ[FD]Á ziet er gelukkig uit[...][nb2][FC]ÉÈDAISY: Daar! Helemaal klaar.[br]Is het niet mooi?[nb2]Hihi[...][br]Het is zo een schattige POKéMON.")));
            TranslationLines.Add(new TranslationItemViewModel(PointerText.FromString("18EAF4| 1|243|0|N|D|B|Er is een e-mail.[nb2][...][nb2]Eindelijk![br]De ultieme TRAINERS van de[nb]POKéMON LEAGUE zijn klaar om[nb]elke uitdaging te aanvaarden![nb2]Breng je beste POKéMON en zie[br]hoe goed je bent als TRAINER.[nb2]POKéMON LEAGUE HQ[br]INDIGO PLATEAU[nb2]PROF. OAK, kom ons bezoeken![br][...]")));
            TranslationLines.Add(new TranslationItemViewModel(PointerText.FromString("18F085| 1| 90|0|N|D|C|[...] [...] [...]  [...] [...] [...][nb2][...] [...] [...]  [...] [...] [...][nb2][...] En klaar![nb2]Nu kunnen er nog meer POKéMON[br]geregistreerd worden.")));
            TranslationLines.Add(new TranslationItemViewModel(PointerText.FromString("18F352| 1|224|0|N|D|C|Hahahaha![br]Welkom in mijn schuilplaats![nb2]Hier schuil ik tot als ik[br]TEAM ROCKET in ere kan herstellen.[nb2]Maar je hebt me opnieuw gevonden.[br]deze keer ga ik voluit![nb2]Je mag nog eens vechten tegen[br]GIOVANNI, de beste TRAINER ooit![FC]ÎéÀ")));
            TranslationLines.Add(new TranslationItemViewModel(PointerText.FromString("18F4A2| 1|200|0|N|D|C|Nu ik zo verloren heb kan ik[br]mijn volgers niet onder ogen komen.[nb]Ik heb hun vertrouwen verraden.[nb2]Vanaf vandaag is TEAM ROCKET[br]voorgoed gedaan.[nb2]Ik ga mijn leven opnieuw[br]aan training wijden.[nb2]Tot nog eens![br]Vaarwel!")));
        }
    }
}
