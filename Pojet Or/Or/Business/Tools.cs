using Or.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Or.Business
{
    /// <summary>
    /// Enumération décrivant le type de transaction
    /// Facilite la prise en charge par GestionBancaire
    /// </summary>
    public enum Operation
    {
        DepotSimple = 0,
        RetraitSimple = 1,
        InterCompte = 2
    }

    public static class Tools
    {
        public static DateTime ConversionDate(string horodatage)
        {
            CultureInfo culture = CultureInfo.CurrentCulture;
            return DateTime.ParseExact(horodatage, "dd/MM/yyyy HH:mm:ss", culture);
        }

        /// <summary>
        /// Est ce que l'identifiant désigne l'extérieur ?
        /// </summary>
        /// <param name="identifiant"></param>
        /// <returns></returns>
        private static bool EstExterieur(int identifiant)
        {
            return identifiant == 0;
        }

        /// <summary>
        /// Est ce que la transaction est Extérieur -> Extérieur ?
        /// Si oui, elle est invalide
        /// </summary>
        /// <param name="expediteur"></param>
        /// <param name="destinataire"></param>
        /// <returns></returns>
        public static bool EstTransactionExterieure(int expediteur, int destinataire)
        {
            return expediteur + destinataire == 0;
        }


        public static Operation TypeTransaction(int expediteur, int destinataire)
        {
            // 0 -> Compte
            if (EstExterieur(expediteur))
            {
                return Operation.DepotSimple;
            }
            // Compte -> Compte
            else if (EstExterieur(destinataire))
            {
                return Operation.RetraitSimple;
            }
            // Compte -> Compte
            else
            {
                return Operation.InterCompte;
            }
        }

        public static string TypeTransacConverter(Operation operation)
        {
            // 0 -> Compte
            switch (operation)
            {
                case Operation.DepotSimple:
                    return "Dépôt";
                case Operation.RetraitSimple:
                    return "Retrait";
                case Operation.InterCompte:
                    return "Virement";
                default:
                    // je ne trouve pas le bon type d'exception
                    throw new NotImplementedException();
            }
        }
    }
}
