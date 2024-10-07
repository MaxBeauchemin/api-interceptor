using Json.Path;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Maxbeauchemin.Api.Interceptor.Utilities;

public static class JsonMatchUtility
{
    public static bool MatchesJson(JsonNode node, string path, List<string?> values)
    {
        var jsonPath = JsonPath.Parse(path);

        var results = jsonPath.Evaluate(node);

        if (results.Matches == null || results.Matches.Count == 0) return false;

        var nonNullValues = values.Where(v => v != null).Select(v => v.Trim().ToLower()).ToHashSet();
        var valuesIncludedNull = values.Count != nonNullValues.Count;

        foreach (var match in results.Matches)
        {
            if (match.Value == null) return valuesIncludedNull;

            switch (match.Value.GetValueKind())
            {
                case JsonValueKind.String:
                    {
                        if (nonNullValues.Contains(match.Value.GetValue<string>().Trim().ToLower())) return true;
                        break;
                    }
                case JsonValueKind.Number:
                    {
                        var doubleValStr = match.Value.GetValue<double>().ToString().Trim().ToLower();

                        if (nonNullValues.Contains(doubleValStr)) return true;
                        break;
                    }
                case JsonValueKind.False:
                    {
                        if (nonNullValues.Contains("false")) return true;
                        break;
                    }
                case JsonValueKind.True:
                    {
                        if (nonNullValues.Contains("true")) return true;
                        break;
                    }
            }
        }

        return false;
    }
}