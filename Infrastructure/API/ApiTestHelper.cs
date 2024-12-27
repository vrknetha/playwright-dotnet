using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using ParkPlaceSample.Infrastructure.Config;
using ParkPlaceSample.Infrastructure.TestData;
using ParkPlaceSample.Infrastructure.TestData.Models;

namespace ParkPlaceSample.Infrastructure.API;

/// <summary>
/// Provides helper methods for API test setup and teardown operations.
/// </summary>
public class ApiTestHelper
{
    private readonly ILogger _logger;
    private readonly TestSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly TestDataGenerator _dataGenerator;
    private readonly List<string> _createdResources;

    /// <summary>
    /// Initializes a new instance of the ApiTestHelper class.
    /// </summary>
    /// <param name="logger">The logger instance for logging operations.</param>
    /// <param name="settings">The test settings containing configuration values.</param>
    /// <param name="httpClient">The HTTP client for making API requests.</param>
    public ApiTestHelper(ILogger logger, TestSettings settings, HttpClient httpClient)
    {
        _logger = logger;
        _settings = settings;
        _httpClient = httpClient;
        _dataGenerator = new TestDataGenerator(logger, settings);
        _createdResources = new List<string>();
    }

    /// <summary>
    /// Creates a test user through the API.
    /// </summary>
    /// <returns>The created user data.</returns>
    public async Task<UserData> CreateTestUserAsync()
    {
        var userData = _dataGenerator.GenerateUserData();
        _logger.LogInformation("Creating test user: {Username}", userData.Username);

        var response = await _httpClient.PostAsJsonAsync("/api/users", userData);
        response.EnsureSuccessStatusCode();

        var createdUser = await response.Content.ReadFromJsonAsync<UserData>();
        _createdResources.Add($"users/{createdUser!.Username}");
        return createdUser;
    }

    /// <summary>
    /// Creates a test product through the API.
    /// </summary>
    /// <returns>The created product data.</returns>
    public async Task<ProductData> CreateTestProductAsync()
    {
        var productData = _dataGenerator.GenerateProductData();
        _logger.LogInformation("Creating test product: {Name}", productData.Name);

        var response = await _httpClient.PostAsJsonAsync("/api/products", productData);
        response.EnsureSuccessStatusCode();

        var createdProduct = await response.Content.ReadFromJsonAsync<ProductData>();
        _createdResources.Add($"products/{createdProduct!.SKU}");
        return createdProduct;
    }

    /// <summary>
    /// Creates a test order through the API.
    /// </summary>
    /// <param name="itemCount">The number of items to include in the order.</param>
    /// <returns>The created order data.</returns>
    public async Task<OrderData> CreateTestOrderAsync(int itemCount = 3)
    {
        var orderData = _dataGenerator.GenerateOrderData(itemCount);
        _logger.LogInformation("Creating test order with {ItemCount} items", itemCount);

        var response = await _httpClient.PostAsJsonAsync("/api/orders", orderData);
        response.EnsureSuccessStatusCode();

        var createdOrder = await response.Content.ReadFromJsonAsync<OrderData>();
        _createdResources.Add($"orders/{createdOrder!.OrderNumber}");
        return createdOrder;
    }

    /// <summary>
    /// Creates a test company through the API.
    /// </summary>
    /// <returns>The created company data.</returns>
    public async Task<CompanyData> CreateTestCompanyAsync()
    {
        var companyData = _dataGenerator.GenerateCompanyData();
        _logger.LogInformation("Creating test company: {Name}", companyData.Name);

        var response = await _httpClient.PostAsJsonAsync("/api/companies", companyData);
        response.EnsureSuccessStatusCode();

        var createdCompany = await response.Content.ReadFromJsonAsync<CompanyData>();
        _createdResources.Add($"companies/{createdCompany!.Name}");
        return createdCompany;
    }

    /// <summary>
    /// Cleans up all resources created during the test.
    /// </summary>
    public async Task CleanupTestResourcesAsync()
    {
        _logger.LogInformation("Cleaning up {Count} test resources", _createdResources.Count);

        foreach (var resource in _createdResources)
        {
            try
            {
                await _httpClient.DeleteAsync($"/api/{resource}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete test resource: {Resource}", resource);
            }
        }

        _createdResources.Clear();
    }

    /// <summary>
    /// Gets the list of resources created during the test.
    /// </summary>
    /// <returns>A list of resource identifiers.</returns>
    public IReadOnlyList<string> GetCreatedResources() => _createdResources.AsReadOnly();
}