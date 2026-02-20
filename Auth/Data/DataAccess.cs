using Dapper;
using JWTAuth.Dtos;
using JWTAuth.Models;
using System.Data;

namespace JWTAuth.Data
{
    public class DataAccess(IDbConnection dbConnection) : IDataAccess
    {
        private const string ID_VARIABLE = "id";
        private const string EMAIL_VARIABLE = "email";
        private const string PASSWORDHASH_VARIABLE = "passwordhash";
        private const string USERNAME_VARIABLE = "username";
        private const string USERROLE_VARIABLE = "userrole";
        private const string REFRESHTOKEN_VARIABLE = "refreshtoken";
        private const string REFRESHTOKENEXPIRATIONTIME_VARIABLE = "refreshtokenexpirationtime";

        private const string USERID_VARIABLE = "userid";


        private const string REGISTER_USER_PROCEDURE = "dbo.registerUser";
        private const string USER_EXISTS_PROCEDURE = "dbo.userExists";
        private const string GET_USER_FROM_ID_PROCEDURE = "dbo.getUserFromId";
        private const string GET_USER_FROM_EMAIL_PROCEDURE = "dbo.getUserFromEmail";
        private const string SAVE_REFRESH_TOKEN_PROCEDURE = "dbo.saveRefreshToken";
        private const string GET_REFRESH_TOKEN_DATA_PROCEDURE = "dbo.getRefreshTokenData";
        private const string DELETE_USER_PROCEDURE = "dbo.deleteUser";

        public async Task RegisterUserAsync(User user)
        {
            //Set up DynamicParameters object to pass parameters
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(EMAIL_VARIABLE, user.Email);
            parameters.Add(PASSWORDHASH_VARIABLE, user.PasswordHash);
            parameters.Add(USERNAME_VARIABLE, user.Username);
            parameters.Add(USERROLE_VARIABLE, user.UserRole.ToString());

            //Execute stored procedure
            await dbConnection.ExecuteAsync
            (
                REGISTER_USER_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<bool> UserExistsAsync(UserRegisterDto userDto)
        {
            //Set up DynamicParameters object to pass parameters
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(EMAIL_VARIABLE, userDto.Email);

            //Execute stored procedure
            bool userExists = await dbConnection.QuerySingleAsync<bool>
            (
                USER_EXISTS_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return userExists;
        }

        public async Task<User?> GetUserFromIdAsync(int userId)
        {
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ID_VARIABLE, userId);

            User? user = await dbConnection.QueryFirstOrDefaultAsync<User>
            (
                GET_USER_FROM_ID_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );
            return user;
        }

        public async Task<User?> GetUserFromEmailAsync(string userEmail)
        {
            //Set up DynamicParameters object to pass parameters
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(EMAIL_VARIABLE, userEmail);

            //Execute stored procedure
            User? user = await dbConnection.QuerySingleOrDefaultAsync<User>
            (
                GET_USER_FROM_EMAIL_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return user;
        }

        public async Task SaveRefreshTokenAsync(int userId, RefreshTokenData refreshTokenData)
        {

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ID_VARIABLE, userId);
            parameters.Add(REFRESHTOKEN_VARIABLE, refreshTokenData.RefreshToken);
            parameters.Add(REFRESHTOKENEXPIRATIONTIME_VARIABLE, refreshTokenData.RefreshTokenExpirationTime);

            await dbConnection.ExecuteAsync
            (
                SAVE_REFRESH_TOKEN_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<RefreshTokenData?> GetRefreshTokenDataAsync(int userId)
        {
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(USERID_VARIABLE, userId);

            RefreshTokenData? refreshTokenData = await dbConnection.QueryFirstOrDefaultAsync<RefreshTokenData>
            (
                GET_REFRESH_TOKEN_DATA_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return refreshTokenData;
        }

        public async Task DeleteUserAsync(int userId)
        {
            DynamicParameters parameters = new();
            parameters.Add(ID_VARIABLE, userId);

            await dbConnection.ExecuteAsync
            (
                DELETE_USER_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
    }
}
