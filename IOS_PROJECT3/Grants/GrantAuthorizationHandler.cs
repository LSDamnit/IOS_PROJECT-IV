using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using IOS_PROJECT3.Models;

namespace IOS_PROJECT3.Grants
{
	internal class GrantAuthorizationHandler : AuthorizationHandler<GrantRequirement>
    {
        DBMergedContext db;
        UserManager<EUser> userManager;
        RoleManager<IdentityRole> roleManager;

        public GrantAuthorizationHandler(DBMergedContext DBContext, UserManager<EUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.db = DBContext;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, GrantRequirement requirement)
        {
            if (context.User == null)
            {
                return;
            }

            // Get all the roles the user belongs to and check if any of the roles has the permission required
            // for the authorization to succeed.
            var user = await userManager.GetUserAsync(context.User);
            var userRoleNames = await userManager.GetRolesAsync(user);
            var userRoles = roleManager.Roles.Where(x => userRoleNames.Contains(x.Name)).ToList();

            foreach (var role in userRoles)
            {
                var roleGrants = db.RolesToGrants.Where(rtg => rtg.RoleId == role.Id).Select(rtg => rtg.GrantId).ToList();
                var grants = db.Grants.Where(g => roleGrants.Contains(g.Id));
                var access = grants.Where(g => g.Name == requirement.Grant).FirstOrDefault();

                if (access != null)
                {
                    context.Succeed(requirement);
                    return;
                }
            }
        }
    }
}
