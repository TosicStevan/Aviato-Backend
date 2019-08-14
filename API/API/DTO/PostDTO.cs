using API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.DTO
{
    public class PostDTO
    {
        public virtual Post post { get; set; }
        public virtual List<CommentDTO> comments { get; set; }
        public int numberOfLikes { get; set; }

    }
}
