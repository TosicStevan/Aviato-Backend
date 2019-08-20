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
    [Route("api/User")]
    public class UserController : Controller
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

        [HttpGet("getUserByUsername")]
        public IActionResult GetUserProfileByUsername([FromQuery(Name = "username")]string username)
        {
            User userInToken = GetUserInToken();

            if(userInToken == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            User user = db.Users.SingleOrDefault(q => q.username == username);

            if(user == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }



            UserDTO userDTO = new UserDTO();
            if(user.id == userInToken.id)
            {
                userDTO.isMine = true;
            }
            else
            {
                userDTO.isMine = false;
            }
            userDTO.id = user.id;
            userDTO.image = user.image;
            userDTO.lastName = user.lastName;
            userDTO.firstName = user.firstName;
            userDTO.username = user.username;
            userDTO.email = user.email;
            userDTO.isPublic = user.isPublic;

            int numberOfPosts = db.Posts.Where(q => q.userId == user).Count();
            

            var isFollowing = db.Followings.SingleOrDefault(q => q.follower == userInToken && q.followed == user && q.isAccept == true);
            if(isFollowing == null)
            {
                userDTO.isFollowing = false;
            }
            else
            {
                userDTO.isFollowing = true;
            }

            Following requestSend = db.Followings.SingleOrDefault(q => q.follower == userInToken && q.followed == user && q.isAccept==false);
            if(requestSend == null)
            {
                userDTO.requestSend = false;
            }
            else
            {
                userDTO.requestSend = true;
            }

            return Ok(userDTO);
        }

        [HttpPost("changeNames")]
        public IActionResult ChangeNames([FromBody] User userParam)
        {
            User user = GetUserInToken();

            if (user == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            // username is unique
            User usernameExist = db.Users.SingleOrDefault(q => q.username == userParam.username);

            if (user.username != userParam.username &&  usernameExist != null)
            {
                return BadRequest(new { msg = "Username already exist" });
            }
            


            try
            {
                user.username = userParam.username;
                user.firstName = userParam.firstName;
                user.lastName = userParam.lastName;

                db.SaveChanges();
                user.password = null;
                return Ok(user);
            }
            catch
            {
                return BadRequest(new { msg = "Database error" });
            }
        }

        [HttpPost("changeImage")]
        public IActionResult ChangeImage([FromBody] User userParam)
        {
            User user = GetUserInToken();

            if (user == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            try
            {
                user.image = userParam.image;
                db.SaveChanges();

                user.password = null;
                return Ok(user);
            }
            catch
            {
                return BadRequest(new { msg = "Database error" });
            }
        }

        [HttpPost("changeEmail")]
        public IActionResult ChangeEmail([FromBody] User userParam)
        {
            User user = GetUserInToken();

            if (user == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            User emailExist = db.Users.SingleOrDefault(q => q.email == userParam.email);
            if(emailExist != null)
            {
                return BadRequest(new { msg = "Email already exist" });
            }

            try
            {
                user.email = userParam.email;
                db.SaveChanges();

                user.password = null;
                return Ok(user);
            }
            catch
            {
                return BadRequest(new { msg = "Database error" });
            }
        }

        [HttpPost("changePassword")]
        public IActionResult ChangePassword([FromBody] UserPasswordDTO userParam)
        {
            User user = GetUserInToken();

            if (user == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            bool validOldPassword = BCrypt.Net.BCrypt.Verify(userParam.oldPassword, user.password);

            if (!validOldPassword)
            {
                return BadRequest(new { msg = "Incorrect old password" });
            }


            bool validPassword = BCrypt.Net.BCrypt.Verify(userParam.password, user.password);

            if (validPassword)
            {
                return BadRequest(new { msg = "Same old and new password" });
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userParam.password);
            

            try
            {
                user.password = hashedPassword;
                db.SaveChanges();
                user.password = null;
                return Ok(user);
            }
            catch
            {
                return BadRequest(new { msg = "Database error" });
            }

        }

        [HttpPost("searchUsers")]
        public IActionResult SearchUsers([FromBody] SearchDTO searchParam)
        {
            //search by username and full name
            var users = db.Users.Select(q => new { name = q.firstName + " " + q.lastName, id=q.id, image=q.image, username = q.username, email= q.email})
                .Where(q => q.name.ToLower().Contains(searchParam.search.ToLower()) || q.username.ToLower().Contains(searchParam.search.ToLower())).ToList().Take(5);
            return Ok(users);
        }

        [HttpPost("changePrivacy")]
        public IActionResult ChangePrivacy()
        {
            User user = GetUserInToken();

            if (user == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            try
            {
                user.isPublic = !user.isPublic;
                db.SaveChanges();
                user.password = null;
                return Ok();
            }
            catch
            {
                return BadRequest(new { msg = "Database error" });
            }
        }

    }
}
