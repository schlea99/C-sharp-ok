using Or.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using static Or.Business.MessagesErreur;

namespace Or.Models
{
    public class Carte
    {
        public long Id { get; set; }
        public decimal Plafond { get; set; }
        public string PrenomClient { get; set; }
        public string NomClient { get; set; }
        public List<int> ListComptesId { get; set; }
        public List<Transaction> Historique { get; private set; }
        public CodeResultat CodeResult { get; set; }


        public Carte(long id, string prenom, string nom, decimal plafondMax = 0)
        {
            Id = id;
            PrenomClient = prenom;
            NomClient = nom;
            Plafond = plafondMax == 0 ? 500 : plafondMax;
            ListComptesId = new List<int>();
            Historique = new List<Transaction>();

        }

        public void AlimenterHistoriqueEtListeComptes(List<Transaction> hist, List<int> comptesId)
        {
            ListComptesId = comptesId;
            Historique = hist;
        }

        public void AjoutTransactionValidee(Transaction transac)
        {
            Historique.Add(transac);
        }

        // -------------------------------------------------------------------------------------------------------------
        //                              Contraintes sur les retraits et virements
        // -------------------------------------------------------------------------------------------------------------

        

        /// <summary>
        /// Est-ce que le retrait (retrait simple, virement) est il autorisé au niveau de la carte ? 
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="Expediteur"></param>
        /// <param name="Destinataire"></param>
        /// <returns></returns>
        /*public bool EstRetraitAutoriseNiveauCarte(Transaction transaction, Compte Expediteur, Compte Destinataire)
        {
            return EstOperationAutoriseeContraintesComptes(Expediteur, Destinataire) && EstEligibleMaximumRetraitHebdomadaire(transaction.Montant, transaction.Horodatage);
        }*/

        // Projet Or - partie 1 : messages d'erreur 
        public CodeResultat EstRetraitAutoriseNiveauCarte(Transaction transaction, Compte Expediteur, Compte Destinataire)
        {
            if (!EstOperationAutoriseeContraintesComptes(Expediteur, Destinataire))
            {
                return CodeResultat.virementinterdit;
            }
            if (!EstEligibleMaximumRetraitHebdomadaire(transaction.Montant, transaction.Horodatage))
            {
                return CodeResultat.plafondmax;
            }
            return CodeResultat.transactionok;
        }

        /// <summary>
        /// Test d'éligibilité par rapport au plafond maximal de la carte
        /// </summary>
        /// <param name="montant"></param>
        /// <param name="dateEffet"></param>
        /// <returns></returns>
        private bool EstEligibleMaximumRetraitHebdomadaire(decimal montant, DateTime dateEffet)
        {
            List<Transaction> retraitsHisto = Historique.Where(x => (x.Horodatage > dateEffet.AddDays(-10)) && ListComptesId.Contains(x.Expediteur)).Select(x => x).ToList();
            decimal sommeHisto = montant + retraitsHisto.Sum(x => x.Montant);
            return sommeHisto < Plafond;
        }

        /// <summary>
        /// Est-ce que les contraintes sur les comptes bancaires sont respectées ? 
        /// </summary>
        /// <param name="Expediteur"></param>
        /// <param name="Destinataire"></param>
        /// <returns></returns>
        private bool EstOperationAutoriseeContraintesComptes(Compte Expediteur, Compte Destinataire)
        {
            // Est-ce que la transaction demandée est possible ?
            if (Tools.EstTransactionExterieure(Expediteur.Id, Destinataire.Id))
            {
                return false;
            }

            // Opération Interne 
            if (EstOperationInterne(Expediteur.Id, Destinataire.Id))
            {
                return true;
            }
            // Opération externe
            else
            {
                return EstOperationExterneAutorise(Expediteur, Destinataire);
            }
        }

        /// <summary>
        /// Le compte appartient-il à la carte ? 
        /// </summary>
        /// <param name="idtCpt"></param>
        /// <returns></returns>
        private bool EstComptePresent(int idtCpt)
        {
            return ListComptesId.Exists(x => x == idtCpt);
        }

        /// <summary>
        /// Est ce qu'il s'agit d'une opération interne possible en principe ? 
        /// </summary>
        /// <param name="cptExt"></param>
        /// <param name="cptDest"></param>
        /// <returns></returns>
        private bool EstOperationInterne(int cptExt, int cptDest)
        {
            Operation operation = Tools.TypeTransaction(cptExt, cptDest);
            return
               (
                operation == Operation.DepotSimple ||
                operation == Operation.RetraitSimple ||
               (operation == Operation.InterCompte && EstComptePresent(cptExt) && EstComptePresent(cptDest))
               );
        }

        /// <summary>
        /// S'agit il d'une opération inter-compte externe possible ? 
        /// </summary>
        /// <param name="Expediteur"></param>
        /// <param name="Destinataire"></param>
        /// <returns></returns>

        private bool EstOperationExterneAutorise(Compte Expediteur, Compte Destinataire)
        {
            Operation operation = Tools.TypeTransaction(Expediteur.Id, Destinataire.Id);

            return operation == Operation.InterCompte &&
                   Expediteur.TypeDuCompte == TypeCompte.Courant && Destinataire.TypeDuCompte == TypeCompte.Courant;
        }

        // Projet Or - partie 1 : messages d'erreur 
        // Opération inter-compte possible si type de compte expéditeur et destinataire sont tout les deux des comptes courants (coderesultat transaction ok) sinon coderesultat virement interdit

        private CodeResultat VirementExterieurAutorise (Compte Expediteur, Compte Destinataire)
        {
            Operation operation = Tools.TypeTransaction(Expediteur.Id, Destinataire.Id);

            if (operation == Operation.InterCompte && Expediteur.TypeDuCompte == TypeCompte.Courant && Destinataire.TypeDuCompte == TypeCompte.Courant)
            {
                return CodeResultat.transactionok;
            }
            return CodeResultat.virementinterdit;
        }

        // Projet Or1 - Partie 1 : Plafond actualisé dans "retrait"
        public decimal SoldeCarteActuel(DateTime date, long numCarte)
        {
            // On récupère la liste des comptes associés à la carte grâce au numéro de carte et on trouve le compte courant 
            Compte compteCourant = SqlRequests.ListeComptesAssociesCarte(numCarte).Find(cp => cp.TypeDuCompte == TypeCompte.Courant);

            // Cas où on ne trouve pas de compte courant associé à la carte 
            if (compteCourant == null)
            {
                return 0;
            }

            // Calcul du total transaction les 10 derniers jours : regarde toutes les transactions effectuées dans l'historique ces dix derniers jours (qui proviennent d'un compte associé à la carte) et fait la somme des montants des transactions
            decimal TotalTransactionsDixJ = Historique.Where(tr => tr.Horodatage > date.AddDays(-10) && ListComptesId.Contains(tr.Expediteur)).Sum(tr => tr.Montant);

            // Calcul plafond actualisé = somme maximale disponible au retrait
            decimal PlafondActualise = Plafond - TotalTransactionsDixJ;

            // Respecte le plafond de la carte : dans le cas où le plafond actualisé est négatif, on lui donne la valeur 0
            if (PlafondActualise < 0)
            {
                PlafondActualise = 0;
            }

            // Comparaison au solde du compte courant : dans le cas où le plafond actualisé est supérieur au solde du compte, on lui donne la valeur du solde du compte
            if (PlafondActualise > compteCourant.Solde)
            {
                PlafondActualise = compteCourant.Solde;
            }

            return PlafondActualise;
        }

    }
}