using System.Data;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Helpers;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class UserCompleteController : ControllerBase
{

    private readonly DataContextDapper _dapper;
    private readonly ReusableSql _reusableSql;
    public UserCompleteController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
        _reusableSql = new ReusableSql(config);
    }

    [HttpGet("TestConnection")]
    public DateTime TestConnection()
    {
        return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
    }

    [HttpGet("GetUsers/{userId}/{isActive}")]
    // public IActionResult Test()
    public IEnumerable<UserComplete> GetUsers(int userId = 0, bool isActive = true)
    {
        string sql = "EXEC TutorialAppSchema.spUsers_Get @UserId = @UserIdParam, @Active = @ActiveParam";

        DynamicParameters sqlParams = new DynamicParameters();
        sqlParams.Add("@UserIdParam", userId, DbType.Int32);
        sqlParams.Add("@ActiveParam", 1, DbType.Int32);

        return _dapper.LoadDataWithParams<UserComplete>(sql, sqlParams);
    }

    [HttpPut("EditUser")]
    public IActionResult UpsertUser(UserComplete user)
    {
        if (_reusableSql.UpsertUser(user))
        {
            return Ok();
        }
        throw new Exception("Failed to update user");
    }

    [HttpDelete("DeleteUser/{userId}")]

    public IActionResult DeleteUser(int userId)
    {
        string sql = "TutorialAppSchema.spUser_Delete @UserId = @UserIdParam";
        DynamicParameters sqlParams = new DynamicParameters();
        sqlParams.Add("@UserIdParam", userId, DbType.Int32);
        
        if (_dapper.ExecuteSqlWithParams(sql, sqlParams))
        {
            return Ok();
        }
        throw new Exception("Failed to delete user");
    }
}

