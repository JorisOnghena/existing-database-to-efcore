namespace existing_database_to_efcore.DataBaseGeneric
{
    using System.Collections.Generic;

    /// <summary>
    /// This class describes a table in the database.
    /// </summary>
    public class Table
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Table"/> class.
        /// </summary>
        /// <param name="name">
        /// The name of the table.
        /// </param>
        /// <param name="columns">
        /// The columns of the table.
        /// </param>
        public Table(string name, List<Column> columns) : this(name)
        {
            this.Columns = columns;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Table"/> class.
        /// </summary>
        /// <param name="name">
        /// The name of the table.
        /// </param>
        public Table(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets or sets the columns of the table.
        /// </summary>
        public List<Column> Columns { get; }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the name to use instead of the real table when code generation is run.
        /// </summary>
        public string NameOverride { get; set; }
    }
}