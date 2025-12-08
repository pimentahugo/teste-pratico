using Microsoft.EntityFrameworkCore;
using OrderTracking.Domain.Entities;

namespace OrderTracking.Infrastructure.Data.SqlServer;
public class OrderTrackingContext : DbContext
{
	public OrderTrackingContext(DbContextOptions options) : base(options)
	{
	}

	public DbSet<Order> Orders { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		DefaultColumnLength(modelBuilder);

		modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderTrackingContext).Assembly);
	}

	private void DefaultColumnLength(ModelBuilder builder)
	{
		foreach (var entityType in builder.Model.GetEntityTypes())
		{
			// Ignora tabelas do Identity
			var tableName = entityType.GetTableName();
			if (tableName?.StartsWith("AspNet", StringComparison.OrdinalIgnoreCase) == true)
				continue;

			foreach (var property in entityType.GetProperties())
			{
				// Apenas define um padrão onde não há configuração alguma
				if (property.ClrType == typeof(string)
					&& property.GetMaxLength() == null
					&& string.IsNullOrEmpty(property.GetColumnType()))
				{
					property.SetMaxLength(100);
				}
			}
		}
	}
}