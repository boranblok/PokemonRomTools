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
        public MainWindowViewModelDesign() : base(new IOServiceMock(), new TextHandler(new System.IO.FileInfo("SharedLibrary\\table file.tbl")))
        {
            TranslationLines.Add(new TranslationItemViewModel("1C5D4B| 1|115|0|Voor sommigen zijn POKéMON huisdieren.[br]Anderen gebruiken ze om te vechten[nb2]Ikzelf[...][nb2]Ik bestudeer POKéMON als beroep.[nb2]", _textHandler));
            TranslationLines.Add(new TranslationItemViewModel("18D8FE| 0|116|0|[FC]ÉÁ[FD]Á ziet er gelukkig uit[...][nb2][FC]ÉÈDAISY: Daar! Helemaal klaar.[br]Is het niet mooi?[nb2]Hihi[...][br]Het is zo een schattige POKéMON.", _textHandler));
            TranslationLines.Add(new TranslationItemViewModel("18EAF4| 1|240|0|Er is een e-mail.[nb2][...][nb2]Eindelijk![br]De ultieme TRAINERS van de[nb]POKéMON LEAGUE zijn klaar om[nb]elke uitdaging te aanvaarden![nb2]Breng je beste POKéMON en zie[br]hoe goed je bent als TRAINER.[nb2]POKéMON LEAGUE HQ[br]INDIGO PLATEAU[nb2]PROF. OAK, kom ons bezoeken![br][...]", _textHandler));
            TranslationLines.Add(new TranslationItemViewModel("18F085| 1| 89|0|[...] [...] [...]  [...] [...] [...][nb2][...] [...] [...]  [...] [...] [...][nb2][...] En klaar![nb2]Nu kunnen er nog meer POKéMON[br]geregistreerd worden.", _textHandler));
            TranslationLines.Add(new TranslationItemViewModel("18F352| 1|224|0|Hahahaha![br]Welkom in mijn schuilplaats![nb2]Hier schuil ik tot als ik[br]TEAM ROCKET in ere kan herstellen.[nb2]Maar je hebt me opnieuw gevonden.[br]deze keer ga ik voluit![nb2]Je mag nog eens vechten tegen[br]GIOVANNI, de beste TRAINER ooit![FC]ÎéÀ", _textHandler));
            TranslationLines.Add(new TranslationItemViewModel("18F4A2| 1|211|0|Nu ik zo verloren heb kan ik[br]mijn volgers niet onder ogen komen.[nb]Ik heb hun vertrouwen verraden.[nb2]Vanaf vandaag is TEAM ROCKET[br]voorgoed gedaan.[nb2]Ik ga mijn leven opnieuw[br]aan training wijden.[nb2]Tot nog eens![br]Vaarwel!", _textHandler));
        }
    }
}
