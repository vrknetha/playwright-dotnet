using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PlaywrightDemo.Infrastructure.Logging;
using PlaywrightDemo.Infrastructure.TestData.Models;

namespace PlaywrightDemo.Infrastructure.TestData
{
    public class UserDataGenerator : IUserData
    {
        private readonly ILogger _logger;

        public UserDataGenerator()
        {
            _logger = LoggerManager.Current; // Use existing logger
        }

        public List<User> GenerateUserData(int numberOfUsers)
        {
            _logger.LogInformation("Generating user data for {NumberOfUsers} users", numberOfUsers);
            // Implementation for generating user data dynamically (if needed in the future)
            // ... (can utilize TestDataGenerator class here)
            return new List<User>(); // Placeholder
        }

        public List<User> GetUserDataFromJson(string filePath)
        {
            _logger.LogInformation("Reading user data from JSON file: {FilePath}", filePath);
            if (!File.Exists(filePath))
            {
                _logger.LogError("User data file not found: {FilePath}", filePath);
                throw new FileNotFoundException("User data file not found.", filePath);
            }

            var jsonData = File.ReadAllText(filePath);
            var users = JsonSerializer.Deserialize<List<User>>(jsonData) ?? new List<User>();
            return users;
        }
    }
}