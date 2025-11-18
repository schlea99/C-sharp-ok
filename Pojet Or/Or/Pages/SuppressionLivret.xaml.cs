using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Text.RegularExpressions;
using Or.Business;
using Or.Models;
using System.Linq;

namespace Or.Pages
{
    /// <summary>
    /// Logique d'interaction pour Page1.xaml
    /// </summary>
    public partial class SuppressionLivret : PageFunction<long>
    {
        private readonly long NumCarte;

        public SuppressionLivret(long numCarte)
        {
            InitializeComponent();
            this.NumCarte = numCarte;
            TransfererLivret();
        }

        private void TransfererLivret()
        {
            listViewLivrets.ItemsSource = SqlRequests.ListeComptesAssociesCarte(NumCarte).Where(c => c.TypeDuCompte == TypeCompte.Livret).ToList();

        }

        public void Supprimer_click(object sender, RoutedEventArgs e)
        {
            if (listViewLivrets.SelectedItem is Compte livret)
            {
                MessageBoxResult result = MessageBox.Show($"Voulez-vous supprimer le livret {livret.Id} ? Le solde du livret sera transféré sur le compte courant.", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        SqlRequests.TransfertLivretversCourant(livret.Id, NumCarte);
                        SqlRequests.SupprimerLivret(livret.Id);

                        MessageBox.Show("Livret supprimé avec succès", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                        TransfererLivret();
                    }

                    catch (Exception ex)
                    {
                        MessageBox.Show("Erreur lors de la suppression : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            else
            {
                MessageBox.Show("Veuillez sélectionner un compte livret à supprimer.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Retour_Click(object sender, RoutedEventArgs e)
        {
            OnReturn(new ReturnEventArgs<long>(NumCarte));
        }


    }
}
