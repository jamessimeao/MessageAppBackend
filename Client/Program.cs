using JWTAuth.Dtos;
using Microsoft.AspNetCore.SignalR.Client;

namespace Client
{
    internal class Program
    {
        public static async Task Main()
        {
            UserRegisterDto userRegisterDto = new UserRegisterDto()
            {
                Email = "john@hotmail.com",
                Password = "banana",
                Username = "John",
            };

            Client client = new Client(userRegisterDto);

            bool connectionSuccessful = await client.TestConnectionToAuthAsync();
            if(!connectionSuccessful)
            {
                return;
            }

            bool Registered = await client.RegisterAsync(userRegisterDto);
            if (Registered)
            {
                Console.WriteLine("Registered new user.");
            }
            else
            {
                Console.WriteLine("User already registered or error.");
                Console.WriteLine("Write q to quit or anything else to proceed.");
                string? quit = Console.ReadLine();
                if(quit == "q")
                {
                    return;
                }
            }

            UserLoginDto userLoginDto = new UserLoginDto()
            {
                Email = userRegisterDto.Email,
                Password = userRegisterDto.Password,
            };
            bool loggedIn = await client.LoginAsync(userLoginDto);
            if(!loggedIn)
            {
                return;
            }

            await client.ConfigureConnectionAsync();

            bool connected = await client.TryToConnectToChatHubAsync();
            if (!connected)
            {
                return;
            }

            string? message = null;
            bool chatting = true;
            const int roomId = 0;
            Console.WriteLine("Starting chat. Send q to quit.");
            while (chatting)
            {
                //Console.Write("Write a message:");
                message = Console.ReadLine();
                if (!string.IsNullOrEmpty(message))
                {
                    if (message == "q")
                    {
                        chatting = false;
                    }
                    else
                    {
                        await client.SendMessageAsync(roomId, message);
                    }
                }
            }

            await client.StopAsync();
        }
    }
}