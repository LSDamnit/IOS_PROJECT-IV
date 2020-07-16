using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using IOS_PROJECT3.Models;
using System.Security.Claims;
namespace IOS_PROJECT3.Grants
{
	public class GrantCheckService
	{
        DBMergedContext db;
        UserManager<EUser> userManager;
        RoleManager<IdentityRole> roleManager;

        public GrantCheckService(DBMergedContext DBContext, UserManager<EUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.db = DBContext;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }
        public async Task<bool> checkAccess(ClaimsPrincipal userCP, string requirement)
        {
            if (userCP == null)
            {
                return false;
            }

            var user = await userManager.GetUserAsync(userCP);
            var userRoleNames = await userManager.GetRolesAsync(user);
            var userRoles = roleManager.Roles.Where(x => userRoleNames.Contains(x.Name)).ToList();

            foreach (var role in userRoles)
            {
                var roleGrants = db.RolesToGrants.Where(rtg => rtg.RoleId == role.Id).Select(rtg => rtg.GrantId).ToList();
                var grants = db.Grants.Where(g => roleGrants.Contains(g.Id));
                var access = grants.Where(g => g.Name == requirement).FirstOrDefault();

                if (access != null) return true;
            }

            return false;
        }

        public async Task<List<string>> getUserGrants(ClaimsPrincipal userCP)
        {
            if (!userCP.Identity.IsAuthenticated)
            {
                return null;
            }

            var user = await userManager.GetUserAsync(userCP);
            var userRoleNames = await userManager.GetRolesAsync(user);
            var userRoles = roleManager.Roles.Where(x => userRoleNames.Contains(x.Name)).ToList();
            List<string> grants = new List<string>();
            foreach (var role in userRoles)
            {
                var roleGrants = db.RolesToGrants.Where(rtg => rtg.RoleId == role.Id).Select(rtg => rtg.GrantId).ToList();
                grants.AddRange(db.Grants.Where(g => roleGrants.Contains(g.Id)).Select(g => g.Name).ToList());
            }

            return grants;
        }
    }
}
