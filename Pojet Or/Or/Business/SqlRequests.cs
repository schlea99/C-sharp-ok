using Microsoft.Data.Sqlite;
using Or.Models;
using Or.Pages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Or.Business
{
    static public class SqlRequests
    {
        // Addresse de la base de données
        static readonly string fileDb = "BaseAppBancaire.db";

        static readonly string queryComptesDispo = "SELECT IdtCpt, NumCarte, Solde, TypeCompte FROM COMPTE WHERE NOT IdtCpt=@IdtCpt";

        static readonly string queryComptesCarte = "SELECT IdtCpt, NumCarte, Solde, TypeCompte FROM COMPTE WHERE NumCarte=@Carte";

        static readonly string queryCompte = "SELECT IdtCpt, NumCarte, Solde, TypeCompte FROM COMPTE WHERE IdtCpt=@IdtCpt";

        static readonly string queryTransacCompte = "SELECT IdtTransaction, Horodatage, Montant, CptExpediteur, CptDestinataire, Statut FROM \"TRANSACTION\" WHERE Statut = 'O' AND (CptExpediteur=@IdtCptEx OR CptDestinataire=@IdtCptDest)";
        static readonly string queryCarte = "SELECT NumCarte, PrenomClient, NomClient, PlafondRetrait from CARTE WHERE NumCarte=@Carte";
        static readonly string queryTransacCarte = "SELECT tr.IdtTransaction, tr.Horodatage, tr.Montant, tr.CptExpediteur, tr.CptDestinataire, tr.Statut FROM \"TRANSACTION\" tr INNER JOIN HISTTRANSACTION t ON t.IdtTransaction = tr.IdtTransaction WHERE tr.Statut = 'O' AND t.NumCarte=@Carte;";

        static readonly string queryInsertTransac = "INSERT INTO \"TRANSACTION\" (Horodatage, Montant, CptExpediteur, CptDestinataire, Statut) VALUES (@Horodatage,@Montant,@CptExp,@CptDest,\"O\")";
        static readonly string queryIdtTransac = "select seq from sqlite_sequence where name=\"TRANSACTION\"";
        static readonly string queryInsertHistTransac = "INSERT INTO HISTTRANSACTION (IdtTransaction,NumCarte) VALUES (@IdtTrans,@Carte)";

        static readonly string queryUpdateCompte = "UPDATE COMPTE SET Solde=Solde-@Montant WHERE IdtCpt=@IdtCompte";

        static readonly string queryAjoutBenef = "INSERT INTO \"BENEFICIAIRES\" (NumCarteClient, IdtCptBenef) VALUES (@numCarteClient, @idCompteBenef)";
        static readonly string querySupprBenef = "DELETE FROM \"BENEFICIAIRES\" WHERE NumCarteClient = @numCarteClient AND IdtCptBenef = @idCompteBenef";

        static readonly string queryListeBeneficiaire = "SELECT b.NumCarteClient, b.IdtCptBenef, ca.NumCarte AS NumCarteBenef, ca.NomClient AS NomBenef, ca.PrenomClient AS PrenomBenef, c.TypeCompte AS TypeDuCompte FROM BENEFICIAIRES b INNER JOIN COMPTE c ON c.IdtCpt = b.IdtCptBenef INNER JOIN CARTE ca ON ca.NumCarte = c.NumCarte WHERE b.NumCarteClient = @numCarteClient";

        static readonly string queryBeneficiairePotentiel = "SELECT COUNT(*) FROM COMPTE c WHERE c.IdtCpt = @idCompte";

        // Requêtes ajoutées pour amélioration du projet (du 12/11/25 au ...)
        static readonly string queryConseiller = "SELECT CONSEILLER.\"IdConseiller \", CONSEILLER.NomConseiller, CONSEILLER.PrenomConseiller, CONSEILLER.EmailConseiller, CONSEILLER.TelConseiller FROM CONSEILLER INNER JOIN CARTE  ON CONSEILLER.\"IdConseiller \" = CARTE.\"IdConseiller \" WHERE CARTE.NumCarte = @NumCarte";

        static readonly string queryCreerClient = "INSERT INTO CARTE (NumCarte, PrenomClient, NomClient, PlafondRetrait, \"IdConseiller \") VALUES (@NumCarte, @Prenom, @Nom, @Plafond, @IdConseiller)"; // commande sql pour récupérer le numéro de carte de la ligne que l'on a crée dans la table CARTE
        static readonly string queryCreerCompte = "INSERT INTO COMPTE (NumCarte, Solde, TypeCompte) VALUES (@NumCarte, @Solde, @TypeCompte); SELECT last_insert_rowid();";
        static readonly string queryConseillerExiste = "SELECT * FROM CONSEILLER WHERE \"IdConseiller \" = @Id";

        static readonly string queryNumLivret = "SELECT IFNULL(MAX(IdtCpt), 0) + 1 FROM COMPTE";
        static readonly string queryCreerLivret = "INSERT INTO COMPTE (IdtCpt, NumCarte, Solde, TypeCompte) VALUES (@Id, @Carte, @Solde, @Type)";

        static readonly string querySupprLivret = "DELETE FROM COMPTE WHERE IdtCpt = @IdtCpt";
        static readonly string queryTransfert = "UPDATE COMPTE SET Solde = Solde + @SoldeLivret WHERE IdtCpt = @IdtCpt";
        static readonly string querySolde = "SELECT Solde FROM COMPTE WHERE IdtCpt = @IdtCpt";
        static readonly string queryCourant = "SELECT IdtCpt FROM COMPTE WHERE NumCarte = @NumCarte AND TypeCompte = 'Courant'";

        static readonly string queryModifPlafond = "UPDATE CARTE SET PlafondRetrait = @Plafond WHERE NumCarte = @NumCarte";

        static readonly string queryCarteExiste = "SELECT * FROM CARTE WHERE NumCarte = @NumCarte";
        static readonly string querySupprBenefCarte = "DELETE FROM BENEFICIAIRES WHERE NumCarteBenef = @NumCarteBenef OR NumCarteClient = @NumCarteClient";
        static readonly string querySupprBenefAutre = "DELETE FROM BENEFICIAIRES WHERE IdtCptBenef = @idCpt";
        static readonly string querySupprHist = "DELETE FROM HISTTRANSACTION WHERE NumCarte = @NumCarte";
        // static readonly string querySupprTransac = "DELETE FROM \"TRANSACTION\" WHERE CptExpediteur = @cpt OR CptDestinataire = @cpt";
        static readonly string queryVerifHist = "DELETE FROM HISTTRANSACTION WHERE IdtTransaction IN (SELECT IdtTransaction FROM \"TRANSACTION\" WHERE CptExpediteur = @cpt OR CptDestinataire = @cpt)";
        static readonly string queryReCompte = "SELECT IdtCpt FROM COMPTE WHERE NumCarte = @NumCarte";
        static readonly string queryReSupprCompte = "DELETE FROM COMPTE WHERE NumCarte = @NumCarte";
        static readonly string queryReSupprCarte = "DELETE FROM CARTE WHERE NumCarte = @NumCarte";

        /* Idées à rajouter :
        Cryptage - décryptage des données
        Simulation de demandes de crédit
        Dans la page virement, ne prendre en compte le montant du plafond que lorsque l'on fait des transactions vers un compte extérieur
        Liste des cartes dans la page Accueil (mais il faut tout changer) avec MAJ lors de la création/suppression de client
        Lors de l'ajout d'un bénéficiaire, on affiche aussi son nom et son prénom
        */


        // Elements ajoutés : pour modifier le plafond d'une carte existante
        public static void ModifPlafond(long numCarte, int newPlafond)
        {
            string connectionString = ConstructionConnexionString(fileDb);

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqliteCommand(queryModifPlafond, connection))
                {
                    command.Parameters.AddWithValue("@NumCarte", numCarte);
                    command.Parameters.AddWithValue("@Plafond", newPlafond);

                    command.ExecuteNonQuery();
                }
            }
        }

        // Elements ajoutés : on récupère le conseiller bancaire associé à la carte
        public static Conseiller ConseillerAssocieCarte(long numCarte)
        {
            Conseiller conseiller = null;

            string connectionString = ConstructionConnexionString(fileDb);

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqliteCommand(queryConseiller, connection))
                {
                    command.Parameters.AddWithValue("@NumCarte", numCarte);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            conseiller = new Conseiller
                            {
                                IdConseiller = reader.GetInt32(0),
                                NomConseiller = reader.GetString(1),
                                PrenomConseiller = reader.GetString(2),
                                EmailConseiller = reader.GetString(3),
                                TelConseiller = reader.GetString(4)
                            };
                        }
                    }
                }
            }
            return conseiller;
        }

        // Elements ajoutés : on vérifie que le conseiller existe avant de le déclarer pour la création de compte 
        public static bool ConseillerExiste(int idconseiller)
        {
            string connectionString = ConstructionConnexionString(fileDb);

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqliteCommand(queryConseillerExiste, connection))
                {
                    command.Parameters.AddWithValue("@Id", idconseiller);

                    object result = command.ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                    {
                        return false;
                    }

                    long count = Convert.ToInt64(result);
                    return count > 0;
                }
            }

        }

        // Elements ajoutés : générer le numéro de carte de façon aléatoire
        private static long GenererNumCarte()
        {
            Random numca = new Random();
            string numCarte = "";

            for (int i = 0; i < 16; i++)
                numCarte += numca.Next(0, 10).ToString();

            return long.Parse(numCarte);
        }


        // Elements ajoutés : on crée un nouveau client, on crée d'abord une carte (numéro aléatoire) puis on lui associe un compte courant  
        public static (long numCarte, int idCompte) CreerClient(string prenom, string nom, int idconseiller)
        {
            // On génère d'abord le numéro de carte aléatoirement 
            long numCarte = GenererNumCarte();

            int idCompte;

            string connectionString = ConstructionConnexionString(fileDb);

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // Créer une nouvelle carte dans la base de données 
                using (var command = new SqliteCommand(queryCreerClient, connection))
                {
                    command.Parameters.AddWithValue("@NumCarte", numCarte);
                    command.Parameters.AddWithValue("@Prenom", prenom);
                    command.Parameters.AddWithValue("@Nom", nom);
                    command.Parameters.AddWithValue("@Plafond", 500M);
                    command.Parameters.AddWithValue("@IdConseiller", idconseiller);

                    command.ExecuteNonQuery();
                }

                // Créer le compte courant associé à la carte que l'on vient de créer
                using (var commandcompte = new SqliteCommand(queryCreerCompte, connection))
                {
                    commandcompte.Parameters.AddWithValue("@NumCarte", numCarte);
                    commandcompte.Parameters.AddWithValue("@TypeCompte", TypeCompte.Courant.ToString());
                    commandcompte.Parameters.AddWithValue("@Solde", 0M);

                    idCompte = Convert.ToInt32(commandcompte.ExecuteScalar());
                }
            }
            return (numCarte, idCompte);
        }

        // Elements ajoutés : on crée un nouveau livret associé à une carte existante 
        public static void CreerLivret(long numCarte)
        {
            string connectionString = ConstructionConnexionString(fileDb);

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // On génère un nouvel id livret (à la suite du dernier id existant)
                int newid = 0;

                using (var command = new SqliteCommand(queryNumLivret, connection))
                {
                    command.Parameters.AddWithValue("@numCarte", numCarte);
                    newid = Convert.ToInt32(command.ExecuteScalar());

                    // Creation du livret dans la base de données 
                    using (var cmdlivret = new SqliteCommand(queryCreerLivret, connection))
                    {
                        cmdlivret.Parameters.AddWithValue("@Id", newid);
                        cmdlivret.Parameters.AddWithValue("@Carte", numCarte);
                        cmdlivret.Parameters.AddWithValue("@Solde", 0M);
                        cmdlivret.Parameters.AddWithValue("@Type", TypeCompte.Livret.ToString());

                        cmdlivret.ExecuteNonQuery();

                        Console.WriteLine($"Livret crée pour la carte {numCarte}, l'identifiant du compte est : {newid}");
                    }
                }
            }
        }

        // Elements ajoutés : on peut supprimer un livret associé à un compte courant
        public static void SupprimerLivret(int idLivret)
        {
            string connectionString = ConstructionConnexionString(fileDb);

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqliteCommand(querySupprLivret, connection))
                {
                    command.Parameters.AddWithValue("@IdtCpt", idLivret);

                    command.ExecuteNonQuery();
                }
            }
        }

        // Elements ajoutés : on transfert le solde du livret vers le compte courant avant de supprimer le livret
        public static void TransfertLivretversCourant(int idLivret, long numCarte)
        {
            string connectionString = ConstructionConnexionString(fileDb);

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // On récupère le solde du livret dans la base de données
                decimal soldeLivret = 0;

                using (var command = new SqliteCommand(querySolde, connection))
                {
                    command.Parameters.AddWithValue("@IdtCpt", idLivret);
                    soldeLivret = Convert.ToDecimal(command.ExecuteScalar());
                }

                // si le solde du livret est supérieur à 0, on le transfere sur le compte courant avant fermeture du livret
                if (soldeLivret > 0)
                {
                    // On récupère le compte courant dans la base de données 
                    int idCourant = 0;

                    using (var com = new SqliteCommand(queryCourant, connection))
                    {
                        com.Parameters.AddWithValue("@NumCarte", numCarte);
                        idCourant = Convert.ToInt32(com.ExecuteScalar());
                    }

                    // On transfère le solde du livret vers le compte courant 
                    using (var comm = new SqliteCommand(queryTransfert, connection))
                    {
                        comm.Parameters.AddWithValue("@SoldeLivret", soldeLivret);
                        comm.Parameters.AddWithValue("@IdtCpt", idCourant);

                        comm.ExecuteNonQuery();
                    }
                }
            }
        }

        public static bool SupprimerCarte(long numCarte)
        {
            string connectionString = ConstructionConnexionString(fileDb);

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // On vérifie que la carte que l'on veut supprimer existe dans la base de données 
                using (var command = new SqliteCommand(queryCarteExiste, connection))
                {
                    command.Parameters.AddWithValue("@NumCarte", numCarte);
                    long exist = (long)command.ExecuteScalar();

                    if (exist == 0)
                    {
                        return false;
                    }
                    else
                    {
                        // On récupère les comptes associés à une carte 
                        List<int> comptes = new List<int>();

                        using (var coma = new SqliteCommand(queryReCompte, connection))
                        {
                            coma.Parameters.AddWithValue("@NumCarte", numCarte);

                            // on ecrit les comptes trouvés dans la liste comptes
                            using (var reader = coma.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    comptes.Add(reader.GetInt32(0));
                                }
                            }
                        }

                        // On supprime l'historique des transactions associés au numero de carte
                        using (var comman = new SqliteCommand(querySupprHist, connection))
                        {
                            comman.Parameters.AddWithValue("@NumCarte", numCarte);

                            comman.ExecuteNonQuery();
                            comman.Parameters.Clear();
                        }

                        // On supprime les bénéficiaires associés de la carte
                        using (var com = new SqliteCommand(querySupprBenefCarte, connection))
                        {
                            com.Parameters.AddWithValue("@NumCarteBenef", numCarte);
                            com.Parameters.AddWithValue("@NumCarteClient", numCarte);

                            com.ExecuteNonQuery();
                            com.Parameters.Clear();
                        }

                        // On supprime les bénéficiaires avec un compte lié à la carte 
                        foreach (int cpt in comptes)
                        {
                            using (var co = new SqliteCommand(querySupprBenefAutre, connection))
                            {
                                co.Parameters.AddWithValue("@idCpt", cpt);

                                co.ExecuteNonQuery();
                                co.Parameters.Clear();
                            }
                        }

                        // On supprime l'historique des transactions pour les transactions liés les comptes 
                        foreach (int cpt in comptes)
                        {
                            using (var verif = new SqliteCommand(queryVerifHist, connection))
                            {
                                verif.Parameters.AddWithValue("@cpt", cpt);

                                verif.ExecuteNonQuery();
                                verif.Parameters.Clear();
                            }
                        }

                       // On supprime les transactions associées à chaque compte de la carte
                       // attention ça efface toutes les transactions des comptes impliquées (le compte à supprimer et les AUTRES comptes aussi)
                       /* foreach (int cpt in comptes)
                        {
                            using (var comm = new SqliteCommand(querySupprTransac, connection))
                            {
                                comm.Parameters.AddWithValue("@cpt", cpt);

                                comm.ExecuteNonQuery();
                                comm.Parameters.Clear();
                            }
                        }*/

                        // On supprime les comptes associés à la carte 
                        using (var comma = new SqliteCommand(queryReSupprCompte, connection))
                        {
                            comma.Parameters.AddWithValue("@NumCarte", numCarte);

                            comma.ExecuteNonQuery();
                            comma.Parameters.Clear();
                        }

                        // On supprime en dernier la carte 
                        using (var comman = new SqliteCommand(queryReSupprCarte, connection))
                        {
                            comman.Parameters.AddWithValue("@NumCarte", numCarte);

                            comman.ExecuteNonQuery();
                            comman.Parameters.Clear();
                        }
                        return true;
                    }
                }
            }
        }

        /// <summary>
        /// Obtention des infos d'une carte
        /// </summary>
        /// <param name="numCarte"></param>
        /// <returns></returns>
        public static Carte InfosCarte(long numCarte)
        {
            Carte carte = null;

            string connectionString = ConstructionConnexionString(fileDb);

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqliteCommand(queryCarte, connection))
                {
                    command.Parameters.AddWithValue("@Carte", numCarte);

                    using (var reader = command.ExecuteReader())
                    {
                        long idtCarte;
                        string prenom;
                        string nom;
                        int plafondRetrait;

                        if (reader.Read())
                        {
                            idtCarte = reader.GetInt64(0);
                            prenom = reader.GetString(1);
                            nom = reader.GetString(2);
                            plafondRetrait = reader.GetInt32(3);

                            carte = new Carte(idtCarte, prenom, nom, plafondRetrait);
                        }
                    }
                }
            }

            return carte;
        }

        /// <summary>
        /// Obtention du dernier identifiant de transaction
        /// </summary>
        /// <returns></returns>
        public static int InfosIdtTrans()
        {
            int idtTransac = 0;

            string connectionString = ConstructionConnexionString(fileDb);

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqliteCommand(queryIdtTransac, connection))
                {

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            idtTransac = reader.GetInt32(0);
                        }
                    }
                }
            }

            return idtTransac;
        }


        /// <summary>
        /// Liste des comptes associés à une carte donnée
        /// </summary>
        /// <param name="numCarte"></param>
        /// <returns></returns>
        public static List<Compte> ListeComptesAssociesCarte(long numCarte)
        {
            List<Compte> comptes = new List<Compte>();

            string connectionString = ConstructionConnexionString(fileDb);

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqliteCommand(queryComptesCarte, connection))
                {
                    command.Parameters.AddWithValue("@Carte", numCarte);

                    using (var reader = command.ExecuteReader())
                    {
                        int idtCpt;
                        long carte;
                        decimal solde;
                        string typeCompte;

                        while (reader.Read())
                        {
                            idtCpt = reader.GetInt32(0);
                            carte = reader.GetInt64(1);
                            solde = reader.GetDecimal(2);
                            typeCompte = reader.GetString(3);

                            Compte compte = new Compte(idtCpt, carte, typeCompte == "Courant" ? TypeCompte.Courant : TypeCompte.Livret, solde);
                            comptes.Add(compte);
                        }
                    }
                }
            }

            return comptes;
        }

        public static List<Compte> ListeComptesId(int idtCpt)
        {
            List<Compte> comptes = new List<Compte>();

            string connectionString = ConstructionConnexionString(fileDb);

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqliteCommand(queryCompte, connection))
                {
                    command.Parameters.AddWithValue("@IdtCpt", idtCpt);

                    using (var reader = command.ExecuteReader())
                    {
                        int idt;
                        long carte;
                        decimal solde;
                        string typeCompte;

                        while (reader.Read())
                        {
                            idt = reader.GetInt32(0);
                            carte = reader.GetInt64(1);
                            solde = reader.GetDecimal(2);
                            typeCompte = reader.GetString(3);

                            Compte compte = new Compte(idt, carte, typeCompte == "Courant" ? TypeCompte.Courant : TypeCompte.Livret, solde);
                            comptes.Add(compte);
                        }
                    }
                }
            }
            return comptes;
        }

        /// <summary>
        /// Liste des comptes associés dispos
        /// </summary>
        /// <param name="idtCpt"></param>
        /// <returns></returns>
        public static List<Compte> ListeComptesDispo(int idtCpt)
        {
            List<Compte> comptes = new List<Compte>();

            string connectionString = ConstructionConnexionString(fileDb);

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqliteCommand(queryComptesDispo, connection))
                {
                    command.Parameters.AddWithValue("@IdtCpt", idtCpt);

                    using (var reader = command.ExecuteReader())
                    {
                        int idt;
                        long carte;
                        decimal solde;
                        string typeCompte;

                        while (reader.Read())
                        {
                            idt = reader.GetInt32(0);
                            carte = reader.GetInt64(1);
                            solde = reader.GetDecimal(2);
                            typeCompte = reader.GetString(3);

                            Compte compte = new Compte(idt, carte, typeCompte == "Courant" ? TypeCompte.Courant : TypeCompte.Livret, solde);
                            comptes.Add(compte);
                        }
                    }
                }
            }

            return comptes;
        }

        /// <summary>
        /// Liste des transactions associées à une carte donnée
        /// </summary>
        /// <param name="numCarte"></param>
        /// <returns></returns>
        public static List<Transaction> ListeTransactionsAssociesCarte(long numCarte)
        {
            List<Transaction> transactions = new List<Transaction>();

            string connectionString = ConstructionConnexionString(fileDb);

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqliteCommand(queryTransacCarte, connection))
                {
                    command.Parameters.AddWithValue("@Carte", numCarte);

                    using (var reader = command.ExecuteReader())
                    {
                        int idtTransaction;
                        string horodatage;
                        decimal montant;
                        int cptDest;
                        int cptExt;

                        while (reader.Read())
                        {
                            idtTransaction = reader.GetInt32(0);
                            horodatage = reader.GetString(1);
                            montant = reader.GetDecimal(2);
                            cptDest = reader.GetInt32(3);
                            cptExt = reader.GetInt32(4);

                            Transaction trans = new Transaction(idtTransaction, Tools.ConversionDate(horodatage), montant, cptDest, cptExt);
                            transactions.Add(trans);
                        }
                    }
                }
            }

            return transactions;
        }

        /// <summary>
        /// Liste des transactions associées à un compte donné
        /// </summary>
        /// <param name="numCarte"></param>
        /// <returns></returns>
        public static List<Transaction> ListeTransactionsAssociesCompte(int idtCpt)
        {
            List<Transaction> transactions = new List<Transaction>();

            string connectionString = ConstructionConnexionString(fileDb);

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqliteCommand(queryTransacCompte, connection))
                {
                    command.Parameters.AddWithValue("@IdtCptEx", idtCpt);
                    command.Parameters.AddWithValue("@IdtCptDest", idtCpt);

                    using (var reader = command.ExecuteReader())
                    {
                        int idtTransaction;
                        string horodatage;
                        decimal montant;
                        int cptDest;
                        int cptExt;

                        while (reader.Read())
                        {
                            idtTransaction = reader.GetInt32(0);
                            horodatage = reader.GetString(1);
                            montant = reader.GetDecimal(2);
                            cptDest = reader.GetInt32(3);
                            cptExt = reader.GetInt32(4);

                            Transaction trans = new Transaction(idtTransaction, Tools.ConversionDate(horodatage), montant, cptDest, cptExt);
                            transactions.Add(trans);
                        }
                    }
                }
            }

            return transactions;
        }

        // Projet Or - Partie 3 : Gestion des bénéficiaires
        public static List<Beneficiaire> ListeBeneficiairesAssocieClient(long numCarteClient)
        {
            // List<Beneficiaire> beneficiaires = new List<Beneficiaire>();
            var beneficiaire = new List<Beneficiaire>();

            string connectionString = ConstructionConnexionString(fileDb);

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqliteCommand(queryListeBeneficiaire, connection))
                {
                    command.Parameters.AddWithValue("@numCarteClient", numCarteClient);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Beneficiaire b = new Beneficiaire()
                            {
                                NumCarteClient = reader.GetInt64(0),
                                IdtCptBenef = reader.GetInt32(1),
                                NumCarteBenef = reader.GetInt64(2),
                                NomBenef = reader.GetString(3),
                                PrenomBenef = reader.GetString(4),
                                TypeDuCompte = reader.GetString(5) == "Courant" ? TypeCompte.Courant : TypeCompte.Livret
                            };
                            beneficiaire.Add(b);
                        }
                    }
                }
            }
            return beneficiaire;
        }


        /// <summary>
        /// Procédure pour mettre à jour les données pour un retrait
        /// </summary>
        /// <param name="trans"></param>
        /// <returns></returns>
        public static bool EffectuerModificationOperationSimple(Transaction trans, long numCarte)
        {
            string connectionString = ConstructionConnexionString(fileDb);

            Operation typeOpe = Tools.TypeTransaction(trans.Expediteur, trans.Destinataire);

            if (typeOpe != Operation.DepotSimple && typeOpe != Operation.RetraitSimple)
            {
                return false;
            }

            int idtTrans = InfosIdtTrans() + 1;

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // Démarrer une transaction
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Insertion de la transaction
                        var insertTransac = ConstructionInsertionTransaction(connection, trans);
                        insertTransac.Transaction = transaction;
                        insertTransac.ExecuteNonQuery();

                        // Insertion de l'historique de transaction
                        var insertHistTransac = ConstructionInsertionHistTransaction(connection, idtTrans, numCarte);
                        insertHistTransac.Transaction = transaction;
                        insertHistTransac.ExecuteNonQuery();

                        // Mise à jour du solde du compte de l'opération simple
                        decimal montant = typeOpe == Operation.RetraitSimple ? trans.Montant : -trans.Montant;
                        int idtCpt = typeOpe == Operation.DepotSimple ? trans.Destinataire : trans.Expediteur;

                        var updateCompte = ConstructionUpdateSolde(connection, idtCpt, montant);
                        updateCompte.Transaction = transaction;
                        updateCompte.ExecuteNonQuery();

                        // Valider la transaction
                        transaction.Commit();
                        Console.WriteLine("Transaction validée.");
                    }
                    catch (Exception ex)
                    {
                        // En cas d’erreur, annuler la transaction
                        Console.WriteLine("Erreur : " + ex.Message);
                        transaction.Rollback();
                        Console.WriteLine("Transaction annulée.");
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Procédure pour mettre à jour les données pour un retrait
        /// </summary>
        /// <param name="trans"></param>
        /// <returns></returns>
        public static bool EffectuerModificationOperationInterCompte(Transaction trans, long numCarteExp, long numCarteDest)
        {
            string connectionString = ConstructionConnexionString(fileDb);

            Operation typeOpe = Tools.TypeTransaction(trans.Expediteur, trans.Destinataire);

            if (typeOpe != Operation.InterCompte)
            {
                return false;
            }

            int idtTrans = InfosIdtTrans() + 1;

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // Démarrer une transaction
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Insertion de la transaction
                        var insertTransac = ConstructionInsertionTransaction(connection, trans);
                        insertTransac.Transaction = transaction;
                        insertTransac.ExecuteNonQuery();

                        // Insertion de l'historique de transaction
                        var insertHistTransac = ConstructionInsertionHistTransaction(connection, idtTrans, numCarteExp);
                        insertHistTransac.Transaction = transaction;
                        insertHistTransac.ExecuteNonQuery();

                        if (numCarteDest != numCarteExp)
                        {
                            // Insertion de l'historique de transaction - côté destinataire
                            var insertHistTransacDest = ConstructionInsertionHistTransaction(connection, idtTrans, numCarteDest);
                            insertHistTransacDest.Transaction = transaction;
                            insertHistTransacDest.ExecuteNonQuery();
                        }

                        // Mise à jour du solde du compte de l'opération inter-compte 
                        // côté expéditeur
                        var updateCompteExp = ConstructionUpdateSolde(connection, trans.Expediteur, trans.Montant);
                        updateCompteExp.Transaction = transaction;
                        updateCompteExp.ExecuteNonQuery();

                        // côté destinataire
                        var updateCompteDest = ConstructionUpdateSolde(connection, trans.Destinataire, -trans.Montant);
                        updateCompteDest.Transaction = transaction;
                        updateCompteDest.ExecuteNonQuery();

                        // Valider la transaction
                        transaction.Commit();
                        Console.WriteLine("Transaction validée.");
                    }
                    catch (Exception ex)
                    {
                        // En cas d’erreur, annuler la transaction
                        Console.WriteLine("Erreur : " + ex.Message);
                        transaction.Rollback();
                        Console.WriteLine("Transaction annulée.");
                    }
                }
            }

            return true;
        }
        private static string ConstructionConnexionString(string fileDb)
        {
            string dossierRef = Directory.GetCurrentDirectory();
            string dossierProjet = Path.GetFullPath(Path.Combine(dossierRef, @"..\..\.."));

            string chemin = Path.Combine(dossierProjet, fileDb);
            return "Data Source=" + chemin;
        }

        private static SqliteCommand ConstructionInsertionTransaction(SqliteConnection connection, Transaction trans)
        {
            // Insertion de la transaction
            var insertTransac = connection.CreateCommand();
            insertTransac.CommandText = queryInsertTransac;

            insertTransac.Parameters.AddWithValue("@Horodatage", trans.Horodatage.ToString("dd/MM/yyyy hh:mm:ss"));
            insertTransac.Parameters.AddWithValue("@Montant", trans.Montant);
            insertTransac.Parameters.AddWithValue("@CptExp", trans.Expediteur);
            insertTransac.Parameters.AddWithValue("@CptDest", trans.Destinataire);

            return insertTransac;
        }
        private static SqliteCommand ConstructionInsertionHistTransaction(SqliteConnection connection, int idtTrans, long numCarte)
        {
            // Insertion de la transaction
            var insertHistTransac = connection.CreateCommand();
            insertHistTransac.CommandText = queryInsertHistTransac;

            insertHistTransac.Parameters.AddWithValue("@IdtTrans", idtTrans);
            insertHistTransac.Parameters.AddWithValue("@Carte", numCarte);

            return insertHistTransac;
        }

        /// <summary>
        /// COnstruction de la commande de mise à jour du solde du compte
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="idtCpt"></param>
        /// <param name="montant">Montant à soustraire au solde</param>
        /// <returns></returns>
        private static SqliteCommand ConstructionUpdateSolde(SqliteConnection connection, int idtCpt, decimal montant)
        {
            // Mise à jour du solde du compte
            var updateCompte = connection.CreateCommand();
            updateCompte.CommandText = queryUpdateCompte;
            updateCompte.Parameters.AddWithValue("@Montant", montant);
            updateCompte.Parameters.AddWithValue("@IdtCompte", idtCpt);

            return updateCompte;
        }

        // Projet Or - Partie 3 : Gestion des beneficiaires
        public static void AjouterBeneficiaire(long numCarteClient, int idCompteBenef)
        {
            string connectionString = ConstructionConnexionString(fileDb);

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // Ajouter bénéficiaires
                using (var ajout = new SqliteCommand(queryAjoutBenef, connection))
                {
                    ajout.Parameters.AddWithValue("@numCarteClient", numCarteClient);
                    ajout.Parameters.AddWithValue("@idCompteBenef", idCompteBenef);
                    ajout.ExecuteNonQuery();
                }
            }
        }
        public static void SupprimerBeneficiaire(long numCarteClient, int idCompteBenef)
        {
            string connectionString = ConstructionConnexionString(fileDb);

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // Supprimer bénéficiaires
                using (var suppr = new SqliteCommand(querySupprBenef, connection))
                {
                    try
                    {
                        suppr.Parameters.AddWithValue("@numCarteClient", numCarteClient);
                        suppr.Parameters.AddWithValue("@idCompteBenef", idCompteBenef);
                        suppr.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Erreur : " + e.Message);
                        Console.WriteLine("Pas de suppression");
                    }
                }
            }
        }

        public static bool EstBeneficiairePotentiel(int idCompte)
        {
            string connectionString = ConstructionConnexionString(fileDb);

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // Bénéficiaire potentiel ?
                using (var estBenef = new SqliteCommand(queryBeneficiairePotentiel, connection))
                {
                    estBenef.Parameters.AddWithValue("@idCompte", idCompte);
                    var count = Convert.ToInt32(estBenef.ExecuteScalar());
                    return count > 0;
                }
            }

        }

    }
}
