namespace MediaTekDocuments.model
{
    public class Service
    {
        public string Id { get; set; }
        public string Libelle { get; set; }

        public Service(string id, string libelle)
        {
            this.Id = id;
            this.Libelle = libelle;
        }
    }
}