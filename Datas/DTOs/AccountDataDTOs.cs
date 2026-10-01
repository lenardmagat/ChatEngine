using System.ComponentModel.DataAnnotations;

public record AccountCredentials(
    [Required]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "Username must contain at least 5 letters with maximum of 20.")]
    string Username,
    [Required]
    string password,
    string? FileName,
    IFormFile? ProfilePicture
);
public record LoginResponseData(
    [Required]
    string JwtToken,
    [Required]
    DateTime timestamp
);

public record PasswordCredentials(
    [Required]
    string OldPassword,

    [Required]
    string NewPassword
);