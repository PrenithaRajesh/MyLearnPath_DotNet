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
        public IEnumerable<Post> GetPosts(){
            string sql = "SELECT * FROM TutorialAppSchema.Posts";
            IEnumerable<Post> posts = _dapper.LoadData<Post>(sql);
            return posts;
        }

        [HttpGet("PostSingle/{postId}")]
        public Post GetPostSingle(int postId){
            string sql = "SELECT * FROM TutorialAppSchema.Posts WHERE PostId = " + postId.ToString();
            Post post = _dapper.LoadDataSingle<Post>(sql);
            return post;
        }

        [HttpGet("PostsByUser/{userId}")]
        public IEnumerable<Post> GetPostsByUser(int userId){
            string sql = "SELECT * FROM TutorialAppSchema.Posts WHERE UserId = " + userId.ToString();
            IEnumerable<Post> posts = _dapper.LoadData<Post>(sql);
            return posts;
        }

        [HttpGet("MyPosts")]
        public IEnumerable<Post> GetMyPosts(){
            string sql = "SELECT * FROM TutorialAppSchema.Posts WHERE UserId = " + User.FindFirst("userId")?.Value;
            IEnumerable<Post> posts = _dapper.LoadData<Post>(sql);
            return posts;
        }

        [HttpPost("Post")]
        public IActionResult AddPost(PostToAddDto postToAdd){
            string sql = @"
                INSERT INTO TutorialAppSchema.Posts (UserId, PostTitle, PostContent, PostCreated, PostUpdated)
                    VALUES (@UserId, @PostTitle, @PostContent, @PostCreated, @PostUpdated)";
            
            var sqlParameters = new{
                UserId = User.FindFirst("userId")?.Value,
                PostTitle = postToAdd.PostTitle,
                PostContent = postToAdd.PostContent,
                PostCreated = DateTime.Now,
                PostUpdated = DateTime.Now
            };

            if(_dapper.ExecuteSql(sql, sqlParameters)){
                return Ok();
            }
            throw new Exception("Failed to add post");
        }

        [HttpPut("Post")]
        public IActionResult EditPost(PostToEditDto postToEdit){
            string sql = @"
                UPDATE TutorialAppSchema.Posts
                    SET PostTitle = @PostTitle, PostContent = @PostContent, PostUpdated = @PostUpdated
                    WHERE PostId = @PostId AND UserId = " + User.FindFirst("userId")?.Value;
            
            var sqlParameters = new{
                PostId = postToEdit.PostId,
                PostTitle = postToEdit.PostTitle,
                PostContent = postToEdit.PostContent,
                PostUpdated = DateTime.Now
            };
            
            if(_dapper.ExecuteSql(sql, sqlParameters)){
                return Ok();
            }
            throw new Exception("Failed to edit post");
        }

        [HttpDelete("Post/{postId}")]
        public IActionResult DeletePost(int postId){
            string sql = "DELETE FROM TutorialAppSchema.Posts WHERE PostId = @PostId AND UserId = " + User.FindFirst("userId")?.Value;
            var sqlParameters = new{PostId = postId};

            if(_dapper.ExecuteSql(sql, sqlParameters)){
                return Ok();
            }
            throw new Exception("Failed to delete post");
        }

        [HttpGet("PostsBySearch/{search}")]
        public IEnumerable<Post> GetPostBySearch(string search){
            string sql = "SELECT * FROM TutorialAppSchema.Posts WHERE PostTitle LIKE '%" + search + "%' OR PostContent LIKE '%" + search + "%'";
            IEnumerable<Post> posts = _dapper.LoadData<Post>(sql);
            return posts;
        }
    }
}