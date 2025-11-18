using Or.Business;
using Or.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Or.Pages
{
    /// <summary>
    /// Logique d'interaction pour HistoriqueTransactions.xaml
    /// </summary>
    public partial class HistoriqueTransactions : PageFunction<long>
    {
        public HistoriqueTransactions(long numCarte)
        {
            InitializeComponent();

            listView.ItemsSource = SqlRequests.ListeTransactionsAssociesCarte(numCarte);
        }

        private void Retour_Click(object sender, RoutedEventArgs e)
        {
            OnReturn(null);
        }

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridView gridView = listView.View as GridView;
            if (gridView != null)
            {
                double totalWidth = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth;
                gridView.Columns[0].Width = totalWidth * 0.10; // 10%
                gridView.Columns[1].Width = totalWidth * 0.30; // 30%
                gridView.Columns[2].Width = totalWidth * 0.25; // 25%
                gridView.Columns[3].Width = totalWidth * 0.15; // 15%
                gridView.Columns[3].Width = totalWidth * 0.15; // 15%
            }
        }
    }
}
