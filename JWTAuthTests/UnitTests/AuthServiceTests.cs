using JWTAuth.Data;
using JWTAuth.Dtos;
using JWTAuth.Models;
using JWTAuth.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

namespace JWTAuthTests.UnitTests
{
    public class AuthServiceTests
    {
        private readonly IConfigurationRoot _configuration;

        public AuthServiceTests()
        {
            // Configuration
            Dictionary<string, string?> configurationDict = new()
            {
                { "AppSettings:Token", "abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789"},
                { "AppSettings:Issuer", "issuer"},
                { "AppSettings:Audience", "audience"},
            };
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            _configuration = configurationBuilder.AddInMemoryCollection(configurationDict).Build();
        }

    }
}
