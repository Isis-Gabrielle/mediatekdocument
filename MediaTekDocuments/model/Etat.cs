namespace MediaTekDocuments.model
{
    /// <summary>
    /// Classe métier Etat (état d'usure d'un document)
    /// </summary>
    public class Etat
    {
        public string Id { get; }
        public string Libelle { get; }

        public Etat(string id, string libelle)
        {
            this.Id = id;
            this.Libelle = libelle;
        }
    }
}