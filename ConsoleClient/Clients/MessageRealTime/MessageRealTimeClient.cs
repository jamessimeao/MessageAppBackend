using Client.Dtos;
using ConsoleClient.Clients.Urls;
using JWTAuth.Dtos;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ConsoleClient.Clients.MessageRealTime
{
    internal class MessageRealTimeClient : IAsyncDisposable
    {
        // SignalR
        private readonly HubConnectionBuilder hubConnectionBuilder;
        private HubConnection connection;

        // Urls to communicate with server
        private readonly Url _url;

        public MessageRealTimeClient(Url url, TokenDto token)
        {
            Console.WriteLine("Constructing MessageRealTime client...");

            _url = url;

            hubConnectionBuilder = new HubConnectionBuilder();
            connection = hubConnectionBuilder
                .WithUrl(url.ChatHub(), options =>
                {

                    options.AccessTokenProvider = () => Task.FromResult<string?>(token.AccessToken);
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Debug);
                })
                .Build();

            ConfigureConnectionActions();

            Console.WriteLine("Finished constructing MessageRealTime client.");
        }

        private void ConfigureConnectionActions()
        {
            // What to do if disconnected
            connection.Closed += OnConnectionClosedAsync;

            // When connection receives a message, print it
            connection.On<ReceiveMessageDto>("ReceiveMessageAsync", async (ReceiveMessageDto receiveMessageDto) =>
            {
                Console.WriteLine(JsonSerializer.Serialize(receiveMessageDto));
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
            if (error != null)
            {
                Console.WriteLine($"Error: {error.Message}");
            }
            /*
            Console.WriteLine("Connection closed, restarting...");
            await Task.Delay(new Random().Next(0, 5) * 1000);
            await connection.StartAsync();
            */
            Console.WriteLine("Connection closed.");
        }

        public async Task<bool> TryToConnectToChatHubAsync()
        {
            Console.WriteLine("Connecting to chat hub...");

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

        public async ValueTask DisposeAsync()
        {
            Console.WriteLine("Stopping SignalR client.");
            await connection.StopAsync();
        }
    }
}
