using JWTAuth.Dtos;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ConsoleClient.Clients.REST
{
    internal class RESTClient
    {
        // Urls to communicate with server
        private readonly RESTUrls urls;

        public RESTClient(bool productionUrls)
        {
            Console.WriteLine("Constructing REST client...");
            urls = new RESTUrls(productionUrls);
            Console.WriteLine("Finished constructing REST client.");
        }

        public async Task<int> CreateRoomAsync(TokenDto token, string name)
        {
            Console.WriteLine("Trying to create a new room...");
            HttpClient httpClient = new HttpClient();

            if (token == null)
            {
                throw new Exception("Error: Null token");
            }
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

            string serializedJson = JsonSerializer.Serialize(new { Name = name });
            Console.WriteLine($"json to post {serializedJson}");
            using StringContent jsonContent = new StringContent(serializedJson, Encoding.UTF8, "application/json");

            HttpResponseMessage responseMessage = await httpClient.PostAsync(urls.createRoomUrl, jsonContent);
            if (responseMessage.IsSuccessStatusCode)
            {
                int roomId = await responseMessage.Content.ReadFromJsonAsync<int>();
                Console.WriteLine($"Created room with roomId = {roomId}");
                return roomId;
            }
            throw new Exception($"Error: Failed to create room:\n{await responseMessage.Content.ReadAsStringAsync()}");
        }
    }
}
