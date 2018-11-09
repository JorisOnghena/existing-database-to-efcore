using System;
using System.Data;
using System.Windows.Forms;

namespace existing_database_to_efcore
{
    public partial class FormMain : Form
    {
        private Configuration configuration;

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            this.tsbtnRefreshConnection.Enabled = false;
        }

        private void tviewTables_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                string id = e.Node.Text;
                this.tviewTables.SelectedNode = e.Node;
                var tables = this.RetrieveDatabase().DescribeTable(id);
                this.dgviewTableDescription.DataSource = tables;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "An error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private IDataBase RetrieveDatabase()
        {
            if (this.configuration != null)
            {
                if (!string.IsNullOrEmpty(this.configuration.ConnectionString))
                {
                    if (this.configuration.Type.ToLower() == "mysql")
                    {
                        return new DataBaseMySQL(this.configuration.ConnectionString);
                    }
                }
            }

            return null;
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable test = (DataTable)this.dgviewTableDescription.DataSource;
                IDataBase db = this.RetrieveDatabase();
                this.txtSource.Text = db.Generate(this.tviewTables.SelectedNode.Text);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "An error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void tsbtnAddConnection_Click(object sender, EventArgs e)
        {
            FormDatabaseConnection frm = new FormDatabaseConnection(this.configuration);
            frm.ShowDialog();
        }

        private void tsbtnRefreshConnection_Click(object sender, EventArgs e)
        {
            try
            {
                var tables = this.RetrieveDatabase().ListAllTables();

                this.tviewTables.Nodes.Clear();
                this.tviewTables.Nodes.Add("TABLES", "Tables", 0);

                foreach (DataRow row in tables.Rows)
                {
                    this.tviewTables.Nodes[0].Nodes.Add(row[0].ToString(), row[0].ToString(), 1);
                }

                this.tviewTables.Nodes[0].Expand();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "An error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ofdOpenIni.ShowDialog();
        }

        private void ofdOpenIni_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.configuration = new Configuration(this.ofdOpenIni.FileName);
            this.tsbtnRefreshConnection_Click(sender, e);
            this.tsbtnRefreshConnection.Enabled = true;
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.tsbtnAddConnection_Click(sender, e);
        }
    }
}
