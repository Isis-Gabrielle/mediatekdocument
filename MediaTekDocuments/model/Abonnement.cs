using System;
using System.ComponentModel;

namespace MediaTekDocuments.model
{
    /// <summary>
    /// Classe métier Abonnement hérite de Commande : contient des propriétés spécifiques aux abonnements de revues
    /// </summary>
    public class Abonnement : Commande
    {
        public DateTime DateFinAbonnement { get;}
        public string IdRevue { get; }

        public Abonnement(string id, DateTime dateCommande, int montant,
                         DateTime dateFinAbonnement, string idRevue)
            : base(id, dateCommande, montant)
        {
            this.DateFinAbonnement = dateFinAbonnement;
            this.IdRevue = idRevue;
        }

        /// <summary>
        /// Méthode pour comparer si la parution se trouve dans l'abonnement en cours
        /// </summary>
        /// <param name="dateCommande">date du début de l'abonnement</param>
        /// <param name="dateFin">date de la fin de l'abonnement</param>
        /// <param name="dateParution">date de la parution</param>
        /// <returns>true si la parution est comprise dans la période d'abonnement</returns>
        public bool ParutionDansAbonnement(DateTime dateCommande, DateTime dateFin, DateTime dateParution)
        {
            return dateParution >= dateCommande && dateParution <= dateFin;
        }
    }
}
