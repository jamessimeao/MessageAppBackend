using ConsoleClient.Clients.Auth;
using JWTAuth.Dtos;

namespace ConsoleClient
{
    internal class Test
    {
        private readonly bool _productionUrls;
        private readonly int _usersQuantity = 3;
        private UserRegisterDto[] userRegisterDtos;
        private TokenDto[] tokens;
        private Random random = new Random();
        private const string ALPHNUM = "ABCEDFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        private const string SYMB = @"&%-/\*#$";

        public Test(bool productionUrls, int usersQuantity)
        {
            _productionUrls = productionUrls;
            _usersQuantity = usersQuantity;
            userRegisterDtos = new UserRegisterDto[usersQuantity];
            tokens = new TokenDto[usersQuantity];
        }

        public async Task ExecuteAsync()
        {
            // Only need a single Auth client to get tokens
            AuthClient authClient = new AuthClient(_productionUrls);

            // Try to register new users until we have the amount of users we need
            await RegisterRandomUsers(authClient);
            PrintRegisteredUsers();

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
            Console.WriteLine("Trying to register new users...");
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
            }
            Console.WriteLine("Finished registering users.");
        }

        private void PrintRegisteredUsers()
        {
            Console.WriteLine("Users registered:");
            for (int i = 0; i < _usersQuantity; i++)
            {
                Console.WriteLine(i);
                Console.WriteLine($"Email = {userRegisterDtos[i].Email}");
                Console.WriteLine($"Username = {userRegisterDtos[i].Username}");
                Console.WriteLine($"Password = {userRegisterDtos[i].Password}");
            }
        }

        }
    }
}
