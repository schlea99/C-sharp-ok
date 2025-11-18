using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Text.RegularExpressions;
using Or.Business;

namespace Or.Pages
{
    /// <summary>
    /// Logique d'interaction pour Page1.xaml
    /// </summary>
    public partial class CreationLivret : PageFunction<long>
    {
        private long NumCarte;

        public CreationLivret(long numCarte)
        {
            InitializeComponent();
            NumCarte = numCarte;
        }

        public void CreerLivret(object sender, RoutedEventArgs e)
        {
            try
            {
                SqlRequests.CreerLivret(NumCarte);
                MessageBox.Show("Livret crée avec succès !", "Opération réussie", MessageBoxButton.OK, MessageBoxImage.Information);
                OnReturn(new ReturnEventArgs<long>(NumCarte));
            }

            catch (Exception ex)
            {    
                MessageBox.Show(ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Retour_Click(object sender, RoutedEventArgs e)
        {
            OnReturn(new ReturnEventArgs<long>(NumCarte));
        }
    } 
}




