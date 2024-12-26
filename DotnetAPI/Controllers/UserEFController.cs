using AutoMapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserEFController : ControllerBase
{
    // DataContextEF _entityFramework;
    IMapper _mapper;
    IUserRepository _userRepository;

    public UserEFController(IConfiguration config, IUserRepository userRepository)
    {
        // _entityFramework = new DataContextEF(config);
        _userRepository = userRepository;

        _mapper = new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<UserToAddDto, User>();
        }));

    }

    [HttpGet("GetUsers")]
    public IEnumerable<User> GetUsers()
    {
        return _userRepository.GetUsers();
    }

    [HttpGet("GetSingleUser/{userId}")]
    public User GetSingleUser(int userId)
    {
        return _userRepository.GetSingleUser(userId);
    }

    [HttpPut("EditUser")]
    public IActionResult EditUser(User user)
    {
        User? userDb = _userRepository.GetSingleUser(user.UserId);

        if (userDb != null)
        {
            userDb.Active = user.Active;
            userDb.FirstName = user.FirstName;
            userDb.LastName = user.LastName;
            userDb.Email = user.Email;
            userDb.Gender = user.Gender;
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }

            throw new Exception("Failed to Update User");
        }

        throw new Exception("Failed to Get User");
    }


    [HttpPost("AddUser")]
    public IActionResult AddUser(UserToAddDto user)
    {
        User userDb = _mapper.Map<User>(user);

        _userRepository.AddEntity<User>(userDb);
        if (_userRepository.SaveChanges())
        {
            return Ok();
        }

        throw new Exception("Failed to Add User");
    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        User? userDb = _userRepository.GetSingleUser(userId);

        if (userDb != null)
        {
            _userRepository.RemoveEntity<User>(userDb);
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }

            throw new Exception("Failed to Delete User");
        }

        throw new Exception("Failed to Get User");
    }
}