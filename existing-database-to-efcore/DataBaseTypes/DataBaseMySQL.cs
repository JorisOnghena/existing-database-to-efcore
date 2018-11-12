namespace existing_database_to_efcore.DataBaseGeneric
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Xml;
    using MySql.Data.MySqlClient;
    using System.Data;

    public class DataBaseMySQL : IDataBase
    {
        /// <inheritdoc cref="ConnectionString"/>
        public string ConnectionString { get; private set; }

        public DataBaseMySQL(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        /// <inheritdoc cref="ListAllTables"/>
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

        /// <inheritdoc cref="DescribeTable"/>
        public Table DescribeTable(string tableName)
        {
            List<Column> columns = new List<Column>();

            DataTable dt = this.GetData("DESCRIBE " + tableName);

            foreach (DataRow row in dt.Rows)
            {
                columns.Add(new Column()
                {
                    Name = row["Field"].ToString(),
                    DeFaultValue = row["Default"].ToString(),
                    Type = row["Type"].ToString(),
                    CanBeNull = (row["Null"].ToString().ToUpper() == "YES"),
                    IsKey = (row["Key"] != null && !string.IsNullOrEmpty(row["Key"].ToString()) && row["Key"].ToString().ToUpper() == "PRI"),
                    //Extra = row["Extra"].ToString(),
                    AutoIncrement = (row["Extra"] != null && !string.IsNullOrEmpty(row["Extra"].ToString()) && row["Extra"].ToString().ToLower().Contains("auto_increment"))
                });
            }

            return new Table(tableName, columns);
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

        private string ToTitleCase(string toConvert)
        {
            return (System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(toConvert.ToLower().Trim().Replace("_", " "))).Trim().Replace(" ", "");
        }

        private string ConvertType(string originalType)
        {
            if (originalType.ToLower().Contains("int"))
            {
                return "int";
            }
            else if (originalType.ToLower().Contains("timestamp") || originalType.ToLower().Contains("datetime") || originalType.ToLower().Contains("date"))
            {
                return "DateTime";
            }
            else if (originalType.ToLower().Contains("text") || originalType.ToLower().Contains("varchar") || originalType.ToLower().Contains("char"))
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

        /// <inheritdoc cref="Generate"/>
        public string Generate(string tableName, string nameSpace = "MyNamespace")
        {
            StringBuilder sb = new StringBuilder();
            Table descriptionOfTable = this.DescribeTable(tableName);
            string tableNameTitleCase = this.ToTitleCase(tableName);

            sb.Append("namespace " + nameSpace);
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append("\tusing System;");
            sb.Append(Environment.NewLine);
            sb.Append("\tusing System.Collections.Generic;");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("\tpublic sealed class " + tableNameTitleCase);
            sb.Append(Environment.NewLine);
            sb.Append("\t{");
            sb.Append(Environment.NewLine);

            List<string> keyFields = new List<string>();
            List<string> fluentConfigurationFields = new List<string>();

            foreach (Column column in descriptionOfTable.Columns)
            {
                string columnNameTitleCase = this.ToTitleCase(column.Name);
                string columnTypeConverted = this.ConvertType(column.Type);

                // Is primary key then add to list of keys...
                if (column.IsKey)
                {
                    keyFields.Add(columnNameTitleCase);
                }

                // Create fluent notation 
                string fluentField = "builder.Property(b => b." + columnNameTitleCase
                        + ").HasColumnName(\"" + column.Name + "\").HasColumnType(\"" + column.Type + "\")";

                if (!string.IsNullOrEmpty(column.DeFaultValue))
                {
                    if (columnTypeConverted == "string")
                    {
                        fluentField += ".HasDefaultValue(\"" + column.DeFaultValue + "\")";
                    }
                    else
                    {
                        fluentField += ".HasDefaultValue(" + column.DeFaultValue + ")";
                    }
                }

                fluentField += ";";
                fluentConfigurationFields.Add(fluentField);

                // Add field as property
                sb.Append("\t\tpublic " + columnTypeConverted + " " + columnNameTitleCase + " { get; set; }");
                sb.Append(Environment.NewLine);
            }

            sb.Append("\t}");
            sb.Append(Environment.NewLine);
            sb.Append("}");

            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);

            sb.Append("namespace " + nameSpace + "Configuration");
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append("\tusing Microsoft.EntityFrameworkCore;");
            sb.Append(Environment.NewLine);
            sb.Append("\tusing Microsoft.EntityFrameworkCore.Metadata.Builders;");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("\tinternal sealed class " + tableNameTitleCase + "Configuration : IEntityTypeConfiguration<" + tableNameTitleCase + ">");
            sb.Append(Environment.NewLine);
            sb.Append("\t{");
            sb.Append(Environment.NewLine);

            sb.Append("\t\tpublic void Configure(EntityTypeBuilder<" + tableNameTitleCase + "> builder)");
            sb.Append(Environment.NewLine);
            sb.Append("\t\t{");
            sb.Append(Environment.NewLine);

            sb.Append("\t\tbuilder.ToTable(\"" + tableName + "\")");

            if (keyFields.Count == 1)
            {
                sb.Append(Environment.NewLine);
                sb.Append("\t\t\t\t.HasKey(b => b." + keyFields[0] + ")");
            }
            else if (keyFields.Count > 1)
            {
                sb.Append(Environment.NewLine);
                sb.Append("\t\t\t\t.HasKey(b => new {");
                for (int i = 0; i < keyFields.Count; i++)
                {
                    sb.Append("b." + keyFields[i]);
                    if (i < keyFields.Count - 1)
                    {
                        sb.Append(", ");
                    }
                }
                sb.Append("})");
            }

            sb.Append(";");

            foreach (var fluent in fluentConfigurationFields)
            {
                sb.Append("\t\t" + fluent);
                sb.Append(Environment.NewLine);
            }

            sb.Append(Environment.NewLine);
            sb.Append("\t\t}");
            sb.Append(Environment.NewLine);
            sb.Append("\t}");
            sb.Append(Environment.NewLine);
            sb.Append("}");
            return sb.ToString();
        }
    }
}
