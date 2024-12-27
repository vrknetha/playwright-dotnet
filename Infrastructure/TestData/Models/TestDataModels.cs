namespace ParkPlaceSample.Infrastructure.TestData.Models;

public class UserData
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty;
    public AddressData Address { get; set; } = new();
}

public class AddressData
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public class PaymentData
{
    public string CardNumber { get; set; } = string.Empty;
    public string CardHolder { get; set; } = string.Empty;
    public string ExpirationDate { get; set; } = string.Empty;
    public string Cvv { get; set; } = string.Empty;
    public AddressData BillingAddress { get; set; } = new();
}

public class ProductData
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public List<string> Tags { get; set; } = new();
}

public class OrderItemData
{
    public ProductData Product { get; set; } = new();
    public int Quantity { get; set; }
}

public class OrderData
{
    public string OrderNumber { get; set; } = string.Empty;
    public string OrderDate { get; set; } = string.Empty;
    public UserData Customer { get; set; } = new();
    public List<OrderItemData> Items { get; set; } = new();
    public AddressData ShippingAddress { get; set; } = new();
    public PaymentData PaymentInfo { get; set; } = new();
}

public class ContactData
{
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public class CompanyData
{
    public string Name { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public AddressData Address { get; set; } = new();
    public List<ContactData> Contacts { get; set; } = new();
}