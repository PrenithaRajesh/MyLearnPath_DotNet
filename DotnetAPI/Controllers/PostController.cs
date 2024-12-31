using System.Data;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PostController : ControllerBase
    {
        private readonly DataContextDapper _dapper;

        public PostController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }

        [HttpGet("Posts/{postId:int}/{userId:int}/{searchParam}")]
        public IEnumerable<Post> GetPosts(int postId = 0, int userId = 0, string searchParam = "none")
        {
            string sql = @"EXEC TutorialAppSchema.spPosts_Get";
            string strparams = " ";

            DynamicParameters sqlParams = new DynamicParameters();

            bool flag = false;

            if (postId != 0)
            {
                sqlParams.Add("@PostIdParam", postId, DbType.Int32);
                strparams += ", @PostId = @PostIdParam";
                flag = true;
            }

            if (userId != 0)
            {
                sqlParams.Add("@UserIdParam", userId, DbType.Int32);
                strparams += flag ? " , @UserId = @UserIdParam" : " @UserId = @UserIdParam";
                flag = true;
            }

            if (searchParam.ToLower() != "none")
            {
                sqlParams.Add("@SearchValParam", searchParam, DbType.String); // Ensure correct parameter name
                strparams += flag ? " , @SearchVal = @SearchValParam" : " @SearchVal = @SearchValParam";
            }

            if (strparams[0] == ',')
            {
                strparams = strparams.Substring(1);
            }
            sql += strparams;
            
            Console.WriteLine("SQL Query: " + sql);

            IEnumerable<Post> posts = _dapper.LoadDataWithParams<Post>(sql, sqlParams);
            return posts;
        }


        [HttpGet("MyPosts")]
        public IEnumerable<Post> GetMyPosts()
        {
            string sql = "EXEC TutorialAppSchema.spPosts_Get @UserId = @UserIdParam";
            DynamicParameters sqlParams = new DynamicParameters();
            sqlParams.Add("@UserIdParam", User.FindFirst("userId")?.Value, DbType.Int32);

            IEnumerable<Post> posts = _dapper.LoadDataWithParams<Post>(sql, sqlParams);
            return posts;
        }

        [HttpPut("UpsertPost")]
        public IActionResult UpsertPost(Post postToAdd)
        {
            string sql = @"
                EXEC TutorialAppSchema.spPosts_Upsert
                    @UserId = @UserIdParam, 
                    @PostTitle = @PostTitleParam,
                    @PostContent = @PostContentParam";

            DynamicParameters sqlParams = new DynamicParameters();
            sqlParams.Add("@UserIdParam", User.FindFirst("userId")?.Value, DbType.Int32);
            sqlParams.Add("@PostTitleParam", postToAdd.PostTitle, DbType.String);
            sqlParams.Add("@PostContentParam", postToAdd.PostContent, DbType.String);

            if (postToAdd.PostId != 0)
            {
                sqlParams.Add("@PostIdParam", postToAdd.PostId, DbType.Int32);
                sql += ", @PostId = @PostIdParam";
            }

            if (_dapper.ExecuteSqlWithParams(sql, sqlParams))
            {
                return Ok();
            }

            throw new Exception("Failed to add post");
        }

        [HttpDelete("Post/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            string sql = "EXEC TutorialAppSchema.spPost_Delete @UserId = @UserIdParam, @PostId = @PostIdParam";

            DynamicParameters sqlParams = new DynamicParameters();
            sqlParams.Add("@UserIdParam", User.FindFirst("userId")?.Value, DbType.Int32);
            sqlParams.Add("@PostIdParam", postId, DbType.Int32);

            if (_dapper.ExecuteSqlWithParams(sql, sqlParams))
            {
                return Ok();
            }
            throw new Exception("Failed to delete post");
        }
    }
}