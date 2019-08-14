using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    public class Comment
    {
        [Key]
        public int id { get; set; }
        public virtual Post post { get; set; }
        public virtual User user { get; set; }
        public string text { get; set; }
        public DateTime date { get; set; }
    }
}
