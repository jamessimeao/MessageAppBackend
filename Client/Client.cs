using Client.Dtos;
using JWTAuth.Dtos;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Client
{
    internal class Client
    {
        // SignalR
        private readonly HubConnectionBuilder hubConnectionBuilder;
        private HubConnection? connection;

        // Serialization options
        private readonly JsonSerializerOptions jsonSerializerOptions;

        private readonly Urls urls;

        // Variables
        private readonly UserRegisterDto _userRegisterDto;
        private TokenDto? token;

        public Client(UserRegisterDto userRegisterDto, bool productionUrls)
        {
            Console.WriteLine("Constructing client...");
            _userRegisterDto = userRegisterDto;

            hubConnectionBuilder = new HubConnectionBuilder();

            jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.PropertyNameCaseInsensitive = true;

            urls = new Urls(productionUrls);
            Console.WriteLine("Finished constructing client.");
        }

        public async Task ConfigureConnectionAsync()
        {
            if (token == null)
            {
                return;
            }

            connection = hubConnectionBuilder
                .WithUrl(urls.chatHubUrl, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult<string?>(token.AccessToken);
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Debug);
                })
                .Build();

            // When the connection is closed, try to reconnect after some time
            connection.Closed += OnConnectionClosedAsync;

            // When connection receives a message, print it
            connection.On<ReceiveMessageDto>("ReceiveMessageAsync", async (ReceiveMessageDto receiveMessageDto) =>
            {
                Console.WriteLine($"id: {receiveMessageDto.Id}, {receiveMessageDto.Time}, room: {receiveMessageDto.RoomId}, sender: {receiveMessageDto.SenderId}: {receiveMessageDto.Content}");
            });

            connection.On<ErrorMessageDto>("ReceiveErrorMessageAsync", async (ErrorMessageDto errorMessageDto) =>
            {
                Console.WriteLine($"Error: {errorMessageDto.Content}");
            });

            connection.On<NotificationDto>("ReceiveNotificationAsync", async (NotificationDto notificationDto) =>
            {
                Console.WriteLine($"Notification: {notificationDto.Content}");
            });
        }

        private async Task OnConnectionClosedAsync(Exception? error)
        {
            if(error != null)
            {
                Console.WriteLine($"Error: {error.Message}");
            }

            if(connection != null)
            {
                /*
                Console.WriteLine("Connection closed, restarting...");
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
                */
                Console.WriteLine("Connection closed.");
            }
        }

        public async Task<bool> TestConnectionToAuthAsync()
        {
            Console.WriteLine("Trying to connect to Auth service...");
            HttpClient httpClient = new HttpClient();

            // Test connection first
            HttpResponseMessage responseMessageConnectionTest = await httpClient.GetAsync(urls.testAuthConnectionUrl);
            if (!responseMessageConnectionTest.IsSuccessStatusCode)
            {
                Console.WriteLine("Error: Failed to connect to Auth service.");
                return false;
            }
            else
            {
                Console.WriteLine("Successfully connected to Auth service.");
                string responseConnectionTest = await responseMessageConnectionTest.Content.ReadAsStringAsync();
                Console.WriteLine($"{responseConnectionTest}");
                return true;
            }
        }

        public async Task<bool> RegisterAsync(UserRegisterDto userRegisterDto)
        {
            Console.WriteLine("Trying to register new user...");

            HttpClient httpClient = new HttpClient();

            string serializedJson = JsonSerializer.Serialize(userRegisterDto);
            Console.WriteLine($"json to post {serializedJson}");
            using StringContent jsonContent = new StringContent(serializedJson, Encoding.UTF8, "application/json");
            HttpResponseMessage responseMessage = await httpClient.PostAsync(urls.registerUrl, jsonContent);
            if(responseMessage.IsSuccessStatusCode)
            {
                string response = await responseMessage.Content.ReadAsStringAsync();
                Console.WriteLine($"response: {response}");
                return true;
            }

            Console.WriteLine("Error: Failed to register new user or error in server");
            return false;
        }

        public async Task<bool> LoginAsync(UserLoginDto userLoginDto)
        {
            Console.WriteLine("Trying to log in...");

            HttpClient httpClient = new HttpClient();

            string serializedString = JsonSerializer.Serialize(userLoginDto);
            Console.WriteLine($"json to post:\n{serializedString}\n");

            using StringContent jsonContent = new(serializedString, Encoding.UTF8, "application/json");

            HttpResponseMessage responseMessage = await httpClient.PostAsync(urls.loginUrl, jsonContent);
            if (responseMessage.IsSuccessStatusCode)
            {
                Console.WriteLine("Login successful.");
                string content = await responseMessage.Content.ReadAsStringAsync();
                Console.WriteLine(content);
                token = JsonSerializer.Deserialize<TokenDto>(content, jsonSerializerOptions);
                if(token == null)
                {
                    return false;
                }
                return true;
            }

            Console.WriteLine($"Error: Failed to log in. Status code: {responseMessage.StatusCode}");
            return false;
        }

        public async Task<int> CreateRoomAsync(string name)
        {
            Console.WriteLine("Trying to create a new room...");
            HttpClient httpClient = new HttpClient();

            if(token == null)
            {
                throw new Exception("Error: Null token");
            }
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

            string serializedJson = JsonSerializer.Serialize(new { Name = name});
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

        public async Task<bool> TryToConnectToChatHubAsync()
        {
            Console.WriteLine("Connecting to chat hub...");
            if (connection == null)
            {
                return false;
            }

            // Try to connect
            try
            {
                await connection.StartAsync();
                Console.WriteLine("Connection started");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return false;
        }

        public async Task SendMessageAsync(int roomId, string content)
        {
            if (connection == null)
            {
                return;
            }

            try
            {
                // Invoke method SendMessage of ChatHub, with arguments user and message
                Console.WriteLine("Sending message to server...");
                DateTime time = DateTime.UtcNow;
                SendMessageDto sendMessageDto = new()
                {
                    RoomId = roomId,
                    Content = content,
                    Time = time,
                };
                await connection.InvokeAsync("SendMessageAsync", sendMessageDto);
                Console.WriteLine("Message sent.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task StopAsync()
        {
            if(connection != null)
            {
                await connection.StopAsync();
            }
        }
    }
}
