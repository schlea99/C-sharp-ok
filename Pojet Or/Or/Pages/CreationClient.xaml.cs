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
    public partial class CreationClient : PageFunction<long>
    {
        public CreationClient()
        {
            InitializeComponent();
        }

        public void CreerNouveauClient(object sender, RoutedEventArgs e)
        {
            // On récupère le nom et prénom du client fourni dans l'application bancaire 
            string prenom = Prenom.Text.Trim();
            string nom = Nom.Text.Trim();

            // Vérification nom et prénom de type alphabétique
            Regex regexNom = new Regex("^[A-Za-z]+$");

            if (!regexNom.IsMatch(prenom) || (!regexNom.IsMatch(nom)))
            {
                MessageBox.Show("Seuls les caractères alphabétiques sont autorisés pour le nom et le prénom du client", "Erreur de saisie", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // On récupère le conseiller associé au client (fourni dans l'application bancaire)
            int idconseiller = int.Parse(IdConseiller.Text);

            // Vérification de la présence de l'id du conseiller dans la base de données 
            bool conseillerExiste = SqlRequests.ConseillerExiste(idconseiller);

            if (!conseillerExiste)
            {
                MessageBox.Show("L'identifiant du conseiller " + idconseiller + " n'existe pas dans la base de données", "Erreur de saisie", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Vérification de l'entrée des informations 
            if (string.IsNullOrWhiteSpace(prenom) || string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(idconseiller.ToString()))
            {
                MessageBox.Show("Veuillez renseigner les informations du client", "Erreur de saisie", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Création du client et affichage
            var (numCarte, idCompte) = SqlRequests.CreerClient(prenom, nom, idconseiller);
            MessageBox.Show($"Nouveau client crée avec succès ! Numéro de carte : {numCarte} et compte courant : {idCompte}");

            OnReturn(new ReturnEventArgs<long>(numCarte));
        }

        private void Retour_Click(object sender, RoutedEventArgs e)
        {
            OnReturn(null);
        }

    }
}
