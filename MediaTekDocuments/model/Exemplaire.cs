using System;

namespace MediaTekDocuments.model
{
    /// <summary>
    /// Classe métier Exemplaire
    /// </summary>
    public class Exemplaire
    {
        public int Numero { get; set; }
        public string Photo { get; set; }
        public DateTime DateAchat { get; set; }
        public string IdEtat { get; set; }
        public string Id { get; set; }
        public string LibelleEtat { get; set; }

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
