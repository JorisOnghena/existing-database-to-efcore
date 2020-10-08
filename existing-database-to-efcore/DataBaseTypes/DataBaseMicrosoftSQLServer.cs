namespace existing_database_to_efcore.DataBaseTypes
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using existing_database_to_efcore.DataBaseGeneric;

    public class DataBaseMicrosoftSQLServer : IDataBase
    {
        /// <inheritdoc />
        public DataBaseMicrosoftSQLServer(string connectionString)
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

            if (originalType.ToLower().Contains("int"))
            {
                return $"int{nullIndicator}";
            }
            else if (originalType.ToLower().Contains("time") || originalType.ToLower().Contains("datetime")
                                                                  || originalType.ToLower().Contains("date"))
            {
                return $"DateTime{nullIndicator}";
            }
            else if (originalType.ToLower().Contains("text") || originalType.ToLower().Contains("varchar")
                                                             || originalType.ToLower().Contains("nvarchar")
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
            else if (originalType.ToLower().Contains("bit"))
            {
                return $"bool{nullIndicator}";
            }

            return "UNDEFINED";
        }

        /// <inheritdoc />
        public Table DescribeTable(string tableName)
        {
            List<Column> columns = new List<Column>();

            DataTable dt = this.GetData($@"
                SELECT  c.TABLE_NAME, c.COLUMN_NAME,c.DATA_TYPE, c.COLUMN_DEFAULT, c.character_maximum_length, c.numeric_precision, c.is_nullable
                     ,CASE WHEN pk.COLUMN_NAME IS NOT NULL THEN 'PRIMARY KEY' ELSE '' END AS KeyType
                    ,CASE WHEN Ident.COLUMN_NAME IS NOT NULL THEN 'IDENTITY' ELSE '' END AS Ident
                FROM INFORMATION_SCHEMA.COLUMNS c
                LEFT JOIN (
                            SELECT ku.TABLE_CATALOG,ku.TABLE_SCHEMA,ku.TABLE_NAME,ku.COLUMN_NAME
                            FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc
                            INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS ku
                                ON tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
                                AND tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
                         )   pk
                ON  c.TABLE_CATALOG = pk.TABLE_CATALOG
                            AND c.TABLE_SCHEMA = pk.TABLE_SCHEMA
                            AND c.TABLE_NAME = pk.TABLE_NAME
                            AND c.COLUMN_NAME = pk.COLUMN_NAME
                LEFT JOIN (
                            select TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME
                              from INFORMATION_SCHEMA.COLUMNS
                              where COLUMNPROPERTY(object_id(TABLE_SCHEMA+'.'+TABLE_NAME), COLUMN_NAME, 'IsIdentity') = 1
                         )   Ident
                ON
                    c.TABLE_CATALOG = Ident.TABLE_CATALOG
                            AND c.TABLE_SCHEMA = Ident.TABLE_SCHEMA
                            AND c.TABLE_NAME = Ident.TABLE_NAME
                            AND c.COLUMN_NAME = Ident.COLUMN_NAME
                where c.TABLE_NAME = '{tableName}'
                ORDER BY c.TABLE_SCHEMA,c.TABLE_NAME, c.ORDINAL_POSITION
            ");

            foreach (DataRow row in dt.Rows)
            {
                Column col = new Column()
                {
                    Name = row["COLUMN_NAME"].ToString(),
                    DeFaultValue = row["COLUMN_DEFAULT"].ToString(),
                    Type = row["DATA_TYPE"].ToString(),
                    CanBeNull = (row["is_nullable"].ToString().ToUpper() == "YES"),
                    IsKey =
                         (row["KeyType"] != null && !string.IsNullOrEmpty(row["KeyType"].ToString())
                                                 && row["KeyType"].ToString().ToUpper() == "PRIMARY KEY"),
                    AutoIncrement = (row["Ident"] != null
                                      && !string.IsNullOrEmpty(row["Ident"].ToString())
                                      && row["Ident"].ToString().ToUpper() == "IDENTITY")
                };

                if (row["character_maximum_length"] != null && !string.IsNullOrEmpty(row["character_maximum_length"].ToString()))
                {
                    col.MaxLength = row["character_maximum_length"].ToString();
                    if (col.MaxLength == "-1")
                    {
                        col.MaxLength = "max";
                        col.Type += "(MAX)";
                    }
                }

                columns.Add(col);
            }

            return new Table(tableName, columns);
        }

        /// <inheritdoc />
        public DataTable GetData(string sql)
        {
            DataTable results = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(this.ConnectionString))
            {
                sqlCon.Open();
                using (SqlCommand sqlCmd = new SqlCommand(sql, sqlCon))
                {
                    sqlCmd.CommandTimeout = 180; // timeout after 180 seconds, 0 is never timeout
                    sqlCmd.CommandType = CommandType.Text;
                    using (SqlDataAdapter oAdapter = new SqlDataAdapter())
                    {
                        oAdapter.SelectCommand = sqlCmd;
                        oAdapter.Fill(results);
                    }
                }

                sqlCon.Close();
            }

            return results;
        }

        /// <inheritdoc />
        public List<Table> ListAllTables()
        {
            List<Table> tables = new List<Table>();

            DataTable dt = this.GetData("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' order by TABLE_NAME");

            foreach (DataRow row in dt.Rows)
            {
                tables.Add(new Table(row["TABLE_NAME"].ToString()));
            }

            return tables;
        }
    }
}
