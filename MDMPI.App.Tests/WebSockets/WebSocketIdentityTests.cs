using MDMPI.App.Api.WebSockets;

namespace MDMPI.App.Tests.WebSockets;

public sealed class WebSocketIdentityTests
{
    [Theory]
    [InlineData("phone-1")]
    [InlineData("rider_02")]
    [InlineData("app.v2:build-7@fleet")]
    public void SanitizeIdentity_ValidId_Preserved(string id)
    {
        Assert.Equal(id, WebSocketConnectionHandler.SanitizeIdentity(id, "anon"));
    }

    [Theory]
    [InlineData("bad\nid")]
    [InlineData("bad\rid")]
    [InlineData("has space")]
    [InlineData("quote\"inject")]
    [InlineData("curly{brace}")]
    public void SanitizeIdentity_DisallowedCharacters_FallsBack(string id)
    {
        Assert.Equal("anon", WebSocketConnectionHandler.SanitizeIdentity(id, "anon"));
    }

    [Fact]
    public void SanitizeIdentity_Overlong_FallsBack()
    {
        Assert.Equal("anon", WebSocketConnectionHandler.SanitizeIdentity(new string('a', 65), "anon"));
        Assert.Equal(new string('a', 64), WebSocketConnectionHandler.SanitizeIdentity(new string('a', 64), "anon"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SanitizeIdentity_EmptyOrWhitespace_FallsBack(string? id)
    {
        Assert.Equal("anon", WebSocketConnectionHandler.SanitizeIdentity(id, "anon"));
    }

    [Theory]
    [InlineData("rider", "rider")]
    [InlineData("watcher", "watcher")]
    [InlineData("admin", "unspecified")]
    [InlineData("RIDER", "unspecified")]
    [InlineData("", "unspecified")]
    [InlineData(null, "unspecified")]
    public void NormalizeRole_AllowlistsKnownRolesOnly(string? raw, string expected)
    {
        Assert.Equal(expected, WebSocketConnectionHandler.NormalizeRole(raw));
    }
}
