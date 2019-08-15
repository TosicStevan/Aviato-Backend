using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    public class Following
    {
        [Key]
        public int id { get; set; }
        public virtual User follower { get; set; }
        public virtual User followed { get; set; }
        public bool isAccept { get; set; }
    }
}
