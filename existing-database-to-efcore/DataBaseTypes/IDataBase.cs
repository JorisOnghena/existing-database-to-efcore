namespace existing_database_to_efcore.DataBaseGeneric
{
    using System.Collections.Generic;
    using System.Data;
    using DataBaseGeneric;

    public interface IDataBase
    {
        /// <summary>
        /// The connection string to use when connecting to the database.
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// List all tables in the database/scheme.
        /// </summary>
        /// <returns>A data set with a list of all tables found.</returns>
        List<Table> ListAllTables();

        /// <summary>
        /// Describe the columns, properties in the given table.
        /// </summary>
        /// <param name="tableName">The table to describe.</param>
        /// <returns>A data set with a description of the table.</returns>
        Table DescribeTable(string tableName);

        /// <summary>
        /// Retrieve a data set from a SQL query
        /// </summary>
        /// <param name="sql">The sql to execute</param>
        /// <returns>A data set with re query result</returns>
        DataTable GetData(string sql);

        /// <summary>
        /// Generate code from a table
        /// </summary>
        /// <param name="tableName">The table to generate code from</param>
        /// <param name="nameSpace">The namespace to use when generating a class</param>
        /// <returns>A class generated from the description</returns>
        string Generate(string tableName, string nameSpace = "MyNamespace");
    }
}
