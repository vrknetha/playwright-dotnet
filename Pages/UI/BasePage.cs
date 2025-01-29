using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using PlaywrightDemo.Infrastructure.Config;
using PlaywrightDemo.Infrastructure.Config.Models;
using PlaywrightDemo.Infrastructure.Logging;
using PlaywrightDemo.Infrastructure.Base;
using System.Threading.Tasks;

namespace PlaywrightDemo.Pages.UI;

public record APIResponseDetails
{
    public string Url { get; init; }
    public string Method { get; init; }
    public int Status { get; init; }

    public APIResponseDetails() { }

    public APIResponseDetails(string url, string method, int status)
    {
        Url = url;
        Method = method;
        Status = status;
    }
}

public abstract class BasePage : PageTest
{
    protected new IPage Page { get; }
    protected readonly TestSettings Settings;
    protected readonly ILogger Logger;

    protected BasePage(IPage page)
    {
        Page = page;
        Logger = LoggerManager.Current;
        Settings = ConfigurationLoader.GetSettings<TestSettings>();
    }

    protected string BuildUrl(string path)
    {
        var baseUrl = Settings.Environment.BaseUrl?.TrimEnd('/');
        path = path.TrimStart('/');
        return $"{baseUrl}/{path}";
    }

    /// <summary>
    /// Waits for an API response with a matching URL and method, and returns the response.
    /// </summary>
    /// <param name="apiResponseDetails">Details about the expected API response</param>
    /// <returns>The matched IResponse object</returns>
    /// <exception cref="Exception">Thrown when response is not received within timeout or status code doesn't match</exception>
    protected async Task<IResponse> RunAndWaitForResponseAsync(APIResponseDetails apiResponseDetails)
    {
        try
        {
            IResponse response;
            bool isWildCardIncluded = apiResponseDetails.Url.Contains("*");

            if (isWildCardIncluded)
            {
                var urlParts = apiResponseDetails.Url.Split('*');
                var urlStart = urlParts[0];
                var urlEnd = urlParts[1];

                response = await Page.WaitForResponseAsync(resp =>
                    resp.Url.Contains(urlStart) &&
                    resp.Url.Contains(urlEnd) &&
                    resp.Request.Method == apiResponseDetails.Method);
            }
            else
            {
                response = await Page.WaitForResponseAsync(resp =>
                    resp.Url.Contains(apiResponseDetails.Url) &&
                    resp.Request.Method == apiResponseDetails.Method);
            }

            string expectedStatus = apiResponseDetails.Status.ToString();
            string actualStatus = response.Status.ToString();

            // If status is 1 or 2 digits (e.g., 20), treat it as a contains check
            // If status is 3 digits (e.g., 200), treat it as exact match
            bool shouldDoContainsCheck = expectedStatus.Length <= 2;

            if (shouldDoContainsCheck)
            {
                if (!actualStatus.Contains(expectedStatus))
                {
                    throw new Exception($"Expected status containing {expectedStatus}, but got {actualStatus}.");
                }
            }
            else if (response.Status != apiResponseDetails.Status)
            {
                throw new Exception($"Expected status {expectedStatus}, but got {actualStatus}.");
            }

            return response;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error waiting for API response: {ex.Message}. Parameters: URL={apiResponseDetails.Url}, Method={apiResponseDetails.Method}, Status={apiResponseDetails.Status}");
            throw;
        }
    }
}