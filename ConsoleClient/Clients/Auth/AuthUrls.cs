using Microsoft.Extensions.Configuration;

namespace ConsoleClient.Clients.Auth
{
    internal struct AuthUrls
    {
        // Auth urls
        public readonly string testAuthConnectionUrl;
        public readonly string registerUrl;
        public readonly string loginUrl;
        public readonly string deleteUrl;

        public AuthUrls(bool productionUrls)
        {
            Console.WriteLine("Getting urls from configuration...");
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();

            if (productionUrls)
            {
                configurationBuilder.AddJsonFile("urls.Production.json");
            }
            else
            {
                configurationBuilder.AddJsonFile("urls.Development.json");
            }

            IConfiguration configuration = configurationBuilder.Build();

            testAuthConnectionUrl = configuration.GetValue<string>("TestAuthConnectionUrl")
                                        ?? throw new Exception("Failed to get TestAuthConnectionUrl");
            registerUrl = configuration.GetValue<string>("RegisterUrl")
                                        ?? throw new Exception("Failed to get RegisterUrl");
            loginUrl = configuration.GetValue<string>("LoginUrl")
                                        ?? throw new Exception("Failed to get LoginUrl");
            deleteUrl = configuration.GetValue<string>("DeleteUrl")
                            ?? throw new Exception("Failed to get DeleteUrl");

            Console.WriteLine("Got the auth urls from configuration successfully.");
        }
    }
}
