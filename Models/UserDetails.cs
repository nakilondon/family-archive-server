using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace family_archive_server.Models
{
    public class UserDetails
    {
        public string Uid { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public bool Admin { get; set; }
        public bool Edit { get; set; }
        public bool Verified { get; set; }
    }
}
