using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Or.Business;
using Or.Models;
using Or.Pages;


namespace Or.Pages
{
    /// <summary>
    /// Logique ht'interaction pour ConsultationCarte.xaml
    /// </summary>
    public partial class ConsultationCarte : PageFunction<long>
    {
        long numeroCarte;

        public ConsultationCarte(long numCarte)
        {
            InitializeComponent();
            Carte c = SqlRequests.InfosCarte(numCarte);

            numeroCarte = numCarte;

            //if (c == null)
            //{
            //    MessageBox.Show("Numéro de carte non présent dans la base de données", "Carte inexistante", MessageBoxButton.OK, MessageBoxImage.Error);
            //}

            Numero.Text = c.Id.ToString();
            Prenom.Text = c.PrenomClient;
            Nom.Text = c.NomClient;

            MessageBox.Show($"Bienvenue sur le compte de {c.PrenomClient} {c.NomClient}.", "Compte client", MessageBoxButton.OK, MessageBoxImage.Information);
            listView.ItemsSource = SqlRequests.ListeComptesAssociesCarte(numCarte);
        }
        private void GoDetailsCompte(object sender, RoutedEventArgs e)
        {
            PageFunctionNavigate(new DetailsCompte(long.Parse(Numero.Text), (int)(sender as Button).CommandParameter));
        }

        private void GoHistoTransactions(object sender, RoutedEventArgs e)
        {
            PageFunctionNavigate(new HistoriqueTransactions(long.Parse(Numero.Text)));
        }

        private void GoVirement(object sender, RoutedEventArgs e)
        {
            PageFunctionNavigate(new Virement(long.Parse(Numero.Text)));
        }

        private void GoRetrait(object sender, RoutedEventArgs e)
        {
            PageFunctionNavigate(new Retrait(long.Parse(Numero.Text)));
        }

        private void GoDepot(object sender, RoutedEventArgs e)
        {
            PageFunctionNavigate(new Depot(long.Parse(Numero.Text)));
        }

        // Projet or - partie 3 - bénéficiaire
        private void GoBeneficiaire(object sender, RoutedEventArgs e)
        {
            PageFunctionNavigate(new ListeBeneficiaire(long.Parse(Numero.Text)));
        }

        // Ajout informations du conseiller bancaire
        private void GoInfoConseiller(object sender, RoutedEventArgs e)
        {
            PageFunctionNavigate(new ConsultationConseiller(long.Parse(Numero.Text)));
        }

        private void GoCreationLivret(object sender, RoutedEventArgs e)
        {
            PageFunctionNavigate(new CreationLivret(long.Parse(Numero.Text)));
        }

        private void GoSuppressionLivret(object sender, RoutedEventArgs e)
        {
            PageFunctionNavigate(new SuppressionLivret(long.Parse(Numero.Text)));
        }

        private void GoModifier(object sender, RoutedEventArgs e)
        {
            PageFunctionNavigate(new ModifierPlafond(long.Parse(Numero.Text)));
        }


        // Projet or - partie 2 : export xml
        private void ExportXML(object sender, RoutedEventArgs e)
        {
            try
            {
                SerialisationXML.SerialiserComptesTransaction(@"C:\Users\Formation\Desktop\ExportXML.xml", long.Parse(Numero.Text));
                MessageBox.Show("Export XML ok");
            }
            catch
            {
                MessageBox.Show("Export XML non executé");
            }

        }

        private void ImportXML(object sender, RoutedEventArgs e)
        {

        }

        void PageFunctionNavigate(PageFunction<long> page)
        {
            page.Return += new ReturnEventHandler<long>(PageFunction_Return);
            NavigationService.Navigate(page);
        }

        void PageFunction_Return(object sender, ReturnEventArgs<long> e)
        {
            listView.ItemsSource = SqlRequests.ListeComptesAssociesCarte(long.Parse(Numero.Text));
        }

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridView gridView = listView.View as GridView;
            if (gridView != null)
            {
                double totalWidth = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth;
                gridView.Columns[0].Width = totalWidth * 0.10; // 10%
                gridView.Columns[1].Width = totalWidth * 0.30; // 40%
                gridView.Columns[2].Width = totalWidth * 0.30; // 20%
                gridView.Columns[3].Width = totalWidth * 0.30; // 20%
            }
        }

        private void Retour_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Accueil());
        }
    }
}
