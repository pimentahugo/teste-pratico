using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderTracking.Domain.Entities;

namespace OrderTracking.Infrastructure.Data.SqlServer.Mapping;

public class OrderMapping : IEntityTypeConfiguration<Order>
{
	public void Configure(EntityTypeBuilder<Order> builder)
	{
		builder.HasKey(p => p.Id);

		builder.Property(p => p.Cliente)
			.IsRequired();

		builder.Property(p => p.Valor)
			.IsRequired()
			.HasPrecision(18, 2)
			.HasColumnType("decimal(18,2)");

		builder.Property(p => p.DataPedido)
			.IsRequired()
			.HasColumnType("datetime2");


		//Indexes
		builder.HasIndex(p => p.Cliente)
			.HasDatabaseName("IX_Pedidos_Cliente");

		builder.HasIndex(p => p.DataPedido)
			.HasDatabaseName("IX_Pedidos_DataPedido");

		builder.HasIndex(p => new { p.Cliente, p.DataPedido })
			.HasDatabaseName("IX_Pedidos_Cliente_DataPedido");
	}
}