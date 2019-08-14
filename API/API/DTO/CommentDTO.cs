using API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.DTO
{
    public class CommentDTO
    {
        public int id { get; set; }
        public virtual Post post { get; set; }
        public virtual User user { get; set; }
        public string text { get; set; }
        public DateTime date { get; set; }
        public bool isMy { get; set; }
    }
}
