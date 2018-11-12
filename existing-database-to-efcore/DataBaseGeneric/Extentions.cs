namespace existing_database_to_efcore.DataBaseGeneric
{
    public static class Extentions
    {
        public static string ToTitleCase(this string toConvert)
        {
            return (System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(
                           toConvert.ToLower().Trim().Replace("_", " "))).Trim().Replace(" ", "");
        }
    }
}