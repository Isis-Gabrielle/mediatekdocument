using MediaTekDocuments.controller;
using MediaTekDocuments.model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MediaTekDocuments.view
{
    /// <summary>
    /// Fenêtre de gestion de commandes et d'abonnements
    /// </summary>
    public partial class FrmMediatekCommande : Form
    {
        #region commun

        /// <summary>
        /// Source de données pour la liste des commandes d'un livre
        /// </summary>
        private readonly BindingSource bdgCommandeLivreListe = new BindingSource();

        /// <summary>
        /// Source de données pour la liste des commandes d'un dvd
        /// </summary>
        private readonly BindingSource bdgCommandedDvdListe = new BindingSource();

        /// <summary>
        /// Source de données pour la liste des abonnements d'une revue
        /// </summary>
        private readonly BindingSource bdgAbonnementsListe = new BindingSource();

        /// <summary>
        /// Source de données pour la liste des étapes de suivi
        /// </summary>
        private readonly BindingSource bdgSuivi = new BindingSource();

        /// <summary>
        /// liste des commandes d'un livre
        /// </summary>
        private List<CommandeDocument> lesCommandesLivres = new List<CommandeDocument>();

        /// <summary>
        /// liste des commandes d'un dvd
        /// </summary>
        private List<CommandeDocument> lesCommandesDvd = new List<CommandeDocument>();

        /// <summary>
        /// liste des abonnements d'une revue
        /// </summary>
        private List<Abonnement> lesAbonnements = new List<Abonnement>();

        /// <summary>
        /// liste des livres récupérés depuis la bdd
        /// </summary>
        private List<Livre> lesLivres;

        /// <summary>
        /// liste des dvd récupérés depuis la bdd
        /// </summary>
        private List<Dvd> lesDvd;

        /// <summary>
        /// liste des revues récupérées depuis la bdd
        /// </summary>
        private List<Revue> lesRevues;

        /// <summary>
        /// Instance du contrôleur
        /// </summary>
        private readonly FrmMediatekController controller;
        private string idDocumentPreselectionne;

        /// <summary>
        /// Constante pour l'état de commande : Réglée
        /// </summary>
        private static string reglee = "Réglée";

        /// <summary>
        /// Constante pour l'état de commande : Livrée
        /// </summary>
        private static string livree = "Livrée";

        /// <summary>
        /// Constructeur : création du contrôleur lié à ce formulaire
        /// </summary>
        /// <param name="idDocument">id du document présélectionné</param>
        /// <param name="estLivre"> si le document est un livre ou un DVD par défaut</param>
        /// <param name="estRevue"> si le document est une revue</param>
        public FrmMediatekCommande(string idDocument = null, bool estLivre = true, bool estRevue = false)
        {
            InitializeComponent();
            this.controller = new FrmMediatekController();
            this.idDocumentPreselectionne = idDocument;

            // charge la liste des étapes de suivi
            bdgSuivi.DataSource = controller.GetAllSuivi();

            // liste des étapes de suivi dans la comboBox de suivi pour les Livres et DVD
            cbxLivresCommandesSuivi.DataSource = bdgSuivi;
            cbxLivresCommandesSuivi.DisplayMember = "Libelle";
            cbxLivresCommandesSuivi.ValueMember = "Id";

            cbxDvdCommandesSuivi.DataSource = bdgSuivi;
            cbxDvdCommandesSuivi.DisplayMember = "Libelle";
            cbxDvdCommandesSuivi.ValueMember = "Id";

            // charge les catalogues
            lesLivres = controller.GetAllLivres();
            lesDvd = controller.GetAllDvd();
            lesRevues = controller.GetAllRevues();

            // lancement automatique pour afficher les abonnements en expiration
            VerifierAbonnementsExpirants();

            // présélection : si on vient de l'onglet catalogue avec un id préselectionné dans la fenêtre précédente
            if (!string.IsNullOrEmpty(idDocumentPreselectionne))
            {
                if (estLivre)
                {
                    tabOngletsApplication.SelectedTab = tabLivres;
                    txbLivresNumRecherche.Text = idDocumentPreselectionne;
                    btnReceptionRechercher_Click(null, null);
                }
                else if (estRevue)
                {
                    tabOngletsApplication.SelectedTab = tabRevues;
                    tbxRevuesNumero.Text = idDocumentPreselectionne;
                    btnRechercherRevuesCommande_Click(null, null);
                }
                else
                {
                    tabOngletsApplication.SelectedTab = tabDvd;
                    tbxDvdNumero.Text = idDocumentPreselectionne;
                    btnReceptionRechercher_Click(null, null);
                }
            }
        }

        /// <summary>
        /// Énumération des états possibles pour gérer les commandes (Consultation par défaut)
        /// </summary>
        private enum EtatCommande
        {
            Consultation,
            Ajout,
            Modification
        }
        private EtatCommande etatCommande = EtatCommande.Consultation;

        /// <summary>
        /// Recherche d'un numéro d'un document (livre/dvd) et affiche ses informations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReceptionRechercher_Click(object sender, EventArgs e)
        {
            bool estOngletLivre = tabOngletsApplication.SelectedTab == tabLivres;

            if (estOngletLivre) // onglet livre
            {
                string idRecherche = txbLivresNumRecherche.Text.Trim();
                if (!string.IsNullOrEmpty(idRecherche))
                {
                    // Recherche dans la liste locale chargée
                    Livre livre = lesLivres.Find(x => x.Id.Equals(idRecherche));
                    if (livre != null)
                    {
                        AfficheLivresInfos(livre);
                        // recupère les commandes spécifiques au document
                        lesCommandesLivres = controller.GetCommandesDocument(livre.Id);
                        RemplirCommandeListe(lesCommandesLivres, dgvCommandeLivresListe, bdgCommandeLivreListe);
                    }
                    else { MessageBox.Show("Livre introuvable"); }
                }
            }
            else //onglet dvd
            {
                string idRecherche = tbxDvdNumero.Text.Trim();
                if (!string.IsNullOrEmpty(idRecherche))
                {
                    Dvd dvd = lesDvd.Find(x => x.Id.Equals(idRecherche));
                    if (dvd != null)
                    {
                        AfficheDvdInfos(dvd);
                        lesCommandesDvd = controller.GetCommandesDocument(dvd.Id);
                        RemplirCommandeListe(lesCommandesDvd, dgvCommandeDvdListe, bdgCommandedDvdListe);
                    }
                    else { MessageBox.Show("DVD introuvable"); }
                }
            }
        }

        /// <summary>
        /// Lors du changement d'onglet : réinitialise l'état et lance la recherche si un numéro est présent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabOngletsApplication_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetEtatCommande(EtatCommande.Consultation);

            // rafraîchit la recherche si un numéro est déjà saisi
            if (tabOngletsApplication.SelectedTab == tabLivres && !string.IsNullOrEmpty(txbLivresNumRecherche.Text))
            {
                btnReceptionRechercher_Click(null, null);
            }
            else if (tabOngletsApplication.SelectedTab == tabDvd && !string.IsNullOrEmpty(tbxDvdNumero.Text))
            {
                btnReceptionRechercher_Click(null, null);
            }
        }

        /// <summary>
        /// Modifie l'état de l'interface en fonction du mode (Consultation, Ajout, Modification)
        /// </summary>
        /// <param name="etatcommande">Le nouvel état déclenché</param>
        private void SetEtatCommande(EtatCommande etatcommande)
        {
            etatCommande = etatcommande;
            bool edition = (etatcommande != EtatCommande.Consultation);
            bool ajout = (etatcommande == EtatCommande.Ajout);
            bool estOngletLivre = tabOngletsApplication.SelectedTab == tabLivres;

            string idRecherche = estOngletLivre ? txbLivresNumRecherche.Text.Trim() : tbxDvdNumero.Text.Trim();

            // édition autorisée que si un document est affiché
            if (!string.IsNullOrEmpty(idRecherche))
            {
                if (estOngletLivre) // livre
                {
                    dtpLivresCommandesDate.Focus();

                    // visibilité
                    btnLivresSaveCommande.Visible = edition;
                    btnLivresAnnulerCommande.Visible = edition;
                    btnLivresSuppCommande.Visible = !edition;
                    btnLivresAjoutCommande.Visible = !edition;
                    btnLivresModifCommande.Visible = !edition;

                    // readonly
                    tbxLivresCommandesQuantite.ReadOnly = !ajout;
                    txbLivresCommandesMontant.ReadOnly = !ajout;

                    // enabled
                    dgvCommandeLivresListe.Enabled = !edition;
                    dtpLivresCommandesDate.Enabled = ajout;
                }
                else // dvd
                {
                    dtpDvdCommandesDate.Focus();

                    // visiblité
                    btnDvdSaveCommande.Visible = edition;
                    btnDvdAnnulerCommande.Visible = edition;
                    btnDvdSuppCommande.Visible = !edition;
                    btnDvdAjoutCommande.Visible = !edition;
                    btnDvdModifCommande.Visible = !edition;

                    // readonly
                    tbxDvdCommandesQuantite.ReadOnly = !ajout;     
                    txbDvdCommandesMontant.ReadOnly = !ajout;

                    // enabled
                    dtpDvdCommandesDate.Enabled = ajout;
                    dgvCommandeDvdListe.Enabled = !edition;
                }
            }
        }
        /// <summary>
        /// Remplit le datagrid avec la liste reçue en paramètre
        /// </summary>
        /// <param name="commandes">liste de commandes</param>
        /// <param name="dgv">le datagrid concerné</param>
        /// <param name="bdg">la source de données concernée</param>
        private void RemplirCommandeListe(List<CommandeDocument> commandes, DataGridView dgv, BindingSource bdg)
        {
            bdg.DataSource = null; // réinitialise la source
            bdg.DataSource = commandes;
            dgv.DataSource = bdg;

            if (dgv.Columns.Count > 0)
            {
                if (dgv.Columns.Contains("idLivreDvd")) dgv.Columns["idLivreDvd"].Visible = false;
                if (dgv.Columns.Contains("idSuivi")) dgv.Columns["idSuivi"].Visible = false;

                dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                dgv.Columns["id"].DisplayIndex = 0;
                dgv.Columns["id"].HeaderText = "Id";

                dgv.Columns["dateCommande"].DisplayIndex = 1;
                dgv.Columns["dateCommande"].HeaderText = "Date de la commande";

                dgv.Columns["montant"].DisplayIndex = 2;
                dgv.Columns["montant"].HeaderText = "Montant";

                dgv.Columns["nbExemplaire"].DisplayIndex = 3;
                dgv.Columns["nbExemplaire"].HeaderText = "Quantité";

                dgv.Columns["libelleSuivi"].DisplayIndex = 4;
                dgv.Columns["libelleSuivi"].HeaderText = "État du suivi";
            }
        }

        #endregion commun

        #region Livre

        /// <summary>
        /// Affichage des informations du livre
        /// </summary>
        /// <param name="livre">le livre</param>
        private void AfficheLivresInfos(Livre livre)
        {
            txbLivresAuteur.Text = livre.Auteur;
            txbLivresCollection.Text = livre.Collection;
            txbLivresImage.Text = livre.Image;
            txbLivresIsbn.Text = livre.Isbn;
            txbLivresGenre.Text = livre.Genre;
            txbLivresPublic.Text = livre.Public;
            txbLivresRayon.Text = livre.Rayon;
            txbLivresTitre.Text = livre.Titre;
            try
            {
                pcbLivresImage.Image = Image.FromFile(livre.Image);
            }
            catch
            {
                pcbLivresImage.Image = null;
            }
        }

        /// <summary>
        /// Tri sur les colonnes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DgvLivresListe_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string titreColonne = dgvCommandeLivresListe.Columns[e.ColumnIndex].HeaderText;
            List<CommandeDocument> sortedList = new List<CommandeDocument>();
            switch (titreColonne)
            {
                case
              "Id":
                    sortedList = lesCommandesLivres.OrderBy(o => o.Id).ToList();
                    break;

                case
              "DateCommande":
                    sortedList = lesCommandesLivres.OrderBy(o => o.DateCommande).ToList();
                    break;

                case
              "Montant":
                    sortedList = lesCommandesLivres.OrderBy(o => o.Montant).ToList();
                    break;

                case
              "NbExemplaire":
                    sortedList = lesCommandesLivres.OrderBy(o => o.NbExemplaire).ToList();
                    break;

                case
              "LibelleSuivi":
                    sortedList = lesCommandesLivres.OrderBy(o => o.LibelleSuivi).ToList();
                    break;
            }
            RemplirCommandeListe(lesCommandesLivres, dgvCommandeLivresListe, bdgCommandeLivreListe);
        }

        /// <summary>
        /// Sur le clic du bouton d'ajout, affichage interface pour la saisie d'une nouvelle commande de livre
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLivresAjoutCommande_Click(object sender, EventArgs e)
        {
            SetEtatCommande(EtatCommande.Ajout);
        }
        /// <summary>
        /// Sur le clic du bouton de modification : prépare l'interface avec les données de la commande du livre sélectionné
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLivresModifCommande_Click(object sender, EventArgs e)
        {
            if (bdgCommandeLivreListe.Current != null)
            {
                SetEtatCommande(EtatCommande.Modification);
                cbxLivresCommandesSuivi.Enabled = true; // autorise le changement d'étape de suivi
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une commande à modifier.");
            }
        }

        /// <summary>
        /// Sur le clic du bouton Suppression : supprime la commande du livre sélectionné après confirmation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLivresSuppCommande_Click(object sender, EventArgs e)
        {
            if (bdgCommandeLivreListe.Current != null)
            {
                CommandeDocument commande = (CommandeDocument)bdgCommandeLivreListe.Current;

                if (commande.LibelleSuivi == livree || commande.LibelleSuivi == reglee)
                {
                    MessageBox.Show("Une commande déjà livrée ou réglée ne peut pas être supprimée.", "Interdit");
                    return;
                }

                string message = $"Voulez-vous vraiment supprimer la commande n°{commande.Id} ?";
                if (
                  MessageBox.Show(
                    message,
                    "Confirmation de suppression",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                  ) == DialogResult.Yes
                )
                {
                    if (controller.DeleteCommande(commande))
                    {
                        lesCommandesLivres.Remove(commande);
                        bdgCommandeLivreListe.ResetBindings(false);
                        MessageBox.Show("Commande supprimée avec succès.", "Information");
                    }
                    else
                    {
                        MessageBox.Show(
                      "Erreur lors de la suppression technique.",
                      "Erreur"
                    );
                    }
                }
            }
            else
            {
                MessageBox.Show(
              "Veuillez sélectionner une commande dans la liste.",
              "Avertissement"
            );
            }
        }

        /// <summary>
        /// Sur le clic du bouton Enregistrer : enregistre les modifications (Ajout ou Modification) en bdd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLivresSaveCommande_Click(object sender, EventArgs e)
        {
            // mode modification uniquement pour le suivi
            if (etatCommande == EtatCommande.Modification)
            {
                CommandeDocument laCommande = (CommandeDocument)bdgCommandeLivreListe.Current;
                Suivi leSuivi = (Suivi)cbxLivresCommandesSuivi.SelectedItem;
                if (laCommande != null && leSuivi != null)
                {
                    // interdiction de revenir en arrière si c'est livré/réglé
                    if ((laCommande.LibelleSuivi == livree || laCommande.LibelleSuivi == reglee)
                        && (leSuivi.Libelle == "En cours" || leSuivi.Libelle == "Relancée"))
                    {
                        MessageBox.Show("Impossible de revenir à une étape précédente pour une commande livrée ou réglée.");
                        return;
                    }

                    // obligation de passer par l'état livrée avant d'être à l'état réglée
                    if (leSuivi.Libelle == reglee && laCommande.LibelleSuivi != livree)
                    {
                        MessageBox.Show("Une commande doit être à l'état 'Livrée' avant d'être passée à 'Réglée'.");
                        return;
                    }

                    if (
                  controller.EditSuiviCommande(laCommande.Id, leSuivi.Id)
                )
                    {
                        FinirEdition("Suivi mis à jour avec succès.");
                    }
                }
            }

            // mode ajout pour la nouvelle commande
            else if (etatCommande == EtatCommande.Ajout)
            {
                try
                {
                    // prépare les données
                    string idCommande = controller.GenerateNewCommandeId();
                    DateTime dateCommande = dtpLivresCommandesDate.Value;
                    int montant = int.Parse(txbLivresCommandesMontant.Text);
                    int nbExemplaires = int.Parse(tbxLivresCommandesQuantite.Text);
                    string idLivre = txbLivresNumRecherche.Text;
                    string idSuivi = "00001"; // par défaut à l'état de suivi en cours

                    if (string.IsNullOrEmpty(idCommande))
                    {
                        MessageBox.Show("Le numéro de commande est obligatoire.");
                        return;
                    }
                    CommandeDocument nouvelleCommande = new CommandeDocument(
                      idCommande,
                      dateCommande,
                      montant,
                      nbExemplaires,
                      idLivre,
                      idSuivi,
                      "En cours"
                    );
                    if (
                      controller.CreerCommandeDocument(nouvelleCommande)
                    )
                    {
                        lesCommandesLivres.Add(nouvelleCommande);
                        FinirEdition("Commande enregistrée avec succès.");
                    }
                    else
                    {
                        MessageBox.Show("Erreur lors de l'enregistrement technique.");
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show(
                      "Veuillez vérifier la saisie des montants et quantités."
                    );
                }
            }
        }

        /// <summary>
        /// Sur le clic du le bouton annuler, interface en mode consultation après confirmation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLivresAnnulerCommande_Click(object sender, EventArgs e)
        {
            if (
                  MessageBox.Show(
                    "Voulez-vous vraiment annuler ?",
                    "Confirmation",
                    MessageBoxButtons.YesNo
                  ) == DialogResult.Yes
                )
            {
                SetEtatCommande(EtatCommande.Consultation);
                cbxLivresCommandesSuivi.Enabled = false;
            }
        }

        /// <summary>
        /// rafraîchit la liste des commandes et réinitialise l'état de l'interface après la fin de l'édition.
        /// </summary>
        /// <param name="message">Message à afficher</param>
        private void FinirEdition(string message)
        {
            MessageBox.Show(message);

            if (tabOngletsApplication.SelectedTab == tabLivres)
            {
                RemplirCommandeListe(lesCommandesLivres, dgvCommandeLivresListe, bdgCommandeLivreListe);
            }
            else
            {
                RemplirCommandeListe(lesCommandesDvd, dgvCommandeDvdListe, bdgCommandedDvdListe);
            }

            SetEtatCommande(EtatCommande.Consultation);
        }

        #endregion Livre

        #region Dvd
        /// <summary>
        /// Affichage des informations du dvd
        /// </summary>
        /// <param name="dvd">le dvd</param>
        private void AfficheDvdInfos(Dvd dvd)
        {
            tbxDvdRealisateur.Text = dvd.Realisateur;
            tbxDvdSynopsis.Text = dvd.Synopsis;
            tbxDvdDuree.Text = dvd.Duree.ToString();
            tbxDvdTitre.Text = dvd.Titre;
            tbxDvdGenre.Text = dvd.Genre;
            tbxDvdPublic.Text = dvd.Public;
            tbxDvdRayon.Text = dvd.Rayon;
            tbxDvdImage.Text = dvd.Image;
            try
            {
                pcbDvdImage.Image = Image.FromFile(dvd.Image);
            }
            catch
            {
                pcbDvdImage.Image = null;
            }
        }

        /// <summary>
        /// Sur le clic du bouton Enregistrer : enregistre les modifications (Ajout ou Modification) en bdd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDvdSaveCommande_Click(object sender, EventArgs e)
        {
            if (etatCommande == EtatCommande.Modification)
            {
                CommandeDocument laCommande = (CommandeDocument)bdgCommandedDvdListe.Current;
                Suivi leSuivi = (Suivi)cbxDvdCommandesSuivi.SelectedItem;
                if (laCommande != null && leSuivi != null && controller.EditSuiviCommande(laCommande.Id, leSuivi.Id))
                {
                        FinirEdition("Suivi mis à jour avec succès.");
                }
            }
            else if (etatCommande == EtatCommande.Ajout)
            {
                try
                {
                    string idCommande = controller.GenerateNewCommandeId();
                    DateTime dateCommande = dtpDvdCommandesDate.Value;
                    int montant = int.Parse(txbDvdCommandesMontant.Text);
                    int nbExemplaires = int.Parse(tbxDvdCommandesQuantite.Text);
                    string idDvd = tbxDvdNumero.Text;
                    string idSuivi = "00001";
                    if (string.IsNullOrEmpty(idCommande))
                    {
                        MessageBox.Show("Le numéro de commande est obligatoire.");
                        return;
                    }
                    CommandeDocument nouvelleCommande = new CommandeDocument(
                      idCommande,
                      dateCommande,
                      montant,
                      nbExemplaires,
                      idDvd,
                      idSuivi,
                      "En cours"
                    );
                    if (
                      controller.CreerCommandeDocument(nouvelleCommande)
                    )
                    {
                        lesCommandesDvd.Add(nouvelleCommande);
                        FinirEdition("Commande enregistrée avec succès.");
                    }
                    else
                    {
                        MessageBox.Show("Erreur lors de l'enregistrement technique.");
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show(
                      "Veuillez vérifier la saisie des montants et quantités."
                    );
                }
            }
        }

        /// <summary>
        /// Sur le clic du le bouton annuler, interface en mode consultation après confirmation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDvdAnnulerCommande_Click(object sender, EventArgs e)
        {
            if (
                  MessageBox.Show(
                    "Voulez-vous vraiment annuler ?",
                    "Confirmation",
                    MessageBoxButtons.YesNo
                  ) == DialogResult.Yes
                )
            {
                SetEtatCommande(EtatCommande.Consultation);
                cbxDvdCommandesSuivi.Enabled = false;
            }
        }

        /// <summary>
        /// Sur le clic du bouton Suppression : supprime la commande du dvd sélectionné après confirmation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDvdSuppCommande_Click(object sender, EventArgs e)
        {
            if (bdgCommandedDvdListe.Current != null)
            {
                CommandeDocument commande = (CommandeDocument)bdgCommandedDvdListe.Current;
                if (commande.LibelleSuivi == livree || commande.LibelleSuivi == reglee)
                {
                    MessageBox.Show("Une commande déjà livrée ou réglée ne peut pas être supprimée.", "Interdit");
                    return;
                }
                string message = $"Voulez-vous vraiment supprimer la commande n°{commande.Id} ?";
                if (
                  MessageBox.Show(
                    message,
                    "Confirmation de suppression",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                  ) == DialogResult.Yes
                )
                {
                    if (controller.DeleteCommande(commande))
                    {
                        lesCommandesDvd.Remove(commande);
                        bdgCommandedDvdListe.ResetBindings(false);
                        MessageBox.Show("Commande supprimée avec succès.", "Information");
                    }
                    else
                    {
                        MessageBox.Show(
                      "Erreur lors de la suppression technique.",
                      "Erreur"
                    );
                    }
                }
            }
            else
            {
                MessageBox.Show(
              "Veuillez sélectionner une commande dans la liste.",
              "Avertissement"
            );
            }
        }

        /// <summary>
        /// Sur le clic du bouton de modification : prépare l'interface avec les données de la commande du livre sélectionné
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDvdModifCommande_Click(object sender, EventArgs e)
        {
            if (bdgCommandedDvdListe.Current != null)
            {
                SetEtatCommande(EtatCommande.Modification);
                cbxDvdCommandesSuivi.Enabled = true;
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une commande à modifier.");
            }
        }

        /// <summary>
        /// Sur le clic du bouton d'ajout, affichage interface pour la saisie d'une noouvelle commande de DVD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDvdAjoutCommande_Click(object sender, EventArgs e)
        {
            SetEtatCommande(EtatCommande.Ajout);
        }

        /// <summary>
        /// Tri sur les colonnes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvCommandeDvdListe_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string titreColonne = dgvCommandeDvdListe.Columns[e.ColumnIndex].HeaderText;
            List<CommandeDocument> sortedList = new List<CommandeDocument>();
            switch (titreColonne)
            {
                case
              "Id":
                    sortedList = lesCommandesDvd.OrderBy(o => o.Id).ToList();
                    break;

                case
              "DateCommande":
                    sortedList = lesCommandesDvd.OrderBy(o => o.DateCommande).ToList();
                    break;

                case
              "Montant":
                    sortedList = lesCommandesDvd.OrderBy(o => o.Montant).ToList();
                    break;

                case
              "NbExemplaire":
                    sortedList = lesCommandesDvd.OrderBy(o => o.NbExemplaire).ToList();
                    break;

                case
              "LibelleSuivi":
                    sortedList = lesCommandesDvd.OrderBy(o => o.LibelleSuivi).ToList();
                    break;
            }
            RemplirCommandeListe(sortedList, dgvCommandeDvdListe, bdgCommandedDvdListe);
        }

        /// <summary>
        /// Recherche d'un numéro d'un dvd et affiche ses informations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRechercherDvdCommande_Click(object sender, EventArgs e)
        {
            string idRecherche = tbxDvdNumero.Text.Trim();
            if (!string.IsNullOrEmpty(idRecherche))
            {
                Dvd dvd = lesDvd.Find(x => x.Id.Equals(idRecherche));

                if (dvd != null)
                {
                    AfficheDvdInfos(dvd);

                    lesCommandesDvd = controller.GetCommandesDocument(dvd.Id);

                    RemplirCommandeListe(lesCommandesDvd, dgvCommandeDvdListe, bdgCommandedDvdListe);

                    SetEtatCommande(EtatCommande.Consultation);
                }
                else
                {
                    MessageBox.Show("DVD introuvable", "Information");
                }
            }
            else
            {
                MessageBox.Show("Veuillez saisir un numéro de DVD", "Avertissement");
           
        
 }
        }
        #endregion Dvd

        #region Revues

        /// <summary>
        /// Vérifie les abonnements arrivant à échéance dans les 30 prochains jours
        /// </summary>
        private void VerifierAbonnementsExpirants()
        {
            try
            {
                List<Abonnement> tousLesAbonnements = controller.GetAllAbonnements();

                if (tousLesAbonnements != null && tousLesAbonnements.Count > 0 && lesRevues != null)
                {
                    StringBuilder sb = new StringBuilder("Rappel - Abonnements se terminant dans moins de 30 jours :\n\n");
                    bool aDesMatchs = false;
                    DateTime aujourdhui = DateTime.Now;
                    DateTime dans30Jours = aujourdhui.AddDays(30);

                    foreach (var abo in tousLesAbonnements)
                    {
                        if (abo.ParutionDansAbonnement(aujourdhui, dans30Jours, abo.DateFinAbonnement))
                        {
                            Revue r = lesRevues.Find(rev => rev.Id == abo.IdRevue);
                            if (r != null)
                            {
                                int joursRestants = (abo.DateFinAbonnement - aujourdhui).Days;
                                sb.AppendLine($"- {r.Titre} : finit le {abo.DateFinAbonnement:dd/MM/yyyy} ({joursRestants} jours restants)");
                                aDesMatchs = true;
                            }
                        }
                    }
                    if (aDesMatchs)
                    {
                        MessageBox.Show(sb.ToString(), "Alerte Fin d'Abonnement", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors du filtrage des abonnements : " + ex.Message);
            }
        }

        /// <summary>
        /// Modifie l'état de l'interface en fonction du mode (Consultation, Ajout, Modification)
        /// </summary>
        /// <param name="etat">Le nouvel état déclenché</param>
        private void SetEtatCommandeRevues(EtatCommande etat)
        {
            bool edition = (etat == EtatCommande.Ajout || etat == EtatCommande.Modification);
            btnRevuesSaveCommande.Visible = edition;
            btnRevuesAnnulerCommande.Visible = edition;
            btnRevuesAjoutCommande.Visible = (etat == EtatCommande.Consultation);
            btnRevuesSuppCommande.Visible = (etat == EtatCommande.Consultation);

            dtpRevuesCommandesDate.Enabled = edition;
            dtpRevuesCommandesFinAbo.Enabled = edition;
            txbRevuesCommandesMontant.ReadOnly = !edition;
            dgvCommandeRevuesListe.Enabled = !edition;
        }

        /// <summary>
        /// Tri sur les colonnes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvCommandeRevuesListe_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string nomPropriete = dgvCommandeRevuesListe.Columns[e.ColumnIndex].DataPropertyName;
            if (lesAbonnements.Count > 0)
            {
                List<Abonnement> sorted = lesAbonnements.OrderBy(x => x.GetType().GetProperty(nomPropriete).GetValue(x, null)).ToList();
                bdgAbonnementsListe.DataSource = sorted;
            }
        }

        /// <summary>
        /// Sur le clic du bouton de modification : prépare l'interface avec les données de l'abonnement de la revue sélectionnée
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRevuesEditCommande_Click(object sender, EventArgs e)
        {
            if (bdgAbonnementsListe.Current is Abonnement abo)
            {
                dtpRevuesCommandesDate.Value = abo.DateCommande;
                dtpRevuesCommandesFinAbo.Value = abo.DateFinAbonnement;
                txbRevuesCommandesMontant.Text = abo.Montant.ToString();

                SetEtatCommandeRevues(EtatCommande.Modification);
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un abonnement à modifier.");
            }
        }

        /// <summary>
        /// Sur le clic du bouton Suppression : supprime l'abonnement de la revue sélectionnée après confirmation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRevuesSuppCommande_Click(object sender, EventArgs e)
        {
            if (bdgAbonnementsListe.Current is Abonnement abo)
            {
                List<Exemplaire> exemplairesRevue = controller.GetExemplaires(abo.IdRevue);

                bool aDesExemplairesRattaches = exemplairesRevue.Any(ex =>
                    abo.ParutionDansAbonnement(abo.DateCommande, abo.DateFinAbonnement, ex.DateAchat));

                if (aDesExemplairesRattaches)
                {
                    MessageBox.Show("Suppression impossible : des exemplaires ont déjà été reçus pour cet abonnement.",
                                    "Sécurité", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (MessageBox.Show("Voulez-vous vraiment supprimer cet abonnement ?", "Confirmation",
                    MessageBoxButtons.YesNo) == DialogResult.Yes && controller.DeleteCommande(abo))
                {
                        lesAbonnements.Remove(abo);
                        RemplirAbonnementListe(lesAbonnements);
                }
            }
        }

        /// <summary>
        /// Sur le clic du bouton d'ajout, affichage interface pour la saisie d'un nouvel abonnement pour une revue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRevuesAjoutCommande_Click(object sender, EventArgs e)
        {
            SetEtatCommandeRevues(EtatCommande.Ajout);
        }

        /// <summary>
        /// Sur le clic du le bouton annuler, interface en mode consultation après confirmation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRevuesAnnulerCommande_Click(object sender, EventArgs e)
        {
            SetEtatCommandeRevues(EtatCommande.Consultation);
        }

        /// <summary>
        /// Sur le clic du bouton Enregistrer : enregistre les modifications (Ajout ou Modification) en bdd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRevuesSaveCommande_Click(object sender, EventArgs e)

        {
            if (string.IsNullOrEmpty(tbxRevuesNumero.Text)) return;
            try

            {
                string id = controller.GenerateNewCommandeId();
                DateTime dateCommande = dtpRevuesCommandesDate.Value;
                DateTime dateFin = dtpRevuesCommandesFinAbo.Value;
                if (dateFin <= dateCommande)
                {
                    MessageBox.Show("La date de fin doit être postérieure à la date de commande.");
                    return;
                }
                int montant = int.Parse(txbRevuesCommandesMontant.Text);
                Abonnement nouvelAbo = new Abonnement(id, dateCommande, montant, dateFin, tbxRevuesNumero.Text);
                if (controller.CreerAbonnement(nouvelAbo))
                {
                    lesAbonnements.Add(nouvelAbo);
                    RemplirAbonnementListe(lesAbonnements);
                    SetEtatCommandeRevues(EtatCommande.Consultation);
                    MessageBox.Show("Commande d'abonnement enregistrée avec succès.");
                }
            }
            catch { MessageBox.Show("Erreur de saisie : vérifiez le montant."); }

        }

        /// <summary>
        /// Recherche d'un numéro d'une revue et affiche ses informations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRechercherRevuesCommande_Click(object sender, EventArgs e)
        {
            string idRecherche = tbxRevuesNumero.Text.Trim();
            if (!string.IsNullOrEmpty(idRecherche))
            {
                Revue revue = lesRevues.Find(x => x.Id.Equals(idRecherche));
                if (revue != null)
                {
                    tbxRevuesTitre.Text = revue.Titre;
                    tbxRevuesPeriodicite.Text = revue.Periodicite;
                    tbxRevuesGenre.Text = revue.Genre;
                    tbxRevuesRayon.Text = revue.Rayon;
                    tbxRevuesPublic.Text = revue.Public;
                    lesAbonnements = controller.GetAbonnements(revue.Id);
                    RemplirAbonnementListe(lesAbonnements);
                    SetEtatCommandeRevues(EtatCommande.Consultation);
                }
                else { MessageBox.Show("Revue introuvable"); }
            }
        }

        /// <summary>
        /// Remplit le dategrid avec la liste reçue en paramètre
        /// </summary>
        /// <param name="abonnements">liste d'abonnements</param>
        private void RemplirAbonnementListe(List<Abonnement> abonnements)
        {
            bdgAbonnementsListe.DataSource = null;
            bdgAbonnementsListe.DataSource = abonnements.OrderByDescending(x => x.DateCommande).ToList();
            dgvCommandeRevuesListe.DataSource = bdgAbonnementsListe;

            if (dgvCommandeRevuesListe.Columns.Count > 0)
            {
                dgvCommandeRevuesListe.Columns["id"].Visible = true;
                dgvCommandeRevuesListe.Columns["idRevue"].Visible = false;
                dgvCommandeRevuesListe.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
        }

        #endregion
    }
}