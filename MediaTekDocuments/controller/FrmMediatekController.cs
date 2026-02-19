using System.Collections.Generic;
using MediaTekDocuments.model;
using MediaTekDocuments.dal;
using System.Xml.Linq;
using System.Linq;
using System;

namespace MediaTekDocuments.controller
{
    /// <summary>
    /// Contrôleur lié à FrmMediatek
    /// </summary>
    class FrmMediatekController
    {
        /// <summary>
        /// Objet d'accès aux données
        /// </summary>
        private readonly Access access;

        /// <summary>
        /// Récupération de l'instance unique d'accès aux données
        /// </summary>
        public FrmMediatekController()
        {
            access = Access.GetInstance();
        }

        /// <summary>
        /// Vérifie les identifiants d'un utilisateur
        /// </summary>
        /// <param name="email">Email de l'utilisateur</param>
        /// <param name="password">Mot de passe</param>
        /// <returns>Objet Utilisateur si trouvé, sinon null</returns>
        public Utilisateur Login(string email, string password)
        {
            return access.Login(email, password);

        }

        #region GET
        /// <summary>
        /// getter sur la liste des genres
        /// </summary>
        /// <returns>Liste d'objets Genre</returns>
        public List<Categorie> GetAllGenres()
        {
            return access.GetAllGenres();
        }

        /// <summary>
        /// getter sur la liste des livres
        /// </summary>
        /// <returns>Liste d'objets Livre</returns>
        public List<Livre> GetAllLivres()
        {
            return access.GetAllLivres();
        }

        /// <summary>
        /// getter sur la liste des Dvd
        /// </summary>
        /// <returns>Liste d'objets dvd</returns>
        public List<Dvd> GetAllDvd()
        {
            return access.GetAllDvd();
        }

        /// <summary>
        /// getter sur la liste des revues
        /// </summary>
        /// <returns>Liste d'objets Revue</returns>
        public List<Revue> GetAllRevues()
        {
            return access.GetAllRevues();
        }

        /// <summary>
        /// getter sur les rayons
        /// </summary>
        /// <returns>Liste d'objets Rayon</returns>
        public List<Categorie> GetAllRayons()
        {
            return access.GetAllRayons();
        }

        /// <summary>
        /// getter sur les publics
        /// </summary>
        /// <returns>Liste d'objets Public</returns>
        public List<Categorie> GetAllPublics()
        {
            return access.GetAllPublics();
        }

        /// <summary>
        /// getter sur la liste des commandes de documents
        /// </summary>
        /// <returns>Liste d'objets Commande</returns>
        public List<Commande> GetAllCommandes()
        {
            return access.GetAllCommandes();
        }

        /// <summary>
        /// getter sur la liste des abonnements
        /// </summary>
        /// <returns>Liste d'objets Abonnement</returns>
        public List<Abonnement> GetAllAbonnements()
        {
            return access.GetAllAbonnements();
        }

        /// <summary>
        /// récupère les exemplaires d'une revue
        /// </summary>
        /// <param name="idDocument">id de la revue concernée</param>
        /// <returns>Liste d'objets Exemplaire</returns>
        public List<Exemplaire> GetExemplaires(string idDocument)
        {
            return access.GetExemplaires(idDocument);
        }

        /// <summary>
        /// récupère les commandes d'un document
        /// </summary>
        /// <param name="idDocument">id du document</param>
        /// <returns>Liste d'objets CommandeDocument</returns>
        public List<CommandeDocument> GetCommandesDocument(string idDocument)
        {
            return access.GetCommandesDocument(idDocument);
        }

        /// <summary>
        /// getter sur les étapes de suivi
        /// </summary>
        /// <returns>Liste d'objets Suivi</returns>
        public List<Suivi> GetAllSuivi()
        {
            return access.GetAllSuivi();
        }

        /// <summary>
        /// getter sur la liste d'états
        /// </summary>
        /// <returns>Liste d'objets Etat</returns>
        public List<Etat> GetAllEtats()
        {
            return access.GetAllEtats();
        }

        /// <summary>
        /// récupère les abonnements d'une revue
        /// </summary>
        /// <param name="idDocument">id de la revue</param>
        /// <returns>Liste d'objets Abonnement</returns>
        public List<Abonnement> GetAbonnements(string idDocument)
        {
            return access.GetAbonnements(idDocument);
        }


        #endregion

        #region DELETE

        /// <summary>
        /// supprime un livre de la base de données
        /// </summary>
        /// <param name="livre">objet Livre à supprimer</param>
        /// <returns>True si la suppression est réussie</returns>
        public bool DeleteLivre(Livre livre)
        {
            return access.DeleteLivre(livre);
        }

        /// <summary>
        /// supprime une commande de la base de données
        /// </summary>
        /// <param name="commande">objet Commande à supprimer</param>
        /// <returns>True si la suppression est réussie</returns>
        public bool DeleteCommande(Commande commande)
        {
            return access.DeleteCommande(commande);
        }

        /// <summary>
        /// supprime un exemplaire de la base de données
        /// </summary>
        /// <param name="exemplaire">objet Exemplaire à supprimer</param>
        /// <returns>True si la suppression est réussie</returns>
        public bool DeleteExemplaire(Exemplaire exemplaire)
        {
            return access.DeleteExemplaire(exemplaire);
        }

        /// <summary>
        /// supprime une revue de la base de données
        /// </summary>
        /// <param name="revue">objet Revue à supprimer</param>
        /// <returns>True si la suppression est réussie</returns>
        public bool DeleteRevue(Revue revue)
        {
            return access.DeleteRevue(revue);
        }

        /// <summary>
        /// supprime un DVD de la base de données
        /// </summary>
        /// <param name="dvd">objet Dvd à supprimer</param>
        /// <returns>True si la suppression est réussie</returns>
        public bool DeleteDVD(Dvd dvd)
        {
            return access.DeleteDvd(dvd);
        }

        #endregion

        #region POST
        /// <summary>
        /// Crée un exemplaire d'une revue dans la bdd
        /// </summary>
        /// <param name="exemplaire">L'objet Exemplaire concerné</param>
        /// <returns>True si la création a pu se faire</returns>
        public bool CreerExemplaire(Exemplaire exemplaire)
        {
            return access.CreerExemplaire(exemplaire);
        }

        /// <summary>
        /// Crée un livre dans la bdd
        /// </summary>
        /// <param name="livre">L'objet Livre concerné</param>
        /// <returns>True si la création a pu se faire</returns>
        public bool AddLivre(Livre livre)
        {
            return access.AddLivre(livre);
        }

        /// <summary>
        /// Crée une revue dans la bdd
        /// </summary>
        /// <param name="revue">L'objet Revue concerné</param>
        /// <returns>True si la création a pu se faire</returns>
        public bool AddRevue(Revue revue)
        {
            return access.AddRevue(revue);
        }

        /// <summary>
        /// Crée un DVD dans la bdd
        /// </summary>
        /// <param name="dvd">L'objet Dvd concerné</param>
        /// <returns>True si la création a pu se faire</returns>
        public bool AddDvd(Dvd dvd)
        {
            return access.AddDVD(dvd);
        }

        /// <summary>
        /// Crée une commande d'un document dans la bdd
        /// </summary>
        /// <param name="commandedocument">L'objet CommandeDocument concerné</param>
        /// <returns>True si la création a pu se faire</returns>
        public bool CreerCommandeDocument(CommandeDocument commandedocument)
        {
            return access.CreerCommandeDocument(commandedocument);
        }

        /// <summary>
        /// Crée un abonnement dans la bdd
        /// </summary>
        /// <param name="abonnement">L'objet Abonnement concerné</param>
        /// <returns>True si la création a pu se faire</returns>
        public bool CreerAbonnement(Abonnement abonnement)
        {
            return access.CreerAbonnement(abonnement);
        }


        #endregion

        #region PUT

        /// <summary>
        /// Modifie les informations d'un livre
        /// </summary>
        /// <param name="livre">Objet Livre avec les nouvelles données</param>
        /// <returns>True si la modification est réussie</returns>
        public bool EditLivre(Livre livre)
        {
            return access.EditLivre(livre);
        }


        /// <summary>
        /// Modifie les informations d'un dvd
        /// </summary>
        /// <param name="dvd">Objet Dvd avec les nouvelles données</param>
        /// <returns>True si la modification est réussie</returns>
        public bool EditDvd(Dvd dvd)
        {
            return access.EditDvd(dvd);
        }

        /// <summary>
        /// Modifie les informations d'une revue
        /// </summary>
        /// <param name="revue">Objet Revue avec les nouvelles données</param>
        /// <returns>True si la modification est réussie</returns>
        public bool EditRevue(Revue revue)
        {
            return access.EditRevue(revue);
        }

        /// <summary>
        /// Modifie l'étape de suivi d'une commande
        /// </summary>
        /// <param name="idCommande">id de la commande</param>
        /// <param name="idSuivi">id du nouvel état de suivi</param>
        /// <returns>True si la modification est réussie</returns>
        public bool EditSuiviCommande(string idCommande, string idSuivi)
        {
            return access.EditSuiviCommande(idCommande, idSuivi);
        }

        /// <summary>
        /// Modifie l'état d'un exemplaire
        /// </summary>
        /// <param name="idDocument">id du document</param>
        /// <param name="numero">id de l'exemplaire</param>
        /// <param name="idEtat">id du nouvel état</param>
        /// <returns>True si la modification est réussie</returns>
        public bool EditExemplaireEtat(string idDocument, int numero, string idEtat)
        {
            return access.EditExemplaireEtat(idDocument, numero, idEtat);
        }
        #endregion

        /// <summary>
        /// Génère un nouvel identifiant pour un livre
        /// </summary>
        /// <returns>string de l'id généré</returns>
        public string GenerateNewLivresId()
        {
            var maxId = GetAllLivres()
                .Select(l => int.TryParse(l.Id, out int id) ? id : 0)
                .DefaultIfEmpty(0)
                .Max();

            return (maxId + 1).ToString("D4");
        }

        /// <summary>
        /// Génère un nouvel identifiant pour une revue
        /// </summary>
        /// <returns>string de l'id généré</returns>
        public string GenerateNewRevuesId()
        {
            var maxId = GetAllRevues()
                .Select(r => int.TryParse(r.Id, out int id) ? id : 0)
                .DefaultIfEmpty(0)
                .Max();
        
            return (maxId + 1).ToString("D4");
        }

        /// <summary>
        /// Génère un nouvel identifiant pour un dvd
        /// </summary>
        /// <returns>string de l'id généré</returns>
        public string GenerateNewDvdId()
        {
            var maxId = GetAllDvd()
                .Select(d => int.TryParse(d.Id, out int id) ? id : 0)
                .DefaultIfEmpty(0)
                .Max();
            return (maxId + 1).ToString("D4");
        }

        /// <summary>
        /// Génère un nouvel identifiant pour une commande
        /// </summary>
        /// <returns>string de l'id généré</returns>
        public string GenerateNewCommandeId()
        {
            var toutesLesCommandes = access.GetAllCommandes();
            var maxId = toutesLesCommandes
                .Select(d => int.TryParse(d.Id, out int id) ? id : 0)
                .DefaultIfEmpty(0)
                .Max();
            return (maxId + 1).ToString("D4");
        }


    }
}
