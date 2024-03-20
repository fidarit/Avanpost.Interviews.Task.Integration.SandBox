using Avanpost.Interviews.Task.Integration.Data.Models.Models;

namespace Avanpost.Interviews.Task.Integration.SandBox.Connector
{
    internal static class BaseModelExtensions
    {
        public static bool? GetBoolOrNull(this IEnumerable<UserProperty> properties, string name)
        {
            var property = properties.FirstOrDefault(t => t.Name == name);

            if (bool.TryParse(property?.Value, out var result))
                return result;

            return null;
        }

        public static string? GetStringOrNull(this IEnumerable<UserProperty> properties, string name)
        {
            return properties.FirstOrDefault(t => t.Name == name)?.Value;
        }
    }
}
