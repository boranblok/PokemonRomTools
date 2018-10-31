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
            TranslationLines.Add(new TranslationItemViewModel(PointerText.FromString("1C5D4B| 1|113|0|D|Introduction                            |For some people, POKéMON are pets.[br]Others use them for battling.[nb2]As for myself[...][nb2]I study POKéMON as a profession.[nb2]|Voor sommigen zijn POKéMON huisdieren.[br]Anderen gebruiken ze om te vechten[nb2]Ikzelf[...][nb2]Ik bestudeer POKéMON als beroep.[nb2]")));
            TranslationLines.Add(new TranslationItemViewModel(PointerText.FromString("18D8FE| 1|124|0|D|Rival's House                           |[small][var1] looks dreamily content[...][nb2][fnt]ÈDAISY: There you go! All done.[br]See? Doesn't it look nice?[nb2]Giggle[...][br]It's such a cute POKéMON.|[small][var1] ziet er gelukkig uit[...][nb2][fnt]ÈDAISY: Daar! Helemaal klaar.[br]Is het niet mooi?[nb2]Hihi[...][br]Het is zo een schattige POKéMON.")));
            TranslationLines.Add(new TranslationItemViewModel(PointerText.FromString("18EAF4| 1|241|0|D|Oak's Lab                               |There's an e-mail message here.[nb2][...][nb2]Finally![br]The ultimate TRAINERS of the[nb]POKéMON LEAGUE are ready to[nb]take on all comers![nb2]Bring your best POKéMON and see[br]how you rate as a TRAINER![nb2]POKéMON LEAGUE HQ[br]INDIGO PLATEAU[nb2]PROF. OAK, please visit us![br][...]|Er is een e-mail.[nb2][...][nb2]Eindelijk![br]De ultieme TRAINERS van de[nb]POKéMON LEAGUE zijn klaar om[nb]elke uitdaging te aanvaarden![nb2]Breng je beste POKéMON en zie[br]hoe goed je bent als TRAINER.[nb2]POKéMON LEAGUE HQ[br]INDIGO PLATEAU[nb2]PROF. OAK, kom ons bezoeken![br][...]")));
            TranslationLines.Add(new TranslationItemViewModel(PointerText.FromString("18F085| 1| 98|0|D|Oak's Lab                               |[...] [...] [...]  [...] [...] [...][nb2][...] [...] [...]  [...] [...] [...][nb2][...]And that's done![nb2]Now these units can record data on[br]a lot more POKéMON.|[...] [...] [...]  [...] [...] [...][nb2][...] [...] [...]  [...] [...] [...][nb2][...] En klaar![nb2]Nu kunnen er nog meer POKéMON[br]geregistreerd worden.")));
            TranslationLines.Add(new TranslationItemViewModel(PointerText.FromString("18F352| 1|236|0|D|Viridian Gym                            |Fwahahaha![br]Welcome to my hideout![nb2]It shall be so until I can restore[br]TEAM ROCKET to its former glory.[nb2]But, you have found me again.[br]So be it.[nb]This time, I'm not holding back![nb2]Once more, you shall face[br]GIOVANNI, the greatest TRAINER![FC]ÎéÀ|Hahahaha![br]Welkom in mijn schuilplaats![nb2]Hier schuil ik tot als ik[br]TEAM ROCKET in ere kan herstellen.[nb2]Maar je hebt me opnieuw gevonden.[br]deze keer ga ik voluit![nb2]Je mag nog eens vechten tegen[br]GIOVANNI, de beste TRAINER ooit![FC]ÎéÀ")));
            TranslationLines.Add(new TranslationItemViewModel(PointerText.FromString("18F4A2| 1|227|0|D|Viridian Gym                            |Having lost in this fashion, [br]I can't face my followers.[nb]I have betrayed their trust.[nb2]As of today, TEAM ROCKET is[br]finished forever![nb2]As for myself, I shall dedicate[br]my life to training again.[nb2]Let us meet again someday![br]Farewell!|Nu ik zo verloren heb kan ik[br]mijn volgers niet onder ogen komen.[nb]Ik heb hun vertrouwen verraden.[nb2]Vanaf vandaag is TEAM ROCKET[br]voorgoed gedaan.[nb2]Ik ga mijn leven opnieuw[br]aan training wijden.[nb2]Tot nog eens![br]Vaarwel!")));
            Groups.Add(new GroupViewModel("Group 1", "G1"));
            Groups.Add(new GroupViewModel("Group 2", "G2"));
            Groups.Add(new GroupViewModel("Group 3", "G3"));
        }
    }
}
