using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace examProject.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual List<UserFile> Files { get; set; }
        public User()
        {
            Files = new List<UserFile>();
        }
    }
}
