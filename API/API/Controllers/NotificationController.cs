using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Produces("application/json")]
    [Route("api/Notification")]
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

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

        [HttpGet("getNotifications")]
        public IActionResult GetNotifications()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Request");
            }

            User user = GetUserInToken();
            if(user == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            var notifications = db.Notifications.Include(q => q.user).Include(q => q.post).Where(q => q.post.userId == user).OrderByDescending(q => q.id).Take(10).ToList();

            foreach (var item in notifications)
            {
                item.user.password = null;
                item.post.userId = null;
            }

            return Ok(notifications);
        }
    }
}
