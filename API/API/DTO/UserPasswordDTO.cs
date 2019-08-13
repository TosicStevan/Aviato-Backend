using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.DTO
{
    public class UserPasswordDTO
    {
        public string oldPassword { get; set; }
        public string password { get; set; }
    }
}
