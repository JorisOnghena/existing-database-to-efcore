namespace existing_database_to_efcore.DataBaseGeneric
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using existing_database_to_efcore.DataBaseTypes;

    /// <summary>
    /// The code generator used to generate code from a table.
    /// </summary>
    public static class CodeGenerator
    {
        private struct KeyField
        {
            public string Name;
            public bool AutoIncrement;
        }

        public static string GenerateCSharp(IDataBase dataBase, string tableName, string nameSpace = "MyNamespace")
        {
            List<string> specialDefaultFunctions = new List<string>()
            {
                "CURRENT_TIME", "CURRENT_TIMESTAMP", "CURRENT_DATE",
                "CURTIME", "CURDATE", "LOCALTIME", "LOCALTIMESTAMP"
            };

            StringBuilder sb = new StringBuilder();
            Table descriptionOfTable = dataBase.DescribeTable(tableName);
            string tableNameTitleCase = tableName.ToTitleCase();

            sb.Append($"namespace {nameSpace}");
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append("\tusing System;");
            sb.Append(Environment.NewLine);
            sb.Append("\tusing System.Collections.Generic;");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append($"\tpublic sealed class {tableNameTitleCase}");
            sb.Append(Environment.NewLine);
            sb.Append("\t{");
            sb.Append(Environment.NewLine);

            List<KeyField> keyFields = new List<KeyField>();
            List<string> fluentConfigurationFields = new List<string>();

            foreach (Column column in descriptionOfTable.Columns)
            {
                string columnNameTitleCase = column.Name.ToTitleCase();
                string columnTypeConverted = dataBase.ConvertDBTypeCSharp(column.Type);

                // Is primary key then add to list of keys...
                if (column.IsKey)
                {
                    keyFields.Add(new KeyField()
                    {
                        Name = columnNameTitleCase,
                        AutoIncrement = column.AutoIncrement
                    });
                }

                // Create fluent notation 
                string fluentField = $"builder.Property(b => b.{columnNameTitleCase})";
                fluentField += Environment.NewLine;
                fluentField += $"\t\t\t\t.HasColumnName(\"{column.Name}\")";
                fluentField += Environment.NewLine;
                fluentField += $"\t\t\t\t.HasColumnType(\"{column.Type}\")";

                if (!string.IsNullOrEmpty(column.DeFaultValue))
                {
                    if (columnTypeConverted == "string")
                    {
                        fluentField += Environment.NewLine;
                        fluentField += $"\t\t\t\t.HasDefaultValue(\"{column.DeFaultValue}\")";
                    }
                    else if (specialDefaultFunctions.Contains(column.DeFaultValue.ToUpper()))
                    {
                        fluentField += Environment.NewLine;
                        fluentField += $"\t\t\t\t.HasDefaultValueSql(\"{column.DeFaultValue}\")";
                    }
                    else
                    {
                        fluentField += Environment.NewLine;
                        fluentField += $"\t\t\t\t.HasDefaultValue({column.DeFaultValue})";
                    }
                }

                string maxLength = column.Type.ExtractMaxLength();
                if (!string.IsNullOrEmpty(maxLength))
                {
                    fluentField += Environment.NewLine;
                    fluentField += $"\t\t\t\t{maxLength}";
                }

                if (column.AutoIncrement)
                {
                    fluentField += Environment.NewLine;
                    fluentField += "\t\t\t\t.ValueGeneratedOnAdd()";
                }

                fluentField += ";";
                fluentField += Environment.NewLine;
                fluentConfigurationFields.Add(fluentField);

                // Add field as property
                sb.Append($"\t\tpublic {columnTypeConverted} {columnNameTitleCase} {{ get; set; }}");
                sb.Append(Environment.NewLine);
            }

            sb.Append("\t}");
            sb.Append(Environment.NewLine);
            sb.Append("}");

            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);

            sb.Append($"namespace {nameSpace}Configuration");
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append("\tusing Microsoft.EntityFrameworkCore;");
            sb.Append(Environment.NewLine);
            sb.Append("\tusing Microsoft.EntityFrameworkCore.Metadata.Builders;");
            sb.Append(Environment.NewLine);
            sb.Append("\tusing " + nameSpace + ";");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append($"\tinternal sealed class {tableNameTitleCase}Configuration : IEntityTypeConfiguration<{tableNameTitleCase}>");
            sb.Append(Environment.NewLine);
            sb.Append("\t{");
            sb.Append(Environment.NewLine);

            sb.Append($"\t\tpublic void Configure(EntityTypeBuilder<{tableNameTitleCase}> builder)");
            sb.Append(Environment.NewLine);
            sb.Append("\t\t{");
            sb.Append(Environment.NewLine);

            sb.Append($"\t\tbuilder.ToTable(\"{tableName}\")");

            if (keyFields.Count == 1)
            {
                sb.Append(Environment.NewLine);
                sb.Append($"\t\t\t\t.HasKey(b => b.{keyFields[0].Name})");
            }
            else if (keyFields.Count > 1)
            {
                sb.Append(Environment.NewLine);
                sb.Append("\t\t\t\t.HasKey(b => new {");
                for (int i = 0; i < keyFields.Count; i++)
                {
                    sb.Append($"b.{keyFields[i].Name}");
                    if (i < keyFields.Count - 1)
                    {
                        sb.Append(", ");
                    }
                }
                sb.Append("})");
            }

            sb.Append(";");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);

            /*foreach (var keyField in keyFields)
            {
                if (keyField.AutoIncrement)
                {
                    sb.Append("\t\tbuilder.Property(b => b." + keyField.Name + ").ValueGeneratedOnAdd();");
                    sb.Append(Environment.NewLine);
                }
            }*/

            foreach (var fluent in fluentConfigurationFields)
            {
                sb.Append($"\t\t{fluent}");
                sb.Append(Environment.NewLine);
            }

            sb.Append("\t\t}");
            sb.Append(Environment.NewLine);
            sb.Append("\t}");
            sb.Append(Environment.NewLine);
            sb.Append("}");
            return sb.ToString();
        }
    }
}
