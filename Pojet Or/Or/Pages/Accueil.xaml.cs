using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Or.Business;
using Or.Models;
using System.ComponentModel;

namespace Or.Pages
{
    /// <summary>
    /// Logique d'interaction pour Accueil.xaml
    /// </summary>
    public partial class Accueil : PageFunction<long>
    {
        public Accueil()
        {
            InitializeComponent();
        }

        public void GoConsultationCarte(object sender, RoutedEventArgs e)
        {
            bool estCarteValide = long.TryParse(NumeroCarte.Text, out long result);

            if (estCarteValide)
            {
                var carte = SqlRequests.InfosCarte(result);

                // Debug lorsque le numero de carte n'existe pas dans la base de données SQL
                if (carte == null)
                {
                    MessageBox.Show("Numéro de carte non présent dans la base de données", "Carte inexistante", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                // Si la carte existe, on peut naviguer vers la page Consultation carte
                else
                {
                    PageFunctionNavigate(new ConsultationCarte(result));
                }
            }
            // Cas où le numéro de carte n'est pas valide (caractère autre que des chiffres)
            else
            {
                MessageBox.Show("Numéro de carte invalide", "Saisie invalide", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GoCreerClient(object sender, RoutedEventArgs e)
        {
            PageFunctionNavigate(new CreationClient());
        }

        private void GoSupprimerClient(object sender, RoutedEventArgs e)
        {
            PageFunctionNavigate(new SuppressionClient());
        }

        void PageFunctionNavigate(PageFunction<long> page)
        {
            page.Return += new ReturnEventHandler<long>(PageFunction_Return);
            NavigationService.Navigate(page);
        }

        void PageFunction_Return(object sender, ReturnEventArgs<long> e)
        {

        }

        public void GoMouse(object sender, RoutedEvent e)
        {

        }
    }
}
