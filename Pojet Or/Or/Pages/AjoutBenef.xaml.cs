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
    /// Logique d'interaction pour Page1.xaml
    /// </summary>
    public partial class AjoutBenef : PageFunction<long>
    {
        private long NumCarteClient { get; set; }
        private long NumCarteBenef { get; set; }
 
        public AjoutBenef(long numCarte)
        {
            InitializeComponent();

            NumCarteBenef = numCarte;
        }

        private void Ajouter_Click(object sender, RoutedEventArgs e)
        {
            InitializeComponent();
           
            if (int.TryParse(Numero.Text, out int idCptBenef))
            {
                if (SqlRequests.EstBeneficiairePotentiel(idCptBenef))
                {
                    List<Compte> comptes = SqlRequests.ListeComptesId(idCptBenef).ToList();
                                                                                     
                    foreach (Compte c in comptes)
                    {
                        if ((c.TypeDuCompte == TypeCompte.Courant) || (c.TypeDuCompte == TypeCompte.Livret && NumCarteBenef == c.IdentifiantCarte))
                        {
                            try
                            {
                                SqlRequests.AjouterBeneficiaire(NumCarteBenef, idCptBenef);
                                MessageBox.Show("Bénéficiaire ajouté !");                               
                                break;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Erreur : " + ex.Message);
                                MessageBox.Show("Erreur lors de l'ajout du bénéficiaire");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Impossible d'ajouter un livret exterieur en destinataire");
                            break;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Numéro compte incorrect");
                    
                }
            }
            else
            {
                MessageBox.Show("Saisie bénéficiaire invalide");
                
            }

        }

        private void Retour_Click(object sender, RoutedEventArgs e)
        {
            OnReturn(null);
        }


    }
}

