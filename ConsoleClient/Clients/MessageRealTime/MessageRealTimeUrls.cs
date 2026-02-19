using Microsoft.Extensions.Configuration;

namespace ConsoleClient.Clients.MessageRealTime
{
    internal struct MessageRealTimeUrls
    {
        // MessageRealTime urls
        public readonly string chatHubUrl;

        public MessageRealTimeUrls(bool productionUrls)
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

            chatHubUrl = configuration.GetValue<string>("ChatHubUrl")
                                        ?? throw new Exception("Failed to get ChatHubUrl");

            Console.WriteLine("Got the MessageRealTime urls from configuration successfully");
        }
    }
}
