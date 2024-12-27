using System.Data;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PostController : ControllerBase{ 
        private readonly DataContextDapper _dapper;

        public PostController(IConfiguration config){
            _dapper = new DataContextDapper(config);
        }

        [HttpGet("Posts")]
        public IEnumerable<Post> GetPosts(int postId = 0, int userId = 0, string searchParam=""){
            string sql = @"EXEC TutorialAppSchema.spPosts_Get @UserId = @UserIdParam, @PostId = @PostIdParam";

            DynamicParameters sqlParams = new DynamicParameters();
            sqlParams.Add("@PostIdParam", postId, DbType.Int32);
            sqlParams.Add("@UserIdParam", userId, DbType.Int32);
            
            if(searchParam.ToLower() != "none"){
                sqlParams.Add("@SearchValParam", searchParam, DbType.String);
                sql += ", @SearchVal = @SearchValParam";
            }

            IEnumerable<Post> posts = _dapper.LoadDataWithParams<Post>(sql,sqlParams);
            return posts;
        }

        [HttpGet("MyPosts")]
        public IEnumerable<Post> GetMyPosts(){
            string sql = "EXEC TutorialAppSchema.spPosts_Get @UserId = @UserIdParam";
            DynamicParameters sqlParams = new DynamicParameters();
            sqlParams.Add("@UserIdParam", User.FindFirst("userId")?.Value, DbType.Int32);

            IEnumerable<Post> posts = _dapper.LoadData<Post>(sql);
            return posts;
        }

        [HttpPut("UpsertPost")]
        public IActionResult UpsertPost(Post postToAdd){
            string sql = @"
                EXEC TutorialAppSchema.spPosts_Upsert
                    @UserId = @UserIdParam, 
                    @PostTitle = @PostTitleParam,
                    @PostContent = @PostContentParam";
            
            DynamicParameters sqlParams = new DynamicParameters();
            sqlParams.Add("@UserIdParam", User.FindFirst("userId")?.Value, DbType.Int32);
            sqlParams.Add("@PostTitleParam", postToAdd.PostTitle, DbType.String);
            sqlParams.Add("@PostContentParam", postToAdd.PostContent, DbType.String);

            if(postToAdd.PostId != 0){
                sqlParams.Add("@PostIdParam", postToAdd.PostId, DbType.Int32);
                sql += ", @PostId = @PostIdParam";
            }

            if(_dapper.ExecuteSql(sql)){
                return Ok();
            }

            throw new Exception("Failed to add post");
        }

        [HttpDelete("Post/{postId}")]
        public IActionResult DeletePost(int postId){
            string sql = "EXEC TutorialAppSchema.spPost_Delete @UserId = @UserIdParam, @PostId = @PostIdParam";

            DynamicParameters sqlParams = new DynamicParameters();
            sqlParams.Add("@UserIdParam", User.FindFirst("userId")?.Value, DbType.Int32);
            sqlParams.Add("@PostIdParam", postId, DbType.Int32);

            if(_dapper.ExecuteSql(sql)){
                return Ok();
            }
            throw new Exception("Failed to delete post");
        }
    }
}