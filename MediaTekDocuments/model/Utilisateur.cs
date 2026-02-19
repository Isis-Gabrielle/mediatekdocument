namespace MediaTekDocuments.model
{
    /// <summary>
    /// Classe métier Utilisateur
    /// </summary>
    public class Utilisateur
    {
        public string Id { get; }
        public string Email { get; }
        public string Password { get; }
        public string IdService { get; }
        public string Service { get; }

        public Utilisateur(string id, string email, string password, string idService, string service)
        {
            Id = id;
            Email = email;
            Password = password;
            IdService = idService;
            Service = service;
        }
    }
}