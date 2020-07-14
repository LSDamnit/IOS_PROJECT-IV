using IOS_PROJECT3.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Collections;
using System.Linq;
using System;

namespace IOS_PROJECT3
{
    public class FirstRunRoleInit
    {
        public static async Task InitializeAsync(UserManager<EUser> userManager, RoleManager<IdentityRole> roleManager, DBMergedContext context)
        {
            string adminEmail = "firstadmin@admin.adm";
            string password = "@FirstAccess000";
            string admFio = "Первый Администратор";
           
            if (await roleManager.FindByNameAsync("Admin") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            if (await roleManager.FindByNameAsync("Manager") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("Manager"));
            }
            if (await roleManager.FindByNameAsync("Teacher") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("Teacher"));
            }
            if (await roleManager.FindByNameAsync("Student") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("Student"));
            }
            var firstAdmin = await userManager.FindByNameAsync(adminEmail);
            if (firstAdmin == null)
            {
                EUser admin = new EUser { Email = adminEmail, UserName = adminEmail, FIO=admFio };
                IdentityResult result = await userManager.CreateAsync(admin, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
            else if(!await userManager.IsInRoleAsync(firstAdmin,"Admin"))
            {
                await userManager.AddToRoleAsync(firstAdmin, "Admin");
   
            }
            if (await roleManager.FindByNameAsync("Manager") != null&& await userManager.IsInRoleAsync(firstAdmin, "Manager"))
            {
                await userManager.RemoveFromRoleAsync(firstAdmin, "Manager");
            }
            if (await roleManager.FindByNameAsync("Teacher") != null && await userManager.IsInRoleAsync(firstAdmin, "Teacher"))
            {
                await userManager.RemoveFromRoleAsync(firstAdmin, "Teacher");
            }
            if (await roleManager.FindByNameAsync("Student") != null && await userManager.IsInRoleAsync(firstAdmin, "Student"))
            {
                await userManager.RemoveFromRoleAsync(firstAdmin, "Student");
            }

            var adminRole = await roleManager.FindByNameAsync("Admin");

            if ((from g in context.Grants where g.Name == "Grant.UsersAdmin.View" select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Grant.UsersAdmin.View",
                    Description = "Позволяет пользователю просматривать страницу \"Список пользователей\""
                });

                context.RolesToGrants.Add(new ERolesToGrants()
				{
					RoleId = adminRole.Id,
					GrantId = grant.Entity.Id.ToString()
				});
			}

			if ((from g in context.Grants where g.Name == "Grant.Roles.Edit" select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Grant.Roles.Edit",
                    Description = "Позволяет пользователю редактировать роли пользователей"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == "Grant.Roles.EditRole" select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Grant.Roles.EditRole",
                    Description = "Позволяет пользователю редактировать роли"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == "Grant.Roles.Delete" select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Grant.Roles.Delete",
                    Description = "Позволяет пользователю удалять роли"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == "Grant.Roles.View" select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Grant.Roles.View",
                    Description = "Позволяет пользователю просматривать страницу \"Список ролей\""
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == "Grant.Roles.Create" select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Grant.Roles.Create",
                    Description = "Позволяет пользователю создавать роли"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }

            if ((from g in context.Grants where g.Name == "Grant.Institutions.View" select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Grant.Institutions.View",
                    Description = "Позволяет пользователю просматривать страницу \"Управление институтами\""
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }

            if ((from g in context.Grants where g.Name == "Grant.Specialities.View" select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Grant.Specialities.View",
                    Description = "Позволяет пользователю просматривать страницу со специальностями"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }

            await context.SaveChangesAsync();
        }
    }
}