using System;
using System.Windows.Forms;
using MediaTekDocuments.model;
using MediaTekDocuments.controller;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.IO;
using System.Xml.Linq;
using System.Collections;
using static System.Collections.Specialized.BitVector32;

namespace MediaTekDocuments.view

{
    /// <summary>
    /// Fenêtre de gestion des documents et leurs exemplaires
    /// </summary>
    public partial class FrmMediatek : Form
    {
        #region Commun

        /// <summary>
        /// Source de données pour les genres
        /// </summary>
        private readonly BindingSource bdgGenres = new BindingSource();

        /// <summary>
        /// Source de données pour les publics
        /// </summary>
        /// 
        private readonly BindingSource bdgPublics = new BindingSource();
        /// <summary>
        /// Source de données pour les rayons
        /// </summary>
        private readonly BindingSource bdgRayons = new BindingSource();

        /// <summary>
        /// Source de données pour les états des exemplaires
        /// </summary>
        private readonly BindingSource bdgEtats = new BindingSource();

        /// <summary>
        /// Filtre pour les boîtes de dialogue de fichiers images
        /// </summary>
        private const string files = "Files|*.jpg;*.bmp;*.jpeg;*.png;*.gif";

        /// <summary>
        /// Constante pour l'état de commande : Livrée
        /// </summary>
        private const string livree = "Livrée";

        /// <summary>
        /// Constante pour l'état de commande : Réglée
        /// </summary>
        private const string reglee = "Réglée";

        private const string selection = "Une ligne doit être sélectionnée.";
        private const string erreur = "Erreur";
        private const string numeroErreur = "numéro introuvable";
        private const string info = "Information";
        private const string confirmation = "Confirmation";

        /// <summary>
        /// Instance du contrôleur
        /// </summary>
        private readonly FrmMediatekController controller;

        /// <summary>
        /// Constructeur : création du contrôleur lié à ce formulaire + gestion des droits selon le service
        /// </summary>
        /// /// <param name="user">Utilisateur connecté pour les permissions</param>
        internal FrmMediatek(Utilisateur user)
        {
            InitializeComponent();
            this.controller = new FrmMediatekController();

            switch (user.IdService)
            {
                case "00001":
                    // Administrateur : tous les accès sont ouverts
                    break;

                case "00002":
                    // Prêts : restriction d'accès
                    tabReceptionRevue.Enabled = false;
                    cbxLivresExemplaireEtat.Enabled = false;
                    btnCommandesLivres.Enabled = false;
                    btnLivresExemplairesSupp.Enabled = false;
                    btnLivresExemplairesSaveEtat.Enabled = false;
                    btnLivresAjoutDocument.Enabled = false;
                    btnLivresModifDocument.Enabled = false;
                    btnLivresSuppDocument.Enabled = false;
                    break;

                case "00003":
                    // Culturel : accès bloqués
                    MessageBox.Show("Droits insuffisants pour accéder à l'application.");
                    Application.Exit();
                    break;
            }
        }

        /// <summary>
        /// Rempli un des 3 combo pour filtrer (genre, public, rayon)
        /// </summary>
        /// <param name="lesCategories">liste des objets de type Genre ou Public ou Rayon</param>
        /// <param name="bdg">bindingsource contenant les informations</param>
        /// <param name="cbx">combobox à remplir</param>
        public void RemplirComboCategorie(List<Categorie> lesCategories, BindingSource bdg, ComboBox cbx)
        {
            CombotCategorie(lesCategories, bdg, cbx);
        }

        /// <summary>
        /// Rempli un des 3 combo pour gérer les documents (genre, public, rayon)
        /// </summary>
        /// <param name="lesCategories">liste des objets de type Genre ou Public ou Rayon</param>
        /// <param name="bdg">bindingsource contenant les informations</param>
        /// <param name="cbx">combobox à remplir</param>
        public void RemplirComboEditCategorie(List<Categorie> lesCategories, BindingSource bdg, ComboBox cbx)
        {
            CombotCategorie(lesCategories, bdg, cbx);
        }

        /// <summary>
        /// Méthode qui lie une liste de catégories et un ComboBox via un BindingSource
        /// </summary>
        /// <param name="lesCategories">liste des objets de type Genre ou Public ou Rayon</param>
        /// <param name="bdg">bindingsource contenant les informations</param>
        /// <param name="cbx">>combobox à remplir</param>
        public void CombotCategorie(List<Categorie> lesCategories, BindingSource bdg, ComboBox cbx)
        {
            bdg.DataSource = lesCategories;
            cbx.DataSource = bdg;
            if (cbx.Items.Count > 0)
            {
                cbx.SelectedIndex = -1;
            }
        }

        #endregion Commun

        #region Onglet Livres

        /// <summary>
        /// Source de données pour la liste des livres
        /// </summary>
        private readonly BindingSource bdgLivresListe = new BindingSource();

        /// <summary>
        /// liste des livres récupérés depuis la bdd
        /// </summary>
        private List<Livre> lesLivres = new List<Livre>();

        /// <summary>
        /// Énumération des états possibles pour gérer les livres (Consultation par défaut)
        /// </summary>
        enum EtatLivre
        {
            Consultation,
            Ajout,
            Modification
        }
        private EtatLivre etatLivre = EtatLivre.Consultation;

        /// <summary>
        /// Affichage des informations du livre sélectionné
        /// </summary>
        /// <param name="livre">le livre</param>
        private void AfficheLivresInfos(Livre livre)
        {
            txbLivresAuteur.Text = livre.Auteur;
            txbLivresCollection.Text = livre.Collection;
            txbLivresImage.Text = livre.Image;
            txbLivresIsbn.Text = livre.Isbn;
            txbLivresNumero.Text = livre.Id;
            txbLivresGenre.Text = livre.Genre;
            txbLivresPublic.Text = livre.Public;
            txbLivresRayon.Text = livre.Rayon;
            txbLivresTitre.Text = livre.Titre;
            string image = livre.Image;

            // si le fichier n'existe pas ou s'il y a un problème, l'image ne se charge pas
            try
            {
                pcbLivresImage.Image = Image.FromFile(image);
            }
            catch
            {
                pcbLivresImage.Image = null;
            }
        }

        /// <summary>
        /// Sur le clic du bouton d'ajout, affichage interface pour la saisie d'un nouveau livre
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLivresAjoutDocument_Click(object sender, EventArgs e)
        {
            // chargement des catégories pour la saisie
            cbxLivresGenreAddEdit.DataSource = controller.GetAllGenres();
            cbxLivresPublicAddEdit.DataSource = controller.GetAllPublics();
            cbxLivresRayonAddEdit.DataSource = controller.GetAllRayons();

            // bascule l'interface en mode "Ajout"
            SetEtatLivre(EtatLivre.Ajout);
            VideLivresInfos();

            // Génère un nouvel id
            txbLivresNumero.Text = controller.GenerateNewLivresId();
        }

        /// <summary>
        /// Sur le clic du le bouton annuler, interface en mode consultation après confirmation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLivresAnnulerDocument_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Voulez-vous vraiment annuler ?", confirmation, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                SetEtatLivre(EtatLivre.Consultation);
            }
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des dvd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnLivresAnnulGenres_Click(object sender, EventArgs e)
        {
            RemplirLivresListeComplete();
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des dvd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnLivresAnnulPublics_Click(object sender, EventArgs e)
        {
            RemplirLivresListeComplete();
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des dvd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnLivresAnnulRayons_Click(object sender, EventArgs e)
        {
            RemplirLivresListeComplete();
        }

        /// <summary>
        /// Recherche image sur disque
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLivreSearchImage_Click(object sender, EventArgs e)
        {
            string filePath = "";
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                InitialDirectory = Path.GetPathRoot(Environment.CurrentDirectory),
                Filter = files
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;
            }
            txbLivresImage.Text = filePath;
            try
            {
                pcbLivresImage.Image = Image.FromFile(filePath);
            }
            catch
            {
                pcbLivresImage.Image = null;
            }
        }

        /// <summary>
        /// Sur le clic du bouton de modification : prépare l'interface avec les données du livre sélectionné
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLivresModifDocument_Click(object sender, EventArgs e)
        {
            // vérifie si un livre est bien sélectionné dans la grille
            if (dgvLivresListe.SelectedRows.Count > 0)
            {
                Livre livre = (Livre)bdgLivresListe.List[bdgLivresListe.Position];

                // charge la liste des catégories
                cbxLivresGenreAddEdit.DataSource = controller.GetAllGenres();
                cbxLivresPublicAddEdit.DataSource = controller.GetAllPublics();
                cbxLivresRayonAddEdit.DataSource = controller.GetAllRayons();

                // les catégories sont présélectionnées dans les combobox
                cbxLivresGenreAddEdit.SelectedIndex = cbxLivresGenreAddEdit.FindStringExact(livre.Genre);
                cbxLivresPublicAddEdit.SelectedIndex = cbxLivresPublicAddEdit.FindStringExact(livre.Public);
                cbxLivresRayonAddEdit.SelectedIndex = cbxLivresRayonAddEdit.FindStringExact(livre.Rayon);

                // bascule l'interface en mode "Modification"
                SetEtatLivre(EtatLivre.Modification);

                txbLivresAuteur.Text = livre.Auteur;
                txbLivresCollection.Text = livre.Collection;
                txbLivresImage.Text = livre.Image;
                txbLivresIsbn.Text = livre.Isbn;
                txbLivresNumero.Text = livre.Id;
                txbLivresTitre.Text = livre.Titre;
            }
            else
            {
                MessageBox.Show(selection);
            }
        }

        /// <summary>
        /// Recherche et affichage du livre dont on a saisi le numéro.
        /// Si non trouvé, affichage d'un MessageBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnLivresNumRecherche_Click(object sender, EventArgs e)
        {
            if (!txbLivresNumRecherche.Text.Equals(""))
            {
                txbLivresTitreRecherche.Text = "";
                cbxLivresGenres.SelectedIndex = -1;
                cbxLivresRayons.SelectedIndex = -1;
                cbxLivresPublics.SelectedIndex = -1;
                Livre livre = lesLivres.Find(x => x.Id.Equals(txbLivresNumRecherche.Text));
                if (livre != null)
                {
                    List<Livre> dvd = new List<Livre>() { livre };
                    RemplirLivresListe(dvd);
                }
                else
                {
                    MessageBox.Show(numeroErreur);
                    RemplirLivresListeComplete();
                }
            }
            else
            {
                RemplirLivresListeComplete();
            }
        }

        /// <summary>
        /// Sur le clic du bouton Enregistrer : enregistre les modifications (Ajout ou Modification) en bdd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLivresSaveDocument_Click(object sender, EventArgs e)
        {
            // vérifie que les champs obligatoires ne sont pas vides
            if (string.IsNullOrWhiteSpace(txbLivresTitre.Text) || cbxLivresGenreAddEdit.SelectedItem == null ||
                cbxLivresPublicAddEdit.SelectedItem == null || cbxLivresRayonAddEdit.SelectedItem == null)
            { MessageBox.Show("Tous les champs doivent être remplis.", info); return;
            }

            // récupère les catégories sélectionnées dans les combobox
            Genre genre = (Genre)cbxLivresGenreAddEdit.SelectedItem;
            Public lePublic = (Public)cbxLivresPublicAddEdit.SelectedItem;
            Rayon rayon = (Rayon)cbxLivresRayonAddEdit.SelectedItem;

            // instance le nouveau livre avec les données du formulaire
            Livre livre = new Livre(
                txbLivresNumero.Text,
                txbLivresTitre.Text,
                txbLivresImage.Text,
                txbLivresIsbn.Text,
                txbLivresAuteur.Text,
                txbLivresCollection.Text,
                genre.Id, genre.Libelle,
                lePublic.Id, lePublic.Libelle,
                rayon.Id, rayon.Libelle
            );

            bool operationOk = false;

            // contrôleur appelé pour sauvegarder en BDD selon l'état actuel (Ajout ou Modif)
            try
            {
                if (etatLivre == EtatLivre.Ajout)
                {
                    operationOk = controller.AddLivre(livre);
                    if (!operationOk)
                        MessageBox.Show("Erreur lors de l'ajout du livre. Le numéro existe peut-être déjà probablement.", erreur);
                }
                else
                {
                    operationOk = controller.EditLivre(livre);
                    if (!operationOk)
                        MessageBox.Show("Erreur lors de la modification du livre.", erreur);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Une erreur inattendue est survenue : " + ex.Message, erreur);
                return;
            }

            // rafraichit la liste locale et bascule en mode "Consultation"
            if (operationOk)
            {
                lesLivres = controller.GetAllLivres();
                RemplirLivresListeComplete();
                SetEtatLivre(EtatLivre.Consultation);
            }
        }

        /// <summary>
        /// Sur le clic du bouton Suppression : supprime le livre sélectionné après confirmation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLivresSuppDocument_Click(object sender, EventArgs e)
        {
            if (dgvLivresListe.SelectedRows.Count > 0)
            {
                Livre livre = (Livre)bdgLivresListe.List[bdgLivresListe.Position];
                if (MessageBox.Show("Voulez-vous vraiment supprimer " + livre.Titre + " de l'auteur " + 
                    livre.Auteur + " ?", "Confirmation de suppression", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    // supprime en base de données, recharge la liste
                    controller.DeleteLivre(livre);
                    lesLivres = controller.GetAllLivres();
                    RemplirLivresListeComplete();
                }
            }
            else
            {
                MessageBox.Show(selection);
            }
        }

        /// <summary>
        /// Filtre sur le genre
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbxLivresGenres_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxLivresGenres.SelectedIndex >= 0)
            {
                txbLivresTitreRecherche.Text = "";
                txbLivresNumRecherche.Text = "";
                Genre genre = (Genre)cbxLivresGenres.SelectedItem;
                List<Livre> livre = lesLivres.FindAll(x => x.Genre.Equals(genre.Libelle));
                RemplirLivresListe(livre);
                cbxLivresRayons.SelectedIndex = -1;
                cbxLivresPublics.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Filtre sur la catégorie de public
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbxLivresPublics_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxLivresPublics.SelectedIndex >= 0)
            {
                txbLivresTitreRecherche.Text = "";
                txbLivresNumRecherche.Text = "";
                Public lePublic = (Public)cbxLivresPublics.SelectedItem;
                List<Livre> livre = lesLivres.FindAll(x => x.Public.Equals(lePublic.Libelle));
                RemplirLivresListe(livre);
                cbxLivresRayons.SelectedIndex = -1;
                cbxLivresGenres.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Filtre sur le rayon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbxLivresRayons_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxLivresRayons.SelectedIndex >= 0)
            {
                txbLivresTitreRecherche.Text = "";
                txbLivresNumRecherche.Text = "";
                Rayon rayon = (Rayon)cbxLivresRayons.SelectedItem;
                List<Livre> livre = lesLivres.FindAll(x => x.Rayon.Equals(rayon.Libelle));
                RemplirLivresListe(livre);
                cbxLivresGenres.SelectedIndex = -1;
                cbxLivresPublics.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Tri sur les colonnes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DgvLivresListe_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            VideLivresZones();
            string titreColonne = dgvLivresListe.Columns[e.ColumnIndex].HeaderText;
            List<Livre> sortedList = new List<Livre>();
            switch (titreColonne)
            {
                case "Id":
                    sortedList = lesLivres.OrderBy(o => o.Id).ToList();
                    break;

                case "Titre":
                    sortedList = lesLivres.OrderBy(o => o.Titre).ToList();
                    break;

                case "Collection":
                    sortedList = lesLivres.OrderBy(o => o.Collection).ToList();
                    break;

                case "Auteur":
                    sortedList = lesLivres.OrderBy(o => o.Auteur).ToList();
                    break;

                case "Genre":
                    sortedList = lesLivres.OrderBy(o => o.Genre).ToList();
                    break;

                case "Public":
                    sortedList = lesLivres.OrderBy(o => o.Public).ToList();
                    break;

                case "Rayon":
                    sortedList = lesLivres.OrderBy(o => o.Rayon).ToList();
                    break;
            }
            RemplirLivresListe(sortedList);
        }

        /// <summary>
        /// Sur la sélection d'une ligne ou cellule dans le grid
        /// affichage des informations du livre
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DgvLivresListe_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvLivresListe.CurrentCell != null)
            {
                try
                {
                    Livre livre = (Livre)bdgLivresListe.List[bdgLivresListe.Position];
                    AfficheLivresInfos(livre);
                    AfficheExemplairesLivres(livre);
                }
                catch
                {
                    VideLivresZones();
                }
            }
            else
            {
                VideLivresInfos();
            }
        }

        /// <summary>
        /// Modifie l'état de l'interface en fonction du mode (Consultation, Ajout, Modification)
        /// </summary>
        /// <param name="etatlivre">Le nouvel état déclenché</param>
        private void SetEtatLivre(EtatLivre etatlivre)
        {
            etatLivre = etatlivre;
            bool edition = (etatlivre != EtatLivre.Consultation);

            // visibilité
            cbxLivresGenreAddEdit.Visible = edition;
            cbxLivresPublicAddEdit.Visible = edition;
            cbxLivresRayonAddEdit.Visible = edition;
            btnLivreSearchImage.Visible = edition;
            btnLivresSaveDocument.Visible = edition;
            btnLivresAnnulerDocument.Visible = edition;
            btnLivresSuppDocument.Visible = !edition;
            btnLivresAjoutDocument.Visible = !edition;
            btnLivresModifDocument.Visible = !edition;
            dgvLivresListe.Enabled = !edition;

            // readonly
            txbLivresAuteur.ReadOnly = !edition;
            txbLivresCollection.ReadOnly = !edition;
            txbLivresImage.ReadOnly = !edition;
            txbLivresIsbn.ReadOnly = !edition;
            txbLivresTitre.ReadOnly = !edition;

            txbLivresGenre.Visible = !edition;
            txbLivresPublic.Visible = !edition;
            txbLivresRayon.Visible = !edition;

            // titre du groupe
            grpLivresInfos.Text =
                etatlivre == EtatLivre.Ajout ? "Ajouter le livre" :
                etatlivre == EtatLivre.Modification ? "Modifier le livre" :
                "Informations du livre";
        }

        /// <summary>
        /// Remplit le datagrid avec la liste reçue en paramètre
        /// </summary>
        /// <param name="livre">liste de livre</param>
        private void RemplirLivresListe(List<Livre> livre)
        {
            bdgLivresListe.DataSource = livre;
            dgvLivresListe.DataSource = bdgLivresListe;
            dgvLivresListe.Columns["isbn"].Visible = false;
            dgvLivresListe.Columns["idRayon"].Visible = false;
            dgvLivresListe.Columns["idGenre"].Visible = false;
            dgvLivresListe.Columns["idPublic"].Visible = false;
            dgvLivresListe.Columns["image"].Visible = false;
            dgvLivresListe.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvLivresListe.Columns["id"].DisplayIndex = 0;
            dgvLivresListe.Columns["titre"].DisplayIndex = 1;
        }

        /// <summary>
        /// Affichage de la liste complète des dvd
        /// et annulation de toutes les recherches et filtres
        /// </summary>
        private void RemplirLivresListeComplete()
        {
            RemplirLivresListe(lesLivres);
            VideLivresZones();
        }

        /// <summary>
        /// Ouverture de l'onglet Livres :
        /// appel des méthodes pour remplir le datagrid des dvd et des combos (genre, rayon, public)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabLivres_Enter(object sender, EventArgs e)
        {
            lesLivres = controller.GetAllLivres();
            RemplirComboCategorie(controller.GetAllGenres(), bdgGenres, cbxLivresGenres);
            RemplirComboCategorie(controller.GetAllPublics(), bdgPublics, cbxLivresPublics);
            RemplirComboCategorie(controller.GetAllRayons(), bdgRayons, cbxLivresRayons);
            bdgEtats.DataSource = controller.GetAllEtats();
            cbxLivresExemplaireEtat.DataSource = bdgEtats;
            if (cbxLivresExemplaireEtat.Items.Count > 0)
            {
                cbxLivresExemplaireEtat.SelectedIndex = -1;
            }
            cbxLivresExemplaireEtat.DisplayMember = "Libelle";
            cbxLivresExemplaireEtat.ValueMember = "Id";

            RemplirLivresListeComplete();
        }
        /// <summary>
        /// Recherche et affichage des dvd dont le titre matche acec la saisie.
        /// Cette procédure est exécutée à chaque ajout ou suppression de caractère
        /// dans le textBox de saisie.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxbLivresTitreRecherche_TextChanged(object sender, EventArgs e)
        {
            if (!txbLivresTitreRecherche.Text.Equals(""))
            {
                cbxLivresGenres.SelectedIndex = -1;
                cbxLivresRayons.SelectedIndex = -1;
                cbxLivresPublics.SelectedIndex = -1;
                txbLivresNumRecherche.Text = "";
                List<Livre> lesLivresParTitre;
                lesLivresParTitre = lesLivres.FindAll(x => x.Titre.ToLower().Contains(txbLivresTitreRecherche.Text.ToLower()));
                RemplirLivresListe(lesLivresParTitre);
            }
            else
            {
                // si la zone de saisie est vide et aucun élément combo sélectionné, réaffichage de la liste complète
                if (cbxLivresGenres.SelectedIndex < 0 && cbxLivresPublics.SelectedIndex < 0 && cbxLivresRayons.SelectedIndex < 0
                    && txbLivresNumRecherche.Text.Equals(""))
                {
                    RemplirLivresListeComplete();
                }
            }
        }
        /// <summary>
        /// Vide les zones d'affichage des informations du livre
        /// </summary>
        private void VideLivresInfos()
        {
            txbLivresAuteur.Text = "";
            txbLivresCollection.Text = "";
            txbLivresImage.Text = "";
            txbLivresIsbn.Text = "";
            txbLivresNumero.Text = "";
            txbLivresGenre.Text = "";
            txbLivresPublic.Text = "";
            txbLivresRayon.Text = "";
            txbLivresTitre.Text = "";
            pcbLivresImage.Image = null;
        }
        /// <summary>
        /// vide les zones de recherche et de filtre
        /// </summary>
        private void VideLivresZones()
        {
            cbxLivresGenres.SelectedIndex = -1;
            cbxLivresRayons.SelectedIndex = -1;
            cbxLivresPublics.SelectedIndex = -1;
            txbLivresNumRecherche.Text = "";
            txbLivresTitreRecherche.Text = "";
        }
        #endregion Onglet Livres

        #region Onglet Dvd

        /// <summary>
        /// Source de données pour la liste des DVD
        /// </summary>
        private readonly BindingSource bdgDvdListe = new BindingSource();

        /// <summary>
        /// liste des dvd récupérés depuis la bdd
        /// </summary>
        private List<Dvd> lesDvd = new List<Dvd>();

        /// <summary>
        /// Énumération des états possibles pour gérer les dvd (Consultation par défaut)
        /// </summary>
        enum EtatDvd
        {
            Consultation,
            Ajout,
            Modification
        }
        private EtatDvd etatDvd = EtatDvd.Consultation;

        /// <summary>
        /// Affichage des informations du dvd sélectionné
        /// </summary>
        /// <param name="dvd">le dvd</param>
        private void AfficheDvdInfos(Dvd dvd)
        {
            txbDvdRealisateur.Text = dvd.Realisateur;
            txbDvdSynopsis.Text = dvd.Synopsis;
            txbDvdImage.Text = dvd.Image;
            txbDvdDuree.Text = dvd.Duree.ToString();
            txbDvdNumero.Text = dvd.Id;
            txbDvdGenre.Text = dvd.Genre;
            txbDvdPublic.Text = dvd.Public;
            txbDvdRayon.Text = dvd.Rayon;
            txbDvdTitre.Text = dvd.Titre;
            string image = dvd.Image;
            try
            {
                pcbDvdImage.Image = Image.FromFile(image);
            }
            catch
            {
                pcbDvdImage.Image = null;
            }
        }

        /// <summary>
        /// Modifie l'état de l'interface en fonction du mode (Consultation, Ajout, Modification)
        /// </summary>
        /// <param name="etatdvd">Le nouvel état déclenché</param>
        private void SetEtatDvd(EtatDvd etatdvd)
        {
            etatDvd = etatdvd;
            bool edition = (etatdvd != EtatDvd.Consultation);

            // visibilité
            cbxDvdGenreAddEdit.Visible = edition;
            cbxDvdPublicAddEdit.Visible = edition;
            cbxDvdRayonAddEdit.Visible = edition;
            btnDVDSearchImage.Visible = edition;
            btnDVDSaveDocument.Visible = edition;
            btnDVDAnnulerDocument.Visible = edition;
            btnDVDSuppDocument.Visible = !edition;
            btnDVDAjoutDocument.Visible = !edition;
            btnDVDModifDocument.Visible = !edition;
            dgvDvdListe.Enabled = !edition;

            // readonly
            txbDvdDuree.ReadOnly = !edition;
            txbDvdSynopsis.ReadOnly = !edition;
            txbDvdImage.ReadOnly = !edition;
            txbDvdRealisateur.ReadOnly = !edition;
            txbDvdTitre.ReadOnly = !edition;

            txbDvdGenre.Visible = !edition;
            txbDvdPublic.Visible = !edition;
            txbDvdRayon.Visible = !edition;

            // titre du groupe
            grpDvdInfos.Text =
                etatdvd == EtatDvd.Ajout ? "Ajouter le dvd" :
                etatdvd == EtatDvd.Modification ? "Modifier le dvd" :
                "Informations détaillées";
        }

        /// <summary>
        /// Sur le clic du bouton d'ajout, affichage interface pour la saisie d'un nouveau DVD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDVDAjoutDocument_Click(object sender, EventArgs e)
        {
            cbxDvdGenreAddEdit.DataSource = controller.GetAllGenres();
            cbxDvdPublicAddEdit.DataSource = controller.GetAllPublics();
            cbxDvdRayonAddEdit.DataSource = controller.GetAllRayons();

            SetEtatDvd(EtatDvd.Ajout);
            VideDvdInfos();

            txbDvdNumero.Text = controller.GenerateNewDvdId();
        }

        /// <summary>
        /// Sur le clic du le bouton annuler, interface en mode consultation après confirmation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDVDAnnulerDocument_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Voulez-vous vraiment annuler ?", confirmation, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                SetEtatDvd(EtatDvd.Consultation);
            }
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des Dvd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDvdAnnulGenres_Click(object sender, EventArgs e)
        {
            RemplirDvdListeComplete();
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des Dvd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDvdAnnulPublics_Click(object sender, EventArgs e)
        {
            RemplirDvdListeComplete();
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des Dvd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDvdAnnulRayons_Click(object sender, EventArgs e)
        {
            RemplirDvdListeComplete();
        }

        /// <summary>
        /// Sur le clic du bouton de modification : prépare l'interface avec les données du dvd sélectionné
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDVDModifDocument_Click(object sender, EventArgs e)
        {
            if (dgvDvdListe.SelectedRows.Count > 0)
            {
                Dvd dvd = (Dvd)bdgDvdListe.List[bdgDvdListe.Position];
                cbxDvdGenreAddEdit.DataSource = controller.GetAllGenres();
                cbxDvdPublicAddEdit.DataSource = controller.GetAllPublics();
                cbxDvdRayonAddEdit.DataSource = controller.GetAllRayons();

                cbxDvdGenreAddEdit.SelectedIndex = cbxDvdGenreAddEdit.FindStringExact(dvd.Genre);
                cbxDvdPublicAddEdit.SelectedIndex = cbxDvdPublicAddEdit.FindStringExact(dvd.Public);
                cbxDvdRayonAddEdit.SelectedIndex = cbxDvdRayonAddEdit.FindStringExact(dvd.Rayon);


                SetEtatDvd(EtatDvd.Modification);
                txbDvdDuree.Text = dvd.Duree.ToString();
                txbDvdSynopsis.Text = dvd.Synopsis;
                txbDvdImage.Text = dvd.Image;
                txbDvdRealisateur.Text = dvd.Realisateur;
                txbDvdNumero.Text = dvd.Id;
                txbDvdTitre.Text = dvd.Titre;

            }
            else
            {
                MessageBox.Show(selection);
            }
        }

        /// <summary>
        /// Recherche et affichage du Dvd dont on a saisi le numéro.
        /// Si non trouvé, affichage d'un MessageBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDvdNumRecherche_Click(object sender, EventArgs e)
        {
            if (!txbDvdNumRecherche.Text.Equals(""))
            {
                txbDvdTitreRecherche.Text = "";
                cbxDvdGenres.SelectedIndex = -1;
                cbxDvdRayons.SelectedIndex = -1;
                cbxDvdPublics.SelectedIndex = -1;
                Dvd dvd = lesDvd.Find(x => x.Id.Equals(txbDvdNumRecherche.Text));
                if (dvd != null)
                {
                    List<Dvd> Dvd = new List<Dvd>() { dvd };
                    RemplirDvdListe(Dvd);
                }
                else
                {
                    MessageBox.Show(numeroErreur);
                    RemplirDvdListeComplete();
                }
            }
            else
            {
                RemplirDvdListeComplete();
            }
        }

        /// <summary>
        /// Sur le clic du bouton Enregistrer : enregistre les modifications (Ajout ou Modification) en bdd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDVDSaveDocument_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txbDvdTitre.Text) ||
               string.IsNullOrWhiteSpace(txbDvdRealisateur.Text) ||
               cbxDvdGenreAddEdit.SelectedItem == null ||
               cbxDvdPublicAddEdit.SelectedItem == null ||
               cbxDvdRayonAddEdit.SelectedItem == null)
            {
                MessageBox.Show("Tous les champs doivent être remplis.", info);
                return;
            }

            Genre genre = (Genre)cbxDvdGenreAddEdit.SelectedItem;
            Public lePublic = (Public)cbxDvdPublicAddEdit.SelectedItem;
            Rayon rayon = (Rayon)cbxDvdRayonAddEdit.SelectedItem;

            if (!int.TryParse(txbDvdDuree.Text, out int duree))
            {
                MessageBox.Show("La durée doit être un nombre entier.", erreur);
                return;
            }
            Dvd dvd = new Dvd(
                txbDvdNumero.Text,
                txbDvdTitre.Text,
                txbDvdImage.Text,
                duree,
                txbDvdRealisateur.Text,
                txbDvdSynopsis.Text,
                genre.Id, genre.Libelle,
                lePublic.Id, lePublic.Libelle,
                rayon.Id, rayon.Libelle

            );

            bool operationOk = false;

            try
            {
                if (etatDvd == EtatDvd.Ajout)
                {
                    operationOk = controller.AddDvd(dvd);
                    if (!operationOk)
                        MessageBox.Show("Erreur lors de l'ajout du dvd. Le numéro existe peut-être déjà.", erreur);
                }
                else
                {
                    operationOk = controller.EditDvd(dvd);
                    if (!operationOk)
                        MessageBox.Show("Erreur lors de la modification du dvd.", erreur);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Une erreur inattendue est survenue : " + ex.Message, erreur);
                return;
            }

            if (operationOk)
            {
                lesDvd = controller.GetAllDvd();
                RemplirDvdListeComplete();
                SetEtatDvd(EtatDvd.Consultation);
            }
        }
        /// <summary>
        /// Recherche image sur disque
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDVDSearchImage_Click(object sender, EventArgs e)
        {
            string filePath = "";
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                InitialDirectory = Path.GetPathRoot(Environment.CurrentDirectory),
                Filter = files
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;
            }
            txbDvdImage.Text = filePath;
            try
            {
                pcbDvdImage.Image = Image.FromFile(filePath);
            }
            catch
            {
                pcbDvdImage.Image = null;
            }
        }

        /// <summary>
        /// Sur le clic du bouton Suppression : supprime le dvd sélectionné après confirmation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDVDSuppDocument_Click(object sender, EventArgs e)
        {
            if (dgvDvdListe.SelectedRows.Count > 0)
            {
                Dvd dvd = (Dvd)bdgDvdListe.List[bdgDvdListe.Position];
                if (MessageBox.Show("Voulez-vous vraiment supprimer " + dvd.Titre + " du réalisateur " + dvd.Realisateur + " ?", "Confirmation de suppression", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {

                    controller.DeleteDVD(dvd);
                    lesDvd = controller.GetAllDvd();
                    RemplirDvdListeComplete();
                }
            }
            else
            {
                MessageBox.Show(selection);
            }
        }

        /// <summary>
        /// Filtre sur le genre
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxDvdGenres_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxDvdGenres.SelectedIndex >= 0)
            {
                txbDvdTitreRecherche.Text = "";
                txbDvdNumRecherche.Text = "";
                Genre genre = (Genre)cbxDvdGenres.SelectedItem;
                List<Dvd> Dvd = lesDvd.FindAll(x => x.Genre.Equals(genre.Libelle));
                RemplirDvdListe(Dvd);
                cbxDvdRayons.SelectedIndex = -1;
                cbxDvdPublics.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Filtre sur la catégorie de public
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxDvdPublics_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxDvdPublics.SelectedIndex >= 0)
            {
                txbDvdTitreRecherche.Text = "";
                txbDvdNumRecherche.Text = "";
                Public lePublic = (Public)cbxDvdPublics.SelectedItem;
                List<Dvd> Dvd = lesDvd.FindAll(x => x.Public.Equals(lePublic.Libelle));
                RemplirDvdListe(Dvd);
                cbxDvdRayons.SelectedIndex = -1;
                cbxDvdGenres.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Filtre sur le rayon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxDvdRayons_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxDvdRayons.SelectedIndex >= 0)
            {
                txbDvdTitreRecherche.Text = "";
                txbDvdNumRecherche.Text = "";
                Rayon rayon = (Rayon)cbxDvdRayons.SelectedItem;
                List<Dvd> Dvd = lesDvd.FindAll(x => x.Rayon.Equals(rayon.Libelle));
                RemplirDvdListe(Dvd);
                cbxDvdGenres.SelectedIndex = -1;
                cbxDvdPublics.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Tri sur les colonnes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvDvdListe_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            VideDvdZones();
            string titreColonne = dgvDvdListe.Columns[e.ColumnIndex].HeaderText;
            List<Dvd> sortedList = new List<Dvd>();
            switch (titreColonne)
            {
                case "Id":
                    sortedList = lesDvd.OrderBy(o => o.Id).ToList();
                    break;

                case "Titre":
                    sortedList = lesDvd.OrderBy(o => o.Titre).ToList();
                    break;

                case "Duree":
                    sortedList = lesDvd.OrderBy(o => o.Duree).ToList();
                    break;

                case "Realisateur":
                    sortedList = lesDvd.OrderBy(o => o.Realisateur).ToList();
                    break;

                case "Genre":
                    sortedList = lesDvd.OrderBy(o => o.Genre).ToList();
                    break;

                case "Public":
                    sortedList = lesDvd.OrderBy(o => o.Public).ToList();
                    break;

                case "Rayon":
                    sortedList = lesDvd.OrderBy(o => o.Rayon).ToList();
                    break;
            }
            RemplirDvdListe(sortedList);
        }

        /// <summary>
        /// Sur la sélection d'une ligne ou cellule dans le grid
        /// affichage des informations du dvd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvDvdListe_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvDvdListe.CurrentCell != null)
            {
                try
                {
                    Dvd dvd = (Dvd)bdgDvdListe.List[bdgDvdListe.Position];
                    AfficheDvdInfos(dvd);
                    AfficheExemplairesDvd(dvd);

                }
                catch
                {
                    VideDvdZones();
                }
            }
            else
            {
                VideDvdInfos();
            }
        }

        /// <summary>
        /// Remplit le dategrid avec la liste reçue en paramètre
        /// </summary>
        /// <param name="Dvds">liste de dvd</param>
        private void RemplirDvdListe(List<Dvd> Dvds)
        {
            bdgDvdListe.DataSource = Dvds;
            dgvDvdListe.DataSource = bdgDvdListe;
            dgvDvdListe.Columns["idRayon"].Visible = false;
            dgvDvdListe.Columns["idGenre"].Visible = false;
            dgvDvdListe.Columns["idPublic"].Visible = false;
            dgvDvdListe.Columns["image"].Visible = false;
            dgvDvdListe.Columns["synopsis"].Visible = false;
            dgvDvdListe.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvDvdListe.Columns["id"].DisplayIndex = 0;
            dgvDvdListe.Columns["titre"].DisplayIndex = 1;
        }

        /// <summary>
        /// Affichage de la liste complète des Dvd
        /// et annulation de toutes les recherches et filtres
        /// </summary>
        private void RemplirDvdListeComplete()
        {
            RemplirDvdListe(lesDvd);
            VideDvdZones();
        }

        /// <summary>
        /// Ouverture de l'onglet Dvds :
        /// appel des méthodes pour remplir le datagrid des dvd et des combos (genre, rayon, public)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabDvd_Enter(object sender, EventArgs e)
        {
            lesDvd = controller.GetAllDvd();
            RemplirComboCategorie(controller.GetAllGenres(), bdgGenres, cbxDvdGenres);
            RemplirComboCategorie(controller.GetAllPublics(), bdgPublics, cbxDvdPublics);
            RemplirComboCategorie(controller.GetAllRayons(), bdgRayons, cbxDvdRayons);
            bdgEtats.DataSource = controller.GetAllEtats();
            cbxDvdExemplaireEtat.DataSource = bdgEtats;
            if (cbxDvdExemplaireEtat.Items.Count > 0)
            {
                cbxDvdExemplaireEtat.SelectedIndex = -1;
            }
            cbxDvdExemplaireEtat.DisplayMember = "Libelle";
            cbxDvdExemplaireEtat.ValueMember = "Id";
            RemplirDvdListeComplete();
        }
        /// <summary>
        /// Recherche et affichage des Dvd dont le titre matche acec la saisie.
        /// Cette procédure est exécutée à chaque ajout ou suppression de caractère
        /// dans le textBox de saisie.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txbDvdTitreRecherche_TextChanged(object sender, EventArgs e)
        {
            if (!txbDvdTitreRecherche.Text.Equals(""))
            {
                cbxDvdGenres.SelectedIndex = -1;
                cbxDvdRayons.SelectedIndex = -1;
                cbxDvdPublics.SelectedIndex = -1;
                txbDvdNumRecherche.Text = "";
                List<Dvd> lesDvdParTitre;
                lesDvdParTitre = lesDvd.FindAll(x => x.Titre.ToLower().Contains(txbDvdTitreRecherche.Text.ToLower()));
                RemplirDvdListe(lesDvdParTitre);
            }
            else
            {
                if (cbxDvdGenres.SelectedIndex < 0 && cbxDvdPublics.SelectedIndex < 0 && cbxDvdRayons.SelectedIndex < 0
                    && txbDvdNumRecherche.Text.Equals(""))
                {
                    RemplirDvdListeComplete();
                }
            }
        }
        /// <summary>
        /// Vide les zones d'affichage des informations du dvd
        /// </summary>
        private void VideDvdInfos()
        {
            txbDvdRealisateur.Text = "";
            txbDvdSynopsis.Text = "";
            txbDvdImage.Text = "";
            txbDvdDuree.Text = "";
            txbDvdNumero.Text = "";
            txbDvdGenre.Text = "";
            txbDvdPublic.Text = "";
            txbDvdRayon.Text = "";
            txbDvdTitre.Text = "";
            pcbDvdImage.Image = null;
        }
        /// <summary>
        /// vide les zones de recherche et de filtre
        /// </summary>
        private void VideDvdZones()
        {
            cbxDvdGenres.SelectedIndex = -1;
            cbxDvdRayons.SelectedIndex = -1;
            cbxDvdPublics.SelectedIndex = -1;
            txbDvdNumRecherche.Text = "";
            txbDvdTitreRecherche.Text = "";
        }
        #endregion Onglet Dvd

        #region Onglet Revues

        /// <summary>
        /// Source de données pour la liste des revues
        /// </summary>
        private readonly BindingSource bdgRevuesListe = new BindingSource();

        /// <summary>
        /// liste des revues récupérées depuis la bdd
        /// </summary>
        private List<Revue> lesRevues = new List<Revue>();

        /// <summary>
        /// Énumération des états possibles pour gérer les revues (Consultation par défaut)
        /// </summary>
        enum EtatRevue
        {
            Consultation,
            Ajout,
            Modification
        }
        private EtatRevue etatRevue = EtatRevue.Consultation;

        /// <summary>
        /// Modifie l'état de l'interface en fonction du mode (Consultation, Ajout, Modification)
        /// </summary>
        /// <param name="etat">Le nouvel état déclenché</param>
        private void SetEtatRevue(EtatRevue etat)
        {
            etatRevue = etat;
            bool edition = (etat != EtatRevue.Consultation);

            cbxRevuesGenreAddEdit.Visible = edition;
            cbxRevuesPublicAddEdit.Visible = edition;
            cbxRevuesRayonAddEdit.Visible = edition;
            btnRevueSearchImage.Visible = edition;
            btnRevuesSaveDocument.Visible = edition;
            btnRevuesAnnulerDocument.Visible = edition;

            btnRevuesSuppDocument.Visible = !edition;
            btnRevuesAjoutDocument.Visible = !edition;
            btnRevuesModifDocument.Visible = !edition;
            dgvRevuesListe.Enabled = !edition;

            txbRevuesPeriodicite.ReadOnly = !edition;
            txbRevuesDateMiseADispo.ReadOnly = !edition;
            txbRevuesImage.ReadOnly = !edition;
            txbRevuesTitre.ReadOnly = !edition;

            txbRevuesGenre.Visible = !edition;
            txbRevuesPublic.Visible = !edition;
            txbRevuesRayon.Visible = !edition;

            grpRevuesInfos.Text =
                etat == EtatRevue.Ajout ? "Ajouter une revue" :
                etat == EtatRevue.Modification ? "Modifier la revue" :
                "Informations détaillées";
        }

        /// <summary>
        /// Affichage des informations de la revue sélectionné
        /// </summary>
        /// <param name="revue">la revue</param>
        private void AfficheRevuesInfos(Revue revue)
        {
            txbRevuesPeriodicite.Text = revue.Periodicite;
            txbRevuesImage.Text = revue.Image;
            txbRevuesDateMiseADispo.Text = revue.DelaiMiseADispo.ToString();
            txbRevuesNumero.Text = revue.Id;
            txbRevuesGenre.Text = revue.Genre;
            txbRevuesPublic.Text = revue.Public;
            txbRevuesRayon.Text = revue.Rayon;
            txbRevuesTitre.Text = revue.Titre;
            string image = revue.Image;
            try
            {
                pcbRevuesImage.Image = Image.FromFile(image);
            }
            catch
            {
                pcbRevuesImage.Image = null;
            }
        }

        /// <summary>
        /// Sur le clic du bouton d'ajout, affichage interface pour la saisie d'une nouvelle revue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRevuesAjoutDocument_Click(object sender, EventArgs e)
        {
            cbxRevuesGenreAddEdit.DataSource = controller.GetAllGenres();
            cbxRevuesPublicAddEdit.DataSource = controller.GetAllPublics();
            cbxRevuesRayonAddEdit.DataSource = controller.GetAllRayons();

            SetEtatRevue(EtatRevue.Ajout);
            VideRevuesInfos();

            txbRevuesNumero.Text = controller.GenerateNewRevuesId();

        }

        /// <summary>
        /// Sur le clic du le bouton annuler, interface en mode consultation après confirmation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRevuesAnnulerDocument_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Voulez-vous vraiment annuler ?", confirmation, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                SetEtatRevue(EtatRevue.Consultation);
            }
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des revues
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRevuesAnnulGenres_Click(object sender, EventArgs e)
        {
            RemplirRevuesListeComplete();
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des revues
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRevuesAnnulPublics_Click(object sender, EventArgs e)
        {
            RemplirRevuesListeComplete();
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des revues
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRevuesAnnulRayons_Click(object sender, EventArgs e)
        {
            RemplirRevuesListeComplete();
        }

        /// <summary>
        /// Recherche image sur disque
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRevueSearchImage_Click(object sender, EventArgs e)
        {
            string filePath = "";
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                InitialDirectory = Path.GetPathRoot(Environment.CurrentDirectory),
                Filter = files
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;
            }
            txbRevuesImage.Text = filePath;
            try
            {
                pcbRevuesImage.Image = Image.FromFile(filePath);
            }
            catch
            {
                pcbRevuesImage.Image = null;
            }
        }

        /// <summary>
        /// Sur le clic du bouton de modification : prépare l'interface avec les données de la revue sélectionnée
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRevuesModifDocument_Click(object sender, EventArgs e)
        {
            if (dgvRevuesListe.SelectedRows.Count > 0)
            {
                Revue revue = (Revue)bdgRevuesListe.List[bdgRevuesListe.Position];

                cbxRevuesGenreAddEdit.DataSource = controller.GetAllGenres();
                cbxRevuesPublicAddEdit.DataSource = controller.GetAllPublics();
                cbxRevuesRayonAddEdit.DataSource = controller.GetAllRayons();

                cbxRevuesGenreAddEdit.SelectedIndex = cbxRevuesGenreAddEdit.FindStringExact(revue.Genre);
                cbxRevuesPublicAddEdit.SelectedIndex = cbxRevuesPublicAddEdit.FindStringExact(revue.Public);
                cbxRevuesRayonAddEdit.SelectedIndex = cbxRevuesRayonAddEdit.FindStringExact(revue.Rayon);

                SetEtatRevue(EtatRevue.Modification);

                txbRevuesPeriodicite.Text = revue.Periodicite;
                txbRevuesDateMiseADispo.Text = revue.DelaiMiseADispo.ToString();
                txbRevuesImage.Text = revue.Image;
                txbRevuesNumero.Text = revue.Id;
                txbRevuesTitre.Text = revue.Titre;
            }
            else
            {
                MessageBox.Show(selection);
            }
        }

        /// <summary>
        /// Recherche et affichage de la revue dont on a saisi le numéro.
        /// Si non trouvé, affichage d'un MessageBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRevuesNumRecherche_Click(object sender, EventArgs e)
        {
            if (!txbRevuesNumRecherche.Text.Equals(""))
            {
                txbRevuesTitreRecherche.Text = "";
                cbxRevuesGenres.SelectedIndex = -1;
                cbxRevuesRayons.SelectedIndex = -1;
                cbxRevuesPublics.SelectedIndex = -1;
                Revue revue = lesRevues.Find(x => x.Id.Equals(txbRevuesNumRecherche.Text));
                if (revue != null)
                {
                    List<Revue> revues = new List<Revue>() { revue };
                    RemplirRevuesListe(revues);
                }
                else
                {
                    MessageBox.Show(numeroErreur);
                    RemplirRevuesListeComplete();
                }
            }
            else
            {
                RemplirRevuesListeComplete();
            }
        }

        /// <summary>
        /// Sur le clic du bouton Enregistrer : enregistre les modifications (Ajout ou Modification) en bdd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRevuesSaveDocument_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txbRevuesTitre.Text) ||
                cbxRevuesGenreAddEdit.SelectedItem == null)
            {
                MessageBox.Show("Tous les champs doivent être remplis.", info);
                return;
            }

            Genre genre = (Genre)cbxRevuesGenreAddEdit.SelectedItem;
            Public lePublic = (Public)cbxRevuesPublicAddEdit.SelectedItem;
            Rayon rayon = (Rayon)cbxRevuesRayonAddEdit.SelectedItem;

            if (!int.TryParse(txbRevuesDateMiseADispo.Text, out int delai))
            {
                MessageBox.Show("Le délai de mise à disposition doit être un nombre entier.", erreur);
                return;
            }

            Revue laRevue = new Revue(
                txbRevuesNumero.Text,
                txbRevuesTitre.Text,
                txbRevuesImage.Text,
                genre.Id, genre.Libelle,
                lePublic.Id, lePublic.Libelle,
                rayon.Id, rayon.Libelle,
                txbRevuesPeriodicite.Text,
                delai
            );

            bool operationOk = false;
            try
            {
                if (etatRevue == EtatRevue.Ajout)
                {
                    operationOk = controller.AddRevue(laRevue);
                }
                else
                {
                    operationOk = controller.EditRevue(laRevue);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
                return;
            }

            if (operationOk)
            {
                lesRevues = controller.GetAllRevues();
                RemplirRevuesListeComplete();
                SetEtatRevue(EtatRevue.Consultation);
            }
        }

        /// <summary>
        /// Sur le clic du bouton Suppression : supprime la revue sélectionnée après confirmation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRevuesSuppDocument_Click(object sender, EventArgs e)
        {
            if (dgvRevuesListe.SelectedRows.Count > 0)
            {
                Revue revue = (Revue)bdgRevuesListe.List[bdgRevuesListe.Position];
                if (MessageBox.Show("Voulez-vous vraiment supprimer " + revue.Titre + " ?", "Confirmation de suppression", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {

                    controller.DeleteRevue(revue);
                    lesRevues = controller.GetAllRevues();
                    RemplirRevuesListeComplete();
                }
            }
            else
            {
                MessageBox.Show(selection);
            }
        }

        /// <summary>
        /// Filtre sur le genre
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxRevuesGenres_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxRevuesGenres.SelectedIndex >= 0)
            {
                txbRevuesTitreRecherche.Text = "";
                txbRevuesNumRecherche.Text = "";
                Genre genre = (Genre)cbxRevuesGenres.SelectedItem;
                List<Revue> revues = lesRevues.FindAll(x => x.Genre.Equals(genre.Libelle));
                RemplirRevuesListe(revues);
                cbxRevuesRayons.SelectedIndex = -1;
                cbxRevuesPublics.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Filtre sur la catégorie de public
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxRevuesPublics_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxRevuesPublics.SelectedIndex >= 0)
            {
                txbRevuesTitreRecherche.Text = "";
                txbRevuesNumRecherche.Text = "";
                Public lePublic = (Public)cbxRevuesPublics.SelectedItem;
                List<Revue> revues = lesRevues.FindAll(x => x.Public.Equals(lePublic.Libelle));
                RemplirRevuesListe(revues);
                cbxRevuesRayons.SelectedIndex = -1;
                cbxRevuesGenres.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Filtre sur le rayon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxRevuesRayons_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxRevuesRayons.SelectedIndex >= 0)
            {
                txbRevuesTitreRecherche.Text = "";
                txbRevuesNumRecherche.Text = "";
                Rayon rayon = (Rayon)cbxRevuesRayons.SelectedItem;
                List<Revue> revues = lesRevues.FindAll(x => x.Rayon.Equals(rayon.Libelle));
                RemplirRevuesListe(revues);
                cbxRevuesGenres.SelectedIndex = -1;
                cbxRevuesPublics.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Tri sur les colonnes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvRevuesListe_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            VideRevuesZones();
            string titreColonne = dgvRevuesListe.Columns[e.ColumnIndex].HeaderText;
            List<Revue> sortedList = new List<Revue>();
            switch (titreColonne)
            {
                case "Id":
                    sortedList = lesRevues.OrderBy(o => o.Id).ToList();
                    break;

                case "Titre":
                    sortedList = lesRevues.OrderBy(o => o.Titre).ToList();
                    break;

                case "Periodicite":
                    sortedList = lesRevues.OrderBy(o => o.Periodicite).ToList();
                    break;

                case "DelaiMiseADispo":
                    sortedList = lesRevues.OrderBy(o => o.DelaiMiseADispo).ToList();
                    break;

                case "Genre":
                    sortedList = lesRevues.OrderBy(o => o.Genre).ToList();
                    break;

                case "Public":
                    sortedList = lesRevues.OrderBy(o => o.Public).ToList();
                    break;

                case "Rayon":
                    sortedList = lesRevues.OrderBy(o => o.Rayon).ToList();
                    break;
            }
            RemplirRevuesListe(sortedList);
        }

        /// <summary>
        /// Sur la sélection d'une ligne ou cellule dans le grid
        /// affichage des informations de la revue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvRevuesListe_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvRevuesListe.CurrentCell != null)
            {
                try
                {
                    Revue revue = (Revue)bdgRevuesListe.List[bdgRevuesListe.Position];
                    AfficheRevuesInfos(revue);
                }
                catch
                {
                    VideRevuesZones();
                }
            }
            else
            {
                VideRevuesInfos();
            }
        }

        /// <summary>
        /// Remplit le dategrid avec la liste reçue en paramètre
        /// </summary>
        /// <param name="revues"></param>
        private void RemplirRevuesListe(List<Revue> revues)
        {
            bdgRevuesListe.DataSource = revues;
            dgvRevuesListe.DataSource = bdgRevuesListe;
            dgvRevuesListe.Columns["idRayon"].Visible = false;
            dgvRevuesListe.Columns["idGenre"].Visible = false;
            dgvRevuesListe.Columns["idPublic"].Visible = false;
            dgvRevuesListe.Columns["image"].Visible = false;
            dgvRevuesListe.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvRevuesListe.Columns["id"].DisplayIndex = 0;
            dgvRevuesListe.Columns["titre"].DisplayIndex = 1;
        }

        /// <summary>
        /// Affichage de la liste complète des revues
        /// et annulation de toutes les recherches et filtres
        /// </summary>
        private void RemplirRevuesListeComplete()
        {
            RemplirRevuesListe(lesRevues);
            VideRevuesZones();
        }

        /// <summary>
        /// Ouverture de l'onglet Revues :
        /// appel des méthodes pour remplir le datagrid des revues et des combos (genre, rayon, public)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabRevues_Enter(object sender, EventArgs e)
        {
            lesRevues = controller.GetAllRevues();
            RemplirComboCategorie(controller.GetAllGenres(), bdgGenres, cbxRevuesGenres);
            RemplirComboCategorie(controller.GetAllPublics(), bdgPublics, cbxRevuesPublics);
            RemplirComboCategorie(controller.GetAllRayons(), bdgRayons, cbxRevuesRayons);
            RemplirRevuesListeComplete();
        }
        /// <summary>
        /// Recherche et affichage des revues dont le titre matche acec la saisie.
        /// Cette procédure est exécutée à chaque ajout ou suppression de caractère
        /// dans le textBox de saisie.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txbRevuesTitreRecherche_TextChanged(object sender, EventArgs e)
        {
            if (!txbRevuesTitreRecherche.Text.Equals(""))
            {
                cbxRevuesGenres.SelectedIndex = -1;
                cbxRevuesRayons.SelectedIndex = -1;
                cbxRevuesPublics.SelectedIndex = -1;
                txbRevuesNumRecherche.Text = "";
                List<Revue> lesRevuesParTitre;
                lesRevuesParTitre = lesRevues.FindAll(x => x.Titre.ToLower().Contains(txbRevuesTitreRecherche.Text.ToLower()));
                RemplirRevuesListe(lesRevuesParTitre);
            }
            else
            {
                // si la zone de saisie est vide et aucun élément combo sélectionné, réaffichage de la liste complète
                if (cbxRevuesGenres.SelectedIndex < 0 && cbxRevuesPublics.SelectedIndex < 0 && cbxRevuesRayons.SelectedIndex < 0
                    && txbRevuesNumRecherche.Text.Equals(""))
                {
                    RemplirRevuesListeComplete();
                }
            }
        }
        /// <summary>
        /// Vide les zones d'affichage des informations de la reuve
        /// </summary>
        private void VideRevuesInfos()
        {
            txbRevuesPeriodicite.Text = "";
            txbRevuesImage.Text = "";
            txbRevuesDateMiseADispo.Text = "";
            txbRevuesNumero.Text = "";
            txbRevuesGenre.Text = "";
            txbRevuesPublic.Text = "";
            txbRevuesRayon.Text = "";
            txbRevuesTitre.Text = "";
            pcbRevuesImage.Image = null;
        }
        /// <summary>
        /// vide les zones de recherche et de filtre
        /// </summary>
        private void VideRevuesZones()
        {
            cbxRevuesGenres.SelectedIndex = -1;
            cbxRevuesRayons.SelectedIndex = -1;
            cbxRevuesPublics.SelectedIndex = -1;
            txbRevuesNumRecherche.Text = "";
            txbRevuesTitreRecherche.Text = "";
        }
        #endregion Onglet Revues

        #region Onglet Parutions

        private const string ETATNEUF = "00001";

        /// <summary>
        /// Source de données pour la liste de parutions d'une revue
        /// </summary>
        private readonly BindingSource bdgExemplairesRevuesListe = new BindingSource();

        /// <summary>
        /// Source de données pour la liste des exemplaires d'un livre
        /// </summary>
        private readonly BindingSource bdgExemplairesLivresListe = new BindingSource();

        /// <summary>
        /// Source de données pour la liste des exemplaires d'un dvd
        /// </summary>
        private readonly BindingSource bdgExemplairesDvdListe = new BindingSource();

        /// <summary>
        /// liste des exemplaires d'une revue récupérés depuis la bdd
        /// </summary>
        private List<Exemplaire> lesExemplairesRevues = new List<Exemplaire>();

        /// <summary>
        /// liste des exemplaires d'un livre récupérés depuis la bdd
        /// </summary>
        private List<Exemplaire> lesExemplairesLivres = new List<Exemplaire>();

        /// <summary>
        /// liste des exemplaires d'un dvd récupérés depuis la bdd
        /// </summary>
        private List<Exemplaire> lesExemplairesDvd = new List<Exemplaire>();

        /// <summary>
        /// Remplit le datagrid avec la liste reçue en paramètre
        /// </summary>
        /// <param name="dgv">le datagrid concerné</param>
        /// <param name="bdg">la source de données concernée</param>        
        /// <param name="exemplaires">liste d'exemplaires</param>
        private void RemplirListeExemplaires(DataGridView dgv, BindingSource bdg, List<Exemplaire> exemplaires)
        {
            bdg.DataSource = exemplaires;
            dgv.DataSource = bdg;

            if (exemplaires != null)
            {
                string[] colonnesAMasquer = { "idEtat", "id", "photo" };
                foreach (string col in colonnesAMasquer)
                    if (dgv.Columns.Contains(col)) dgv.Columns[col].Visible = false;

                dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgv.Columns["dateAchat"].HeaderCell.SortGlyphDirection = SortOrder.Descending;
            }
        }

        /// <summary>
        /// Sur le clic du bouton Enregistrer : enregistre les modifications (Ajout ou Modification) en bdd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SauvegarderEtatExemplaire(BindingSource bdg, ComboBox cbx)
        {
            Exemplaire exemplaire = (Exemplaire)bdg.Current;
            Etat nouvelEtat = (Etat)cbx.SelectedItem;

            if (exemplaire != null && nouvelEtat != null && controller.EditExemplaireEtat(exemplaire.Id, exemplaire.Numero, nouvelEtat.Id))
            {
                    MessageBox.Show("État mis à jour.", info);
            }
        }

        #region revues exemplaires
        /// <summary>
        /// Permet ou interdit l'accès à la gestion de la réception d'un exemplaire
        /// et vide les objets graphiques
        /// </summary>
        /// <param name="acces">true ou false</param>
        private void AccesReceptionExemplaireGroupBox(bool acces)
        {
            grpReceptionExemplaire.Enabled = acces;
            txbReceptionExemplaireImage.Text = "";
            txbReceptionExemplaireNumero.Text = "";
            pcbReceptionExemplaireImage.Image = null;
            dtpReceptionExemplaireDate.Value = DateTime.Now;
        }

        /// <summary>
        /// Récupère et affiche les exemplaires d'une revue
        /// </summary>
        private void AfficheReceptionExemplairesRevue()
        {
            string idDocument = txbReceptionRevueNumero.Text;
            lesExemplairesRevues = controller.GetExemplaires(idDocument);
            RemplirListeExemplaires(dgvReceptionExemplairesListe, bdgExemplairesRevuesListe, lesExemplairesRevues);
            AccesReceptionExemplaireGroupBox(true);
        }

        /// <summary>
        /// Affichage des informations de la revue sélectionnée et les exemplaires
        /// </summary>
        /// <param name="revue">la revue</param>
        private void AfficheReceptionRevueInfos(Revue revue)
        {
            // informations sur la revue
            txbReceptionRevuePeriodicite.Text = revue.Periodicite;
            txbReceptionRevueImage.Text = revue.Image;
            txbReceptionRevueDelaiMiseADispo.Text = revue.DelaiMiseADispo.ToString();
            txbReceptionRevueNumero.Text = revue.Id;
            txbReceptionRevueGenre.Text = revue.Genre;
            txbReceptionRevuePublic.Text = revue.Public;
            txbReceptionRevueRayon.Text = revue.Rayon;
            txbReceptionRevueTitre.Text = revue.Titre;
            string image = revue.Image;
            try
            {
                pcbReceptionRevueImage.Image = Image.FromFile(image);
            }
            catch
            {
                pcbReceptionRevueImage.Image = null;
            }
            // affiche la liste des exemplaires de la revue
            AfficheReceptionExemplairesRevue();
        }

        /// <summary>
        /// Recherche image sur disque (pour l'exemplaire à insérer)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReceptionExemplaireImage_Click(object sender, EventArgs e)
        {
            string filePath = "";
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                // positionnement à la racine du disque où se trouve le dossier actuel
                InitialDirectory = Path.GetPathRoot(Environment.CurrentDirectory),
                Filter = files
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;
            }
            txbReceptionExemplaireImage.Text = filePath;
            try
            {
                pcbReceptionExemplaireImage.Image = Image.FromFile(filePath);
            }
            catch
            {
                pcbReceptionExemplaireImage.Image = null;
            }
        }

        /// <summary>
        /// Enregistrement du nouvel exemplaire
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReceptionExemplaireValider_Click(object sender, EventArgs e)
        {
            if (!txbReceptionExemplaireNumero.Text.Equals(""))
            {
                try
                {
                    int numero = int.Parse(txbReceptionExemplaireNumero.Text);
                    DateTime dateAchat = dtpReceptionExemplaireDate.Value;
                    string photo = txbReceptionExemplaireImage.Text;
                    string idEtat = ETATNEUF;
                    string libelleEtat = "neuf";
                    string idDocument = txbReceptionRevueNumero.Text;
                    Exemplaire exemplaire = new Exemplaire(numero, dateAchat, photo, idEtat, idDocument, libelleEtat); 
                    if (controller.CreerExemplaire(exemplaire))
                    {
                        AfficheReceptionExemplairesRevue();
                    }
                    else
                    {
                        MessageBox.Show("numéro de publication déjà existant", erreur);
                    }
                }
                catch
                {
                    MessageBox.Show("le numéro de parution doit être numérique", info);
                    txbReceptionExemplaireNumero.Text = "";
                    txbReceptionExemplaireNumero.Focus();
                }
            }
            else
            {
                MessageBox.Show("numéro de parution obligatoire", info);
            }
        }

        /// <summary>
        /// Recherche d'un numéro de revue et affiche ses informations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReceptionRechercher_Click(object sender, EventArgs e)
        {
            if (!txbReceptionRevueNumero.Text.Equals(""))
            {
                Revue revue = lesRevues.Find(x => x.Id.Equals(txbReceptionRevueNumero.Text));
                if (revue != null)
                {
                    AfficheReceptionRevueInfos(revue);
                }
                else
                {
                    MessageBox.Show(numeroErreur);
                }
            }
        }

        /// <summary>
        /// Tri sur une colonne
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvExemplairesListe_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string titreColonne = dgvReceptionExemplairesListe.Columns[e.ColumnIndex].HeaderText;
            List<Exemplaire> sortedList = new List<Exemplaire>();
            switch (titreColonne)
            {
                case "Numero":
                    sortedList = lesExemplairesRevues.OrderBy(o => o.Numero).Reverse().ToList();
                    break;

                case "DateAchat":
                    sortedList = lesExemplairesRevues.OrderBy(o => o.DateAchat).Reverse().ToList();
                    break;
            }
            RemplirListeExemplaires(dgvReceptionExemplairesListe, bdgExemplairesRevuesListe, sortedList);
        }

        /// <summary>
        /// affichage de l'image de l'exemplaire suite à la sélection d'un exemplaire dans la liste
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvReceptionExemplairesListe_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvReceptionExemplairesListe.CurrentCell != null)
            {
                bdgEtats.DataSource = controller.GetAllEtats();
                cbxRevuesExemplaireEtat.DataSource = bdgEtats;
                if (cbxRevuesExemplaireEtat.Items.Count > 0)
                {
                    cbxRevuesExemplaireEtat.SelectedIndex = -1;
                }
                cbxRevuesExemplaireEtat.DisplayMember = "Libelle";
                cbxRevuesExemplaireEtat.ValueMember = "Id";

                Exemplaire exemplaire = (Exemplaire)bdgExemplairesRevuesListe.List[bdgExemplairesRevuesListe.Position];
                string image = exemplaire.Photo;
                try
                {
                    pcbReceptionExemplaireRevueImage.Image = Image.FromFile(image);
                }
                catch
                {
                    pcbReceptionExemplaireRevueImage.Image = null;
                }
            }
            else
            {
                pcbReceptionExemplaireRevueImage.Image = null;
            }
        }

        /// <summary>
        /// Sur le clic du bouton Enregistrer : enregistre les modifications (Ajout ou Modification) en bdd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRevuesExemplairesSaveEtat_Click(object sender, EventArgs e)
        {
             SauvegarderEtatExemplaire(bdgExemplairesRevuesListe, cbxRevuesExemplaireEtat);
        }

        /// <summary>
        /// Sur le clic du bouton Suppression : supprime la parution de la revue sélectionnée après confirmation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRevuesExemplairesSupp_Click(object sender, EventArgs e)
        {
            if (dgvReceptionExemplairesListe.CurrentRow != null)
            {
                Exemplaire exemplaire = (Exemplaire)bdgExemplairesRevuesListe.Current;
                if (MessageBox.Show("Supprimer l'exemplaire n°" + exemplaire.Numero + " ?", confirmation, MessageBoxButtons.YesNo) == DialogResult.Yes && controller.DeleteExemplaire(exemplaire))
                {
                        AfficheReceptionExemplairesRevue();
                }
            }
        }
        /// <summary>
        /// Ouverture de l'onglet : récupère le revues et vide tous les champs.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabReceptionRevue_Enter(object sender, EventArgs e)
        {
            lesRevues = controller.GetAllRevues();
            txbReceptionRevueNumero.Text = "";
        }
        /// <summary>
        /// Si le numéro de revue est modifié, la zone de l'exemplaire est vidée et inactive
        /// les informations de la revue son aussi effacées
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txbReceptionRevueNumero_TextChanged(object sender, EventArgs e)
        {
            txbReceptionRevuePeriodicite.Text = "";
            txbReceptionRevueImage.Text = "";
            txbReceptionRevueDelaiMiseADispo.Text = "";
            txbReceptionRevueGenre.Text = "";
            txbReceptionRevuePublic.Text = "";
            txbReceptionRevueRayon.Text = "";
            txbReceptionRevueTitre.Text = "";
            pcbReceptionRevueImage.Image = null;
            RemplirListeExemplaires(null, null, null);
            AccesReceptionExemplaireGroupBox(false);
        }
        #endregion

        #region livres exemplaires

        /// <summary>
        /// Sur le clic du bouton Enregistrer : enregistre les modifications (Ajout ou Modification) en bdd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLivresExemplairesSaveEtat_Click(object sender, EventArgs e)
        {
            SauvegarderEtatExemplaire(bdgExemplairesLivresListe, cbxLivresExemplaireEtat);

        }

        /// <summary>
        /// Sur le clic du bouton Suppression : supprime l'exemplaire du livre sélectionné après confirmation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLivresExemplairesSupp_Click(object sender, EventArgs e)
        {
            if (dgvExemplaireLivresListe.CurrentRow != null)
            {
                Exemplaire exemplaire = (Exemplaire)bdgExemplairesLivresListe.Current;
                if (MessageBox.Show("Supprimer l'exemplaire n°" + exemplaire.Numero + " ?", confirmation, MessageBoxButtons.YesNo) == DialogResult.Yes && controller.DeleteExemplaire(exemplaire))
                {
                        AfficheExemplairesLivres((Livre)bdgLivresListe.Current);
                }
            }
        }

        /// <summary>
        /// Tri sur les colonnes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvExemplaireLivresListe_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string titreColonne = dgvExemplaireLivresListe.Columns[e.ColumnIndex].HeaderText;
            List<Exemplaire> sortedList = new List<Exemplaire>();
            switch (titreColonne)
            {
                case "Numero":
                    sortedList = lesExemplairesLivres.OrderBy(o => o.Numero).Reverse().ToList();
                    break;

                case "DateAchat":
                    sortedList = lesExemplairesLivres.OrderBy(o => o.DateAchat).Reverse().ToList();
                    break;
            }

            RemplirListeExemplaires(dgvExemplaireLivresListe, bdgExemplairesLivresListe, sortedList);
        }

        /// <summary>
        /// Affichage des informations des exemplaires du livre sélectionné
        /// </summary>
        /// <param name="livre">le livre</param>
        private void AfficheExemplairesLivres(Livre livre)
        {
            string idDocument = livre.Id;
            lesExemplairesLivres = controller.GetExemplaires(idDocument);
            RemplirListeExemplaires(dgvExemplaireLivresListe, bdgExemplairesLivresListe, lesExemplairesLivres);
        }

        #endregion

        #region dvd exemplaires

        /// <summary>
        /// Sur le clic du bouton Enregistrer : enregistre les modifications (Ajout ou Modification) en bdd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDvdExemplairesSaveEtat_Click(object sender, EventArgs e)
        {
            SauvegarderEtatExemplaire(bdgExemplairesDvdListe, cbxDvdExemplaireEtat);

        }

        /// <summary>
        /// Sur le clic du bouton Suppression : supprime l'exemplaire du dvd sélectionné après confirmation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDvdExemplairesSupp_Click(object sender, EventArgs e)
        {
            if (dgvExemplaireDvdListe.CurrentRow != null)
            {
                Exemplaire exemplaire = (Exemplaire)bdgExemplairesDvdListe.Current;
                if (MessageBox.Show("Supprimer l'exemplaire n°" + exemplaire.Numero + " ?", confirmation, MessageBoxButtons.YesNo) == DialogResult.Yes && controller.DeleteExemplaire(exemplaire))
                {
                        AfficheExemplairesDvd((Dvd)bdgDvdListe.Current);
                }
            }

        }

        /// <summary>
        /// Tri sur les colonnes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvExemplaireDvdListe_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string titreColonne = dgvExemplaireDvdListe.Columns[e.ColumnIndex].HeaderText;
            List<Exemplaire> sortedList = new List<Exemplaire>();
            switch (titreColonne)
            {
                case "Numero":
                    sortedList = lesExemplairesDvd.OrderBy(o => o.Numero).Reverse().ToList();
                    break;

                case "DateAchat":
                    sortedList = lesExemplairesDvd.OrderBy(o => o.DateAchat).Reverse().ToList();
                    break;
            }

            RemplirListeExemplaires(dgvExemplaireDvdListe, bdgExemplairesDvdListe, sortedList);
        }

        /// <summary>
        /// Affichage des informations des exemplaires du dvd sélectionné
        /// </summary>
        /// <param name="dvd">le dvd</param>
        private void AfficheExemplairesDvd(Dvd dvd)
        {
            string idDocument = dvd.Id;
            lesExemplairesDvd = controller.GetExemplaires(idDocument);
            RemplirListeExemplaires(dgvExemplaireDvdListe, bdgExemplairesDvdListe, lesExemplairesDvd);
        }

        #endregion
        #endregion Onglet Parutions


        /// <summary>
        /// Sur le clic du bouton Commander un exemplaire: ouvre la fenêtre de gestion des commandes sur l'onglet Livres avec le livre sélectionné
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCommandesLivres_Click(object sender, EventArgs e)
        {
            string idLivre = (bdgLivresListe.Current != null) ? ((Livre)bdgLivresListe.Current).Id : null;

            FrmMediatekCommande frm = new FrmMediatekCommande(idLivre, true);
            frm.ShowDialog();
        }

        /// <summary>
        /// Sur le clic du bouton Commander un exemplaire: ouvre la fenêtre de gestion des commandes sur l'onglet Dvd avec le dvd sélectionné
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCommandesDvd_Click(object sender, EventArgs e)
        {
            string idDvd = (bdgDvdListe.Current != null) ? ((Dvd)bdgDvdListe.Current).Id : null;

            FrmMediatekCommande frm = new FrmMediatekCommande(idDvd, false);
            frm.ShowDialog();
        }

        /// <summary>
        /// Sur le clic du bouton Commander un abonnement: ouvre la fenêtre de gestion des commandes sur l'onglet Revues avec la revue sélectionnée
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCommandesRevues_Click(object sender, EventArgs e)
        {
            string idRevue = (bdgRevuesListe.Current != null) ? ((Revue)bdgRevuesListe.Current).Id : null;
            FrmMediatekCommande frm = new FrmMediatekCommande(idRevue, false, true);
            frm.ShowDialog();
        }


    }
}