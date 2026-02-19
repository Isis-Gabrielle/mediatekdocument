using MediaTekDocuments.controller;
using MediaTekDocuments.model;
using MediaTekDocuments.view;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediaTekDocuments
{
    /// <summary>
    /// Fenêtre d'authentification
    /// </summary>
    public partial class FrmMediatekAuth : Form
    {
        /// <summary>
        /// Instance du contrôleur
        /// </summary>
        private readonly FrmMediatekController controller;

        /// <summary>
        /// Constructeur : création du contrôleur 
        /// </summary>
        public FrmMediatekAuth()
        {
            InitializeComponent();
            this.controller = new FrmMediatekController();
        }

        /// <summary>
        /// sur le clic du bouton de connexion : récupère les identifiants et appelle contrôleur pour authentifier
        /// </summary>
        /// <param name="sender">Source de l'événement</param>
        /// <param name="e">Arguments de l'événement</param>
        private void btnConnect_Click(object sender, EventArgs e)
        {
            string email = txtemail.Text;
            string password = txtPassword.Text;

            if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(password))
            {
                MessageBox.Show("Tous les champs doivent être remplis.", "Information");
            }
            else
            {
                Utilisateur user = controller.Login(email, password);
                if (user != null)
                {
                    FrmMediatek frm = new FrmMediatek(user);
                    frm.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Authentification incorrecte", "Alerte");
                }
            }
        }
    }
}
