using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTO;
using API.Hubs;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Produces("application/json")]
    [Route("api/Like")]
    [Authorize]
    public class LikeController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();
        private readonly IHubContext<PostHub> _hubContext;

        public LikeController(IHubContext<PostHub> hubContext)
        {
            _hubContext = hubContext;
        }

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

        private void SendNotification(Notification notification, string groupName)
        {
            _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", notification);
        }

        [HttpPost("likePost")]
        public IActionResult LikePost([FromBody] Post postParam)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Request");
            }

            Post post = db.Posts.Include(q => q.userId).SingleOrDefault(q => q.id == postParam.id);
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

                Notification n = new Notification();
                n.user = user;
                n.post = post;
                n.text = " unlike your post ";

                
                
                try
                {
                    db.Likes.Remove(isLiked);
                    db.Notifications.Add(n);
                    db.SaveChanges();

                    n.user.password = null;
                    n.post.userId.password = null;

                    SendNotification(n, post.userId.username + "Notification");

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

            Notification notification = new Notification();
            notification.user = user;
            notification.post = post;
            notification.text = " like your post ";

            try
            {
                db.Likes.Add(like);
                db.Notifications.Add(notification);
                db.SaveChanges();

                notification.user.password = null;
                notification.post.userId.password = null;

                SendNotification(notification, post.userId.username + "Notification");

                return Ok(new { like=true});
            }
            catch
            {
                return BadRequest(new { msg = "Database error" });
            }
            
        }
        
        
    }
}
