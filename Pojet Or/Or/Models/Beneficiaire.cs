using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Or.Models
{
    public class Beneficiaire
    {
        // Numéro de carte du propriétaire du compte qui veut faire un virement
        public long NumCarteClient { get; set; }

        // Identifiant compte bénéficiaire
        public int IdtCptBenef { get; set; }
        // Numéro carte bénéficiaire
        public long NumCarteBenef { get; set; }
        public string PrenomBenef { get; set; }
        public string NomBenef { get; set; }
        // Type compte du compte bénéficiaire
        public TypeCompte TypeDuCompte { get; set; }


        /*public Beneficiaire(long numCarte, int idtCompte, long numcarteClient, string prenomClient, string nomClient, TypeCompte typeCompte)
        {
            NumCarteCompte = numCarte;
            IdtCompte = idtCompte;
            NumCarteBenef = numcarteClient;
            PrenomBenef = prenomClient;
            NomBenef = nomClient;
            TypeDuCompte = typeCompte;
        }*/

        public Beneficiaire()
        {
        }
    }
}
