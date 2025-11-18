using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Or.Models
{
    public class Conseiller
    {
        // Informations du conseiller bancaire
        public int IdConseiller { get; set; }
        public string NomConseiller { get; set; }
        public string PrenomConseiller { get; set; }
        public string EmailConseiller { get; set; }
        public string TelConseiller { get; set; }

        public Conseiller(int idConseil, string nomConseil, string prenomConseil, string emailConseil , string telConseil)
        {
            IdConseiller = idConseil;
            NomConseiller = nomConseil;
            PrenomConseiller = prenomConseil;
            EmailConseiller = emailConseil;
            TelConseiller = telConseil;
        }

        public Conseiller()
        {

        }
    }

}

