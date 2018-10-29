using System;
using System.Windows.Forms;

namespace existing_database_to_efcore
{
    using IniParser;
    using IniParser.Model;
    using System.IO;

    public partial class FormDatabaseConnection : Form
    {
        public FormDatabaseConnection()
        {
            InitializeComponent();
        }

        private void FormCSharpCode_Load(object sender, EventArgs e)
        {
            Configuration config = new Configuration();
            this.txtDisplayName.Text = config.DisplayName;
            this.txtConnectionString.Text = config.ConnectionString;
            this.cboxType.Text = config.Type;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Configuration config = new Configuration();
            config.DisplayName = this.txtDisplayName.Text;
            config.ConnectionString = this.txtConnectionString.Text;
            config.Type = this.cboxType.Text;

            config.Save();

            this.Close();
        }
    }
}
