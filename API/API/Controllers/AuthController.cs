using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.Helpers;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        AppDbContext db = new AppDbContext();
        private readonly AppSettings _appSettings;

        public AuthController(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody]User user )
        {

            var emailExist = db.Users.Where(q => q.email == user.email);
            var usernameExist = db.Users.Where(q => q.username == user.username);
            
            //provera emaila i usernamea u bazi
            if (emailExist.Any())
            {
                return BadRequest(new { msg = "Email already exist" });
            }

            if (usernameExist.Any())
            {
                return BadRequest(new { msg = "Username already exist" });
            }


            //hash sifre
            var hashedSifra = BCrypt.Net.BCrypt.HashPassword(user.password);
            user.password= hashedSifra;
            user.isPublic = true;

            try
            {
                db.Users.Add(user);
                db.SaveChanges();

                return Ok(new { msg = "Successful register" });
            }
            catch
            {
                return BadRequest(new { msg = "Database error" });
            }
            
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] User userParam)
        {

            var user = db.Users.SingleOrDefault(q => q.username == userParam.username);
            if (user == null)
            {
                return BadRequest(new { msg = "Invalid username" });
            }
            bool validPassword = false;
            if (user != null)
            {
                validPassword = BCrypt.Net.BCrypt.Verify(userParam.password, user.password);
            }


            if (validPassword == false)
            {
                return BadRequest(new { msg = "Invalid password" });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.id.ToString()),
                    new Claim("id", user.id.ToString()),

                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.isOnline = true;
            db.SaveChanges();

            user.token = tokenHandler.WriteToken(token);
            
            user.password = null;
            

            return Ok(user);
        }

        [HttpPost("logout")]
        public IActionResult LogOut()
        {
            // find user in token
            var idClaim = User.Claims.FirstOrDefault(x => x.Type.Equals("id", StringComparison.InvariantCultureIgnoreCase));

            if (idClaim == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            User user = db.Users.SingleOrDefault(q => q.id.ToString() == idClaim.Value);
            if(user == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            try
            {
                user.isOnline = false;
                db.SaveChanges();
                return Ok();
            }
            catch
            {
                return BadRequest(new { msg = "Database error" });
            }
            
        }


    }
}