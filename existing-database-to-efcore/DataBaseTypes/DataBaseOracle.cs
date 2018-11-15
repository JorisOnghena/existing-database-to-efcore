namespace existing_database_to_efcore.DataBaseTypes
{
    using System.Collections.Generic;
    using System.Data;
    using existing_database_to_efcore.DataBaseGeneric;
    using Oracle.ManagedDataAccess.Client;

    public class DataBaseOracle : IDataBase
    {
        /// <inheritdoc />
        public DataBaseOracle(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        /// <inheritdoc cref="ConnectionString"/>
        public string ConnectionString { get; private set; }

        /// <inheritdoc />
        public string ConvertDBTypeCSharp(string originalType, bool canBeNull)
        {
            string nullIndicator = "";

            if (canBeNull)
            {
                nullIndicator = "?";
            }

            if (originalType.ToLower().Contains("number"))
            {
                return $"int{nullIndicator}";
            }
            else if (originalType.ToLower().Contains("timestamp") || originalType.ToLower().Contains("datetime")
                                                                  || originalType.ToLower().Contains("date"))
            {
                return $"DateTime{nullIndicator}";
            }
            else if (originalType.ToLower().Contains("text") || originalType.ToLower().Contains("varchar")
                                                             || originalType.ToLower().Contains("char"))
            {
                return "string";
            }
            else if (originalType.ToLower().Contains("decimal"))
            {
                return $"decimal{nullIndicator}";
            }
            else if (originalType.ToLower().Contains("double"))
            {
                return $"double{nullIndicator}";
            }
            else if (originalType.ToLower().Contains("clob"))
            {
                return "string";
            }

            return "UNDEFINED";
        }

        /// <inheritdoc />
        public Table DescribeTable(string tableName)
        {
            List<Column> columns = new List<Column>();

            DataTable dt = this.GetData($@"select user_tab_columns.*, CASE WHEN COLUMN_NAME in (SELECT column_name FROM all_cons_columns WHERE constraint_name = (
                  SELECT constraint_name FROM user_constraints
                  WHERE UPPER(table_name) = UPPER('{tableName}') AND CONSTRAINT_TYPE = 'P'
                )) THEN 'PRIMARY_KEY' ELSE '' END AS KEY
                  from user_tab_columns
                 where UPPER(table_name) =  UPPER('{tableName}')
                 order by column_id");

            foreach (DataRow row in dt.Rows)
            {
                columns.Add(
                    new Column()
                    {
                        Name = row["COLUMN_NAME"].ToString(),
                        DeFaultValue = row["DATA_DEFAULT"].ToString(),
                        Type = row["DATA_TYPE"].ToString(),
                        MaxLength = (row["CHAR_LENGTH"] != null && !string.IsNullOrEmpty(row["CHAR_LENGTH"].ToString()) && row["CHAR_LENGTH"].ToString() != "0") ? row["CHAR_LENGTH"].ToString() : string.Empty,
                        CanBeNull = (row["NULLABLE"].ToString().ToUpper() == "Y"),
                        IsKey = (row["KEY"] != null && !string.IsNullOrEmpty(row["KEY"].ToString()) && row["KEY"].ToString().ToUpper() == "PRIMARY_KEY"),
                        AutoIncrement = (row["IDENTITY_COLUMN"] != null && !string.IsNullOrEmpty(row["IDENTITY_COLUMN"].ToString())
                                                                  && row["IDENTITY_COLUMN"].ToString().ToLower().Contains("yes"))
                    });
            }

            return new Table(tableName, columns);
        }

        /// <inheritdoc />
        public DataTable GetData(string sql)
        {
            DataTable results = new DataTable();
            using (OracleConnection oracleConnection = new OracleConnection(this.ConnectionString))
            {
                oracleConnection.Open();
                using (OracleCommand oracleCmd = new OracleCommand(sql, oracleConnection))
                {
                    oracleCmd.CommandTimeout = 180; // timeout after 180 seconds, 0 is never timeout
                    oracleCmd.CommandType = CommandType.Text;
                    using (OracleDataAdapter oAdapter = new OracleDataAdapter())
                    {
                        oAdapter.SelectCommand = oracleCmd;
                        oAdapter.Fill(results);
                    }
                }

                oracleConnection.Close();
            }

            return results;
        }

        /// <inheritdoc />
        public List<Table> ListAllTables()
        {
            List<Table> tables = new List<Table>();

            DataTable dt = this.GetData("SELECT table_name FROM user_tables order by table_name");

            foreach (DataRow row in dt.Rows)
            {
                tables.Add(new Table(row[0].ToString()));
            }

            return tables;
        }
    }
}