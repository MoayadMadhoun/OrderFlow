using OrderFlow.Domain.Exceptions;

namespace OrderFlow.Domain.ValueObjects;

public sealed class Money
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = default!;

    private Money()
    {
    }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new DomainException("Amount cannot be negative.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency is required.");

        Amount = amount;
        Currency = currency;
    }
}