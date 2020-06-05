using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using IOS_PROJECT3.Models;

namespace IOS_PROJECT3.ViewModels
{
    public class UserPageViewModel
    {
        public string UserFIO { get; set; }
        public IList<EInstitution> Institutions { get; set; }
    }
}
