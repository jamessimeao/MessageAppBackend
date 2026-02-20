using ConsoleClient.Clients.Urls;
using ConsoleClient.Enums;
using JWTAuth.Dtos;
using REST.Dtos.Rooms;
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

        private async Task<HttpResponseMessage> RequestWithJsonAsync(
            TokenDto token,
            HttpMethod method,
            Service service,
            Controller controller,
            string action,
            object dto)
        {
            HttpClient httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

            string serializedJson = JsonSerializer.Serialize(dto);
            Console.WriteLine($"json to post {serializedJson}");
            using StringContent jsonContent = new StringContent(serializedJson, Encoding.UTF8, "application/json");

            string url = _url.FromControllerAction(
                service,
                controller,
                action);

            HttpRequestMessage request = new();
            request.Method = method;
            request.Content = jsonContent;
            request.RequestUri = new Uri(url);

            HttpResponseMessage responseMessage = await httpClient.SendAsync(request);
            return responseMessage;
        }

        public async Task<int> CreateRoomAndAddUserToItAsync(TokenDto token, CreateRoomDto createRoomDto)
        {
            Console.WriteLine("Trying to create a new room...");

            HttpResponseMessage responseMessage = await RequestWithJsonAsync(
                token,
                HttpMethod.Post,
                Service.REST,
                Controller.Rooms,
                RoomsAction.CreateRoomAndAddUserToIt.ToString(),
                createRoomDto);

            if (responseMessage.IsSuccessStatusCode)
            {
                int roomId = await responseMessage.Content.ReadFromJsonAsync<int>();
                Console.WriteLine($"Created room with roomId = {roomId}");
                return roomId;
            }
            string content = await responseMessage.Content.ReadAsStringAsync();
            throw new Exception($"Error: Failed to create room. Status code: {responseMessage.StatusCode}.");
        }

        public async Task DeleteRoomAsync(TokenDto token, DeleteRoomDto deleteRoomDto)
        {
            Console.WriteLine("Trying to delete room...");

            HttpResponseMessage responseMessage = await RequestWithJsonAsync(
                token,
                HttpMethod.Delete,
                Service.REST,
                Controller.Rooms,
                RoomsAction.DeleteRoom.ToString(),
                deleteRoomDto);

            if (responseMessage.IsSuccessStatusCode)
            {
                Console.WriteLine("Room deleted successfully.");
                return;
            }
            else
            {
                throw new Exception($"Error: Failed to delete room. Status code: {responseMessage.StatusCode}.");
            }
        }

        public async Task UpdateRoomNameAsync(TokenDto token, UpdateRoomNameDto updateRoomNameDto)
        {
            Console.WriteLine("Trying to rename room...");
            
            HttpResponseMessage responseMessage = await RequestWithJsonAsync(
                token,
                HttpMethod.Put,
                Service.REST,
                Controller.Rooms,
                RoomsAction.UpdateRoomName.ToString(),
                updateRoomNameDto);

            if (responseMessage.IsSuccessStatusCode)
            {
                Console.WriteLine("Room renamed successfully.");
                return;
            }
            else
            {
                throw new Exception($"Error: Failed to rename room. Status code: {responseMessage.StatusCode}.");
            }
        }

        public async Task<string> GenerateInvitationTokenAsync(TokenDto token, GenerateInvitationTokenDto generateInvitationTokenDto)
        {
            Console.WriteLine("Trying to generate invitation...");

            HttpResponseMessage responseMessage = await RequestWithJsonAsync(
                token,
                HttpMethod.Post,
                Service.REST,
                Controller.Rooms,
                RoomsAction.GenerateInvitationToken.ToString(),
                generateInvitationTokenDto);

            if (responseMessage.IsSuccessStatusCode)
            {
                string content = await responseMessage.Content.ReadAsStringAsync();
                return content;
            }
            else
            {
                throw new Exception($"Error: Failed to generate invitation. Status code: {responseMessage.StatusCode}.");
            }
        }

        public async Task JoinRoomAsync(TokenDto token, JoinRoomDto joinRoomDto)
        {
            Console.WriteLine("Trying to join room...");

            HttpResponseMessage responseMessage = await RequestWithJsonAsync(
                token,
                HttpMethod.Post,
                Service.REST,
                Controller.Rooms,
                RoomsAction.JoinRoom.ToString(),
                joinRoomDto);

            if (responseMessage.IsSuccessStatusCode)
            {
                return;
            }
            else
            {
                throw new Exception($"Error: Failed to join room. Status code: {responseMessage.StatusCode}.");
            }
        }
    }
}
