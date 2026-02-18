using System;

namespace MediaTekDocuments.model
{

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
