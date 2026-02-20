using ConsoleClient.Clients.Auth;
using ConsoleClient.Clients.REST;
using ConsoleClient.Clients.Urls;
using JWTAuth.Dtos;
using REST.Dtos.Rooms;

namespace ConsoleClient
{
    internal class Test
    {
        private readonly int _usersQuantity = 3;
        private readonly Url url;
        private UserRegisterDto[] userRegisterDtos;
        private TokenDto[] tokens;
        private Random random = new Random();
        private const string ALPHNUM = "ABCEDFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        private const string SYMB = @"&%-/\*#$";
        private const string originalRoomName = "original name";

        private const int DELAY_MILLISECS = 500;

        public Test(bool productionUrls, int usersQuantity)
        {
            url = new Url(productionUrls);

            _usersQuantity = usersQuantity;
            userRegisterDtos = new UserRegisterDto[usersQuantity];
            tokens = new TokenDto[usersQuantity];
        }

        public async Task ExecuteAsync()
        {
            // Only need a single Auth client to get tokens
            AuthClient authClient = new AuthClient(url);
            // And only need a single REST client
            RESTClient restClient = new RESTClient(url);

            // Try to register new users until we have the amount of users we need
            await RegisterRandomUsers(authClient);
            PrintRegisteredUsers();

            // Login each user
            await LoginUsers(authClient);

            // Create a room for the first user
            CreateRoomDto createRoomDto = new()
            {
                Name = originalRoomName 
            };
            int roomId = await restClient.CreateRoomAndAddUserToItAsync(tokens[0], createRoomDto);

            // Get a room invitation
            GenerateInvitationTokenDto generateInvitationTokenDto = new()
            {
                RoomId = roomId
            };
            string invitationToken = await restClient.GenerateInvitationTokenAsync(tokens[0], generateInvitationTokenDto);
            Console.WriteLine($"Invitation token = {invitationToken}");

            // Delete the room
            DeleteRoomDto deleteRoomDto = new()
            {
                RoomId = roomId
            };
            await restClient.DeleteRoomAsync(tokens[0], deleteRoomDto);

            // Delete users
            await DeleteUsers(authClient);
        }

        private async Task<bool> TryToRegisterARandomUserAsync(AuthClient authClient, int index)
        {
            string emailLeft = random.GetString(ALPHNUM, 10);
            string email = emailLeft + "@something.com";
            string username = random.GetString(ALPHNUM, 5);
            string password = random.GetString(ALPHNUM,15) + random.GetString(SYMB,15);

            UserRegisterDto randomDto = new()
            {
                Email = email,
                Username = username,
                Password = password,
            };

            bool succeded = await authClient.RegisterAsync(randomDto);

            if (succeded)
            {
                userRegisterDtos[index] = randomDto;
            }

            return succeded;
        }

        private async Task RegisterRandomUsers(AuthClient authClient)
        {
            // Keep trying to register new users
            Console.WriteLine("\nTrying to register new users...");
            int i = 0;
            bool registered;
            int count = 0;
            while (i < _usersQuantity)
            {
                Console.WriteLine($"Attempt {count}");
                registered = await TryToRegisterARandomUserAsync(authClient, i);
                if (registered)
                {
                    i++;
                }
                count++;
                await Task.Delay(DELAY_MILLISECS);
            }
            Console.WriteLine("Finished registering users.");
        }

        private void PrintRegisteredUsers()
        {
            Console.WriteLine("Users registered:");
            for (int i = 0; i < _usersQuantity; i++)
            {
                Console.WriteLine($"\n{i}");
                Console.WriteLine($"Email = {userRegisterDtos[i].Email}");
                Console.WriteLine($"Username = {userRegisterDtos[i].Username}");
                Console.WriteLine($"Password = {userRegisterDtos[i].Password}");
            }
        }

        private async Task LoginUsers(AuthClient authClient)
        {
            Console.WriteLine("\nLogin users...");
            for (int i = 0; i < _usersQuantity; i++)
            {
                TokenDto? token = null;
                while (token == null)
                {
                    UserLoginDto userLoginDto = new()
                    {
                        Email = userRegisterDtos[i].Email,
                        Password = userRegisterDtos[i].Password,
                    };
                    token = await authClient.LoginAsync(userLoginDto);
                    if(token != null)
                    {
                        // Clone the token
                        tokens[i] = new TokenDto()
                        {
                            AccessToken = token.AccessToken,
                            RefreshToken = token.RefreshToken,
                        };
                    }
                    await Task.Delay(DELAY_MILLISECS);
                }
            }
            Console.WriteLine("Finished login users.");
        }

        private async Task DeleteUsers(AuthClient authClient)
        {
            Console.WriteLine("\nDeleting users...");
            for (int i = 0; i < _usersQuantity; i++)
            {
                bool succeded = false;
                while (!succeded)
                {
                    succeded = await authClient.DeleteAsync(tokens[i]);
                    await Task.Delay(DELAY_MILLISECS);
                }
            }
            Console.WriteLine("Finished deleting users.");
        }
    }
}
