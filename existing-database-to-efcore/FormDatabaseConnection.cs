﻿namespace existing_database_to_efcore
{
    using System;
    using System.Windows.Forms;

    public partial class FormDatabaseConnection : Form
    {
        private Configuration existingConfiguration = null;

        public FormDatabaseConnection(Configuration existingConfiguration)
        {
            InitializeComponent();
            this.existingConfiguration = existingConfiguration;
        }

        private void FormCSharpCode_Load(object sender, EventArgs e)
        {
            if (this.existingConfiguration != null)
            {
                this.txtDisplayName.Text = this.existingConfiguration.DisplayName;
                this.txtConnectionString.Text = this.existingConfiguration.ConnectionString;
                this.cboxType.Text = this.existingConfiguration.Type;
                this.sfdIniFile.FileName = this.existingConfiguration.FileConfiguration;
                this.txtNamespace.Text = this.existingConfiguration.CodeNamespace;
                this.cbSealed.Checked = this.existingConfiguration.CodeGenerateSealedClasses;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.sfdIniFile.ShowDialog();
        }

        private void sfdIniFile_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Configuration configToSave = new Configuration(this.sfdIniFile.FileName);
            configToSave.DisplayName = this.txtDisplayName.Text;
            configToSave.ConnectionString = this.txtConnectionString.Text;
            configToSave.Type = this.cboxType.Text;
            configToSave.CodeNamespace = this.txtNamespace.Text;
            configToSave.CodeGenerateSealedClasses = this.cbSealed.Checked;
            configToSave.Save();
            this.Close();
        }
    }
}
