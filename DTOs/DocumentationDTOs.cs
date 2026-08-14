namespace ChatSystem.DTOs.Documentation;
public enum DocumentTarget
{
    User,
    Message,
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