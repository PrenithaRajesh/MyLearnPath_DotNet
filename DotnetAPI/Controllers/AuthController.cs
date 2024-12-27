using AutoMapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Helpers;
using DotnetAPI.Models;
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
        private readonly ReusableSql _reusableSql;
        private readonly AuthHelper _authHelper;
        private readonly IMapper _mapper;
        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _authHelper = new AuthHelper(config);
            _reusableSql = new ReusableSql(config);
            _mapper = new Mapper(new MapperConfiguration(cfg => cfg.CreateMap<UserForRegistrationDto, UserComplete>()));
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
                    UserForLoginDto userForLogin = new UserForLoginDto
                    {
                        Email = userForRegistration.Email,
                        Password = userForRegistration.Password
                    };

                    if (_authHelper.SetPassword(userForLogin))
                    {
                        if (_reusableSql.UpsertUser(_mapper.Map<UserComplete>(userForRegistration)))
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
            string sqlForHashAndSalt = "EXEC TutorialAppSchema.spLoginConfirmation_Get @Email = " + userForLogin.Email;
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

        [HttpPut("ResetPassword")]
        public IActionResult ResetPassword(UserForLoginDto userForSetPassword){
            if(userForSetPassword.Password == userForSetPassword.ConfirmPassword){
                if(_authHelper.SetPassword(userForSetPassword)){
                    return Ok();
                }
                throw new Exception("Failed to reset password");
            }
            throw new Exception("Passwords do not match");
        }
        
    }
}