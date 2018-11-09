namespace existing_database_to_efcore
{
    using System.IO;
    using IniParser;
    using IniParser.Model;

    /// <summary>
    /// The configuration class used to read/write configuration files.
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// The configuration section name inside the file.
        /// </summary>
        private const string ConnectionSectionName = "Connection";

        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration"/> class.
        /// </summary>
        /// <param name="fileConfiguration">
        /// The configuration file to read.
        /// </param>
        public Configuration(string fileConfiguration)
        {
            this.FileConfiguration = fileConfiguration;
            var parser = new FileIniDataParser();
            if (File.Exists(this.FileConfiguration))
            {
                var data = parser.ReadFile(this.FileConfiguration);
                this.DisplayName = data[ConnectionSectionName]["DisplayName"];
                this.ConnectionString = data[ConnectionSectionName]["String"];
                this.Type = data[ConnectionSectionName]["Type"];
            }
        }

        /// <summary>
        /// Gets the configuration file to work on.
        /// </summary>
        public string FileConfiguration { get; }

        /// <summary>
        /// Gets or sets the connection string used to connect to the database.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the display name used for the connection.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the type of the database connection, determines the database driver to use.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Save configuration to file.
        /// </summary>
        public void Save()
        {
            var parser = new FileIniDataParser();
            IniData data;
            if (File.Exists(this.FileConfiguration))
            {
                data = parser.ReadFile(this.FileConfiguration);
                data[ConnectionSectionName]["DisplayName"] = this.DisplayName;
                data[ConnectionSectionName]["String"] = this.ConnectionString;
                data[ConnectionSectionName]["Type"] = this.Type;
            }
            else
            {
                data = new IniData();
                data[ConnectionSectionName]["DisplayName"] = this.DisplayName;
                data[ConnectionSectionName]["String"] = this.ConnectionString;
                data[ConnectionSectionName]["Type"] = this.Type;
            }

            parser.WriteFile(this.FileConfiguration, data);
        }
    }
}