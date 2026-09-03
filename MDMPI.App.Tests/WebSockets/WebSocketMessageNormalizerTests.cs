using System.Globalization;
using System.Text.Json;
using MDMPI.App.Api.WebSockets;

namespace MDMPI.App.Tests.WebSockets;

public sealed class WebSocketMessageNormalizerTests
{
    [Fact]
    public void Normalize_WrappedLocation_PreservesCasingAndValues()
    {
        var json = """
        {
          "Message": "Location",
          "LocationUpdate": {
            "Type": "location_update",
            "RequestID": "REQ-001",
            "RiderId": "R-001",
            "Latitude": 14.5995,
            "Longitude": 120.9842,
            "Timestamp": "2026-05-26T10:00:00Z",
            "Status": "en_route",
            "RiderInitial": "JB",
            "ETA": "15 mins",
            "Distance": "3 km",
            "Client": "Client A"
          },
          "NotificationUpdate": {
            "Title": "",
            "Body": ""
          }
        }
        """;

        var result = WebSocketMessageNormalizer.Normalize(json);

        Assert.True(result.Success);
        using var document = JsonDocument.Parse(result.NormalizedJson!);
        var root = document.RootElement;
        Assert.Equal("Location", root.GetProperty("Message").GetString());
        Assert.Equal("REQ-001", root.GetProperty("LocationUpdate").GetProperty("RequestID").GetString());
        Assert.Equal("R-001", root.GetProperty("LocationUpdate").GetProperty("RiderId").GetString());
        Assert.True(root.GetProperty("LocationUpdate").TryGetProperty("RiderInitial", out _));
    }

    [Fact]
    public void Normalize_WrappedNotification_PreservesTitleAndBody()
    {
        var json = """
        {
          "Message": "Notification",
          "LocationUpdate": {},
          "NotificationUpdate": {
            "Title": "Delivery update",
            "Body": "Rider is nearby"
          }
        }
        """;

        var result = WebSocketMessageNormalizer.Normalize(json);

        Assert.True(result.Success);
        using var document = JsonDocument.Parse(result.NormalizedJson!);
        var notification = document.RootElement.GetProperty("NotificationUpdate");
        Assert.Equal("Notification", document.RootElement.GetProperty("Message").GetString());
        Assert.Equal("Delivery update", notification.GetProperty("Title").GetString());
        Assert.Equal("Rider is nearby", notification.GetProperty("Body").GetString());
    }

    [Fact]
    public void Normalize_DirectLocationUpdate_WrapsAsLocationMessage()
    {
        var json = """
        {
          "Type": "location_update",
          "RequestID": "REQ-002",
          "RiderId": "R-002",
          "Latitude": 14.5,
          "Longitude": 121.0
        }
        """;

        var result = WebSocketMessageNormalizer.Normalize(json);

        Assert.True(result.Success);
        using var document = JsonDocument.Parse(result.NormalizedJson!);
        var root = document.RootElement;
        Assert.Equal("Location", root.GetProperty("Message").GetString());
        Assert.Equal("location_update", root.GetProperty("LocationUpdate").GetProperty("Type").GetString());
        Assert.Equal("REQ-002", root.GetProperty("LocationUpdate").GetProperty("RequestID").GetString());
        Assert.Equal("", root.GetProperty("NotificationUpdate").GetProperty("Title").GetString());
    }

    [Fact]
    public void Normalize_MissingFields_DefaultsStringsAndNumbers()
    {
        var json = """
        {
          "Type": "location_update",
          "RequestID": "REQ-003"
        }
        """;

        var result = WebSocketMessageNormalizer.Normalize(json);

        Assert.True(result.Success);
        using var document = JsonDocument.Parse(result.NormalizedJson!);
        var location = document.RootElement.GetProperty("LocationUpdate");
        Assert.Equal("", location.GetProperty("RiderId").GetString());
        Assert.Equal(0, location.GetProperty("Latitude").GetDouble());
        Assert.Equal(0, location.GetProperty("Longitude").GetDouble());
        Assert.Equal("", location.GetProperty("ETA").GetString());
    }

    [Fact]
    public void Normalize_InvalidJson_ReturnsHandledFailure()
    {
        var result = WebSocketMessageNormalizer.Normalize("{ invalid json");

        Assert.False(result.Success);
        Assert.True(result.InvalidJson);
        Assert.Null(result.NormalizedJson);
    }

    [Fact]
    public void Normalize_StringCoordinates_ParsesInvariantOfServerCulture()
    {
        // Regression: GetDouble used culture-sensitive double.TryParse, so on a de-DE
        // server "14.5995" parsed as 145995 and on fr-FR it parsed as 0.
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            var json = """
            {
              "Type": "location_update",
              "RequestID": "REQ-004",
              "Latitude": "14.5995",
              "Longitude": "120.9842"
            }
            """;

            var result = WebSocketMessageNormalizer.Normalize(json);

            Assert.True(result.Success);
            using var document = JsonDocument.Parse(result.NormalizedJson!);
            var location = document.RootElement.GetProperty("LocationUpdate");
            Assert.Equal(14.5995, location.GetProperty("Latitude").GetDouble(), 4);
            Assert.Equal(120.9842, location.GetProperty("Longitude").GetDouble(), 4);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
