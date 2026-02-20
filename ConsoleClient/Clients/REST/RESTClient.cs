using ConsoleClient.Clients.Urls;
using ConsoleClient.Enums;
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
        private readonly Url _url;

        public RESTClient(Url url)
        {
            Console.WriteLine("Constructing REST client...");
            _url = url;
            Console.WriteLine("Finished constructing REST client.");
        }

        public async Task<int> CreateRoomAsync(TokenDto token, string name)
        {
            Console.WriteLine("Trying to create a new room...");
            HttpClient httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

            string serializedJson = JsonSerializer.Serialize(new { Name = name });
            Console.WriteLine($"json to post {serializedJson}");
            using StringContent jsonContent = new StringContent(serializedJson, Encoding.UTF8, "application/json");

            string url = _url.FromControllerAction(
                Service.REST,
                Controller.Rooms,
                RoomsAction.CreateRoomAndAddUserToIt.ToString());
            HttpResponseMessage responseMessage = await httpClient.PostAsync(url, jsonContent);
            if (responseMessage.IsSuccessStatusCode)
            {
                int roomId = await responseMessage.Content.ReadFromJsonAsync<int>();
                Console.WriteLine($"Created room with roomId = {roomId}");
                return roomId;
            }
            string content = await responseMessage.Content.ReadAsStringAsync();
            throw new Exception($"Error: Failed to create room:\n{content}");
        }

        public async Task RenameRoom(TokenDto token, string name)
        {
            Console.WriteLine("Trying to rename room...");
            HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

            string serializedJson = JsonSerializer.Serialize(new { Name = name });
            Console.WriteLine($"json to post {serializedJson}");
            using StringContent jsonContent = new StringContent(serializedJson, Encoding.UTF8, "application/json");

            string url = _url.FromControllerAction(
                Service.REST,
                Controller.Rooms,
                RoomsAction.UpdateRoomName.ToString());
            HttpResponseMessage responseMessage = await httpClient.PutAsync(url, jsonContent);
            if (responseMessage.IsSuccessStatusCode)
            {
                Console.WriteLine("Room renamed successfully.");
                return;
            }
            else
            {
                string content = await responseMessage.Content.ReadAsStringAsync();
                throw new Exception($"Error: Failed to rename room:\n{content}"); 
            }
        }
    }
}
