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

namespace MediaTekDocuments.view

{
    /// <summary>
    /// Classe d'affichage
    /// </summary>
    public partial class FrmMediatek : Form
    {
        #region Commun

        private readonly BindingSource bdgGenres = new BindingSource();
        private readonly BindingSource bdgPublics = new BindingSource();
        private readonly BindingSource bdgRayons = new BindingSource();
        private readonly BindingSource bdgEtats = new BindingSource();

        private readonly FrmMediatekController controller;


        /// <summary>
        /// Constructeur : création du contrôleur lié à ce formulaire
        /// </summary>
        internal FrmMediatek()
        {
            InitializeComponent();
            this.controller = new FrmMediatekController();
        }

        /// <summary>
        /// Rempli un des 3 combo (genre, public, rayon)
        /// </summary>
        /// <param name="lesCategories">liste des objets de type Genre ou Public ou Rayon</param>
        /// <param name="bdg">bindingsource contenant les informations</param>
        /// <param name="cbx">combobox à remplir</param>
        public void RemplirComboCategorie(List<Categorie> lesCategories, BindingSource bdg, ComboBox cbx)
        {
            bdg.DataSource = lesCategories;
            cbx.DataSource = bdg;
            if (cbx.Items.Count > 0)
            {
                cbx.SelectedIndex = -1;
            }
        }

        public void RemplirComboEditCategorie(List<Categorie> lesCategories, BindingSource bdg, ComboBox cbx)
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

        private readonly BindingSource bdgLivresListe = new BindingSource();
        private List<Livre> lesLivres = new List<Livre>();        
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
            try
            {
                pcbLivresImage.Image = Image.FromFile(image);
            }
            catch
            {
                pcbLivresImage.Image = null;
            }
        }

        private void btnLivresAjoutDocument_Click(object sender, EventArgs e)
        {
            cbxLivresGenreAddEdit.DataSource = controller.GetAllGenres();
            cbxLivresPublicAddEdit.DataSource = controller.GetAllPublics();
            cbxLivresRayonAddEdit.DataSource = controller.GetAllRayons();

            SetEtatLivre(EtatLivre.Ajout);
            VideLivresInfos();

            txbLivresNumero.Text = controller.GenerateNewLivresId();
        }

        private void btnLivresAnnulerDocument_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Voulez-vous vraiment annuler ?", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
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

        private void btnLivreSearchImage_Click(object sender, EventArgs e)
        {
            string filePath = "";
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                InitialDirectory = Path.GetPathRoot(Environment.CurrentDirectory),
                Filter = "Files|*.jpg;*.bmp;*.jpeg;*.png;*.gif"
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

        private void btnLivresModifDocument_Click(object sender, EventArgs e)
        {
            if (dgvLivresListe.SelectedRows.Count > 0)
            {
                Livre livre = (Livre)bdgLivresListe.List[bdgLivresListe.Position];
                cbxLivresGenreAddEdit.DataSource = controller.GetAllGenres();
                cbxLivresPublicAddEdit.DataSource = controller.GetAllPublics();
                cbxLivresRayonAddEdit.DataSource = controller.GetAllRayons();

                cbxLivresGenreAddEdit.SelectedIndex = cbxLivresGenreAddEdit.FindStringExact(livre.Genre);
                cbxLivresPublicAddEdit.SelectedIndex = cbxLivresPublicAddEdit.FindStringExact(livre.Public);
                cbxLivresRayonAddEdit.SelectedIndex = cbxLivresRayonAddEdit.FindStringExact(livre.Rayon);


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
                MessageBox.Show("Une ligne doit être sélectionnée.");
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
                    MessageBox.Show("numéro introuvable");
                    RemplirLivresListeComplete();
                }
            }
            else
            {
                RemplirLivresListeComplete();
            }
        }

        private void btnLivresSaveDocument_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txbLivresTitre.Text) || cbxLivresGenreAddEdit.SelectedItem == null ||
                cbxLivresPublicAddEdit.SelectedItem == null || cbxLivresRayonAddEdit.SelectedItem == null)
            { MessageBox.Show("Tous les champs doivent être remplis.", "Information"); return;
            }

            Genre genre = (Genre)cbxLivresGenreAddEdit.SelectedItem;
            Public lePublic = (Public)cbxLivresPublicAddEdit.SelectedItem;
            Rayon rayon = (Rayon)cbxLivresRayonAddEdit.SelectedItem;

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

            try
            {
                if (etatLivre == EtatLivre.Ajout)
                {
                    operationOk = controller.AddLivre(livre);
                    if (!operationOk)
                        MessageBox.Show("Erreur lors de l'ajout du livre. Le numéro existe peut-être déjà probablement.", "Erreur");
                }
                else
                {
                    operationOk = controller.EditLivre(livre);
                    if (!operationOk)
                        MessageBox.Show("Erreur lors de la modification du livre.", "Erreur");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Une erreur inattendue est survenue : " + ex.Message, "Erreur");
                return;
            }

            if (operationOk)
            {
                lesLivres = controller.GetAllLivres();
                RemplirLivresListeComplete();
                SetEtatLivre(EtatLivre.Consultation);
            }
        }

        private void btnLivresSuppDocument_Click(object sender, EventArgs e)
        {
            if (dgvLivresListe.SelectedRows.Count > 0)
            {
                Livre livre = (Livre)bdgLivresListe.List[bdgLivresListe.Position];
                if (MessageBox.Show("Voulez-vous vraiment supprimer " + livre.Titre + " de l'auteur " + 
                    livre.Auteur + " ?", "Confirmation de suppression", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    
                    controller.DeleteLivre(livre);
                    lesLivres = controller.GetAllLivres();
                    RemplirLivresListeComplete();
                }
            }
            else
            {
                MessageBox.Show("Une ligne doit être sélectionnée.");
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
        /// Remplit le dategrid avec la liste reçue en paramètre
        /// </summary>
        /// <param name="dvd">liste de dvd</param>
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

        private readonly BindingSource bdgDvdListe = new BindingSource();
        private List<Dvd> lesDvd = new List<Dvd>();
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

        private void btnDVDAjoutDocument_Click(object sender, EventArgs e)
        {
            cbxDvdGenreAddEdit.DataSource = controller.GetAllGenres();
            cbxDvdPublicAddEdit.DataSource = controller.GetAllPublics();
            cbxDvdRayonAddEdit.DataSource = controller.GetAllRayons();

            SetEtatDvd(EtatDvd.Ajout);
            VideDvdInfos();

            txbDvdNumero.Text = controller.GenerateNewDvdId();
        }

        private void btnDVDAnnulerDocument_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Voulez-vous vraiment annuler ?", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
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
                MessageBox.Show("Une ligne doit être sélectionnée.");
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
                    MessageBox.Show("numéro introuvable");
                    RemplirDvdListeComplete();
                }
            }
            else
            {
                RemplirDvdListeComplete();
            }
        }

        private void btnDVDSaveDocument_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txbDvdTitre.Text) ||
               string.IsNullOrWhiteSpace(txbDvdRealisateur.Text) ||
               cbxDvdGenreAddEdit.SelectedItem == null ||
               cbxDvdPublicAddEdit.SelectedItem == null ||
               cbxDvdRayonAddEdit.SelectedItem == null)
            {
                MessageBox.Show("Tous les champs doivent être remplis.", "Information");
                return;
            }

            Genre genre = (Genre)cbxDvdGenreAddEdit.SelectedItem;
            Public lePublic = (Public)cbxDvdPublicAddEdit.SelectedItem;
            Rayon rayon = (Rayon)cbxDvdRayonAddEdit.SelectedItem;

            if (!int.TryParse(txbDvdDuree.Text, out int duree))
            {
                MessageBox.Show("La durée doit être un nombre entier.", "Erreur");
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
                        MessageBox.Show("Erreur lors de l'ajout du dvd. Le numéro existe peut-être déjà.", "Erreur");
                }
                else
                {
                    operationOk = controller.EditDvd(dvd);
                    if (!operationOk)
                        MessageBox.Show("Erreur lors de la modification du dvd.", "Erreur");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Une erreur inattendue est survenue : " + ex.Message, "Erreur");
                return;
            }

            if (operationOk)
            {
                lesDvd = controller.GetAllDvd();
                RemplirDvdListeComplete();
                SetEtatDvd(EtatDvd.Consultation);
            }
        }

        private void btnDVDSearchImage_Click(object sender, EventArgs e)
        {
            string filePath = "";
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                InitialDirectory = Path.GetPathRoot(Environment.CurrentDirectory),
                Filter = "Files|*.jpg;*.bmp;*.jpeg;*.png;*.gif"
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
                MessageBox.Show("Une ligne doit être sélectionnée.");
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

        private readonly BindingSource bdgRevuesListe = new BindingSource();
        private List<Revue> lesRevues = new List<Revue>();
        enum EtatRevue
        {
            Consultation,
            Ajout,
            Modification
        }
        private EtatRevue etatRevue = EtatRevue.Consultation;

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

        private void btnRevuesAjoutDocument_Click(object sender, EventArgs e)
        {
            cbxRevuesGenreAddEdit.DataSource = controller.GetAllGenres();
            cbxRevuesPublicAddEdit.DataSource = controller.GetAllPublics();
            cbxRevuesRayonAddEdit.DataSource = controller.GetAllRayons();

            SetEtatRevue(EtatRevue.Ajout);
            VideRevuesInfos();

            txbRevuesNumero.Text = controller.GenerateNewRevuesId();

        }

        private void btnRevuesAnnulerDocument_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Voulez-vous vraiment annuler ?", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
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

        private void btnRevueSearchImage_Click(object sender, EventArgs e)
        {
            string filePath = "";
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                InitialDirectory = Path.GetPathRoot(Environment.CurrentDirectory),
                Filter = "Files|*.jpg;*.bmp;*.jpeg;*.png;*.gif"
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
                MessageBox.Show("Une ligne doit être sélectionnée.");
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
                    MessageBox.Show("numéro introuvable");
                    RemplirRevuesListeComplete();
                }
            }
            else
            {
                RemplirRevuesListeComplete();
            }
        }
        private void btnRevuesSaveDocument_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txbRevuesTitre.Text) ||
                cbxRevuesGenreAddEdit.SelectedItem == null)
            {
                MessageBox.Show("Tous les champs doivent être remplis.", "Information");
                return;
            }

            Genre genre = (Genre)cbxRevuesGenreAddEdit.SelectedItem;
            Public lePublic = (Public)cbxRevuesPublicAddEdit.SelectedItem;
            Rayon rayon = (Rayon)cbxRevuesRayonAddEdit.SelectedItem;

            if (!int.TryParse(txbRevuesDateMiseADispo.Text, out int delai))
            {
                MessageBox.Show("Le délai de mise à disposition doit être un nombre entier.", "Erreur");
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
                MessageBox.Show("Une ligne doit être sélectionnée.");
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

        private readonly BindingSource bdgExemplairesRevuesListe = new BindingSource();
        private readonly BindingSource bdgExemplairesLivresListe = new BindingSource();
        private readonly BindingSource bdgExemplairesDvdListe = new BindingSource();


        private List<Exemplaire> lesExemplairesRevues = new List<Exemplaire>();
        private List<Exemplaire> lesExemplairesLivres = new List<Exemplaire>();
        private List<Exemplaire> lesExemplairesDvd = new List<Exemplaire>();

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

        private void SauvegarderEtatExemplaire(BindingSource bdg, ComboBox cbx)
        {
            Exemplaire exemplaire = (Exemplaire)bdg.Current;
            Etat nouvelEtat = (Etat)cbx.SelectedItem;

            if (exemplaire != null && nouvelEtat != null)
            {
                if (controller.EditExemplaireEtat(exemplaire.Id, exemplaire.Numero, nouvelEtat.Id))
                {
                    exemplaire.IdEtat = nouvelEtat.Id;
                    exemplaire.LibelleEtat = nouvelEtat.Libelle;
                    MessageBox.Show("État mis à jour.", "Information");
                }
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
                Filter = "Files|*.jpg;*.bmp;*.jpeg;*.png;*.gif"
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
                    Exemplaire exemplaire = new Exemplaire(numero, dateAchat, photo, idEtat, libelleEtat, idDocument); if (controller.CreerExemplaire(exemplaire))
                    {
                        AfficheReceptionExemplairesRevue();
                    }
                    else
                    {
                        MessageBox.Show("numéro de publication déjà existant", "Erreur");
                    }
                }
                catch
                {
                    MessageBox.Show("le numéro de parution doit être numérique", "Information");
                    txbReceptionExemplaireNumero.Text = "";
                    txbReceptionExemplaireNumero.Focus();
                }
            }
            else
            {
                MessageBox.Show("numéro de parution obligatoire", "Information");
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
                    MessageBox.Show("numéro introuvable");
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

        private void btnRevuesExemplairesSaveEtat_Click(object sender, EventArgs e)
        {
             SauvegarderEtatExemplaire(bdgExemplairesRevuesListe, cbxRevuesExemplaireEtat);
        }

        private void btnRevuesExemplairesSupp_Click(object sender, EventArgs e)
        {
            if (dgvReceptionExemplairesListe.CurrentRow != null)
            {
                Exemplaire exemplaire = (Exemplaire)bdgExemplairesRevuesListe.Current;
                if (MessageBox.Show("Supprimer l'exemplaire n°" + exemplaire.Numero + " ?", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (controller.DeleteExemplaire(exemplaire))
                    {
                        AfficheReceptionExemplairesRevue();
                    }
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
        private void btnLivresExemplairesSaveEtat_Click(object sender, EventArgs e)
        {
            SauvegarderEtatExemplaire(bdgExemplairesLivresListe, cbxLivresExemplaireEtat);

        }

        private void btnLivresExemplairesSupp_Click(object sender, EventArgs e)
        {
            if (dgvExemplaireLivresListe.CurrentRow != null)
            {
                Exemplaire exemplaire = (Exemplaire)bdgExemplairesLivresListe.Current;
                if (MessageBox.Show("Supprimer l'exemplaire n°" + exemplaire.Numero + " ?", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (controller.DeleteExemplaire(exemplaire))
                    {
                        AfficheExemplairesLivres((Livre)bdgLivresListe.Current);
                    }
                }
            }
        }

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


        private void AfficheExemplairesLivres(Livre livre)
        {
            string idDocument = livre.Id;
            lesExemplairesLivres = controller.GetExemplaires(idDocument);
            RemplirListeExemplaires(dgvExemplaireLivresListe, bdgExemplairesLivresListe, lesExemplairesLivres);
        }

        #endregion

        #region dvd exemplaires

        private void btnDvdExemplairesSaveEtat_Click(object sender, EventArgs e)
        {
            SauvegarderEtatExemplaire(bdgExemplairesDvdListe, cbxDvdExemplaireEtat);

        }

        private void btnDvdExemplairesSupp_Click(object sender, EventArgs e)
        {
            if (dgvExemplaireDvdListe.CurrentRow != null)
            {
                Exemplaire exemplaire = (Exemplaire)bdgExemplairesDvdListe.Current;
                if (MessageBox.Show("Supprimer l'exemplaire n°" + exemplaire.Numero + " ?", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (controller.DeleteExemplaire(exemplaire))
                    {
                        AfficheExemplairesDvd((Dvd)bdgDvdListe.Current);
                    }
                }
            }

        }        
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

        private void AfficheExemplairesDvd(Dvd dvd)
        {
            string idDocument = dvd.Id;
            lesExemplairesDvd = controller.GetExemplaires(idDocument);
            RemplirListeExemplaires(dgvExemplaireDvdListe, bdgExemplairesDvdListe, lesExemplairesDvd);
        }

        #endregion
        #endregion Onglet Parutions

        private void btnCommandesLivres_Click(object sender, EventArgs e)
        {
            string idLivre = (bdgLivresListe.Current != null) ? ((Livre)bdgLivresListe.Current).Id : null;

            FrmMediatekCommande frm = new FrmMediatekCommande(idLivre, true);
            frm.ShowDialog();
        }

        private void btnCommandesDvd_Click(object sender, EventArgs e)
        {
            string idDvd = (bdgDvdListe.Current != null) ? ((Dvd)bdgDvdListe.Current).Id : null;

            FrmMediatekCommande frm = new FrmMediatekCommande(idDvd, false);
            frm.ShowDialog();
        }

        private void btnCommandesRevues_Click(object sender, EventArgs e)
        {
            string idRevue = (bdgRevuesListe.Current != null) ? ((Revue)bdgRevuesListe.Current).Id : null;
            FrmMediatekCommande frm = new FrmMediatekCommande(idRevue, false, true);
            frm.ShowDialog();
        }


    }
}