using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Text.RegularExpressions;
using Or.Business;


// Fonction ajoutée 
namespace Or.Pages
{
    /// <summary>
    /// Logique d'interaction pour Page1.xaml
    /// </summary>
    public partial class SuppressionClient : PageFunction<long>
    {
        public SuppressionClient()
        {
            InitializeComponent();
        }

        private void SupprimerClient(object sender, RoutedEventArgs e)
        {
            // On vérifie si la vriable en entrée correspond à la variable attentue
            if (!long.TryParse(Carte.Text, out long numCarte))
            {
                MessageBox.Show("Numéro de carte invalide, veuillez saisir un numéro valide.", "Erreur de saisie", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                // on récupère nom et prénom du client
                var client = SqlRequests.InfosCarte(numCarte);

                // Confirmation de la suppression de la carte
                var conf = MessageBox.Show($"Etes-vous certain de vouloir supprimer la carte n°{numCarte} de {client.PrenomClient} {client.NomClient} ?\n" + "Toutes les données associées à la carte seront définitivement supprimées.", "Confirmation de suppression", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (conf == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool suppr = SqlRequests.SupprimerCarte(numCarte);

                        if (suppr)
                        {
                            MessageBox.Show("Carte supprimée avec succès", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("La carte n'existe pas", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erreur lors de la suppression : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void Retour_Click(object sender, RoutedEventArgs e)
        {
            OnReturn(null);
        }
    }
}