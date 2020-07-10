using IOS_PROJECT3.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;
namespace IOS_PROJECT3
{
    public class FirstRunRoleInit
    {
        
        public static async Task InitializeAsync(UserManager<EUser> userManager, RoleManager<IdentityRole> roleManager,DBMergedContext context)
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
            if((from f in context.ForumNodes where f.CreatorId=="-1" select f).FirstOrDefault()==null)
            {
                var MainForum = new EForumNode()
                {
                    CreatorId="-1",
                    Name="Main",
                    ParentNode=null,
                    CreationDate=System.DateTime.Now,
                    CreatorEmail="System",
                    CreatorFio="System"
                };
                context.ForumNodes.Add(MainForum);
                await context.SaveChangesAsync();
            }
           /* if (await roleManager.FindByNameAsync("Manager") != null&& await userManager.IsInRoleAsync(firstAdmin, "Manager"))
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
            }*/

        }
    }
}