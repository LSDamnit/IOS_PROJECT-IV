using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOS_PROJECT3.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IOS_PROJECT3.ViewModels
{
    public class RolesViewModel
    {
        DBMergedContext DBContext;
        public RolesViewModel(DBMergedContext DBContext)
        {
            this.DBContext = DBContext;
            this.allGrants = DBContext.Grants.ToList();
        }
        public List<IdentityRole> allRoles { get; set; }
        public List<EGrant> allGrants { get; set; }

        public List<EGrant> getRoleGrants(IdentityRole role)
		{
            List<EGrant> result = new List<EGrant>();
            /*var joinres = DBContext.RolesToGrants.Join(DBContext.Grants, 
                rtg => rtg.GrantId,
                g => g.Id.ToString(), 
                (rtg, g) => new
                {
                    RoleId = rtg.RoleId,
                    GrantId = g.Id,
                    Name = g.Name,
                    Description = g.Description
                });
            */
            foreach (var elem in allGrants)
			{
                if ((from rtg in DBContext.RolesToGrants where rtg.RoleId == role.Id && rtg.GrantId == elem.Id.ToString() select rtg).FirstOrDefault() != null) result.Add(elem);
			}

            return result;
		}
    }
}
