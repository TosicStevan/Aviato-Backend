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
    [Route("api/Following")]
    [Authorize]
    public class FollowingController : Controller
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


        [HttpPost("follow")]
        public IActionResult Follow([FromBody] User userParam)
        {
            User user = GetUserInToken();

            if (user == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            Following follow = new Following();
            follow.follower = user;

            
            

            User userFollowed = db.Users.SingleOrDefault(q => q.username == userParam.username);

            if (userFollowed.isPublic == true)
            {
                follow.isAccept = true;
            }
            else
            {
                follow.isAccept = false;
            }
            if (userFollowed == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            follow.followed = userFollowed;

            Following validFollow = db.Followings.SingleOrDefault(q => q.followed == userFollowed && q.follower == user);
            if(validFollow != null)
            {
                return BadRequest(new { msg = "Follow already exist" });
            }

            try
            {
                db.Followings.Add(follow);
                db.SaveChanges();
                return Ok(new { msg="Follow send"});
            }
            catch
            {
                return BadRequest(new { msg = "Database error" });
            }
            
        }

        [HttpPost("declineFollow")]
        public IActionResult DeclineFoloow([FromBody] Following followParam)
        {
            Following follow = db.Followings.SingleOrDefault(q => q.id == followParam.id);

            if (follow == null)
            {
                return BadRequest(new { msg = "Invalid follow" });
            }

            try
            {
                db.Followings.Remove(follow);
                db.SaveChanges();
                return Ok(new { msg = "Declined" });
            }
            catch
            {
                return BadRequest(new { msg = "Database error" });
            }
        }

        [HttpPost("unfollow")]
        public IActionResult UnFollow([FromBody] User userParam)
        {
            User userInToken = GetUserInToken();

            if (userInToken == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            User user = db.Users.SingleOrDefault(q => q.username == userParam.username);

            Following follow = db.Followings.SingleOrDefault(q => q.follower == userInToken && q.followed == user);

            try
            {
                db.Followings.Remove(follow);
                db.SaveChanges();
                return Ok(new { msg = "Unfollowed" });
            }
            catch
            {
                return BadRequest(new { msg = "Database error" });
            }
            
        }

        [HttpGet("getFollowRequest")]
        public IActionResult GetFollowRequest()
        {
            User user = GetUserInToken();

            if (user == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            var followRequest = db.Followings.Include(q => q.follower).Where(q => q.followed.id == user.id && q.isAccept == false).ToList();

            foreach (var item in followRequest)
            {
                item.follower.password = null;
                item.followed = null;
            }

            return Ok(followRequest);
        }

        [HttpPost("acceptFollow")]
        public IActionResult AcceptFollow([FromBody] Following followParam)
        {
            Following follow = db.Followings.SingleOrDefault(q => q.id == followParam.id);

            if(follow == null)
            {
                return BadRequest(new { msg = "Invalid follow" });
            }

            try
            {
                follow.isAccept = true;
                db.SaveChanges();
                return Ok(new { msg="Follow accepted" });
            }
            catch
            {
                return BadRequest(new { msg = "Database error" });
            }
            
        }

        [HttpGet("getNumberOfFollowers")]
        public IActionResult GetNumberOfFollowers([FromQuery(Name = "username")]string username)
        {
            User user = db.Users.SingleOrDefault(q => q.username == username);
            if (user == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }
            

            int numOfFollowers = db.Followings.Where(q => q.followed.id == user.id && q.isAccept == true).Count();


            return Ok(numOfFollowers);
        }

        [HttpGet("getFollowers")]
        public IActionResult GetFollowers([FromQuery(Name = "username")]string username)
        {
            User user = db.Users.SingleOrDefault(q => q.username == username);
            if(user == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            var followers = db.Followings.Include(q => q.follower).Where(q => q.followed.id == user.id && q.isAccept == true).ToList();

            foreach (var item in followers)
            {
                item.followed = null;
                item.follower.password = null;
            }

            return Ok(followers);
        }

        [HttpGet("getNumberOfFollowing")]
        public IActionResult GetNumberOfFollowing([FromQuery(Name = "username")]string username)
        {
            User user = db.Users.SingleOrDefault(q => q.username == username);
            if (user == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            int numOfFollowing = db.Followings.Where(q => q.follower.id == user.id && q.isAccept == true).Count();


            return Ok(numOfFollowing);
        }

        [HttpGet("getFollowing")]
        public IActionResult GetFollowing([FromQuery(Name = "username")]string username)
        {
            User user = db.Users.SingleOrDefault(q => q.username == username);
            if (user == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            var following = db.Followings.Include(q => q.followed).Where(q => q.follower.id == user.id && q.isAccept == true).ToList();

            foreach (var item in following)
            {
                item.followed.password = null;
                item.follower= null;
            }

            return Ok(following);
        }

        [HttpGet("getOnlineFollowing")]
        public IActionResult GetOnlineFollowing([FromQuery(Name = "username")]string username)
        {
            User user = db.Users.SingleOrDefault(q => q.username == username);
            if (user == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            var following = db.Followings.Include(q => q.followed).Where(q => q.followed.isOnline == true && q.follower.id == user.id && q.isAccept == true).ToList();

            var onlineFollowing = new List<User>();

            foreach (var item in following)
            {
                item.followed.password = null;
                item.follower = null;
                onlineFollowing.Add(item.followed);
                
            }

            return Ok(onlineFollowing);
        }





    }
}
