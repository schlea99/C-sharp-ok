using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Text.RegularExpressions;
using Or.Business;
using Or.Models;
using System.Linq;

// Fonction ajoutée
namespace Or.Pages
{
    /// <summary>
    /// Logique d'interaction pour Page1.xaml
    /// </summary>
    public partial class ModifierPlafond : PageFunction<long>
    {
        // Numéro de carte que l'on récupère depuis la page consultation carte
        private readonly long NumCarte;

        public ModifierPlafond(long numCarte)
        {
            InitializeComponent();
            NumCarte = numCarte;
        }

        private void Valider_Click(object sender, RoutedEventArgs e)
        {
            // On vérifie la valeur du plafond que l'on déclare dans l'application bancaire
            if((!int.TryParse(NouveauPlafond.Text, out int newPlafond)) || newPlafond <= 0)
            {
                MessageBox.Show("Il faut sélectionner un montant valide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // On appelle la requete SQL pour mettre à jour le plafond dans la table 
            try
            {
                SqlRequests.ModifPlafond(NumCarte, newPlafond);
                MessageBox.Show("Plafond modifié avec succès !", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Retour_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
