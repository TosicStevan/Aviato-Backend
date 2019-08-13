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

        [HttpGet("getUser")]
        public IActionResult GetUserProfile([FromBody] User userParam)
        {
            // find user in token
            var idClaim = User.Claims.FirstOrDefault(x => x.Type.Equals("id", StringComparison.InvariantCultureIgnoreCase));

            if (idClaim == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            var user = db.Users.SingleOrDefault(q => q.id.ToString() == idClaim.Value);
            user.password = null;


            return Ok(user);
        }

        [HttpPost("changeNames")]
        public IActionResult ChangeNames([FromBody] User userParam)
        {
            // find user in token
            var idClaim = User.Claims.FirstOrDefault(x => x.Type.Equals("id", StringComparison.InvariantCultureIgnoreCase));

            if (idClaim == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            var user = db.Users.SingleOrDefault(q => q.id.ToString() == idClaim.Value);

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
            // find user in token
            var idClaim = User.Claims.FirstOrDefault(x => x.Type.Equals("id", StringComparison.InvariantCultureIgnoreCase));

            if (idClaim == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            var user = db.Users.SingleOrDefault(q => q.id.ToString() == idClaim.Value);

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

            // find user in token
            var idClaim = User.Claims.FirstOrDefault(x => x.Type.Equals("id", StringComparison.InvariantCultureIgnoreCase));

            if (idClaim == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            var user = db.Users.SingleOrDefault(q => q.id.ToString() == idClaim.Value);

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
            // find user in token
            var idClaim = User.Claims.FirstOrDefault(x => x.Type.Equals("id", StringComparison.InvariantCultureIgnoreCase));

            if (idClaim == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            var user = db.Users.SingleOrDefault(q => q.id.ToString() == idClaim.Value);

            bool validOldPassword = BCrypt.Net.BCrypt.Verify(userParam.oldPassword, user.password);

            if (!validOldPassword)
            {
                return BadRequest(new { msg = "Incorrect old password" });
            }


            bool validPassword = BCrypt.Net.BCrypt.Verify(userParam.password, user.password);

            if (validPassword)
            {
                return BadRequest(new { poruka = "Same old and new password" });
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

    }
}
