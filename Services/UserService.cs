
using System.Text.Json;
using AuthDemo.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthDemo.Services
{
    public class UserService
    {
        private readonly string _filePath;
        private readonly PasswordHasher<User> _hasher = new();

        public UserService(IWebHostEnvironment environment)
        {
            string dataFolder = Path.Combine(
                environment.ContentRootPath, "data");

            Directory.CreateDirectory(dataFolder);

            _filePath = Path.Combine(dataFolder, "users.json");

            SeedUser();
        }

        private void SeedUser()
        {
            if (File.Exists(_filePath))
                return;

            var admin = new User
            {
                Username = "admin"
            };

            admin.PasswordHash =
                _hasher.HashPassword(admin, "Password123");

            var users = new List<User> { admin };

            string json = JsonSerializer.Serialize(
                users,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            File.WriteAllText(_filePath, json);
        }

        public List<User> GetUsers()
        {
            string json = File.ReadAllText(_filePath);

            return JsonSerializer.Deserialize<List<User>>(json)
                   ?? new List<User>();
        }

        public bool ValidateUser(string username, string password)
        {
            var user = GetUsers().FirstOrDefault(
                u => u.Username.Equals(
                    username,
                    StringComparison.OrdinalIgnoreCase));

            if (user == null)
                return false;

            var result = _hasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                password);

            return result != PasswordVerificationResult.Failed;
        }
    }
}
