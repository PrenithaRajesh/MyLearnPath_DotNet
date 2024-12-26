using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController: ControllerBase{

    DataContextDapper _dapper;

    public UserController(IConfiguration config){
        _dapper = new DataContextDapper(config);
    }

    [HttpGet("TestConnection")]
    public DateTime TestConnection(){
        return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
    }

    [HttpGet("GetUsers")]
    // public IActionResult Test()
    public IEnumerable<User> GetUsers()
    {
        string sql = "SELECT * FROM TutorialAppSchema.Users";
        return _dapper.LoadData<User>(sql);
        
    }

    [HttpGet("GetSingleUser/{userId}")]
    // public IActionResult Test()
    public User GetSingleUser(int userId)
    {
        string sql = $"SELECT * FROM TutorialAppSchema.Users WHERE UserId = {userId}";
        return _dapper.LoadDataSingle<User>(sql);
    }

    [HttpPut("EditUser")]
    public IActionResult EditUser(User user)
    {
        string sql = @"
        UPDATE TutorialAppSchema.Users
            SET FirstName = @FirstName,
                LastName = @LastName,
                Email = @Email,
                Gender = @Gender,
                Active = @Active
            WHERE UserId = "+ user.UserId;

        if(_dapper.ExecuteSql<User>(sql, user)){
            return Ok();
        }
        throw new Exception("Failed to update user");
    }

    [HttpPost("AddUser")]
    public IActionResult AddUser(UserToAddDto user)
    {
        string sql = @"
        INSERT INTO TutorialAppSchema.Users (FirstName, LastName, Email, Gender, Active)
            VALUES (@FirstName, @LastName, @Email, @Gender, @Active)";
        
        if(_dapper.ExecuteSql<UserToAddDto>(sql, user)){
            return Ok();
        }

        throw new Exception("Failed to add user");
    }

    [HttpDelete("DeleteUser/{userId}")]

    public IActionResult DeleteUser(int userId)
    {
        string sql = $"DELETE FROM TutorialAppSchema.Users WHERE UserId = {userId}";
        if(_dapper.ExecuteSql<int>(sql, userId)){
            return Ok();
        }
        throw new Exception("Failed to delete user");
    }
}

