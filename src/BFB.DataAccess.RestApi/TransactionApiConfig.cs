namespace BFB.DataAccess.RestApi;

public class TransactionApiConfig
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int CacheExpirationMinutes { get; set; } = 5;
}