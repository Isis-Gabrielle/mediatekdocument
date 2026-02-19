using System;
using System.ComponentModel;

namespace MediaTekDocuments.model
{
    /// <summary>
    /// Classe métier CommandeDocument hérite de Commande : contient des propriétés spécifiques aux commandes de documents (Dvd, livres)
    /// </summary>
    public class CommandeDocument : Commande
    {
        public int NbExemplaire { get; }
        public string IdLivreDvd { get;}
        public string IdSuivi { get;}
        public string LibelleSuivi { get; }

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
