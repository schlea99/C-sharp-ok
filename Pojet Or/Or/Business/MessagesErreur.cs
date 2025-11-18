using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Or.Business
{
    public static class MessagesErreur
    {
        public enum CodeResultat
        {
            transactionok,
            plafondmax,
            montanttinvalide,
            soldeinsuffisant,
            virementinterdit,
            transactionnulle
        }

        public static string Label(CodeResultat code)
        {
            switch (code)
            {
                case CodeResultat.transactionok:
                    {
                        return "Transaction effectuée";
                    }
                case CodeResultat.plafondmax:
                    {
                        return "Opération non autorisée : Plafond maximal autorisé dépassé";
                    }
                case CodeResultat.montanttinvalide:
                    {
                        return "Opération non autorisée : Montant invalide";
                    }
                case CodeResultat.soldeinsuffisant:
                    {
                        return "Opération non autorisée : Solde insuffisant";
                    }
                case CodeResultat.virementinterdit:
                    {
                        return "Opération non autorisée : Virement vers Livret autre carte interdit";
                    }
                case CodeResultat.transactionnulle:
                    {
                        return "Opération non autorisée : Transaction inexistante";
                    }
                default:
                    {
                        return "Opération non autorisée";
                    }
            }
        }
    }
}
