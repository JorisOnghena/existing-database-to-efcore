using MySql.Data.MySqlClient;
using System.Data;

namespace existing_database_to_efcore
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Xml;

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
            DataTable descriptionOfTable = this.DescribeTable(tableName);
            string field, type, extraProp, defaultValue;
            bool canBeNull, isKey, isAutoIncrement;

            sb.Append("namespace " + nameSpace);
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append("\tusing System;");
            sb.Append(Environment.NewLine);
            sb.Append("\tusing System.Collections.Generic;");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("\tpublic sealed class " + this.ToTitleCase(tableName));
            sb.Append(Environment.NewLine);
            sb.Append("\t{");
            sb.Append(Environment.NewLine);

            List<string> keyFields = new List<string>();
            List<string> fluentConfigurationFields = new List<string>();

            foreach (DataRow row in descriptionOfTable.Rows)
            {
                field = row["Field"].ToString();
                defaultValue = row["Default"].ToString();
                type = row["Type"].ToString();
                canBeNull = (row["Null"].ToString().ToUpper() == "YES");
                isKey = (row["Key"] != null && !string.IsNullOrEmpty(row["Key"].ToString())
                                            && row["Key"].ToString().ToUpper() == "PRI");
                extraProp = row["Extra"].ToString();
                isAutoIncrement = (row["Extra"] != null && !string.IsNullOrEmpty(row["Extra"].ToString())
                                                        && row["Extra"].ToString().ToLower().Contains("auto_increment"));

                // Is primary key then add to list of keys...
                if (isKey)
                {
                    keyFields.Add(this.ToTitleCase(field));
                }

                // Create fluent notation 
                string fluentField = "builder.Property(b => b." + this.ToTitleCase(field)
                        + ").HasColumnName(\"" + field + "\").HasColumnType(\"" + type + "\")";

                if (!string.IsNullOrEmpty(defaultValue))
                {
                    if (this.ConvertType(type) == "string")
                    {
                        fluentField += ".HasDefaultValue(\"" + defaultValue + "\")";
                    }
                    else
                    {
                        fluentField += ".HasDefaultValue(" + defaultValue + ")";
                    }
                }

                fluentField += ";";
                fluentConfigurationFields.Add(fluentField);
    
                // Add field as property
                sb.Append("\t\tpublic " + this.ConvertType(type) + " " + this.ToTitleCase(field) + " { get; set; }");
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
            sb.Append("\tMicrosoft.EntityFrameworkCore;");
            sb.Append(Environment.NewLine);
            sb.Append("\tMicrosoft.EntityFrameworkCore.Metadata.Builders;");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("\tinternal sealed class " + this.ToTitleCase(tableName) + "Configuration : IEntityTypeConfiguration<" + this.ToTitleCase(tableName) + ">");
            sb.Append(Environment.NewLine);
            sb.Append("\t{");
            sb.Append(Environment.NewLine);

            sb.Append("\t\tpublic void Configure(EntityTypeBuilder<" + this.ToTitleCase(tableName) + "> builder)");
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
