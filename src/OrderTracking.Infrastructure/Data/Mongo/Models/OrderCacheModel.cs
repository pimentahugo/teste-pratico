using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OrderTracking.Infrastructure.Data.Mongo.Models;
public class OrderCacheModel
{
	[BsonId]
	[BsonRepresentation(BsonType.String)]
	public Guid Id { get; set; }

	[BsonElement("cliente")]
	public string Cliente { get; set; } = string.Empty;

	[BsonElement("valor")]
	[BsonRepresentation(BsonType.Decimal128)]
	public decimal Valor { get; set; }

	[BsonElement("dataPedido")]
	[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
	public DateTime DataPedido { get; set; }

	[BsonElement("dataCriacao")]
	[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
	public DateTime DataCriacao { get; set; }

	[BsonElement("cachedAt")]
	[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
	public DateTime CachedAt { get; set; }

	[BsonElement("expiresAt")]
	[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
	public DateTime ExpiresAt { get; set; }
}