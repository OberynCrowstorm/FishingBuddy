using System.Text.RegularExpressions;

namespace Oberyn.AnglerAssociate.Services
{
    // Turns enum.ToString()'s raw PascalCase ("OpenWater", "OffshoreFish") into
    // readable display text ("Open Water", "Offshore Fish"). One shared formatter
    // rather than duplicating the same regex everywhere a hole/bait/region name
    // gets shown.
    public static class EnumDisplay
    {
        // Small connector words that read oddly capitalized mid-name -
        // Region.HornOfMaguuma would otherwise split to "Horn Of Maguuma".
        private static readonly string[] LowercaseWords = { "Of", "And", "The" };

        public static string Format(object enumValue)
        {
            string raw = enumValue.ToString();
            string spaced = Regex.Replace(raw, "(?<=[a-z])(?=[A-Z])", " ");

            foreach (var word in LowercaseWords)
                spaced = Regex.Replace(spaced, $@"\b{word}\b", word.ToLowerInvariant());

            return spaced;
        }
    }
}