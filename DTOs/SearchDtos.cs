using ChatSystem.Models;

namespace ChatSystem.DTOs.Search;
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
    Dictionary<string, string>? Filters = null // e.g. {"role": "Admin", "minPrice": "100"}
);

public record UserDto(
    string Username,
    string role,
    bool Status
);