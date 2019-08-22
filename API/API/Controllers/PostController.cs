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
    [Authorize]
    [Produces("application/json")]
    [Route("api/Post")]
    public class PostController : Controller
    {

        private readonly AppDbContext db = new AppDbContext();
        private readonly IHubContext<PostHub> _hubContext;

        public PostController(IHubContext<PostHub> hubContext)
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

        [HttpPost("add")]
        public async Task<IActionResult> AddPostAsync([FromBody] Post postParam )
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

            Post post = new Post();

            post.userId = user;

            post.post = postParam.post;
            post.date = DateTime.Now;

            PostDTO postDTO = new PostDTO();
            postDTO.post = post;
            postDTO.numberOfLikes = 0;
            postDTO.isLiked = false;
            postDTO.comments = new List<CommentDTO>();
            
            await _hubContext.Clients.Group(user.id.ToString()).SendAsync("ReceiveMessage", postDTO);

            try
            {
                db.Posts.Add(post);
                db.SaveChanges();
                
                return Ok(post);
            }
            catch
            {
                return BadRequest(new { msg = "Database error" });
            }
            
        }

        [HttpGet("getMyPosts")]
        public IActionResult GetMyPost()
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

            var posts = db.Posts.Where(q => q.userId == user).ToList().OrderByDescending(q => q.date);

            var postsWithComments = new List<PostDTO>();

            foreach (var item in posts)
            {
                var p = new PostDTO();
                p.post = item;

                var comments = db.Comments.Include( q=> q.user).Where(q => q.post == item).ToList();

                var commentsWithIsMy = new List<CommentDTO>();

                foreach (var c in comments)
                {
                    CommentDTO com = new CommentDTO();
                    com.id = c.id;
                    com.user = c.user;
                    com.post = c.post;
                    com.text = c.text;
                    com.date = c.date;
                    if(c.user == user )
                    {
                        com.isMy = true;
                    }
                    else
                    {
                        com.isMy = false;
                    }
                    commentsWithIsMy.Add(com);
                }


                p.comments = commentsWithIsMy;

                int nubmerOfLikes = db.Likes.Where(q => q.post.id == item.id).Count();
                p.numberOfLikes = nubmerOfLikes;

                var isLiked = db.Likes.SingleOrDefault(q => q.post.id == item.id && item.userId.id == user.id);
                if(isLiked == null)
                {
                    p.isLiked = false;
                }
                else
                {
                    p.isLiked = true;
                }

                postsWithComments.Add(p);

            }


            return Ok(postsWithComments);
        }

        [HttpPost("getPosts")]
        public IActionResult GetPostByUsername([FromBody] User userParam)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Request");
            }

            User userInToken = GetUserInToken();

            if (userInToken == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }


            User user = db.Users.SingleOrDefault(q => q.username == userParam.username);
            if(user == null)
            {
                return BadRequest(new { msg = "Invalid user" });
            }

            var posts = db.Posts.Include(q => q.userId).Where(q => q.userId == user).ToList().OrderByDescending(q => q.date);

            var postsWithComments = new List<PostDTO>();

            foreach (var item in posts)
            {
                var p = new PostDTO();
                p.post = item;

                var comments = db.Comments.Include(q => q.user).Where(q => q.post == item).ToList();

                var commentsWithIsMy = new List<CommentDTO>();

                foreach (var c in comments)
                {
                    CommentDTO com = new CommentDTO();
                    com.id = c.id;
                    com.user = c.user;
                    com.post = c.post;
                    com.text = c.text;
                    com.date = c.date;
                    if (c.user == userInToken)
                    {
                        com.isMy = true;
                    }
                    else
                    {
                        com.isMy = false;
                    }
                    commentsWithIsMy.Add(com);
                }


                p.comments = commentsWithIsMy;

                int nubmerOfLikes = db.Likes.Where(q => q.post.id == item.id).Count();
                p.numberOfLikes = nubmerOfLikes;

                var isLiked = db.Likes.SingleOrDefault(q => q.post.id == item.id && q.user == userInToken);
                if (isLiked == null)
                {
                    p.isLiked = false;
                }
                else
                {
                    p.isLiked = true;
                }

                postsWithComments.Add(p);

            }


            return Ok(postsWithComments);
        }

        [HttpGet("getAllPosts")]
        public IActionResult GetAllPosts()
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

            var followers = db.Followings.Include(q => q.followed).Where(q => q.follower == user && q.isAccept == true).ToList();

            followers.Add(new Following { followed = user, follower = user });
            var followersIds = followers.Select(q => q.followed.id);
            
            var posts = db.Posts.Include(u => u.userId).Where(q => followersIds.Contains(q.userId.id)).ToList().OrderByDescending(q => q.date).Take(10);

            var postsWithComments = new List<PostDTO>();

            foreach (var item in posts)
            {
                PostDTO p = new PostDTO();
                p.post = item;
                item.userId.password = null;
                var comments = db.Comments.Include(q => q.user).Where(q => q.post == item).ToList();

                var commentsWithIsMy = new List<CommentDTO>();

                foreach (var c in comments)
                {
                    CommentDTO com = new CommentDTO();
                    com.id = c.id;
                    com.user = c.user;
                    com.post = c.post;
                    com.text = c.text;
                    com.date = c.date;
                    if (c.user == user)
                    {
                        com.isMy = true;
                    }
                    else
                    {
                        com.isMy = false;
                    }
                    commentsWithIsMy.Add(com);
                }


                p.comments = commentsWithIsMy;

                int nubmerOfLikes = db.Likes.Where(q => q.post.id == item.id).Count();
                p.numberOfLikes = nubmerOfLikes;

                var isLiked = db.Likes.SingleOrDefault(q => q.post.id == item.id && q.user == user);
                if (isLiked == null)
                {
                    p.isLiked = false;
                }
                else
                {
                    p.isLiked = true;
                }

                postsWithComments.Add(p);

            }


            return Ok(postsWithComments);
            
        }
    }
}
