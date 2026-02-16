using System;
using System.ComponentModel;

namespace MediaTekDocuments.model
{
    public class CommandeDocument : Commande
    {
        public int NbExemplaire { get; set; }
        public string IdLivreDvd { get; set; }
        public string IdSuivi { get; set; }
        public string LibelleSuivi { get; set; }

        public CommandeDocument(string id, DateTime dateCommande, int montant,
                                int nbExemplaire, string idLivreDvd, string idSuivi, string libelleSuivi)
                                : base(id, dateCommande, montant)
        {

            this.NbExemplaire = nbExemplaire;
            this.IdLivreDvd = idLivreDvd;
            this.IdSuivi = idSuivi;
            this.LibelleSuivi = libelleSuivi;
        }
    }
}
