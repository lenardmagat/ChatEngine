using System.Text.Json.Serialization;
namespace ChatSystem.DTOs.Search;
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SearchTarget
{
    Users,
    Messages,
    Channels,
    Products
}

public record SearchRequest(
    string Term,
    SearchTarget Target,
    int Page = 1,
    int PageSize = 10,
    Dictionary<string, string>? Filters = null
);
public record UserSearchDTOResponse(
    string Id,
    string Name
);
public record ProductSearchDTOResponse(
    string Id,
    string ProductName,
    decimal BasePrice,
    int QuantityAvailable,
    string ProductStatus
);