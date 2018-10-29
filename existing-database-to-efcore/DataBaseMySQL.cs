using MySql.Data.MySqlClient;
using System.Data;

namespace existing_database_to_efcore
{
    public class DataBaseMySQL : IDataBase
    {
        /// <inheritdoc cref="ConnectionString"/>
        public string ConnectionString { get; private set; }

        public DataBaseMySQL(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        /// <inheritdoc cref="ListAllTables"/>
        public DataTable ListAllTables()
        {
            return this.GetData("SHOW TABLES");
        }

        /// <inheritdoc cref="DescribeTable"/>
        public DataTable DescribeTable(string tableName)
        {
            return this.GetData("DESCRIBE " + tableName);
        }

        /// <inheritdoc cref="GetData"/>
        public DataTable GetData(string sql)
        {
            DataTable results = new DataTable();
            using (MySqlConnection mysqlCon = new MySqlConnection(this.ConnectionString))
            {
                mysqlCon.Open();
                using (MySqlCommand mysqlCmd = new MySqlCommand(sql, mysqlCon))
                {
                    mysqlCmd.CommandTimeout = 180; // timeout after 180 seconds, 0 is never timeout
                    mysqlCmd.CommandType = CommandType.Text;
                    using (MySqlDataAdapter oAdapter = new MySqlDataAdapter())
                    {
                        oAdapter.SelectCommand = mysqlCmd;
                        oAdapter.Fill(results);
                    }
                }
                mysqlCon.Close();
            }
            return results;
        }

        /// <inheritdoc cref="Generate"/>
        public string Generate(DataTable description)
        {
            throw new System.NotImplementedException();
        }
    }
}
