using System.Security.Cryptography;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;

        private readonly AuthHelper _authHelper;

        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _authHelper = new AuthHelper(config);
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register([FromBody] UserForRegistrationDto userForRegistration)
        {
            if (userForRegistration.Password == userForRegistration.ConfirmPassword)
            {
                string sqlCheckUserExists = "SELECT Email FROM TutorialAppSchema.Auth WHERE Email = '" + userForRegistration.Email + "'";
                IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists);
                if (existingUsers.Count() == 0)
                {
                    byte[] passWordSalt = new byte[128 / 8];
                    using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                    {
                        rng.GetNonZeroBytes(passWordSalt);
                    }

                    byte[] passwordHash = _authHelper.HashingFunction(userForRegistration.Password, passWordSalt);

                    string sqlAddUserToAuth = "INSERT INTO TutorialAppSchema.Auth (Email, PasswordHash, PasswordSalt) VALUES ('" + userForRegistration.Email + "', @PasswordHash, @PasswordSalt)";

                    var sqlParameters = new
                    {
                        Email = userForRegistration.Email,
                        PasswordHash = passwordHash,
                        PasswordSalt = passWordSalt
                    };

                    if (_dapper.ExecuteSql(sqlAddUserToAuth, sqlParameters))
                    {
                        string sqlAddUsertoUser = @"
                        INSERT INTO TutorialAppSchema.Users (FirstName, LastName, Email, Gender, Active)
                            VALUES (@FirstName, @LastName, @Email, @Gender, @Active)";

                        var sqlParametersUser = new
                        {
                            FirstName = userForRegistration.FirstName,
                            LastName = userForRegistration.LastName,
                            Email = userForRegistration.Email,
                            Gender = userForRegistration.Gender,
                            Active = true
                        };

                        if (_dapper.ExecuteSql(sqlAddUsertoUser, sqlParametersUser))
                        {
                            return Ok();
                        }

                        throw new Exception("Failed to add user");
                    }

                    throw new Exception("Failed to add user");

                }
                throw new Exception("User already exists");

            }
            throw new Exception("Passwords do not match");
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login([FromBody] UserForLoginDto userForLogin)
        {
            string sqlForHashAndSalt = "SELECT PasswordHash, PasswordSalt FROM TutorialAppSchema.Auth WHERE Email = '" + userForLogin.Email + "'";
            UserForLoginConfirmationDto hashAndSalt = _dapper.LoadDataSingle<UserForLoginConfirmationDto>(sqlForHashAndSalt);

            if (hashAndSalt != null && hashAndSalt.PasswordHash != null && hashAndSalt.PasswordSalt != null)
            {
                byte[] passwordHash = _authHelper.HashingFunction(userForLogin.Password, hashAndSalt.PasswordSalt);

                for (int i = 0; i < passwordHash.Length; i++)
                {
                    if (passwordHash[i] != hashAndSalt.PasswordHash[i])
                    {
                        return StatusCode(401, "Invalid Password");
                    }
                }

                string userIdSql = "SELECT UserId FROM TutorialAppSchema.Users WHERE Email = '" + userForLogin.Email + "'";
                int userId = _dapper.LoadDataSingle<int>(userIdSql);

                return Ok(new { token = _authHelper.CreateToken(userId) });
            }

            throw new Exception("User does not exist");
        }

        [HttpGet("RefreshToken")]
        public IActionResult RefreshToken()
        {
            //User from ControllerBase
            string userIdString = User.FindFirst("userId")?.Value +"";
            
            string userIdSql = "SELECT UserId FROM TutorialAppSchema.Users WHERE UserId = " + userIdString;
            int userIdFromDb = _dapper.LoadDataSingle<int>(userIdSql);

            return Ok(new { token = _authHelper.CreateToken(userIdFromDb) });
        }

        
    }
}