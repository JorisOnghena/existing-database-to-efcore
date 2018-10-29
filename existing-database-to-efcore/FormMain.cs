using System;
using System.Data;
using System.Windows.Forms;

namespace existing_database_to_efcore
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            this.tsbtnRefreshConnection_Click(sender, e);
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
            Configuration config = new Configuration();
            if (!string.IsNullOrEmpty(config.ConnectionString))
            {
                if (config.Type.ToLower() == "mysql")
                {
                    return new DataBaseMySQL(config.ConnectionString);
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
                this.txtSource.Text = db.Generate(db.DescribeTable(this.tviewTables.SelectedNode.Text));
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
            FormDatabaseConnection frm = new FormDatabaseConnection();
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
    }
}
