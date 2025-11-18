using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Or.Business;
using Or.Models;
using Or.Pages;

namespace Or.Pages
{
    /// <summary>
    /// Logique d'interaction pour Page1.xaml
    /// </summary>
    public partial class ConsultationConseiller : PageFunction<long>
    {
        public ConsultationConseiller(long numCarte)
        {
            InitializeComponent();

            Conseiller c = SqlRequests.ConseillerAssocieCarte(numCarte);

            if (c != null)
            {
                Nom.Text = "Nom : " + c.NomConseiller;
                Prenom.Text = "Prénom : " + c.PrenomConseiller;
                Email.Text = "Email : " + c.EmailConseiller;
                Telephone.Text = "Téléphone : " + c.TelConseiller;
            }

            else
            {
                MessageBox.Show("Aucun conseiller associé à la carte", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Retour_Click(object sender, RoutedEventArgs e)
        {
            OnReturn(null);
        }
    }
}

