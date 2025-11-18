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
using Or.Pages;

namespace Or.Pages
{
    /// <summary>
    /// Logique d'interaction pour Page1.xaml
    /// </summary>

    public partial class ListeBeneficiaire : PageFunction<long>
    {
        private long NumCarteClient { get; set; }

        public ListeBeneficiaire(long numCarte)
        {
            InitializeComponent();

            NumCarteClient = numCarte;

            Carte c = SqlRequests.InfosCarte(numCarte);

            Numero.Text = c.Id.ToString();
            Prenom.Text = c.PrenomClient;
            Nom.Text = c.NomClient;

            listView.ItemsSource = SqlRequests.ListeBeneficiairesAssocieClient(NumCarteClient);
        }

        private void Ajouter_Click(object sender, RoutedEventArgs e)
        {        
            PageFunctionNavigate(new AjoutBenef(NumCarteClient));
        }


        private void Supprimer_Click(object sender, RoutedEventArgs e)
        {
            Beneficiaire benef = (sender as Button).DataContext as Beneficiaire;

            try
            {
                SqlRequests.SupprimerBeneficiaire(NumCarteClient, benef.IdtCptBenef);
                listView.ItemsSource = SqlRequests.ListeBeneficiairesAssocieClient(NumCarteClient);
                MessageBox.Show("Bénéficiaire supprimé avec succès");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la suppression du bénéficiaire : {ex.Message}");
            }
        }

        private void Retour_Click(object sender, RoutedEventArgs e)
        {
            OnReturn(null);
        }

        void PageFunctionNavigate(PageFunction<long> page)
        {
            page.Return += new ReturnEventHandler<long>(PageFunction_Return);
            NavigationService.Navigate(page);
        }

        void PageFunction_Return(object sender, ReturnEventArgs<long> e)
        {
            listView.ItemsSource = SqlRequests.ListeBeneficiairesAssocieClient(long.Parse(Numero.Text));
        }

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridView gridView = listView.View as GridView;
            if (gridView != null)
            {
                double totalWidth = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth;
                gridView.Columns[0].Width = totalWidth * 0.25; // 10%
                gridView.Columns[1].Width = totalWidth * 0.25; // 40%
                gridView.Columns[2].Width = totalWidth * 0.20; // 20%
                gridView.Columns[3].Width = totalWidth * 0.30; // 20%
            }
        }
    }
}
