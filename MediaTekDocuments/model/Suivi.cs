namespace MediaTekDocuments.model
{
    public class Suivi
    {
        public string Id { get; }
        public string Libelle { get; }

        public Suivi(string id, string libelle)
        {
            this.Id = id;
            this.Libelle = libelle;
        }
    }
}
