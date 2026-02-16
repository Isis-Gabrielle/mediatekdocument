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
    public partial class FrmMediatekCommande : Form
    {
        #region commun

        private readonly BindingSource bdgCommandeLivreListe = new BindingSource();
        private readonly BindingSource bdgCommandedDvdListe = new BindingSource();
        private readonly BindingSource bdgAbonnementsListe = new BindingSource();
        private readonly BindingSource bdgSuivi = new BindingSource();

        private List<CommandeDocument> lesCommandesLivres = new List<CommandeDocument>();
        private List<CommandeDocument> lesCommandesDvd = new List<CommandeDocument>();
        private List<Abonnement> lesAbonnements = new List<Abonnement>();

        private List<Livre> lesLivres;
        private List<Dvd> lesDvd;
        private List<Revue> lesRevues;

        private readonly FrmMediatekController controller;
        private string idDocumentPreselectionne;

        private static string reglee = "Réglée";
        private static string livree = "Livrée";

        public FrmMediatekCommande(string idDocument = null, bool estLivre = true, bool estRevue = false)
        {
            InitializeComponent();
            this.controller = new FrmMediatekController();
            this.idDocumentPreselectionne = idDocument;

            bdgSuivi.DataSource = controller.GetAllSuivi();

            cbxLivresCommandesSuivi.DataSource = bdgSuivi;
            cbxLivresCommandesSuivi.DisplayMember = "Libelle";
            cbxLivresCommandesSuivi.ValueMember = "Id";

            cbxDvdCommandesSuivi.DataSource = bdgSuivi;
            cbxDvdCommandesSuivi.DisplayMember = "Libelle";
            cbxDvdCommandesSuivi.ValueMember = "Id";

            lesLivres = controller.GetAllLivres();
            lesDvd = controller.GetAllDvd();
            lesRevues = controller.GetAllRevues();
            VerifierAbonnementsExpirants();

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

        private enum EtatCommande
        {
            Consultation,
            Ajout,
            Modification
        }

        private EtatCommande etatCommande = EtatCommande.Consultation;

        private void btnReceptionRechercher_Click(object sender, EventArgs e)
        {
            bool estOngletLivre = tabOngletsApplication.SelectedTab == tabLivres;

            if (estOngletLivre)
            {
                string idRecherche = txbLivresNumRecherche.Text.Trim();
                if (!string.IsNullOrEmpty(idRecherche))
                {
                    Livre livre = lesLivres.Find(x => x.Id.Equals(idRecherche));
                    if (livre != null)
                    {
                        AfficheLivresInfos(livre);
                        lesCommandesLivres = controller.GetCommandesDocument(livre.Id);
                        RemplirCommandeListe(lesCommandesLivres, dgvCommandeLivresListe, bdgCommandeLivreListe);
                    }
                    else { MessageBox.Show("Livre introuvable"); }
                }
            }
            else
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

        private void tabOngletsApplication_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetEtatCommande(EtatCommande.Consultation);

            if (tabOngletsApplication.SelectedTab == tabLivres && !string.IsNullOrEmpty(txbLivresNumRecherche.Text))
            {
                btnReceptionRechercher_Click(null, null);
            }
            else if (tabOngletsApplication.SelectedTab == tabDvd && !string.IsNullOrEmpty(tbxDvdNumero.Text))
            {
                btnReceptionRechercher_Click(null, null);
            }
        }

        private void SetEtatCommande(EtatCommande etatcommande)
        {
            etatCommande = etatcommande;
            bool edition = (etatcommande != EtatCommande.Consultation);
            bool ajout = (etatcommande == EtatCommande.Ajout);
            bool estOngletLivre = tabOngletsApplication.SelectedTab == tabLivres;
            string idRecherche = estOngletLivre ? txbLivresNumRecherche.Text.Trim() : tbxDvdNumero.Text.Trim();
            if (!string.IsNullOrEmpty(idRecherche))
            {
                if (estOngletLivre)
                {
                    dtpLivresCommandesDate.Focus();
                    btnLivresSaveCommande.Visible = edition;
                    btnLivresAnnulerCommande.Visible = edition;
                    btnLivresSuppCommande.Visible = !edition;
                    btnLivresAjoutCommande.Visible = !edition;
                    btnLivresModifCommande.Visible = !edition;
                    tbxLivresCommandesQuantite.ReadOnly = !ajout;
                    dtpLivresCommandesDate.Enabled = ajout;
                    txbLivresCommandesMontant.ReadOnly = !ajout;
                    dgvCommandeLivresListe.Enabled = !edition;
                }
                else
                {
                    dtpDvdCommandesDate.Focus();
                    btnDvdSaveCommande.Visible = edition;
                    btnDvdAnnulerCommande.Visible = edition;
                    btnDvdSuppCommande.Visible = !edition;
                    btnDvdAjoutCommande.Visible = !edition;
                    btnDvdModifCommande.Visible = !edition;
                    tbxDvdCommandesQuantite.ReadOnly = !ajout;
                    dtpDvdCommandesDate.Enabled = ajout;
                    txbDvdCommandesMontant.ReadOnly = !ajout;
                    dgvCommandeDvdListe.Enabled = !edition;
                }
            }
        }

        #endregion commun

        #region Livre

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

        private void RemplirCommandeListe(List<CommandeDocument> commandes, DataGridView dgv, BindingSource bdg)
        {
            bdg.DataSource = null;
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

        private void btnLivresAjoutCommande_Click(object sender, EventArgs e)
        {
            SetEtatCommande(EtatCommande.Ajout);
        }

        private void btnLivresModifCommande_Click(object sender, EventArgs e)
        {
            if (bdgCommandeLivreListe.Current != null)
            {
                SetEtatCommande(EtatCommande.Modification);
                cbxLivresCommandesSuivi.Enabled = true;
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une commande à modifier.");
            }
        }

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

        private void btnLivresSaveCommande_Click(object sender, EventArgs e)
        {
            if (etatCommande == EtatCommande.Modification)
            {
                CommandeDocument laCommande = (CommandeDocument)bdgCommandeLivreListe.Current;
                Suivi leSuivi = (Suivi)cbxLivresCommandesSuivi.SelectedItem;
                if (laCommande != null && leSuivi != null)
                {
                    if ((laCommande.LibelleSuivi == livree || laCommande.LibelleSuivi == reglee)
                        && (leSuivi.Libelle == "En cours" || leSuivi.Libelle == "Relancée"))
                    {
                        MessageBox.Show("Impossible de revenir à une étape précédente pour une commande livrée ou réglée.");
                        return;
                    }

                    if (leSuivi.Libelle == reglee && laCommande.LibelleSuivi != livree)
                    {
                        MessageBox.Show("Une commande doit être à l'état 'Livrée' avant d'être passée à 'Réglée'.");
                        return;
                    }

                    if (
                  controller.EditSuiviCommande(laCommande.Id, leSuivi.Id)
                )
                    {
                        laCommande.IdSuivi = leSuivi.Id;
                        laCommande.LibelleSuivi = leSuivi.Libelle;
                        FinirEdition("Suivi mis à jour avec succès.");
                    }
                }
            }
            else if (etatCommande == EtatCommande.Ajout)
            {
                try
                {
                    string idCommande = controller.GenerateNewCommandeId();
                    DateTime dateCommande = dtpLivresCommandesDate.Value;
                    int montant = int.Parse(txbLivresCommandesMontant.Text);
                    int nbExemplaires = int.Parse(tbxLivresCommandesQuantite.Text);
                    string idLivre = txbLivresNumRecherche.Text;
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

        private void btnDvdSaveCommande_Click(object sender, EventArgs e)
        {
            if (etatCommande == EtatCommande.Modification)
            {
                CommandeDocument laCommande = (CommandeDocument)bdgCommandedDvdListe.Current;
                Suivi leSuivi = (Suivi)cbxDvdCommandesSuivi.SelectedItem;
                if (laCommande != null && leSuivi != null && controller.EditSuiviCommande(laCommande.Id, leSuivi.Id))
                {
                        laCommande.IdSuivi = leSuivi.Id;
                        laCommande.LibelleSuivi = leSuivi.Libelle;
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

        private void btnDvdAjoutCommande_Click(object sender, EventArgs e)
        {
            SetEtatCommande(EtatCommande.Ajout);
        }

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

                    foreach (var abo in tousLesAbonnements)
                    {
                        TimeSpan difference = abo.DateFinAbonnement - aujourdhui;
                        int joursRestants = difference.Days;

                        if (joursRestants >= 0 && joursRestants <= 30)
                        {
                            Revue r = lesRevues.Find(rev => rev.Id == abo.IdRevue);
                            if (r != null)
                            {
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


        private void dgvCommandeRevuesListe_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string nomPropriete = dgvCommandeRevuesListe.Columns[e.ColumnIndex].DataPropertyName;
            if (lesAbonnements.Count > 0)
            {
                List<Abonnement> sorted = lesAbonnements.OrderBy(x => x.GetType().GetProperty(nomPropriete).GetValue(x, null)).ToList();
                bdgAbonnementsListe.DataSource = sorted;
            }
        }



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


        private void btnRevuesAjoutCommande_Click(object sender, EventArgs e)
        {
            SetEtatCommandeRevues(EtatCommande.Ajout);
        }

        private void btnRevuesAnnulerCommande_Click(object sender, EventArgs e)
        {
            SetEtatCommandeRevues(EtatCommande.Consultation);
        }

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