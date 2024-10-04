using Maxbeauchemin.Api.Interceptor.Utilities;
using System.Text.Json;

namespace ApiInterceptor.Tests;

public class JsonMatchUtilityTests
{
    //[Fact]
    //public void Normalize()
    //{
    //    var obj = JsonMatchUtility.NormalizeObject(new List<string> { "A", "B" });

    //    var j = (JsonElement)obj;

    //    if (j.ValueKind == JsonValueKind.Array)
    //    {
    //        foreach (var i in j.EnumerateArray())
    //        {
    //            var ji = (JsonElement)i;
    //        }
    //    }

        
    //}



    [Fact]
    public void Tokenize_BasicPropertyTest()
    {
        var path = "$.X";

        var token = JsonMatchUtility.TokenizePath(path);

        Assert.NotNull(token);
        Assert.Null(token.ChildToken);
        var propertyToken = Assert.IsType<JsonMatchUtility.PropertyToken>(token);
        Assert.Equal("X", propertyToken.PropertyName);
    }
    
    [Fact]
    public void Tokenize_NestedPropertyTest()
    {
        var path = "$.X.Y.Z";

        var token = JsonMatchUtility.TokenizePath(path);
        
        Assert.NotNull(token);
        var propertyTokenX = Assert.IsType<JsonMatchUtility.PropertyToken>(token);
        Assert.Equal("X", propertyTokenX.PropertyName);
        var childTokenY = token.ChildToken;
        Assert.NotNull(childTokenY);
        var propertyTokenY = Assert.IsType<JsonMatchUtility.PropertyToken>(childTokenY);
        Assert.Equal("Y", propertyTokenY.PropertyName);
        var childTokenZ = childTokenY.ChildToken;
        Assert.NotNull(childTokenZ);
        Assert.Null(childTokenZ.ChildToken);
        var propertyTokenZ = Assert.IsType<JsonMatchUtility.PropertyToken>(childTokenZ);
        Assert.Equal("Z", propertyTokenZ.PropertyName);
    }
    
    [Fact]
    public void BasicArrayTest()
    {
        var path = "$[0]";

        var token = JsonMatchUtility.TokenizePath(path);

        Assert.NotNull(token);
        Assert.Null(token.ChildToken);
        var arrayToken = Assert.IsType<JsonMatchUtility.ArrayToken>(token);
        Assert.Equal(0, arrayToken.Position);
    }
    
    [Fact]
    public void Tokenize_NestedArrayTest()
    {
        var path = "$[0][1][*]";

        var token = JsonMatchUtility.TokenizePath(path);
        
        Assert.NotNull(token);
        var arrayTokenZero = Assert.IsType<JsonMatchUtility.ArrayToken>(token);
        Assert.Equal(0, arrayTokenZero.Position);
        var childTokenOne = token.ChildToken;
        Assert.NotNull(childTokenOne);
        var arrayTokenOne = Assert.IsType<JsonMatchUtility.ArrayToken>(childTokenOne);
        Assert.Equal(1, arrayTokenOne.Position);
        var childTokenWild = childTokenOne.ChildToken;
        Assert.NotNull(childTokenWild);
        Assert.Null(childTokenWild.ChildToken);
        var arrayTokenWild = Assert.IsType<JsonMatchUtility.ArrayToken>(childTokenWild);
        Assert.Null(arrayTokenWild.Position);
    }
    
    [Fact]
    public void Tokenize_ComplexMixMatchedTest()
    {
        var path = "$.X.Y[0][12].Items[*]";

        var token = JsonMatchUtility.TokenizePath(path);
        
        Assert.NotNull(token);
        var propertyTokenX = Assert.IsType<JsonMatchUtility.PropertyToken>(token);
        Assert.Equal("X", propertyTokenX.PropertyName);
        var childTokenY = token.ChildToken;
        Assert.NotNull(childTokenY);
        var propertyTokenY = Assert.IsType<JsonMatchUtility.PropertyToken>(childTokenY);
        Assert.Equal("Y", propertyTokenY.PropertyName);
        var childTokenZero = childTokenY.ChildToken;
        Assert.NotNull(childTokenZero);
        var arrayTokenZero = Assert.IsType<JsonMatchUtility.ArrayToken>(childTokenZero);
        Assert.Equal(0, arrayTokenZero.Position);
        var childTokenTwelve = childTokenZero.ChildToken;
        Assert.NotNull(childTokenTwelve);
        var arrayTokenTwelve = Assert.IsType<JsonMatchUtility.ArrayToken>(childTokenTwelve);
        Assert.Equal(12, arrayTokenTwelve.Position);
        var childTokenItems = childTokenTwelve.ChildToken;
        Assert.NotNull(childTokenItems);
        var propertyTokenItems = Assert.IsType<JsonMatchUtility.PropertyToken>(childTokenItems);
        Assert.Equal("Items", propertyTokenItems.PropertyName);
        var childTokenWild = childTokenItems.ChildToken;
        Assert.NotNull(childTokenWild);
        var arrayTokenWild = Assert.IsType<JsonMatchUtility.ArrayToken>(childTokenWild);
        Assert.Null(arrayTokenWild.Position);
        Assert.Null(childTokenWild.ChildToken);
    }

    [Fact]
    public void GetElementTokenValues_BasicPropertyTest()
    {
        var obj = new
        {
            X = "test"
        };

        var element = JsonSerializer.SerializeToElement(obj);

        var token = JsonMatchUtility.TokenizePath("$.X");

        var values = JsonMatchUtility.GetElementTokenValues(element, token);

        Assert.NotNull(values);
        Assert.Single(values);
        Assert.Equal(JsonValueKind.String, values.First().ValueKind);
        Assert.Equal("test", values.First().GetString());
    }

    [Fact]
    public void GetElementTokenValues_BasicArrayTest()
    {
        var arr = new List<string>
        {
            "this",
            "is",
            "a",
            "test"
        };

        var element = JsonSerializer.SerializeToElement(arr);

        var token = JsonMatchUtility.TokenizePath("$[3]");

        var values = JsonMatchUtility.GetElementTokenValues(element, token);

        Assert.NotNull(values);
        Assert.Single(values);
        Assert.Equal(JsonValueKind.String, values.First().ValueKind);
        Assert.Equal("test", values.First().GetString());
    }

    [Fact]
    public void GetElementTokenValues_WildcardArrayTest()
    {
        var arr = new List<string>
        {
            "test1",
            "test2",
            "test3"
        };

        var element = JsonSerializer.SerializeToElement(arr);

        var token = JsonMatchUtility.TokenizePath("$[*]");

        var values = JsonMatchUtility.GetElementTokenValues(element, token);

        Assert.NotNull(values);
        Assert.Equal(3, values.Count);
        Assert.Equal(JsonValueKind.String, values[0].ValueKind);
        Assert.Equal("test1", values[0].GetString());
        Assert.Equal(JsonValueKind.String, values[1].ValueKind);
        Assert.Equal("test2", values[1].GetString());
        Assert.Equal(JsonValueKind.String, values[2].ValueKind);
        Assert.Equal("test3", values[2].GetString());
    }

    [Fact]
    public void GetElementTokenValues_NestedPropertyTest()
    {
        var obj = new
        {
            X = new
            {
                Y = "test"
            }
        };

        var element = JsonSerializer.SerializeToElement(obj);

        var token = JsonMatchUtility.TokenizePath("$.X.Y");

        var values = JsonMatchUtility.GetElementTokenValues(element, token);

        Assert.NotNull(values);
        Assert.Single(values);
        Assert.Equal(JsonValueKind.String, values.First().ValueKind);
        Assert.Equal("test", values.First().GetString());
    }

    [Fact]
    public void GetElementTokenValues_NestedArrayTest()
    {
        var arr = new List<List<string>>
        {
            new List<string>
            {
                "test"
            }
        };

        var element = JsonSerializer.SerializeToElement(arr);

        var token = JsonMatchUtility.TokenizePath("$[0][0]");

        var values = JsonMatchUtility.GetElementTokenValues(element, token);

        Assert.NotNull(values);
        Assert.Single(values);
        Assert.Equal(JsonValueKind.String, values.First().ValueKind);
        Assert.Equal("test", values.First().GetString());
    }

    [Fact]
    public void GetElementTokenValues_NestedWildcardArrayTest()
    {
        var arr = new List<List<string>>
        {
            new List<string>
            {
                "test1",
                "test2",
                "test3"
            },
            new List<string>
            {
                "testA",
                "testB",
                "testC"
            }
        };

        var element = JsonSerializer.SerializeToElement(arr);

        var token = JsonMatchUtility.TokenizePath("$[*][*]");

        var values = JsonMatchUtility.GetElementTokenValues(element, token);

        Assert.NotNull(values);
        Assert.Equal(6, values.Count);
        Assert.Equal(JsonValueKind.String, values[0].ValueKind);
        Assert.Equal("test1", values[0].GetString());
        Assert.Equal(JsonValueKind.String, values[1].ValueKind);
        Assert.Equal("test2", values[1].GetString());
        Assert.Equal(JsonValueKind.String, values[2].ValueKind);
        Assert.Equal("test3", values[2].GetString());
        Assert.Equal(JsonValueKind.String, values[3].ValueKind);
        Assert.Equal("testA", values[3].GetString());
        Assert.Equal(JsonValueKind.String, values[4].ValueKind);
        Assert.Equal("testB", values[4].GetString());
        Assert.Equal(JsonValueKind.String, values[5].ValueKind);
        Assert.Equal("testC", values[5].GetString());
    }

    [Fact]
    public void GetElementTokenValues_ComplexMixedTest()
    {
        var obj = new
        {
            X = new
            {
                Y = new List<object>
                {
                    new
                    {
                        Z = "string"
                    },
                    new
                    {
                        A = 12
                    },
                    new List<object>
                    {
                        new
                        {
                            B = false
                        }
                    }
                }
            }
        };

        var element = JsonSerializer.SerializeToElement(obj);

        var token = JsonMatchUtility.TokenizePath("$.X.Y[*][*].B");

        var values = JsonMatchUtility.GetElementTokenValues(element, token);

        Assert.NotNull(values);
        Assert.Single(values);
        Assert.Equal(JsonValueKind.False, values.First().ValueKind);
    }

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

        var element = JsonSerializer.SerializeToElement(arr);

        var res = JsonMatchUtility.MatchesJson(element, "$[3]", [ "test" ]);

        Assert.True(res);
    }

    [Fact]
    public void MatchesJson_BasicIntArrayTest()
    {
        var arr = new List<int>
        {
            0, 1, 2, 3
        };

        var element = JsonSerializer.SerializeToElement(arr);

        var res = JsonMatchUtility.MatchesJson(element, "$[*]", [ "3" ]);

        Assert.True(res);
    }

    [Fact]
    public void MatchesJson_BasicDoubleArrayTest()
    {
        var arr = new List<double>
        {
            0.3, 0.4, 0.5
        };

        var element = JsonSerializer.SerializeToElement(arr);

        var res = JsonMatchUtility.MatchesJson(element, "$[*]", [ "0.4" ]);

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

        var element = JsonSerializer.SerializeToElement(arr);

        var res = JsonMatchUtility.MatchesJson(element, "$.X.Y.Z", [ "false" ]);

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

        var element = JsonSerializer.SerializeToElement(arr);

        var res = JsonMatchUtility.MatchesJson(element, "$[*][*]", ["5"]);

        Assert.True(res);
    }
}