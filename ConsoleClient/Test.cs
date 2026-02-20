using ConsoleClient.Clients.Auth;
using ConsoleClient.Clients.REST;
using ConsoleClient.Clients.Urls;
using JWTAuth.Dtos;
using REST.Dtos.Rooms;
using REST.Roles;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ConsoleClient
{
    internal class Test
    {
        private readonly int _usersQuantity = 3;
        private readonly Url url;
        private UserRegisterDto[] userRegisterDtos;
        private TokenDto[] tokens;
        private int[] usersIds;
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
            usersIds = new int[usersQuantity];
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

            GetUsersIdsFromTokens();

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

            // Make other users join the room
            for (int i = 1; i < _usersQuantity; i++)
            {
                JoinRoomDto joinRoomDto = new()
                {
                    InvitationToken = invitationToken,
                };
                await restClient.JoinRoomAsync(tokens[i], joinRoomDto);
            }

            // Get users info from room, to verify their roles
            // Use the user 1, because it should not need to be an admin
            GetUsersInfoFromRoomDto getUsersInfoFromRoomDto = new()
            {
                RoomId = roomId
            };
            IEnumerable<UserInfoDto> usersInfo = await restClient.GetUsersInfoFromRoomAsync(tokens[1], getUsersInfoFromRoomDto);

            // Check that the user 0 is admin, but the other users aren't
            foreach (UserInfoDto info in usersInfo)
            {
                if (info.Id == usersIds[0] && info.RoleInRoom != RoleInRoom.Admin.ToString())
                {
                    throw new Exception("Error: Room admin lost privilege.");
                }
                else if (info.Id != usersIds[0] && info.RoleInRoom == RoleInRoom.Admin.ToString())
                {
                    throw new Exception("Error: Room admin that should be regular.");
                }
            }

            // Test if the user 1 can change the name of the room.
            // It shouldn't be possible.
            UpdateRoomNameDto updateRoomNameDto = new()
            {
                Name = newRoomName,
                RoomId = roomId,
            };
            // We need finer control, so use RequestWithJsonAsync directly
            HttpResponseMessage responseMessage = await restClient.RequestWithJsonAsync(
                tokens[1],
                HttpMethod.Put,
                Service.REST,
                Controller.Rooms,
                RoomsAction.UpdateRoomName.ToString(),
                updateRoomNameDto);
            if(responseMessage.StatusCode != System.Net.HttpStatusCode.Forbidden)
            {
                throw new Exception("Error: A regular user should receive a Forbidden response when trying to update the name of a room.");
            }

            // Turn the user 1 into an admin
            UpdateUserRoleInRoomDto updateUserRoleInRoomDto = new()
            { 
                RoleInRoom = RoleInRoom.Admin,
                UserId = usersIds[1],
                RoomId = roomId,
            };
            await restClient.UpdateUserRoleInRoomAsync(tokens[0], updateUserRoleInRoomDto);

            // Get users info from room, to verify their roles
            usersInfo = await restClient.GetUsersInfoFromRoomAsync(tokens[0], getUsersInfoFromRoomDto);

            // Check that user 1 is admin
            foreach (UserInfoDto info in usersInfo)
            {
                if (info.Id == usersIds[1] && info.RoleInRoom != RoleInRoom.Admin.ToString())
                {
                    throw new Exception("Error: Regular user not turned into admin.");
                }
            }

            // Try again to rename the room, now with admin privilege
            await restClient.UpdateRoomNameAsync(tokens[1], updateRoomNameDto);

            // Get the room name
            GetRoomInfoDto getRoomInfoDto = new()
            {
                RoomId = roomId,
            };
            RoomInfoDto roomInfo = await restClient.GetRoomInfoAsync(tokens[0], getRoomInfoDto);

            // Check that the room name was updated
            if(roomInfo.Name != newRoomName)
            {
                throw new Exception("Error: Room name not updated.");
            }




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

        private int GetIdFromJWT(string token)
        {
            JwtSecurityTokenHandler handler = new();

            JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

            Claim claim = jwtToken.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier);
            string idString = claim.Value;
            int id = Int32.Parse(idString);

            return id;
        }

        private void GetUsersIdsFromTokens()
        {
            for (int i = 0; i < _usersQuantity; i++)
            {
                usersIds[i] = GetIdFromJWT(tokens[i].AccessToken);
            }
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
