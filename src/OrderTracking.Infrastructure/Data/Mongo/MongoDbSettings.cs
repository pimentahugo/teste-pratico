namespace OrderTracking.Infrastructure.Data.Mongo;
public class MongoDbSettings
{
	public string ConnectionString { get; set; } = string.Empty;
	public string DatabaseName { get; set; } = string.Empty;
	public string OrdersCollectionName { get; set; } = string.Empty;
	public int CacheExpirationMinutes { get; set; }
}