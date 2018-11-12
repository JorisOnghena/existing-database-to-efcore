namespace existing_database_to_efcore.DataBaseTypes
{
    using System.Collections.Generic;
    using System.Data;
    using existing_database_to_efcore.DataBaseGeneric;
    using MySql.Data.MySqlClient;

    public class DataBaseMySQL : IDataBase
    {
        /// <inheritdoc />
        public DataBaseMySQL(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        /// <inheritdoc cref="ConnectionString"/>
        public string ConnectionString { get; private set; }

        /// <inheritdoc />
        public string ConvertDBTypeCSharp(string originalType)
        {
            if (originalType.ToLower().Contains("int"))
            {
                return "int";
            }
            else if (originalType.ToLower().Contains("timestamp") || originalType.ToLower().Contains("datetime")
                                                                  || originalType.ToLower().Contains("date"))
            {
                return "DateTime";
            }
            else if (originalType.ToLower().Contains("text") || originalType.ToLower().Contains("varchar")
                                                             || originalType.ToLower().Contains("char"))
            {
                return "string";
            }
            else if (originalType.ToLower().Contains("decimal"))
            {
                return "decimal";
            }
            else if (originalType.ToLower().Contains("double"))
            {
                return "double";
            }
            else if (originalType.ToLower().Contains("blob"))
            {
                return "string";
            }

            return "UNDEFINED";
        }

        /// <inheritdoc />
        public Table DescribeTable(string tableName)
        {
            List<Column> columns = new List<Column>();

            DataTable dt = this.GetData("DESCRIBE " + tableName);

            foreach (DataRow row in dt.Rows)
            {
                columns.Add(
                    new Column()
                        {
                            Name = row["Field"].ToString(),
                            DeFaultValue = row["Default"].ToString(),
                            Type = row["Type"].ToString(),
                            CanBeNull = (row["Null"].ToString().ToUpper() == "YES"),
                            IsKey = (row["Key"] != null && !string.IsNullOrEmpty(row["Key"].ToString())
                                                        && row["Key"].ToString().ToUpper() == "PRI"),

                            // Extra = row["Extra"].ToString(),
                            AutoIncrement = (row["Extra"] != null && !string.IsNullOrEmpty(row["Extra"].ToString())
                                                                  && row["Extra"].ToString().ToLower()
                                                                      .Contains("auto_increment"))
                        });
            }

            return new Table(tableName, columns);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public List<Table> ListAllTables()
        {
            List<Table> tables = new List<Table>();

            DataTable dt = this.GetData("SHOW TABLES");

            foreach (DataRow row in dt.Rows)
            {
                tables.Add(new Table(row[0].ToString()));
            }

            return tables;
        }
    }
}