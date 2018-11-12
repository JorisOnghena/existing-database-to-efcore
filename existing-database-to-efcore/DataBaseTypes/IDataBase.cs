namespace existing_database_to_efcore.DataBaseTypes
{
    using System.Collections.Generic;
    using System.Data;
    using existing_database_to_efcore.DataBaseGeneric;

    public interface IDataBase
    {
        /// <summary>
        /// The connection string to use when connecting to the database.
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// Convert a database type to a C# type.
        /// </summary>
        /// <param name="originalType">
        /// The original database type to convert.
        /// </param>
        /// <param name="canBeNull">
        /// Specifies if the column can be Null in the database.
        /// </param>
        /// <returns>
        /// The c# type representation.
        /// </returns>
        string ConvertDBTypeCSharp(string originalType, bool canBeNull);

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
        /// List all tables in the database/scheme.
        /// </summary>
        /// <returns>A data set with a list of all tables found.</returns>
        List<Table> ListAllTables();
    }
}