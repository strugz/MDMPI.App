using System.Text.Json;
using System.Text.Json.Serialization;

namespace MDMPI.App.Api.WebSockets;

public sealed class WebSocketEnvelope
{
    public string Message { get; set; } = "";

    public LocationUpdate LocationUpdate { get; set; } = new();

    public NotificationUpdate NotificationUpdate { get; set; } = new();
}

public sealed class LocationUpdate
{
    public string Type { get; set; } = "";

    public string RequestID { get; set; } = "";

    public string RiderId { get; set; } = "";

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public string Timestamp { get; set; } = "";

    public string Status { get; set; } = "";

    public string RiderInitial { get; set; } = "";

    public string ETA { get; set; } = "";

    public string Distance { get; set; } = "";

    public string Client { get; set; } = "";
}

public sealed class NotificationUpdate
{
    public string Title { get; set; } = "";

    public string Body { get; set; } = "";
}

public sealed class WebSocketMessageNormalizationResult
{
    private WebSocketMessageNormalizationResult(bool success, bool invalidJson, string? normalizedJson, string? messageType)
    {
        Success = success;
        InvalidJson = invalidJson;
        NormalizedJson = normalizedJson;
        MessageType = messageType;
    }

    public bool Success { get; }

    public bool InvalidJson { get; }

    public string? NormalizedJson { get; }

    public string? MessageType { get; }

    public static WebSocketMessageNormalizationResult Valid(string normalizedJson, string messageType)
    {
        return new WebSocketMessageNormalizationResult(true, false, normalizedJson, messageType);
    }

    public static WebSocketMessageNormalizationResult InvalidJsonMessage()
    {
        return new WebSocketMessageNormalizationResult(false, true, null, null);
    }

    public static WebSocketMessageNormalizationResult UnknownShape()
    {
        return new WebSocketMessageNormalizationResult(false, false, null, null);
    }
}

public static class WebSocketMessageNormalizer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = null,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    public static WebSocketMessageNormalizationResult Normalize(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
            {
                return WebSocketMessageNormalizationResult.UnknownShape();
            }

            var message = GetString(root, "Message");
            if (string.Equals(message, "Location", StringComparison.Ordinal))
            {
                var envelope = new WebSocketEnvelope
                {
                    Message = "Location",
                    LocationUpdate = root.TryGetProperty("LocationUpdate", out var locationElement)
                        ? ReadLocationUpdate(locationElement)
                        : new LocationUpdate(),
                    NotificationUpdate = root.TryGetProperty("NotificationUpdate", out var notificationElement)
                        ? ReadNotificationUpdate(notificationElement)
                        : new NotificationUpdate()
                };

                return WebSocketMessageNormalizationResult.Valid(Serialize(envelope), envelope.Message);
            }

            if (string.Equals(message, "Notification", StringComparison.Ordinal))
            {
                var envelope = new WebSocketEnvelope
                {
                    Message = "Notification",
                    LocationUpdate = root.TryGetProperty("LocationUpdate", out var locationElement)
                        ? ReadLocationUpdate(locationElement)
                        : new LocationUpdate(),
                    NotificationUpdate = root.TryGetProperty("NotificationUpdate", out var notificationElement)
                        ? ReadNotificationUpdate(notificationElement)
                        : new NotificationUpdate()
                };

                return WebSocketMessageNormalizationResult.Valid(Serialize(envelope), envelope.Message);
            }

            var type = GetString(root, "Type");
            if (string.Equals(type, "location_update", StringComparison.Ordinal))
            {
                var envelope = new WebSocketEnvelope
                {
                    Message = "Location",
                    LocationUpdate = ReadLocationUpdate(root),
                    NotificationUpdate = new NotificationUpdate()
                };

                return WebSocketMessageNormalizationResult.Valid(Serialize(envelope), envelope.Message);
            }

            return WebSocketMessageNormalizationResult.UnknownShape();
        }
        catch (JsonException)
        {
            return WebSocketMessageNormalizationResult.InvalidJsonMessage();
        }
    }

    private static string Serialize(WebSocketEnvelope envelope)
    {
        envelope.LocationUpdate = Normalize(envelope.LocationUpdate);
        envelope.NotificationUpdate = Normalize(envelope.NotificationUpdate);

        return JsonSerializer.Serialize(envelope, SerializerOptions);
    }

    private static LocationUpdate ReadLocationUpdate(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return new LocationUpdate();
        }

        return new LocationUpdate
        {
            Type = GetString(element, "Type"),
            RequestID = GetString(element, "RequestID"),
            RiderId = GetString(element, "RiderId"),
            Latitude = GetDouble(element, "Latitude"),
            Longitude = GetDouble(element, "Longitude"),
            Timestamp = GetString(element, "Timestamp"),
            Status = GetString(element, "Status"),
            RiderInitial = GetString(element, "RiderInitial"),
            ETA = GetString(element, "ETA"),
            Distance = GetString(element, "Distance"),
            Client = GetString(element, "Client")
        };
    }

    private static NotificationUpdate ReadNotificationUpdate(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return new NotificationUpdate();
        }

        return new NotificationUpdate
        {
            Title = GetString(element, "Title"),
            Body = GetString(element, "Body")
        };
    }

    private static LocationUpdate Normalize(LocationUpdate locationUpdate)
    {
        locationUpdate.Type ??= "";
        locationUpdate.RequestID ??= "";
        locationUpdate.RiderId ??= "";
        locationUpdate.Timestamp ??= "";
        locationUpdate.Status ??= "";
        locationUpdate.RiderInitial ??= "";
        locationUpdate.ETA ??= "";
        locationUpdate.Distance ??= "";
        locationUpdate.Client ??= "";

        return locationUpdate;
    }

    private static NotificationUpdate Normalize(NotificationUpdate notificationUpdate)
    {
        notificationUpdate.Title ??= "";
        notificationUpdate.Body ??= "";

        return notificationUpdate;
    }

    private static string GetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind == JsonValueKind.Null)
        {
            return "";
        }

        return property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? ""
            : property.ToString();
    }

    private static double GetDouble(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind == JsonValueKind.Null)
        {
            return 0;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value))
        {
            return value;
        }

        if (property.ValueKind == JsonValueKind.String && double.TryParse(property.GetString(), out var stringValue))
        {
            return stringValue;
        }

        return 0;
    }
}
