using Microsoft.Extensions.Configuration;

namespace ConsoleClient.Clients.REST
{
    internal struct RESTUrls
    {
        // REST urls
        public readonly string createRoomUrl;

        public RESTUrls(bool productionUrls)
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

            createRoomUrl = configuration.GetValue<string>("CreateRoomUrl")
                                        ?? throw new Exception("Failed to get CreateRoomUrl");

            Console.WriteLine("Got the REST urls from configuration successfully.");
        }
    }
}
