namespace ChatSystem.DTOs.Documentation;
public enum DocumentTarget
{
    User,
    Message,
    Product
}
public record DocumentRequest(
    string DocumentId,
    DocumentTarget Target
);
public record UserDocumentation(
    string id,
    string Username,
    string role,
    bool Status
);

public record ProductDocumentation(
    string id,
    string ProductName,
    string? ProductDescription,
    string ProductStatus
);