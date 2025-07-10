namespace Tournament.Core.Configuration;
public class JwtConfiguration
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string Key { get; set; }
    public int ExpirationsMinutes { get; set; }
}
