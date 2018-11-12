namespace existing_database_to_efcore.DataBaseGeneric
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// A class for all extension methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Transforms the given string to title/camel case notation.
        /// </summary>
        /// <param name="toConvert">
        /// The string to convert.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> that has been transformed.
        /// </returns>
        public static string ToTitleCase(this string toConvert)
        {
            return (System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(
                           toConvert.ToLower().Trim().Replace("_", " "))).Trim().Replace(" ", "");
        }

        /// <summary>
        /// The extract max length from a given field type. 
        /// </summary>
        /// <example>
        /// "varchar (30)" => .HasMaxLength(30)
        /// "nvarchar (max)" => .HasMaxLength(null)
        /// </example>
        /// <param name="fieldType">
        /// The field type to extract the length from.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> with the max length definition.
        /// </returns>
        public static string ExtractMaxLength(this string fieldType)
        {
            Regex r1 = new Regex(@"n?varchar ?\(([\d]+|max)\)");
            Match match = r1.Match(fieldType.ToLower());
            if (match.Success)
            {
                string m = match.Groups[1].Value;
                if (m == "max")
                {
                    return ".HasMaxLength(null)";
                }
                else
                {
                    return $".HasMaxLength({m})";
                }
            }

            return string.Empty;
        }
    }
}