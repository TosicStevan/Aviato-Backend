using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
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
    [Route("api/Comment")]
    [Authorize]
    public class CommentController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();
        private readonly IHubContext<PostHub> _hubContext;

        public CommentController(IHubContext<PostHub> hubContext)
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


        [HttpPost("addComment")]
        public IActionResult AddComment([FromBody] Comment commentParam )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Request");
            }
            

            User user = GetUserInToken();

            if (user == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            Post post = db.Posts.Include(q => q.userId).SingleOrDefault(q => q.id == commentParam.post.id);

            Comment comment = new Comment();

            comment.user = user;
            comment.post = post;
            comment.text = commentParam.text;
            comment.date = DateTime.Now;
            Notification notification = new Notification();
            notification.post = post;
            notification.text = " comment your post ";
            notification.user = user;
            try
            {
                db.Comments.Add(comment);
                db.Notifications.Add(notification);
                db.SaveChanges();

                notification.user.password = null;
                notification.post.userId.password = null;

                SendNotification(notification, post.userId.username + "Notification");

                return Ok(comment);
            }
            catch
            {
                return BadRequest(new { msg = "Database error" });
            }
            
        }

        [HttpPost("deleteComment")]
        public IActionResult DeleteComment([FromBody] Comment commentParam)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Request");
            }

            Comment deleteComment = db.Comments.SingleOrDefault(q => q.id == commentParam.id);

            try
            {
                db.Comments.Remove(deleteComment);
                db.SaveChanges();

                return Ok(new { msg = "Comment successfully deleted" });
            }
            catch
            {
                return BadRequest(new { msg = "Database error" });
            }
            
        }
        



    }
}
