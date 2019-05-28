using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace examProject.Models
{
    public class UserFile
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Path { get; set; }

        public DateTime Time { get; set; }

        public int UserId { get; set; }

        public int Previous { get; set; }
    }
}
