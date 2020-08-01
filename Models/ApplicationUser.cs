using AspNetCore.Identity.DocumentDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentitySample.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : DocumentDbIdentityUser<DocumentDbIdentityRole>
    {
        public int BusinessId { get; set; }
        public int BusinessType { get; set; }
        public string BusinessIdentifier { get; set; }
        public string SetupIdentifier { get; set; }
        public string Fullname { get; set; }
        public string Address { get; set; }
        public bool IsOwner { get; set; } = false;

    }
}
