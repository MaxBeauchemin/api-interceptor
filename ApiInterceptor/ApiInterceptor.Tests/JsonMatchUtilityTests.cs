using Maxbeauchemin.Api.Interceptor.Utilities;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ApiInterceptor.Tests;

public class JsonMatchUtilityTests
{
    [Fact]
    public void MatchesJson_BasicArrayTest()
    {
        var arr = new List<string>
        {
            "this",
            "is",
            "a",
            "test"
        };

        var node = JsonNode.Parse(JsonSerializer.Serialize(arr));

        Assert.NotNull(node);

        var res = JsonMatchUtility.MatchesJson(node, "$[3]", [ "test" ]);

        Assert.True(res);
    }

    [Fact]
    public void MatchesJson_BasicIntArrayTest()
    {
        var arr = new List<int>
        {
            0, 1, 2, 3
        };

        var node = JsonNode.Parse(JsonSerializer.Serialize(arr));

        Assert.NotNull(node);

        var res = JsonMatchUtility.MatchesJson(node, "$[*]", [ "3" ]);

        Assert.True(res);
    }

    [Fact]
    public void MatchesJson_BasicDoubleArrayTest()
    {
        var arr = new List<double>
        {
            0.3, 0.4, 0.5
        };

        var node = JsonNode.Parse(JsonSerializer.Serialize(arr));

        Assert.NotNull(node);

        var res = JsonMatchUtility.MatchesJson(node, "$[*]", [ "0.4" ]);

        Assert.True(res);
    }

    [Fact]
    public void MatchesJson_NestedObjectPropertyTest()
    {
        var arr = new
        {
            X = new
            {
                Y = new
                {
                    Z = false
                }
            }
        };

        var node = JsonNode.Parse(JsonSerializer.Serialize(arr));

        Assert.NotNull(node);

        var res = JsonMatchUtility.MatchesJson(node, "$.X.Y.Z", [ "false" ]);

        Assert.True(res);
    }

    [Fact]
    public void MatchesJson_NestedArrayPartialTest()
    {
        var arr = new List<List<int>>
        {
            new List<int>
            {
                1, 2, 3
            },
            new List<int>
            {
                4, 5, 6
            }
        };

        var node = JsonNode.Parse(JsonSerializer.Serialize(arr));

        Assert.NotNull(node);

        var res = JsonMatchUtility.MatchesJson(node, "$[*][*]", ["5"]);

        Assert.True(res);
    }
}