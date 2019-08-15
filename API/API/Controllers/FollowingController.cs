﻿using System;
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
        
        [HttpPost("follow")]
        public IActionResult Follow([FromBody] User userParam)
        {

            // find user in token
            var idClaim = User.Claims.FirstOrDefault(x => x.Type.Equals("id", StringComparison.InvariantCultureIgnoreCase));

            if (idClaim == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            var user = db.Users.SingleOrDefault(q => q.id.ToString() == idClaim.Value);

            Following follow = new Following();
            follow.follower = user;
            follow.isAccept = false;

            var userFollowed = db.Users.SingleOrDefault(q => q.username == userParam.username);
            if(userFollowed == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            follow.followed = userFollowed;

            var validFollow = db.Followings.SingleOrDefault(q => q.followed == userFollowed && q.follower == user);
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

        [HttpGet("getFollowRequest")]
        public IActionResult GetFollowRequest()
        {
            // find user in token
            var idClaim = User.Claims.FirstOrDefault(x => x.Type.Equals("id", StringComparison.InvariantCultureIgnoreCase));

            if (idClaim == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            var user = db.Users.SingleOrDefault(q => q.id.ToString() == idClaim.Value);

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
            var follow = db.Followings.SingleOrDefault(q => q.id == followParam.id);

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
        public IActionResult GetNumberOfFollowers()
        {
            // find user in token
            var idClaim = User.Claims.FirstOrDefault(x => x.Type.Equals("id", StringComparison.InvariantCultureIgnoreCase));

            if (idClaim == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            var user = db.Users.SingleOrDefault(q => q.id.ToString() == idClaim.Value);

            var numOfFollowers = db.Followings.Where(q => q.followed.id == user.id && q.isAccept == true).Count();


            return Ok(numOfFollowers);
        }

        [HttpGet("getFollowers")]
        public IActionResult GetFollowers()
        {
            // find user in token
            var idClaim = User.Claims.FirstOrDefault(x => x.Type.Equals("id", StringComparison.InvariantCultureIgnoreCase));

            if (idClaim == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            var user = db.Users.SingleOrDefault(q => q.id.ToString() == idClaim.Value);

            var followers = db.Followings.Include(q => q.follower).Where(q => q.followed.id == user.id && q.isAccept == true).ToList();

            foreach (var item in followers)
            {
                item.followed = null;
                item.follower.password = null;
            }

            return Ok(followers);
        }

        [HttpGet("getNumberOfFollowing")]
        public IActionResult GetNumberOfFollowing()
        {
            // find user in token
            var idClaim = User.Claims.FirstOrDefault(x => x.Type.Equals("id", StringComparison.InvariantCultureIgnoreCase));

            if (idClaim == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            var user = db.Users.SingleOrDefault(q => q.id.ToString() == idClaim.Value);

            var numOfFollowing = db.Followings.Where(q => q.follower.id == user.id && q.isAccept == true).Count();


            return Ok(numOfFollowing);
        }

        [HttpGet("getFollowing")]
        public IActionResult GetFollowing()
        {
            // find user in token
            var idClaim = User.Claims.FirstOrDefault(x => x.Type.Equals("id", StringComparison.InvariantCultureIgnoreCase));

            if (idClaim == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            var user = db.Users.SingleOrDefault(q => q.id.ToString() == idClaim.Value);

            var following = db.Followings.Include(q => q.followed).Where(q => q.follower.id == user.id && q.isAccept == true).ToList();

            foreach (var item in following)
            {
                item.followed.password = null;
                item.follower= null;
            }

            return Ok(following);
        }




    }
}
