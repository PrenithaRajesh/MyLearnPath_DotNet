using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;

namespace DotnetAPI.Helpers
{
    public class AuthHelper
    {
        private readonly IConfiguration _config;
        private readonly DataContextDapper _dapper;
        public AuthHelper(IConfiguration config)
        {
            _config = config;
            _dapper = new DataContextDapper(config);
        }

        public byte[] HashingFunction(string password, byte[] salt)
        {
            string passWordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value + Convert.ToBase64String(salt);

            byte[] passwordHash = KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.ASCII.GetBytes(passWordSaltPlusString),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8);

            return passwordHash;
        }

        public string CreateToken(int userId)
        {
            Claim[] claims = new[]
            {
                new Claim("userId", userId.ToString())
            };

            string? tokenKeyString = _config.GetSection("AppSettings:TokenKey").Value;

            SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        tokenKeyString != null ? tokenKeyString : ""
                    )
            );

            SigningCredentials creds = new SigningCredentials(tokenKey, SecurityAlgorithms.HmacSha512Signature);

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public bool SetPassword(UserForLoginDto userForSetPassword)
        {

            byte[] passWordSalt = new byte[128 / 8];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(passWordSalt);
            }

            byte[] passwordHash = HashingFunction(userForSetPassword.Password, passWordSalt);

            string sqlAddUserToAuth = "EXEC TutorialAppSchema.spRegistration_Upsert @Email = @EmailParam, @PasswordHash = @PasswordHashParam, @PasswordSalt = @PasswordSaltParam";

            var sqlParameters = new
            {
                EmailParam = userForSetPassword.Email,
                PasswordHashParam = passwordHash,
                PasswordSaltParam = passWordSalt
            };

            return _dapper.ExecuteSql(sqlAddUserToAuth, sqlParameters);
        }
    }
}