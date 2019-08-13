using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Post")]
    public class PostController : Controller
    {

        AppDbContext db = new AppDbContext();

        [HttpPost("add")]
        public IActionResult AddPost([FromBody] Post postParam )
        {

            Post post = new Post();

            // find user in token
            var idClaim = User.Claims.FirstOrDefault(x => x.Type.Equals("id", StringComparison.InvariantCultureIgnoreCase));

            if (idClaim == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            var user = db.Users.SingleOrDefault(q => q.id.ToString() == idClaim.Value);

            if(user != null)
            {
                post.userId = user;
            }

            post.post = postParam.post;
            post.date = DateTime.Now;

            try
            {
                db.Posts.Add(post);
                db.SaveChanges();
                
                return Ok(post);
            }
            catch
            {
                return BadRequest(new { msg = "Database error" });
            }
            
        }

        [HttpGet("getMyPosts")]
        public IActionResult GetMyPost()
        {

            // find user in token
            var idClaim = User.Claims.FirstOrDefault(x => x.Type.Equals("id", StringComparison.InvariantCultureIgnoreCase));

            if (idClaim == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            var user = db.Users.SingleOrDefault(q => q.id.ToString() == idClaim.Value);

            var posts = db.Posts.Where(q => q.userId == user).ToList();

            return Ok(posts);
        }
    }
}
