using OrderFlow.Domain.Common;
using OrderFlow.Domain.Exceptions;

namespace OrderFlow.Domain.Entities;

public class Customer : Entity
{
    public string Name { get; private set; } = default!;
    public string Phone { get; private set; } = default!;

    private Customer()
    {
    }

    public Customer(string name, string phone)
    {
        ChangeName(name);
        ChangePhone(phone);
    }

    public void ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Customer name is required.");

        Name = name;
    }

    public void ChangePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainException("Customer phone is required.");

        Phone = phone;
    }
}