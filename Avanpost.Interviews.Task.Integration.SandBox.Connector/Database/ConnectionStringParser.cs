using System.Text.RegularExpressions;

namespace Avanpost.Interviews.Task.Integration.SandBox.Connector
{
    internal static partial class ConnectionStringExtractor
    {
        public static Dictionary<string, string> ParseConnectionString(string input)
        {
            var regex = GetRegex();
            
            return regex
                .Matches(input)
                .ToDictionary(t => t.Groups["key"].Value, t => t.Groups["value"].Value);
        }

        [GeneratedRegex("(?<key>\\w+)='(?<value>.*?)'")]
        private static partial Regex GetRegex();
    }
}
