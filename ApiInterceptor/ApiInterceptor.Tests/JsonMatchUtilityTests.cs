using Maxbeauchemin.Api.Interceptor.Utilities;

namespace ApiInterceptor.Tests;

public class JsonMatchUtilityTests
{
    [Fact]
    public void BasicPropertyTest()
    {
        var path = "$.X";

        var token = JsonMatchUtility.TokenizePath(path);

        Assert.NotNull(token);
        Assert.Null(token.ChildToken);
        var propertyToken = Assert.IsType<JsonMatchUtility.PropertyToken>(token);
        Assert.Equal("X", propertyToken.PropertyName);
    }
    
    [Fact]
    public void NestedPropertyTest()
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
    public void NestedArrayTest()
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
    public void ComplexMixMatchedTest()
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
}