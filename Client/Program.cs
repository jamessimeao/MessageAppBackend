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

            bool connectionSuccessful = await client.TestConnectionToAuth();
            if(!connectionSuccessful)
            {
                return;
            }

            bool Registered = await client.Register(userRegisterDto);
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
            bool loggedIn = await client.Login(userLoginDto);
            if(!loggedIn)
            {
                return;
            }

            await client.ConfigureConnection();

            bool connected = await client.TryToConnectToChatHub();
            if (!connected)
            {
                return;
            }

            bool addedToGroups = await client.AddToGroups();
            if(!addedToGroups)
            {
                return;
            }

            string? message = null;
            bool chatting = true;
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
                        await client.SendMessage(userRegisterDto.Email, message);
                    }
                }
            }

            await client.StopAsync();
        }
    }
}