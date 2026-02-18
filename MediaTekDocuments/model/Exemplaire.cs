using System;

namespace MediaTekDocuments.model
{
    /// <summary>
    /// Classe métier Exemplaire
    /// </summary>
    public class Exemplaire
    {
        public int Numero { get; }
        public string Photo { get; }
        public DateTime DateAchat { get;  }
        public string IdEtat { get; }
        public string Id { get;  }
        public string LibelleEtat { get; }

        public Exemplaire(int numero, DateTime dateAchat, string photo, string idEtat, string idDocument, string libelleEtat)
        {
            this.Numero = numero;
            this.DateAchat = dateAchat;
            this.Photo = photo;
            this.IdEtat = idEtat;
            this.Id = idDocument;
            this.LibelleEtat = libelleEtat;
        }
    }
}
