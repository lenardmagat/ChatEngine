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
    Dictionary<string, string>? Filters = null
);


public record UserDto(
    string Username,
    string role,
    bool Status
);