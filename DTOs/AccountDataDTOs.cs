using System.ComponentModel.DataAnnotations;

public record AccountCredentials(
    [Required]
    [StringLength(12, MinimumLength = 5, ErrorMessage = "Username must contain at least 5 letters with maximum of 12.")]
    string Username,
    [Required]
    string password
);
public record LoginResponseData(
    [Required]
    string JwtToken,
    [Required]
    DateTime timestamp
);