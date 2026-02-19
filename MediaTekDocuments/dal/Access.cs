using MediaTekDocuments.manager;
using MediaTekDocuments.model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Net;
using System.Windows.Forms;
using System.Xml.Linq;
using Serilog;
using System.Security.Policy;

namespace MediaTekDocuments.dal
{
    /// <summary>
    /// Classe d'accès aux données
    /// </summary>
    public class Access
    {
        // CONFIGURATION
        
        /// <summary>
        /// clé de configuration pour l'adresse de l'API
        /// </summary>
        private static readonly string uriApi = "MediaTekDocuments.Properties.Settings.apiConnectionString";
        /// <summary>
        /// clé de configuration pour l'adresse de l'authentification
        /// </summary>
        private static readonly string connectionName = "mediatekDocuments.Properties.Settings.mediatekDocumentsConnectionString";

        // Constantes pour requêtes http

        /// <summary>
        /// méthode HTTP pour select
        /// </summary>
        private const string GET = "GET";
        /// <summary>
        /// méthode HTTP pour insert
        /// </summary>
        private const string POST = "POST";
        /// <summary>
        /// méthode HTTP pour update
        /// </summary>
        private const string PUT = "PUT";
        /// <summary>
        /// méthode HTTP pour delete
        /// </summary>
        private const string DELETE = "DELETE";
        /// <summary>
        /// préfixe pour requête
        /// </summary>
        private const string champs = "champs=";

        /// <summary>
        /// instance unique de la classe
        /// </summary>
        private static Access instance = null;

        /// <summary>
        /// instance de ApiRest pour envoyer des demandes vers l'api et recevoir la réponse
        /// </summary>
        private readonly ApiRest api = null;

        /// <summary>
        /// Méthode privée pour créer un singleton
        /// initialise l'accès à l'API
        /// </summary>
        private Access()
        {
            String authenticationString;
            String apiString;
            try
            {
                // configuration du système de logs
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.Console()
                    .WriteTo.File("logs/log.txt")
                    .CreateLogger();

                // récupération des chaînes de connexion dans le fichier App.config
                authenticationString = GetConnectionStringByName(connectionName);
                apiString = GetConnectionStringByName(uriApi);

                // initialisation d'API REST
                api = ApiRest.GetInstance(apiString, authenticationString);     
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Log.Fatal("Access.Access : Erreur critique d'initialisation. Connexion={0}, Erreur={1}", connectionName, e.Message); 
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Récupère une chaîne de connexion par son nom dans le fichier de configuration
        /// </summary>
        /// <param name="name">Nom de la chaîne de connexion</param>
        /// <returns>La chaîne de connexion</returns>
        static string GetConnectionStringByName(string name)
        {
            string returnValue = null;
            // Cherche dans le fichier App.config pour trouver la bonne chaîne
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[name];
            if (settings != null)
                returnValue = settings.ConnectionString;
            return returnValue;
        }

        /// <summary>
        /// Création et retour de l'instance unique de la classe
        /// </summary>
        /// <returns>instance unique de la classe</returns>
        public static Access GetInstance()
        {
            if (instance == null)
            {
                instance = new Access();
            }
            return instance;
        }

        #region AUTH
        /// <summary>
        /// Tente de connecter un utilisateur
        /// </summary>
        /// <param name="email">Email de l'utilisateur</param>
        /// <param name="password">Mot de passe</param>
        /// <returns>Objet Utilisateur</returns>
        public Utilisateur Login(string email, string password)
        {
            // créé un dictionnaire avec les identifiants
            Dictionary<string, object> loginData = new Dictionary<string, object> {
        { "email", email },
        { "password", password }};

            string jsonLogin = JsonConvert.SerializeObject(loginData);
            try
            {
                List<Utilisateur> liste = TraitementRecup<Utilisateur>(GET, "utilisateur/" + jsonLogin, null);
                return (liste != null && liste.Count > 0) ? liste[0] : null;
            }
            catch (Exception e)
            {
                Console.WriteLine("Erreur Authentification : " + e.Message);
                Log.Error("Access.Login catch request {0} on {1} erreur={2}", GET, "utilisateur/", e.Message);
                return null;
            }
        }
        #endregion

        #region GET

        /// <summary>
        /// Retourne tous les genres à partir de la BDD
        /// </summary>
        /// <returns>Liste d'objets Genre</returns>
        public List<Categorie> GetAllGenres()
        {
            IEnumerable<Genre> lesGenres = TraitementRecup<Genre>(GET, "genre", null);
            return new List<Categorie>(lesGenres);
        }

        /// <summary>
        /// Retourne tous les rayons à partir de la BDD
        /// </summary>
        /// <returns>Liste d'objets Rayon</returns>
        public List<Categorie> GetAllRayons()
        {
            IEnumerable<Rayon> lesRayons = TraitementRecup<Rayon>(GET, "rayon", null);
            return new List<Categorie>(lesRayons);
        }

        /// <summary>
        /// Retourne toutes les catégories de public à partir de la BDD
        /// </summary>
        /// <returns>Liste d'objets Public</returns>
        public List<Categorie> GetAllPublics()
        {
            IEnumerable<Public> lesPublics = TraitementRecup<Public>(GET, "public", null);
            return new List<Categorie>(lesPublics);
        }

        /// <summary>
        /// Retourne toutes les livres à partir de la BDD
        /// </summary>
        /// <returns>Liste d'objets Livre</returns>
        public List<Livre> GetAllLivres()
        {
            List<Livre> lesLivres = TraitementRecup<Livre>(GET, "livre", null);
            return lesLivres;
        }

        /// <summary>
        /// Retourne toutes les dvd à partir de la BDD
        /// </summary>
        /// <returns>Liste d'objets Dvd</returns>
        public List<Dvd> GetAllDvd()
        {
            List<Dvd> lesDvd = TraitementRecup<Dvd>(GET, "dvd", null);
            return lesDvd;
        }

        /// <summary>
        /// Retourne toutes les revues à partir de la BDD
        /// </summary>
        /// <returns>Liste d'objets Revue</returns>
        public List<Revue> GetAllRevues()
        {
            List<Revue> lesRevues = TraitementRecup<Revue>(GET, "revue", null);
            return lesRevues;
        }

        /// <summary>
        /// Retourne tous les abonnements à partir de la BDD
        /// </summary>
        /// <returns>Liste d'objets Abonnement</returns>
        public List<Abonnement> GetAllAbonnements()
        {
            List<Abonnement> lesAbonnements = TraitementRecup<Abonnement>(GET, "abonnement", null);
            return lesAbonnements;
        }

        /// <summary>
        /// Retourne toutes les commandes à partir de la BDD
        /// </summary>
        /// <returns>Liste d'objets Commande</returns>
        public List<Commande> GetAllCommandes()
        {
            List<Commande> lesCommandes = TraitementRecup<Commande>(GET, "commande", null);
            return lesCommandes;
        }

        /// <summary>
        /// Retourne les exemplaires d'un document
        /// </summary>
        /// <param name="idDocument">id du document concerné</param>
        /// <returns>Liste d'objets Exemplaire</returns>
        public List<Exemplaire> GetExemplaires(string idDocument)
        {
            String jsonIdDocument = convertToJson("id", idDocument);
            List<Exemplaire> lesExemplaires = TraitementRecup<Exemplaire>(GET, "exemplaire/" + jsonIdDocument, null);
            return lesExemplaires;
        }

        /// <summary>
        /// Retourne les commandes d'un document (Livre ou DVD)
        /// </summary>
        /// <param name="idDocument">id du document concerné</param>
        /// <returns>Liste d'objets CommandeDocument</returns>
        public List<CommandeDocument> GetCommandesDocument(string idDocument)
        {
            String jsonIdDocument = convertToJson("id", idDocument);
            List<CommandeDocument> lesCommandes = TraitementRecup<CommandeDocument>(GET, "commande/" + jsonIdDocument, null);
            return lesCommandes;
        }

        /// <summary>
        /// Retourne tous les états de suivi de commande
        /// </summary>
        /// <returns>Liste d'objets Suivi</returns>
        public List<Suivi> GetAllSuivi()
        {
            IEnumerable<Suivi> lesSuivis = TraitementRecup<Suivi>(GET, "suivi", null);
            return new List<Suivi>(lesSuivis);
        }

        /// <summary>
        /// Retourne tous les états possibles d'un exemplaire
        /// </summary>
        /// <returns>Liste d'objets Etat</returns>
        public List<Etat> GetAllEtats()
        {
            IEnumerable<Etat> lesEtats = TraitementRecup<Etat>(GET, "etat", null);
            return new List<Etat>(lesEtats);
        }

        /// <summary>
        /// Retourne les abonnements d'une revue
        /// </summary>
        /// <param name="idDocument">id de la revue</param>
        /// <returns>Liste d'objets Abonnement</returns>
        public List<Abonnement> GetAbonnements(string idDocument)
        {
            String jsonIdDocument = convertToJson("id", idDocument);
            List<Abonnement> lesAbonnements = TraitementRecup<Abonnement>(GET, "abonnement/" + jsonIdDocument, null);
            return lesAbonnements;
        }



        #endregion GET

        #region DELETE

        /// <summary>
        /// Supprime un livre de la BDD
        /// </summary>
        /// <param name="livre">Objet Livre à supprimer</param>
        /// <returns>True si la suppression est réussie</returns>
        public bool DeleteLivre(Livre livre) => SupprimerDocument("livre", livre.Id);

        /// <summary>
        /// Supprime un DVD de la BDD
        /// </summary>
        /// <param name="dvd">Objet Dvd à supprimer</param>
        /// <returns>True si la suppression est réussie</returns>
        public bool DeleteDvd(Dvd dvd) => SupprimerDocument("dvd", dvd.Id);

        /// <summary>
        /// Supprime une revue de la BDD
        /// </summary>
        /// <param name="revue">Objet Revue à supprimer</param>
        /// <returns>True si la suppression est réussie</returns>
        public bool DeleteRevue(Revue revue) => SupprimerDocument("revue", revue.Id);

        /// <summary>
        /// Méthode générique de suppression de document + vérification des exemplaires
        /// </summary>
        /// <param name="table">Nom de la table</param>
        /// <param name="id">id du document</param>
        /// <returns>True si succès, false si exemplaires rattachés</returns>
        private bool SupprimerDocument(string table, string id)
        {
            // Vérification des exemplaires avant suppression
            List<Exemplaire> exemplaires = GetExemplaires(id);
            if (exemplaires.Count > 0)
            {
                MessageBox.Show("Suppression impossible : des exemplaires sont rattachés.");
                return false;
            }

            try
            {
                string jsonId = convertToJson("id", id);

               
                return TraitementRecup<Livre>(DELETE, table + "/" + jsonId, null) != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur suppression {table} : " + ex.Message);
                Log.Error("Access.SupprimerDocument catch request {0} on {1} erreur={2}", DELETE, table, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Supprime une commande de la BDD
        /// </summary>
        /// <param name="commande">Objet Commande à supprimer</param>
        /// <returns>True si la suppression est réussie</returns>
        public bool DeleteCommande(Commande commande)
        {
            try
            {
                string jsonId = convertToJson("id", commande.Id);

                return TraitementRecup<Commande>(DELETE, "commande/" + jsonId, null) != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur DeleteCommande : " + ex.Message);
                Log.Error("Access.DeleteCommande catch request {0} on {1} erreur={2}", DELETE, "commande/", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Supprime un exemplaire de la BDD
        /// </summary>
        /// <param name="exemplaire">Objet Exemplaire à supprimer</param>
        /// <returns>True si la suppression est réussie</returns>
        public bool DeleteExemplaire(Exemplaire exemplaire)
        {
            try
            {
                // créé un objet anonyme, envoie uniquement les propriétés nécessaires
                var idData = new
                {
                    id = exemplaire.Id,
                    numero = exemplaire.Numero,
                };
                string jsonId = JsonConvert.SerializeObject(idData);

                return TraitementRecup<Exemplaire>(DELETE, "exemplaire/" + jsonId, null) != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur DeleteExemplaire : " + ex.Message);
                Log.Error("Access.DeleteExemplaire catch request {0} on {1} erreur={2}", DELETE, "exemplaire/", ex.Message);
                return false;
            }
        }


        #endregion DELETE

        #region POST

        /// <summary>
        /// ecriture d'un exemplaire en base de données
        /// </summary>
        /// <param name="exemplaire">exemplaire à insérer</param>
        /// <returns>true si l'insertion a pu se faire (retour != null)</returns>
        public bool CreerExemplaire(Exemplaire exemplaire)
        {
            String jsonExemplaire = JsonConvert.SerializeObject(exemplaire, new CustomDateTimeConverter());
            try
            {
                List<Exemplaire> liste = TraitementRecup<Exemplaire>(POST, "exemplaire", champs + jsonExemplaire);
                return (liste != null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Log.Error("Access.CreerExemplaire catch request {0} on {1} erreur={2}", POST, "exemplaire", ex.Message);
            }
            return false;
        }

        /// <summary>
        ///ecriture d'un livre en base de données
        /// </summary>
        /// <param name="livre">Objet Livre à insérer</param>
        /// <returns>true si l'insertion a pu se faire</returns>
        public bool AddLivre(Livre livre)
        {
            try
            {
                // créé un objet anonyme, envoie uniquement les propriétés nécessaires
                var livreData = new
                {
                    id = livre.Id,
                    titre = livre.Titre,
                    image = livre.Image,
                    idRayon = livre.IdRayon,
                    idPublic = livre.IdPublic,
                    idGenre = livre.IdGenre,
                    ISBN = livre.Isbn,
                    auteur = livre.Auteur,
                    collection = livre.Collection
                };

                string json = JsonConvert.SerializeObject(livreData);
                return TraitementRecup<Livre>(POST, "livre", champs + json) != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Log.Error("Access.AddLivre catch request {0} on {1} erreur={2}", POST, "livre", ex.Message);
                return false;
            }
        }

        /// <summary>
        ///ecriture d'une commande de document en base de données
        /// </summary>
        /// <param name="commande">Objet CommandeDocument à insérer</param>
        /// <returns>true si l'insertion a pu se faire</returns>
        public bool CreerCommandeDocument(CommandeDocument commande)
        {
            try
            {
                var commandeData = new
                {
                    id = commande.Id,
                    dateCommande = commande.DateCommande,
                    montant = commande.Montant,
                    nbExemplaire = commande.NbExemplaire,
                    idLivreDvd = commande.IdLivreDvd
                };

                string json = JsonConvert.SerializeObject(commandeData, new CustomDateTimeConverter());
                return TraitementRecup<CommandeDocument>(POST, "commandedocument", champs + json) != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur CreerCommandeDocument : " + ex.Message);
                Log.Error("Access.CreerCommandeDocument catch request {0} on {1} erreur={2}", POST, "commandedocument", ex.Message);
                return false;
            }
        }

        /// <summary>
        ///ecriture d'une revue en base de données
        /// </summary>
        /// <param name="revue">Objet Revue à insérer</param>
        /// <returns>true si l'insertion a pu se faire</returns>
        public bool AddRevue(Revue revue)
        {
            try
            {
                var revueData = new
                {
                    id = revue.Id,
                    titre = revue.Titre,
                    image = revue.Image,
                    idRayon = revue.IdRayon,
                    idPublic = revue.IdPublic,
                    idGenre = revue.IdGenre,
                    periodicite = revue.Periodicite,
                    delaiMiseADispo = revue.DelaiMiseADispo
                };

                string json = JsonConvert.SerializeObject(revueData);
                return TraitementRecup<Revue>(POST, "revue", champs + json) != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Log.Error("Access.AddRevue catch request {0} on {1} erreur={2}", POST, "revue", ex.Message);
                return false;
            }
        }

        /// <summary>
        ///ecriture d'un DVD en base de données
        /// </summary>
        /// <param name="dvd">Objet Dvd à insérer</param>
        /// <returns>true si l'insertion a pu se faire</returns>
        public bool AddDVD(Dvd dvd)
        {
            try
            {
                var dvdData = new
                {
                    id = dvd.Id,
                    titre = dvd.Titre,
                    image = dvd.Image,
                    idRayon = dvd.IdRayon,
                    idPublic = dvd.IdPublic,
                    idGenre = dvd.IdGenre,
                    synopsis = dvd.Synopsis,
                    realisateur = dvd.Realisateur,
                    duree = dvd.Duree
                };

                string json = JsonConvert.SerializeObject(dvdData);
                return TraitementRecup<Dvd>(POST, "dvd", champs + json) != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Log.Error("Access.AddDVD catch request {0} on {1} erreur={2}", POST, "dvd", ex.Message);

                return false;
            }
        }

        /// <summary>
        ///ecriture d'un abonnement en base de données
        /// </summary>
        /// <param name="abonnement">Objet Abonnement à insérer</param>
        /// <returns>true si l'insertion a pu se faire</returns>
        public bool CreerAbonnement(Abonnement abonnement)
        {
            try
            {
                var abonnementData = new
                {
                    id = abonnement.Id,
                    dateCommande = abonnement.DateCommande,
                    montant = abonnement.Montant,
                    dateFinAbonnement = abonnement.DateFinAbonnement,
                    idRevue = abonnement.IdRevue
                };

                string json = JsonConvert.SerializeObject(abonnementData, new CustomDateTimeConverter());
                return TraitementRecup<Abonnement>(POST, "abonnement", champs + json) != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur CreerAbonnement : " + ex.Message);
                Log.Error("Access.CreerAbonnement catch request {0} on {1} erreur={2}", POST, "abonnement", ex.Message);

                return false;
            }
        }
        #endregion POST

        #region PUT

        /// <summary>
        /// modifie les informations d'un livre
        /// </summary>
        /// <param name="livre">Objet Livre modifié</param>
        /// <returns>true si la modification a pu se faire</returns>
        public bool EditLivre(Livre livre)
        {
            try
            {
                var combinedData = new
                {
                    id = livre.Id,
                    titre = livre.Titre,
                    image = livre.Image,
                    idRayon = livre.IdRayon,
                    idPublic = livre.IdPublic,
                    idGenre = livre.IdGenre,
                    ISBN = livre.Isbn,
                    auteur = livre.Auteur,
                    collection = livre.Collection
                };
                string json = JsonConvert.SerializeObject(combinedData);
                 var result = TraitementRecup<Livre>(PUT, "livre/" + livre.Id, champs + json);
                return result != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur EditLivre : " + ex.Message);
                Log.Error("Access.EditLivre catch request {0} on {1} erreur={2}", PUT, "livre/", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// modifie les informations d'un dvd
        /// </summary>
        /// <param name="dvd">Objet Dvd modifié</param>
        /// <returns>true si la modification a pu se faire</returns>
        public bool EditDvd(Dvd dvd)
        {
            try
            {
                var combinedData = new
                {
                    id = dvd.Id,
                    titre = dvd.Titre,
                    image = dvd.Image,
                    idRayon = dvd.IdRayon,
                    idPublic = dvd.IdPublic,
                    idGenre = dvd.IdGenre,
                    synopsis = dvd.Synopsis,
                    realisateur = dvd.Realisateur,
                    duree = dvd.Duree
                };
                string json = JsonConvert.SerializeObject(combinedData);
                return TraitementRecup<Dvd>(PUT, "dvd/" + dvd.Id, champs + json) != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur EditDvd : " + ex.Message);
                Log.Error("Access.EditDvd catch request {0} on {1} erreur={2}", PUT, "dvd/", ex.Message); 
                return false; }
        }

        /// <summary>
        /// modifie les informations d'une revue
        /// </summary>
        /// <param name="revue">Objet Revue modifié</param>
        /// <returns>true si la modification a pu se faire</returns>
        public bool EditRevue(Revue revue)
        {
            try
            {
                var combinedData = new
                {
                    id = revue.Id,
                    titre = revue.Titre,
                    image = revue.Image,
                    idRayon = revue.IdRayon,
                    idPublic = revue.IdPublic,
                    idGenre = revue.IdGenre,
                    periodicite = revue.Periodicite,
                    delaiMiseADispo = revue.DelaiMiseADispo
                };
                string json = JsonConvert.SerializeObject(combinedData);
                return TraitementRecup<Revue>(PUT, "revue/" + revue.Id, champs + json) != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur EditRevue : " + ex.Message);
                Log.Error("Access.EditRevue catch request {0} on {1} erreur={2}", PUT, "revue/", ex.Message); 
                return false; }
        }

        /// <summary>
        /// modifie l'état de suivi d'une commande
        /// </summary>
        /// <param name="idCommande">id de la commande</param>
        /// <param name="idSuivi">nouvel id de suivi</param>
        /// <returns>true si la modification a pu se faire</returns>
        public bool EditSuiviCommande(string idCommande, string idSuivi)
        {
            try
            {
                var suiviData = new
                {
                    idsuivi = idSuivi
                };

                string json = JsonConvert.SerializeObject(suiviData);

                return TraitementRecup<CommandeDocument>(PUT, "commandedocument/" + idCommande, champs + json) != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur EditSuiviCommande : " + ex.Message);
                Log.Error("Access.EditSuiviCommande catch request {0} on {1} erreur={2}", PUT, "commandedocument/", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// modifie l'état d'un exemplaire
        /// </summary>
        /// <param name="idDocument">id du document</param>
        /// <param name="numero">numéro de l'exemplaire</param>
        /// <param name="idEtat">nouvel état de l'exemplaire</param>
        /// <returns>true si la modification a pu se faire</returns>
        public bool EditExemplaireEtat(string idDocument, int numero, string idEtat)
        {
            try
            {
                var etatData = new
                {
                    numero = numero.ToString(),
                    idetat = idEtat
                };

                string json = JsonConvert.SerializeObject(etatData);

                return TraitementRecup<Exemplaire>(PUT, "exemplaire/" + idDocument, champs + json) != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur EditExemplaireEtat : " + ex.Message);
                Log.Error("Access.EditExemplaireEtat catch request {0} on {1} erreur={2}", PUT, "exemplaire/", ex.Message);
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Traitement de la récupération du retour de l'api, avec conversion du json en liste pour les select (GET)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methode">verbe HTTP (GET, POST, PUT, DELETE)</param>
        /// <param name="message">information envoyée dans l'url</param>
        /// <param name="parametres">paramètres à envoyer dans le body, au format "chp1=val1&chp2=val2&..."</param>
        /// <returns>liste d'objets récupérés (ou liste vide)</returns>
        private List<T> TraitementRecup<T>(String methode, String message, String parametres)
        {
            // trans
            List<T> liste = new List<T>();
            try
            {
                JObject retour = api.RecupDistant(methode, message, parametres);
                // extraction du code retourné
                String code = (String)retour["code"];
                if (code.Equals("200"))
                {
                    // dans le cas du GET (select), récupération de la liste d'objets
                    if (methode.Equals(GET))
                    {
                        String resultString = JsonConvert.SerializeObject(retour["result"]);
                        // construction de la liste d'objets à partir du retour de l'api
                        liste = JsonConvert.DeserializeObject<List<T>>(resultString, new CustomBooleanJsonConverter());
                    }
                }
                else
                {
                    string secureMessage = message;
                    if (message.Contains("utilisateur/"))
                    {
                        secureMessage = "utilisateur/";
                    }

                    Console.WriteLine("code erreur = " + code + " message = " + (String)retour["message"]);

                    Log.Error("Access.TraitementRecup erreur API : Methode={0} Requête={1} Code={2}",
                              methode, secureMessage, code);
                }
            }
            catch (Exception e)
            {
                string secureMessage = message.Contains("utilisateur/") ? "utilisateur/ (masqué)" : message;

                Console.WriteLine("Erreur critique API : " + e.Message);
                Log.Fatal("Access.TraitementRecup catch critique : Methode={0} Requête={1} Erreur={2}",
                          methode, secureMessage, e.Message);
                Environment.Exit(0);
            }
            return liste;
        }

        /// <summary>
        /// Convertit en json un couple nom/valeur
        /// </summary>
        /// <param name="nom"></param>
        /// <param name="valeur"></param>
        /// <returns>couple au format json</returns>
        private String convertToJson(Object nom, Object valeur)
        {
            Dictionary<Object, Object> dictionary = new Dictionary<Object, Object>();
            dictionary.Add(nom, valeur);
            return JsonConvert.SerializeObject(dictionary);
        }

        /// <summary>
        /// Modification du convertisseur Json pour gérer le format de date
        /// </summary>
        private sealed class CustomDateTimeConverter : IsoDateTimeConverter
        {
            public CustomDateTimeConverter()
            {
                base.DateTimeFormat = "yyyy-MM-dd";
            }
        }

        /// <summary>
        /// Modification du convertisseur Json pour prendre en compte les booléens
        /// classe trouvée sur le site :
        /// https://www.thecodebuzz.com/newtonsoft-jsonreaderexception-could-not-convert-string-to-boolean/
        /// </summary>
        private sealed class CustomBooleanJsonConverter : JsonConverter<bool>
        {
            public override bool ReadJson(JsonReader reader, Type objectType, bool existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                return Convert.ToBoolean(reader.ValueType == typeof(string) ? Convert.ToByte(reader.Value) : reader.Value);
            }

            public override void WriteJson(JsonWriter writer, bool value, JsonSerializer serializer)
            {
                serializer.Serialize(writer, value);
            }
        }

    }
}