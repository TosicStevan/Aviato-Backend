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
    [Produces("application/json")]
    [Route("api/Like")]
    [Authorize]
    public class LikeController : Controller
    {
        AppDbContext db = new AppDbContext();

        private User GetUserInToken()
        {
            // find user in token
            var idClaim = User.Claims.FirstOrDefault(x => x.Type.Equals("id", StringComparison.InvariantCultureIgnoreCase));

            if (idClaim == null)
            {
                return null;
            }

            User userInToken = db.Users.SingleOrDefault(q => q.id.ToString() == idClaim.Value);

            return userInToken;
        }

        [HttpPost("likePost")]
        public IActionResult LikePost([FromBody] Post postParam)
        {

            Post post = db.Posts.SingleOrDefault(q => q.id == postParam.id);

            if(post == null)
            {
                return BadRequest(new { msg = "Invalid post" });
            }

            User user = GetUserInToken();

            if (user == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            var isLiked = db.Likes.SingleOrDefault(q => q.post.id == postParam.id && q.user.id == user.id);
            if(isLiked != null)
            {
                try
                {
                    db.Likes.Remove(isLiked);
                    db.SaveChanges();
                    
                    return Ok(new { like= false});
                }
                catch
                {
                    return BadRequest(new { msg = "Database eroor" });
                }
            }


            Like like = new Like();
            like.post = post;
            like.user = user;

            try
            {
                db.Likes.Add(like);
                db.SaveChanges();
                return Ok(new { like=true});
            }
            catch
            {
                return BadRequest(new { msg = "Database error" });
            }
            
        }


        //[HttpPost("unlikePost")]
        //public IActionResult UnlikePost([FromBody] Post postParam )
        //{

        //    var post = db.Posts.SingleOrDefault(q => q.id == postParam.id);

        //    if (post == null)
        //    {
        //        return BadRequest(new { msg = "Invalid post" });
        //    }

        //    // find user in token
        //    var idClaim = User.Claims.FirstOrDefault(x => x.Type.Equals("id", StringComparison.InvariantCultureIgnoreCase));

        //    if (idClaim == null)
        //    {
        //        return BadRequest(new { msg = "Invalid user" });
        //    }

        //    var user = db.Users.SingleOrDefault(q => q.id.ToString() == idClaim.Value);

        //    var like = db.Likes.SingleOrDefault(q => q.post.id == postParam.id && q.user.id == user.id);

        //    try
        //    {
        //        db.Likes.Remove(like);
        //        db.SaveChanges();
        //        return Ok(like);
        //    }
        //    catch
        //    {
        //        return BadRequest(new { msg = "Database eroor" });
        //    }
            
        //}
        
    }
}
