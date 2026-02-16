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
    public partial class FrmMediatekAuth : Form
    {
        private readonly FrmMediatekController controller;

        public FrmMediatekAuth()
        {
            InitializeComponent();
            this.controller = new FrmMediatekController();
        }

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
