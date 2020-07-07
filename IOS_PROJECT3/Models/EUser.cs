using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
namespace IOS_PROJECT3.Models
{
    public class EUser:IdentityUser, IComparer<EUser>
    {
        public string FIO { get; set; }
        public ESpeciality Speciality { get; set; }

        public int Compare(EUser x, EUser y)
		{
            return String.Compare(x.FIO, y.FIO);
		}
    }
}
