using Or.Business;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;


namespace Or.Models
{
    // Projet Or - Partie 2 : Sérialisation XML
    public class SerialisationXML
    {
        public static void SerialiserComptesTransaction(string nomFichier, long numCarte)
        {
            // On récupère tous les comptes associés à la carte 
            List<Compte> comptes = SqlRequests.ListeComptesAssociesCarte(numCarte);

            // On récupère les transactions associés à la carte
            List<Transaction> transacCarte = SqlRequests.ListeTransactionsAssociesCarte(numCarte);

            // On récupère les transactions associés au compte 
            // List<Transaction> transacCompte = SqlRequests.ListeTransactionsAssociesCompte(idtCpt);

            ExportComptes exportComptes = new ExportComptes();
            List<ExportCompte> compteExport = new List<ExportCompte>();
            foreach (var c in comptes)
            {
                ExportCompte export = new ExportCompte
                {
                    Id = c.Id,
                    TypeDuCompte = c.TypeDuCompte,
                    Solde = c.Solde.ToString("C2"),
                    Historique = new List<ExportCompteTransactions>()
                };

                foreach (var t in transacCarte)
                {
                    if (t.Expediteur == c.Id || t.Destinataire == c.Id)
                    {
                        ExportCompteTransactions exportTransactions = new ExportCompteTransactions
                        {
                            IdTransaction = t.IdTransaction,
                            Horodatage = t.Horodatage.ToString("dd/MM/yyyy HH:mm:ss"),
                            Type = Tools.TypeTransacConverter(Tools.TypeTransaction(t.Expediteur, t.Destinataire)).ToString(),
                            Expediteur = t.Expediteur != 0 ? t.Expediteur.ToString() : null,
                            Destinataire = t.Destinataire != 0 ? t.Destinataire.ToString() : null,
                            Montant = t.Montant.ToString("C2")
                        };

                        export.Historique.Add(exportTransactions);
                    }
                }
                compteExport.Add(export);
            }

            exportComptes.ListeComptes = compteExport;

            XmlSerializer serializer = new XmlSerializer(typeof(ExportComptes));

            using (TextWriter stream = new StreamWriter(nomFichier))
            {
                serializer.Serialize(stream, exportComptes);
                stream.Close();
            }

        }

    }
}
