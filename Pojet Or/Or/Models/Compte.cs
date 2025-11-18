using System.Collections.Generic;
using Or.Business;
using System;
using System.Linq;
using static Or.Models.Carte;
using static Or.Business.MessagesErreur;

namespace Or.Models
{
    public enum TypeCompte { Courant, Livret }

    public class Compte
    {
        public int Id { get; set; }
        public long IdentifiantCarte { get; set; }
        public TypeCompte TypeDuCompte { get; set; }
        public decimal Solde { get;  set; }
        public CodeResultat CodeResult { get; }
       

        public Compte(int id, long identifiantCarte, TypeCompte type, decimal soldeInitial)
        {
            Id = id;
            IdentifiantCarte = identifiantCarte;
            TypeDuCompte = type;
            Solde = soldeInitial;
         
        }

        /// <summary>
        /// Action de dépôt d'argent sur le compte bancaire
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns>Statut du dépôt</returns>
        public CodeResultat EstDepotValide(Transaction transaction)
        {
            if (transaction.Montant > 0)
            {
                return CodeResultat.transactionok;
            }
            else
            {
                return CodeResultat.montanttinvalide;
            }
        }

           

        /// <summary>
        /// Action de retrait d'argent sur le compte bancaire
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns>Statut du retrait</returns>
        /*public bool EstRetraitValide(Transaction transaction)
        {
            if (EstRetraitAutorise(transaction.Montant))
            {
                return true;
            }
            else
            {
                return false;
            }
        }*/

        // Projet Or - partie 1 : messages d'erreur 
        public CodeResultat EstRetraitValide(Transaction transaction)
        {
            if (transaction.Montant < 0)
            {
                return CodeResultat.montanttinvalide;
            }
            if (Solde < transaction.Montant)
            {
                return CodeResultat.soldeinsuffisant;
            }
            return CodeResultat.transactionok; 
        }


        private CodeResultat EstRetraitAutorise(decimal montant)
        {
            if (Solde >= montant && montant > 0)
            {
                return CodeResultat.transactionok;
            }
            return CodeResultat.montanttinvalide;
        }

    }
}
