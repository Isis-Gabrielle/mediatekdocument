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

        public List<Commande> GetAllCommandes()
        {
            return access.GetAllCommandes();
        }

        public List<Abonnement> GetAllAbonnements()
        {
            return access.GetAllAbonnements();
        }
        /// <summary>
        /// récupère les exemplaires d'une revue
        /// </summary>
        /// <param name="idDocuement">id de la revue concernée</param>
        /// <returns>Liste d'objets Exemplaire</returns>
        public List<Exemplaire> GetExemplaires(string idDocument)
        {
            return access.GetExemplaires(idDocument);
        }

        public List<CommandeDocument> GetCommandesDocument(string idDocument)
        {
            return access.GetCommandesDocument(idDocument);
        }

        public List<Suivi> GetAllSuivi()
        {
            return access.GetAllSuivi();
        }

        public List<Etat> GetAllEtats()
        {
            return access.GetAllEtats();
        }

        public List<Abonnement> GetAbonnements(string idDocument)
        {
            return access.GetAbonnements(idDocument);
        }


        #endregion

        #region DELETE
        public bool DeleteLivre(Livre livre)
        {
            return access.DeleteLivre(livre);
        }
        public bool DeleteCommande(Commande commande)
        {
            return access.DeleteCommande(commande);
        }

        public bool DeleteExemplaire(Exemplaire exemplaire)
        {
            return access.DeleteExemplaire(exemplaire);
        }

        public bool DeleteRevue(Revue revue)
        {
            return access.DeleteRevue(revue);
        }

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

        public bool AddLivre(Livre livre)
        {
            return access.AddLivre(livre);
        }
        public bool AddRevue(Revue revue)
        {
            return access.AddRevue(revue);
        }
        public bool AddDvd(Dvd dvd)
        {
            return access.AddDVD(dvd);
        }
        public bool CreerCommandeDocument(CommandeDocument commandedocument)
        {
            return access.CreerCommandeDocument(commandedocument);
        }
        public bool CreerAbonnement(Abonnement abonnement)
        {
            return access.CreerAbonnement(abonnement);
        }

        
        #endregion

        #region PUT
        public bool EditLivre(Livre livre)
        {
            return access.EditLivre(livre);
        }
        public bool EditDvd(Dvd dvd)
        {
            return access.EditDvd(dvd);
        }
        public bool EditRevue(Revue revue)
        {
            return access.EditRevue(revue);
        }
        public bool EditSuiviCommande(string idCommande, string idSuivi)
        {
            return access.EditSuiviCommande(idCommande, idSuivi);
        }

        public bool EditExemplaireEtat(string idDocument, int numero, string idEtat)
        {
            return access.EditExemplaireEtat(idDocument, numero, idEtat);
        }
        #endregion

        public string GenerateNewLivresId()
        {
            var maxId = GetAllLivres()
                .Select(l => int.TryParse(l.Id, out int id) ? id : 0)
                .DefaultIfEmpty(0)
                .Max();

            return (maxId + 1).ToString("D4");
        }

        public string GenerateNewRevuesId()
        {
            var maxId = GetAllRevues()
                .Select(r => int.TryParse(r.Id, out int id) ? id : 0)
                .DefaultIfEmpty(0)
                .Max();
        
            return (maxId + 1).ToString("D4");
        }


        public string GenerateNewDvdId()
        {
            var maxId = GetAllDvd()
                .Select(d => int.TryParse(d.Id, out int id) ? id : 0)
                .DefaultIfEmpty(0)
                .Max();
            return (maxId + 1).ToString("D4");
        }
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
