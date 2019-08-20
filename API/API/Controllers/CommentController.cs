﻿using System;
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
    [Route("api/Comment")]
    [Authorize]
    public class CommentController : Controller
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


        [HttpPost("addComment")]
        public IActionResult AddComment([FromBody] Comment commentParam )
        {
            User user = GetUserInToken();

            if (user == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            Post post = db.Posts.SingleOrDefault(q => q.id == commentParam.post.id);

            Comment comment = new Comment();

            comment.user = user;
            comment.post = post;
            comment.text = commentParam.text;
            comment.date = DateTime.Now;

            try
            {
                db.Comments.Add(comment);
                db.SaveChanges();
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
