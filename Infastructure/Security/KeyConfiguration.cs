namespace ChatSystem.core.KeyConfiguration;
using System.ComponentModel.DataAnnotations;
public class HashidsSettings
{
    [Required(AllowEmptyStrings = false)]
    public string HasherSalt { get; set; } = string.Empty;
    public int MinHashLength { get; set; } = 12;
}
public class JwtSettings
{
    [Required(AllowEmptyStrings = false)]
    public string Key {get; set;} = string.Empty;
    [Required(AllowEmptyStrings = false)]
    public string Issuer {get; set;} = string.Empty;
    [Required(AllowEmptyStrings = false)]
    public string Audience {get; set;} = string.Empty;
}

public class MeiliSearchSettings
{
    [Required(ErrorMessage = "Meilisearch Host Url is required")]
    [Url(ErrorMessage = "Meilisearch Host must be a valid URL")]
    public string Url { get; set; } = string.Empty;

    [Required(ErrorMessage = "Meilisearch ApiKey is required")]
    public string MasterKey { get; set; } = string.Empty;
}