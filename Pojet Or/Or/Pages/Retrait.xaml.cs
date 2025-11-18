using Or.Business;
using Or.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using static Or.Business.MessagesErreur;

namespace Or.Pages
{
    /// <summary>
    /// Logique d'interaction pour Retrait.xaml
    /// </summary>
    public partial class Retrait : PageFunction<long>
    {
        Carte CartePorteur { get; set; }
        Compte ComptePorteur { get; set; }
        public Retrait(long numCarte)
        {
            // Charge la page retrait.xaml
            InitializeComponent();
            // Initialise la saisie du montant à 0e
            Montant.Text = 0M.ToString("C2");

            // Récupère les infos Carte
            CartePorteur = SqlRequests.InfosCarte(numCarte);
            // Récupère la liste des comptes associés à la carte, et trouve le compte courant (utilisé pour le retrait)
            ComptePorteur = SqlRequests.ListeComptesAssociesCarte(CartePorteur.Id).Find(x => x.TypeDuCompte == TypeCompte.Courant);
            // Récupère les transactions effectuées avec la carte
            List<Transaction> transac = SqlRequests.ListeTransactionsAssociesCarte(numCarte);
            List<int> cpts = SqlRequests.ListeComptesAssociesCarte(numCarte).Select(x => x.Id).ToList();
            // Appel méthode pour alimenter l'historique et la liste des comptes associés à la carte
            CartePorteur.AlimenterHistoriqueEtListeComptes(transac, cpts);

            // Affichage du plafond max
            PlafondMaxRetrait.Text = CartePorteur.Plafond.ToString("C2");
            // Affichage du plafond actualisé (projet or - partie 1)
            PlafondRetraitActualise.Text = CartePorteur.SoldeCarteActuel(DateTime.Now, CartePorteur.Id).ToString("C2");
            // Affichage du solde 
            Solde.Text = ComptePorteur.Solde.ToString("C2");
        }

        private void Retour_Click(object sender, RoutedEventArgs e)
        {
            OnReturn(null);
        }

        private void ValiderRetrait_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(Montant.Text.Replace(".", ",").Trim(new char[] { '€', ' ' }), out decimal montant) && montant > 0)
            {
                //Compte fictif pour permettre la transaction
                Compte compteBanque = new Compte(0, 0, TypeCompte.Courant, 0);
                Transaction t = new Transaction(0, DateTime.Now, montant, ComptePorteur.Id, compteBanque.Id);
                CodeResultat codeResultat;

                if (((codeResultat = CartePorteur.EstRetraitAutoriseNiveauCarte(t, compteBanque, ComptePorteur)) == CodeResultat.transactionok) && ((codeResultat = ComptePorteur.EstRetraitValide(t)) == CodeResultat.transactionok))
                {
                    SqlRequests.EffectuerModificationOperationSimple(t, CartePorteur.Id);

                    OnReturn(null);
                }
                else
                {
                    MessageBox.Show(MessagesErreur.Label(codeResultat));
                }
            }
            else
            {
                MessageBox.Show(MessagesErreur.Label(CodeResultat.montanttinvalide));
            }
        }
    }
}
