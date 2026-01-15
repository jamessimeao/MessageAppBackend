using Dapper;
using JWTAuth.Dtos;
using JWTAuth.Models;
using System.Data;

namespace JWTAuth.Data
{
    public class DataAccess(IDbConnection dbConnection) : IDataAccess
    {
        private const string REGISTER_USER_PROCEDURE = "dbo.registerUser";
        private const string USER_EXISTS_PROCEDURE = "dbo.userExists";
        private const string GET_USER_FROM_ID_PROCEDURE = "dbo.getUserFromId";
        private const string GET_USER_FROM_EMAIL_PROCEDURE = "dbo.getUserFromEmail";
        private const string SAVE_REFRESH_TOKEN_PROCEDURE = "dbo.saveRefreshToken";
        private const string GET_REFRESH_TOKEN_DATA_PROCEDURE = "dbo.getRefreshTokenData";

        public async Task RegisterUser(User user)
        {
            //Set up DynamicParameters object to pass parameters
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("email", user.Email);
            parameters.Add("passwordhash", user.PasswordHash);
            parameters.Add("username", user.Username);
            parameters.Add("userrole", user.UserRole.ToString());

            //Execute stored procedure
            await dbConnection.ExecuteAsync
            (
                REGISTER_USER_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<bool> UserExists(UserRegisterDto userDto)
        {
            //Set up DynamicParameters object to pass parameters
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("email", userDto.Email);

            //Execute stored procedure
            bool userExists = await dbConnection.QuerySingleAsync<bool>
            (
                USER_EXISTS_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return userExists;
        }

        public async Task<User?> GetUserFromId(int userId)
        {
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("id", userId);

            User? user = await dbConnection.QueryFirstOrDefaultAsync<User>
            (
                GET_USER_FROM_ID_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );
            return user;
        }

        public async Task<User?> GetUserFromEmail(string userEmail)
        {
            //Set up DynamicParameters object to pass parameters
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("email", userEmail);

            //Execute stored procedure
            User? user = await dbConnection.QuerySingleOrDefaultAsync<User>
            (
                GET_USER_FROM_EMAIL_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return user;
        }

        public async Task SaveRefreshToken(int userId, RefreshTokenData refreshTokenData)
        {

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("id", userId);
            parameters.Add("refreshtoken", refreshTokenData.RefreshToken);
            parameters.Add("refreshtokenexpirationtime", refreshTokenData.RefreshTokenExpirationTime);

            await dbConnection.ExecuteAsync
            (
                SAVE_REFRESH_TOKEN_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<RefreshTokenData?> GetRefreshTokenData(int userId)
        {
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("userId", userId);

            RefreshTokenData? refreshTokenData = await dbConnection.QueryFirstOrDefaultAsync<RefreshTokenData>
            (
                GET_REFRESH_TOKEN_DATA_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return refreshTokenData;
        }
    }
}
