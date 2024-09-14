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
}