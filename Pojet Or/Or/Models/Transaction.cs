using Or.Business;
using System;

namespace Or.Models
{
    public class Transaction
    {
        public int IdTransaction { get; set; }
        public DateTime Horodatage { get; set; }
        public decimal Montant { get; set; }
        public int Expediteur { get; set; }
        public int Destinataire { get; set; }

        // Projet Or - Partie 1 Ajouter le type de transaction
        public Operation Operation { get; set; }

        public Transaction(int idTransaction, DateTime horodatage, decimal montant, int expediteur, int destinataire)
        {
            IdTransaction = idTransaction;
            Horodatage = horodatage;
            Montant = montant;
            Expediteur = expediteur;
            Destinataire = destinataire;
            Operation = Tools.TypeTransaction(expediteur, destinataire);
        }

    }
}
