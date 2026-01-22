using System.Text;

namespace MDMPI.App.Common.Utilities
{
    public static class TextHelpers
    {
        /// <summary>
        /// Converts text to "Proper Case" similar to the SQL dbo.ProperCase function in the repo.
        /// Behaves like the SQL implementation: returns null when input is null, lowers the whole string
        /// then uppercases the first character and any character that follows a space (' ').
        /// </summary>
        public static string? ProperCase(string? text)
        {
            if (text is null)
                return null;

            var lowered = text.ToLowerInvariant();
            var sb = new StringBuilder(lowered);

            if (sb.Length == 0)
                return sb.ToString();

            // Uppercase first character
            sb[0] = char.ToUpper(sb[0]);

            // Uppercase any character that follows a space (same behavior as the SQL function)
            for (int i = 1; i < sb.Length; i++)
            {
                if (sb[i - 1] == ' ')
                    sb[i] = char.ToUpper(sb[i]);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Extension helper for convenience: myString.ToProperCase()
        /// </summary>
        public static string? ToProperCase(this string? text) => ProperCase(text);
    }
}
