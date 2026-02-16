using Microsoft.Extensions.Configuration;

namespace Client
{
    internal struct Urls
    {
        // Auth urls
        public readonly string testAuthConnectionUrl;
        public readonly string registerUrl;
        public readonly string loginUrl;

        // Message urls
        public readonly string chatHubUrl;

        // Rooms urls
        public readonly string createRoomUrl;

        public Urls(bool productionUrls)
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
            chatHubUrl = configuration.GetValue<string>("ChatHubUrl")
                                        ?? throw new Exception("Failed to get ChatHubUrl");
            createRoomUrl = configuration.GetValue<string>("CreateRoomUrl")
                                        ?? throw new Exception("Failed to get CreateRoomUrl");

            Console.WriteLine("Got the urls from configuration successfully");
        }
    }
}
