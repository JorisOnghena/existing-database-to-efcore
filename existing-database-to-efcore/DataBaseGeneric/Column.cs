namespace existing_database_to_efcore.DataBaseGeneric
{
    /// <summary>
    /// A class to describe a generic column of a database table
    /// </summary>
    public class Column
    {
        public bool AutoIncrement { get; set; }

        public bool CanBeNull { get; set; }

        public string DeFaultValue { get; set; }

        public bool IsKey { get; set; }

        public string Name { get; set; }

        public string NameOverride { get; set; }

        public string Type { get; set; }
    }
}