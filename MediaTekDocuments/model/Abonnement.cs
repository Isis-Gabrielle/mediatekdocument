using System;
using System.ComponentModel;

namespace MediaTekDocuments.model
{
    public class Abonnement : Commande
    {
        public DateTime DateFinAbonnement { get; set; }
        public string IdRevue { get; set; }

        public Abonnement(string id, DateTime dateCommande, int montant,
                         DateTime dateFinAbonnement, string idRevue)
            : base(id, dateCommande, montant)
        {
            this.DateFinAbonnement = dateFinAbonnement;
            this.IdRevue = idRevue;
        }

        public bool ParutionDansAbonnement(DateTime dateCommande, DateTime dateFin, DateTime dateParution)
        {
            return dateParution >= dateCommande && dateParution <= dateFin;
        }
    }
}
