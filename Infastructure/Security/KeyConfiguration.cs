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