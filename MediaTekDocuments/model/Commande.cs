using System;

namespace MediaTekDocuments.model
{
    /// <summary>
    /// Classe métier Commande (réunit les infomations communes à toutes les commandes : CommandeDocument, Abonnement)
    /// </summary>
    /// 
    public class Commande
    {

        public string Id { get; }
        public DateTime DateCommande { get;}
        public int Montant { get; }

        public Commande(string id, DateTime dateCommande, int montant)
        {
            this.Id = id;
            this.DateCommande = dateCommande;
            this.Montant = montant;
        }
    }
}
