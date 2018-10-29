namespace existing_database_to_efcore
{
    using System.IO;
    using IniParser;
    using IniParser.Model;

    internal class Configuration
    {
        private string fileConfiguration = "Configuration.ini";

        private string name = "";
        public string Type { get; set; }
        public string DisplayName { get; set; }
        public string ConnectionString { get; set; }

        public Configuration(string name = "Connection")
        {
            this.name = name;
            var parser = new FileIniDataParser();
            IniData data;
            if (File.Exists(this.fileConfiguration))
            {
                data = parser.ReadFile(this.fileConfiguration);
                this.DisplayName = data[name]["DisplayName"];
                this.ConnectionString = data[name]["String"];
                this.Type = data[name]["Type"];
            }
        }

        public void Save()
        {
            var parser = new FileIniDataParser();
            IniData data;
            if (File.Exists(fileConfiguration))
            {
                data = parser.ReadFile(this.fileConfiguration);
                data[this.name]["DisplayName"] = this.DisplayName;
                data[this.name]["String"] = this.ConnectionString;
                data[this.name]["Type"] = this.Type;
            }
            else
            {
                data = new IniData();
                data[this.name]["DisplayName"] = this.DisplayName;
                data[this.name]["String"] = this.ConnectionString;
                data[this.name]["Type"] = this.Type;
            }
            parser.WriteFile("Configuration.ini", data);
        }
    }
}
