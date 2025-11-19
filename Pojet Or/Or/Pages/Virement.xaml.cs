using Or.Business;
using Or.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;
using static Or.Business.MessagesErreur;

namespace Or.Pages
{
    /// <summary>
    /// Logique d'interaction pour Virement.xaml
    /// </summary>
    public partial class Virement : PageFunction<long>
    {
        Carte CartePorteur { get; set; }
        Compte ComptePorteur { get; set; }

        // Numéro de carte du propriétaire du compte 
        private long NumCarteClient { get; set; }


        public Virement(long numCarte)
        {
            InitializeComponent();

            Montant.Text = 0M.ToString("C2");

            CartePorteur = SqlRequests.InfosCarte(numCarte);
            CartePorteur.AlimenterHistoriqueEtListeComptes(SqlRequests.ListeTransactionsAssociesCarte(numCarte), SqlRequests.ListeComptesAssociesCarte(CartePorteur.Id).Select(x => x.Id).ToList());
            ComptePorteur = SqlRequests.ListeComptesAssociesCarte(CartePorteur.Id).Find(x => x.TypeDuCompte == TypeCompte.Courant);

            // Affichage du plafond max
            PlafondMax.Text = CartePorteur.Plafond.ToString("C2");
            // Affichage du plafond actualisé (projet or - partie 1)
            PlafondActualise.Text = CartePorteur.SoldeCarteActuel(DateTime.Now, CartePorteur.Id).ToString("C2");
            // Affichage du solde 
            Solde.Text = ComptePorteur.Solde.ToString("C2");


            // Pour Compte à débiter
            var viewExpediteur = CollectionViewSource.GetDefaultView(SqlRequests.ListeComptesAssociesCarte(numCarte));
            viewExpediteur.GroupDescriptions.Add(new PropertyGroupDescription("TypeDuCompte"));
            viewExpediteur.SortDescriptions.Add(new SortDescription("TypeDuCompte", ListSortDirection.Ascending));
            viewExpediteur.SortDescriptions.Add(new SortDescription("IdentifiantCarte", ListSortDirection.Ascending));
            Expediteur.ItemsSource = viewExpediteur;

            // Pour compte à créditer 
            var viewDestinataire = CollectionViewSource.GetDefaultView(listvirement());
            viewDestinataire.GroupDescriptions.Add(new PropertyGroupDescription("IdentifiantCarte"));
            viewDestinataire.SortDescriptions.Add(new SortDescription("IdentifiantCarte", ListSortDirection.Ascending));
            viewDestinataire.SortDescriptions.Add(new SortDescription("TypeDuCompte", ListSortDirection.Ascending));
            Destinataire.ItemsSource = viewDestinataire;
        }


        private void Retour_Click(object sender, RoutedEventArgs e)
        {
            OnReturn(null);
        }

        // Renvoi à la méthode ajout bénéficiaire dans le fichier Ajout.Benef.xaml.cs
        private void Ajouter_Click(object sender, RoutedEventArgs e)
        {
            PageFunctionNavigate(new AjoutBenef(CartePorteur.Id));
        }

        void PageFunctionNavigate(PageFunction<long> page)
        {
            page.Return += new ReturnEventHandler<long>(PageFunction_Return);
            NavigationService.Navigate(page);
        }

        void PageFunction_Return(object sender, ReturnEventArgs<long> e)
        {

        }
        private void ValiderVirement_Click(object sender, RoutedEventArgs e)
        {
            // Debug lorsque les comptes destinataire et/ou expéditeur ne sont pas sélectionnés 
            if (Expediteur.SelectedItem == null)
            {
                MessageBox.Show("Il faut sélectionner un compte expediteur", "Erreur de saisie", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Destinataire.SelectedItem == null)
            {
                MessageBox.Show("Il faut sélectionner un compte destinataire", "Erreur de saisie", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (decimal.TryParse(Montant.Text.Replace(".", ",").Trim(new char[] { '€', ' ' }), out decimal montant) && montant > 0)
            {
                Compte ex = Expediteur.SelectedItem as Compte;
                Compte de = Destinataire.SelectedItem as Compte;

                Transaction t = new Transaction(0, DateTime.Now, montant, ex.Id, de.Id);
                CodeResultat codeResultat;

                if (((codeResultat = (Expediteur.SelectedItem as Compte).EstRetraitValide(t)) == CodeResultat.transactionok) && ((codeResultat = CartePorteur.EstRetraitAutoriseNiveauCarte(t, ex, de)) == CodeResultat.transactionok))
                {
                    SqlRequests.EffectuerModificationOperationInterCompte(t, ex.IdentifiantCarte, de.IdentifiantCarte);
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

        // Mise à jour des comptes à créditer avec la listevirement
        public void Expediteur_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewDestinataire = CollectionViewSource.GetDefaultView(listvirement());
            viewDestinataire.GroupDescriptions.Add(new PropertyGroupDescription("IdentifiantCarte"));
            viewDestinataire.SortDescriptions.Add(new SortDescription("IdentifiantCarte", ListSortDirection.Descending));
            viewDestinataire.SortDescriptions.Add(new SortDescription("TypeDuCompte", ListSortDirection.Ascending));
            Destinataire.ItemsSource = viewDestinataire;
        }

        // Liste des comptes pouvant être créditer pour le virement
        public List<Compte> listvirement()
        {
            List<Compte> totalCompte = new List<Compte>();

            if (Expediteur.SelectedItem is Compte ex)
            {
                // si l'expéditeur est un compte courant, on peut faire un virement vers les bénéficiaires enregistrés ou vers les livrets associés à la carte
                if (ex.TypeDuCompte == TypeCompte.Courant)
                {
                    Solde.Text = ex.Solde.ToString("C2");

                    // Liste des comptes associés à la carte
                    var compteClient = SqlRequests.ListeComptesAssociesCarte(CartePorteur.Id).Where(c => c.Id != ex.Id).ToList();

                    // Liste des comptes bénéficiaires
                    var compteBenef = SqlRequests.ListeBeneficiairesAssocieClient(CartePorteur.Id).SelectMany(b => SqlRequests.ListeComptesAssociesCarte(b.NumCarteBenef)).Where(c => c.Id != ex.Id).Where(d => d.TypeDuCompte == TypeCompte.Courant).ToList();

                    // Fusion des deux listes
                    totalCompte = compteClient.Concat(compteBenef).ToList();
                }

                // si l'expéditeur est un compte livret, on ne peut faire un virement que vers le compte courant ou les autres livrets de la même carte
                else
                {
                    Solde.Text = ex.Solde.ToString("C2");

                    // Liste des comptes associés à la carte
                    var compteClient = SqlRequests.ListeComptesAssociesCarte(CartePorteur.Id).Where(c => c.Id != ex.Id).ToList();

                    var compteAutorise = compteClient.Where(c => c.TypeDuCompte == TypeCompte.Courant || c.TypeDuCompte == TypeCompte.Livret).ToList();

                    totalCompte = compteAutorise;
                }
            }
            return totalCompte;
        }

    }
}
