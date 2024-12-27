using System.Globalization;
using Bogus;
using Microsoft.Extensions.Logging;
using ParkPlaceSample.Infrastructure.Config;
using ParkPlaceSample.Infrastructure.TestData.Models;

namespace ParkPlaceSample.Infrastructure.TestData;

/// <summary>
/// Provides methods for generating test data using the Bogus library.
/// </summary>
public class TestDataGenerator
{
    private readonly ILogger _logger;
    private readonly TestSettings _settings;
    private readonly Faker _faker;

    /// <summary>
    /// Initializes a new instance of the TestDataGenerator class.
    /// </summary>
    /// <param name="logger">The logger instance for logging operations.</param>
    /// <param name="settings">The test settings containing configuration values.</param>
    public TestDataGenerator(ILogger logger, TestSettings settings)
    {
        _logger = logger;
        _settings = settings;

        // Ensure we have test data settings
        if (_settings.TestData == null)
        {
            _settings.TestData = new TestDataSettings();
        }

        _faker = new Faker(_settings.TestData.Locale);
        _logger.LogInformation("Initialized TestDataGenerator with locale: {Locale}", _settings.TestData.Locale);
    }

    /// <summary>
    /// Generates test user data with random but realistic values.
    /// </summary>
    /// <returns>A UserData object containing randomly generated user information.</returns>
    public UserData GenerateUserData()
    {
        _logger.LogInformation("Generating user data");
        return new UserData
        {
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
            Email = _faker.Internet.Email(),
            Username = _faker.Internet.UserName(),
            Password = _faker.Internet.Password(12),
            PhoneNumber = _faker.Phone.PhoneNumber(),
            DateOfBirth = _faker.Date.Past(50).ToString("yyyy-MM-dd"),
            Address = new AddressData
            {
                Street = _faker.Address.StreetAddress(),
                City = _faker.Address.City(),
                State = _faker.Address.State(),
                PostalCode = _faker.Address.ZipCode(),
                Country = _faker.Address.Country()
            }
        };
    }

    /// <summary>
    /// Generates test payment data with random but realistic values.
    /// </summary>
    /// <returns>A PaymentData object containing randomly generated payment information.</returns>
    public PaymentData GeneratePaymentData()
    {
        _logger.LogInformation("Generating payment data");
        return new PaymentData
        {
            CardNumber = _faker.Finance.CreditCardNumber(),
            CardHolder = $"{_faker.Name.FirstName()} {_faker.Name.LastName()}",
            ExpirationDate = _faker.Date.Future(4).ToString("MM/yy"),
            Cvv = _faker.Finance.CreditCardCvv(),
            BillingAddress = new AddressData
            {
                Street = _faker.Address.StreetAddress(),
                City = _faker.Address.City(),
                State = _faker.Address.State(),
                PostalCode = _faker.Address.ZipCode(),
                Country = _faker.Address.Country()
            }
        };
    }

    /// <summary>
    /// Generates test product data with random but realistic values.
    /// </summary>
    /// <returns>A ProductData object containing randomly generated product information.</returns>
    public ProductData GenerateProductData()
    {
        _logger.LogInformation("Generating product data");
        return new ProductData
        {
            Name = _faker.Commerce.ProductName(),
            Description = _faker.Commerce.ProductDescription(),
            Price = decimal.Parse(_faker.Commerce.Price(), CultureInfo.InvariantCulture),
            Category = _faker.Commerce.Categories(1)[0],
            SKU = _faker.Commerce.Ean13(),
            Quantity = _faker.Random.Number(1, 100),
            Tags = _faker.Make(3, () => _faker.Commerce.ProductAdjective()).ToList()
        };
    }

    /// <summary>
    /// Generates test order data with random but realistic values.
    /// </summary>
    /// <param name="itemCount">The number of items to include in the order.</param>
    /// <returns>An OrderData object containing randomly generated order information.</returns>
    public OrderData GenerateOrderData(int itemCount = 3)
    {
        _logger.LogInformation("Generating order data with {ItemCount} items", itemCount);
        return new OrderData
        {
            OrderNumber = _faker.Random.AlphaNumeric(10).ToUpper(),
            OrderDate = _faker.Date.Recent().ToString("yyyy-MM-dd"),
            Customer = GenerateUserData(),
            Items = _faker.Make(itemCount, () => new OrderItemData
            {
                Product = GenerateProductData(),
                Quantity = _faker.Random.Number(1, 5)
            }).ToList(),
            ShippingAddress = new AddressData
            {
                Street = _faker.Address.StreetAddress(),
                City = _faker.Address.City(),
                State = _faker.Address.State(),
                PostalCode = _faker.Address.ZipCode(),
                Country = _faker.Address.Country()
            },
            PaymentInfo = GeneratePaymentData()
        };
    }

    /// <summary>
    /// Generates test company data with random but realistic values.
    /// </summary>
    /// <returns>A CompanyData object containing randomly generated company information.</returns>
    public CompanyData GenerateCompanyData()
    {
        _logger.LogInformation("Generating company data");
        return new CompanyData
        {
            Name = _faker.Company.CompanyName(),
            Industry = _faker.Company.CompanySuffix(),
            Description = _faker.Company.CatchPhrase(),
            Website = _faker.Internet.Url(),
            Email = _faker.Internet.Email(_faker.Company.CompanyName()),
            Phone = _faker.Phone.PhoneNumber(),
            Address = new AddressData
            {
                Street = _faker.Address.StreetAddress(),
                City = _faker.Address.City(),
                State = _faker.Address.State(),
                PostalCode = _faker.Address.ZipCode(),
                Country = _faker.Address.Country()
            },
            Contacts = _faker.Make(2, () => new ContactData
            {
                Name = _faker.Name.FullName(),
                Title = _faker.Name.JobTitle(),
                Email = _faker.Internet.Email(),
                Phone = _faker.Phone.PhoneNumber()
            }).ToList()
        };
    }

    /// <summary>
    /// Generates a random string of specified length.
    /// </summary>
    /// <param name="length">The length of the string to generate.</param>
    /// <returns>A randomly generated string.</returns>
    public string GenerateRandomString(int length = 10)
    {
        return _faker.Random.String2(length);
    }

    /// <summary>
    /// Generates a random email address.
    /// </summary>
    /// <returns>A randomly generated email address.</returns>
    public string GenerateRandomEmail()
    {
        return _faker.Internet.Email();
    }

    /// <summary>
    /// Generates a random phone number.
    /// </summary>
    /// <returns>A randomly generated phone number.</returns>
    public string GenerateRandomPhoneNumber()
    {
        return _faker.Phone.PhoneNumber();
    }

    /// <summary>
    /// Generates a random date between the specified range.
    /// </summary>
    /// <param name="minDate">The minimum date (inclusive). If null, defaults to one year ago.</param>
    /// <param name="maxDate">The maximum date (inclusive). If null, defaults to current date.</param>
    /// <returns>A randomly generated date within the specified range.</returns>
    public DateTime GenerateRandomDate(DateTime? minDate = null, DateTime? maxDate = null)
    {
        return _faker.Date.Between(minDate ?? DateTime.Now.AddYears(-1), maxDate ?? DateTime.Now);
    }

    /// <summary>
    /// Generates a random enum value of the specified type.
    /// </summary>
    /// <typeparam name="T">The enum type to generate a value for.</typeparam>
    /// <returns>A randomly selected enum value.</returns>
    public T GenerateRandomEnum<T>() where T : struct, Enum
    {
        return _faker.PickRandom<T>();
    }
}