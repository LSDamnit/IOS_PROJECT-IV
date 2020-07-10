using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
namespace IOS_PROJECT3.Models
{
    public class ERolesToGrants
    {
        public int RoleId { get; set; }
        public int GrantId { get; set; }
    }
}
