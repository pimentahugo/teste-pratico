using FluentAssertions;
using OrderTracking.Application.UseCases.Order.Create;
using OrderTracking.Shared.Messages;
using OrderTracking.UnitTests.Builders;

namespace OrderTracking.UnitTests.Application;

public class CreateOrderRequestValidatorTests
{
	private readonly CreateOrderRequestValidator _validator;
	private readonly CreateOrderRequestFaker _requestFaker;

	public CreateOrderRequestValidatorTests()
	{
		_validator = new CreateOrderRequestValidator();
		_requestFaker = new CreateOrderRequestFaker();
	}

	[Fact]
	public async Task Validate_WithValidRequest_ShouldReturnValid()
	{
		var request = _requestFaker.Generate();

		var result = await _validator.ValidateAsync(request);

		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public async Task Validate_WithEmptyId_ShouldReturnInvalid()
	{
		var request = _requestFaker.WithEmptyId().Generate();

		var result = await _validator.ValidateAsync(request);

		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(e =>
			e.PropertyName == "Id" &&
			e.ErrorMessage.Contains(ValidationMessages.Pedido_IdObrigatorio)
		);
	}

	[Fact]
	public async Task Validate_WithEmptyCliente_ShouldReturnInvalid()
	{
		var request = _requestFaker.WithEmptyCliente().Generate();

		var result = await _validator.ValidateAsync(request);

		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e =>
			e.PropertyName == "Cliente" &&
			e.ErrorMessage.Contains(ValidationMessages.Pedido_ClienteObrigatorio)
		);
	}

	[Fact]
	public async Task Validate_WithNullCliente_ShouldReturnInvalid()
	{
		var request = _requestFaker.WithNullCliente().Generate();

		var result = await _validator.ValidateAsync(request);

		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e =>
			e.PropertyName == "Cliente" &&
			e.ErrorMessage.Contains(ValidationMessages.Pedido_ClienteObrigatorio)
		);
	}

	[Fact]
	public async Task Validate_WithWhitespaceCliente_ShouldReturnInvalid()
	{
		var request = _requestFaker.WithWhitespaceCliente().Generate();

		var result = await _validator.ValidateAsync(request);

		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e =>
			e.PropertyName == "Cliente" &&
			e.ErrorMessage.Contains(ValidationMessages.Pedido_ClienteObrigatorio)
		);
	}

	[Fact]
	public async Task Validate_WithLongCliente_ShouldReturnInvalid()
	{
		var request = _requestFaker.WithLongCliente().Generate();

		var result = await _validator.ValidateAsync(request);

		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e =>
			e.PropertyName == "Cliente" &&
			e.ErrorMessage.Contains(ValidationMessages.Pedido_ClienteMaxLength)
		);
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(0)]
	[InlineData(-50)]
	[InlineData(-100.50)]
	public async Task Validate_WithInvalidValor_ShouldReturnInvalid(decimal valorInvalid)
	{
		var request = _requestFaker.Generate();
		request = request with { Valor = valorInvalid };

		var result = await _validator.ValidateAsync(request);

		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e =>
			e.PropertyName == "Valor" &&
			e.ErrorMessage.Contains(ValidationMessages.Pedido_ValorInvalido)
		);
	}

	[Fact]
	public async Task Validate_WithMinDate_ShouldReturnInvalid()
	{
		var request = _requestFaker.WithMinDate().Generate();

		var result = await _validator.ValidateAsync(request);

		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e =>
			e.PropertyName == "DataPedido" &&
			e.ErrorMessage.Contains(ValidationMessages.Pedido_DataInvalida)
		);
	}
}