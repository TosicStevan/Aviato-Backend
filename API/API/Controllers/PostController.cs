using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTO;
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

            var postsWithComments = new List<PostDTO>();

            foreach (var item in posts)
            {
                var p = new PostDTO();
                p.post = item;

                var comments = db.Comments.Where(q => q.post == item).ToList();

                var commentsWithIsMy = new List<CommentDTO>();

                foreach (var c in comments)
                {
                    var com = new CommentDTO();
                    com.id = c.id;
                    com.user = c.user;
                    com.post = c.post;
                    com.text = c.text;
                    com.date = c.date;
                    if(c.user == user )
                    {
                        com.isMy = true;
                    }
                    else
                    {
                        com.isMy = false;
                    }
                    commentsWithIsMy.Add(com);
                }


                p.comments = commentsWithIsMy;

                var nubmerOfLikes = db.Likes.Where(q => q.post.id == item.id).Count();
                p.numberOfLikes = nubmerOfLikes;

                var isLiked = db.Likes.SingleOrDefault(q => q.post.id == item.id && item.userId.id == user.id);
                if(isLiked == null)
                {
                    p.isLiked = false;
                }
                else
                {
                    p.isLiked = true;
                }

                postsWithComments.Add(p);

            }


            return Ok(postsWithComments);
        }
    }
}
