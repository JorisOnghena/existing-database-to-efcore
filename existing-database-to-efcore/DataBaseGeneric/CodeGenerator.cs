namespace existing_database_to_efcore.DataBaseGeneric
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using existing_database_to_efcore.DataBaseTypes;

    /// <summary>
    /// The code generator used to generate code from a table.
    /// </summary>
    public static class CodeGenerator
    {
        /// <summary>
        /// The generate c# classes.
        /// </summary>
        /// <param name="settings">
        /// The <see cref="CodeGenerator.Settings"/> to use when generating code.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> with the generated code.
        /// </returns>
        public static string GenerateCSharp(Settings settings)
        {
            List<string> specialDefaultFunctions = new List<string>()
            {
                "CURRENT_TIME",
                "CURRENT_TIMESTAMP",
                "CURRENT_DATE",
                "CURTIME",
                "CURDATE",
                "LOCALTIME",
                "LOCALTIMESTAMP"
            };

            StringBuilder sb = new StringBuilder();
            Table descriptionOfTable = settings.DataBase.DescribeTable(settings.TableName);
            string tableNameTitleCase = settings.TableName.ToTitleCase();

            sb.Append($"namespace {settings.Namespace}");
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append("\tusing System;");
            sb.Append(Environment.NewLine);
            sb.Append("\tusing System.Collections.Generic;");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append($"\tpublic {(settings.SealedClasses ? "sealed " : string.Empty)}class {tableNameTitleCase}");
            sb.Append(Environment.NewLine);
            sb.Append("\t{");
            sb.Append(Environment.NewLine);

            List<KeyField> keyFields = new List<KeyField>();
            string constructor = $"\t\tpublic {tableNameTitleCase} (";
            string constructorValue = $"\t\t{{{Environment.NewLine}";
            List<string> fluentConfigurationFields = new List<string>();

            foreach (Column column in descriptionOfTable.Columns)
            {
                string columnNameTitleCase = column.Name.ToTitleCase();
                string columnTypeConverted = settings.DataBase.ConvertDBTypeCSharp(column.Type, column.CanBeNull);

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

                if (!string.IsNullOrEmpty(column.MaxLength))
                {
                    if (column.MaxLength.ToLower() != "max")
                    {
                        fluentField += Environment.NewLine;
                        fluentField += $"\t\t\t\t.HasMaxLength({column.MaxLength})";
                    }
                }
                else
                {
                    // If we didn't get a MaxLength, try to determine it from the type.
                    string maxLength = column.Type.ExtractMaxLengthAsFluent();
                    if (!string.IsNullOrEmpty(maxLength))
                    {
                        fluentField += Environment.NewLine;
                        fluentField += $"\t\t\t\t{maxLength}";
                    }
                }

                if (!column.CanBeNull)
                {
                    fluentField += Environment.NewLine;
                    fluentField += "\t\t\t\t.IsRequired()";
                    constructor += $"{columnTypeConverted} {columnNameTitleCase.FirstCharacterToLower()}, ";
                    constructorValue +=
                        $"\t\t\tthis.{columnNameTitleCase} = {columnNameTitleCase.FirstCharacterToLower()};{Environment.NewLine}";
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

            if (settings.Constructor)
            {
                sb.Append(Environment.NewLine);
                constructor = constructor.TrimEnd().TrimEnd(',') + ")" + Environment.NewLine;
                sb.Append(constructor);

                constructorValue = constructorValue + "\t\t}" + Environment.NewLine;
                sb.Append(constructorValue);
            }

            sb.Append("\t}");
            sb.Append(Environment.NewLine);
            sb.Append("}");

            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);

            sb.Append($"namespace {settings.Namespace}Configuration");
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append("\tusing Microsoft.EntityFrameworkCore;");
            sb.Append(Environment.NewLine);
            sb.Append("\tusing Microsoft.EntityFrameworkCore.Metadata.Builders;");
            sb.Append(Environment.NewLine);
            sb.Append("\tusing " + settings.Namespace + ";");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append(
                $"\tinternal {(settings.SealedClasses ? "sealed " : string.Empty)}class {tableNameTitleCase}Configuration : IEntityTypeConfiguration<{tableNameTitleCase}>");
            sb.Append(Environment.NewLine);
            sb.Append("\t{");
            sb.Append(Environment.NewLine);

            sb.Append($"\t\tpublic void Configure(EntityTypeBuilder<{tableNameTitleCase}> builder)");
            sb.Append(Environment.NewLine);
            sb.Append("\t\t{");
            sb.Append(Environment.NewLine);

            sb.Append($"\t\t\tbuilder.ToTable(\"{settings.TableName}\")");

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
                sb.Append($"\t\t\t{fluent}");
                sb.Append(Environment.NewLine);
            }

            sb.Append("\t\t}");
            sb.Append(Environment.NewLine);
            sb.Append("\t}");
            sb.Append(Environment.NewLine);
            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// The key field class to store the primary keys of the table during code generation.
        /// </summary>
        private struct KeyField
        {
            /// <summary>
            /// The name of the field.
            /// </summary>
            public string Name;

            /// <summary>
            /// Is the field an identity or auto increment column.
            /// </summary>
            public bool AutoIncrement;
        }

        /// <summary>
        /// The settings class used by the code generator.
        /// </summary>
        public class Settings
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Settings"/> class.
            /// </summary>
            /// <param name="dataBase">
            /// The data base.
            /// </param>
            /// <param name="tableName">
            /// The table name.
            /// </param>
            public Settings(IDataBase dataBase, string tableName)
            {
                this.DataBase = dataBase;
                this.TableName = tableName;
                this.Namespace = "MyNamespace";
                this.SealedClasses = true;
                this.Constructor = true;
                this.TestClasses = true;
            }

            /// <summary>
            /// Gets or sets a value indicating whether to generate a constructor.
            /// </summary>
            public bool Constructor { get; set; }

            /// <summary>
            /// Gets the database to use during code generation.
            /// </summary>
            public IDataBase DataBase { get; private set; }

            /// <summary>
            /// Gets or sets the namespace.
            /// </summary>
            public string Namespace { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether to generate sealed classes.
            /// </summary>
            public bool SealedClasses { get; set; }

            /// <summary>
            /// Gets the table name to generate code from.
            /// </summary>
            public string TableName { get; private set; }

            /// <summary>
            /// Gets or sets a value indicating whether to generate test classes.
            /// </summary>
            public bool TestClasses { get; set; }
        }
    }
}