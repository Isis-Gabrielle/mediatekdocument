namespace MediaTekDocuments.model
{

    /// <summary>
    /// Classe métier Suivi (étapes de suivi d'une commande)
    /// </summary>
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
