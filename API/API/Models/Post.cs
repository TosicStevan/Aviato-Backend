using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    public class Post
    {
        [Key]
        public int id { get; set; }
        public string post { get; set; }
        public DateTime date { get; set; }
        public virtual User userId { get; set; }
    }
}
