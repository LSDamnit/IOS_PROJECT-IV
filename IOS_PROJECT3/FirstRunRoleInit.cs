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
        public const string FirstAdminEmail = "firstadmin@admin.adm";
        private const string FirstAdminPassword = "@FirstAccess000";
        public const string FirstAdminFio = "Первый Администратор";
        public static async Task InitializeAsync(UserManager<EUser> userManager, RoleManager<IdentityRole> roleManager, DBMergedContext context)
        {
            
           
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
            var firstAdmin = await userManager.FindByNameAsync(FirstAdminEmail);
            if (firstAdmin == null)
            {
                EUser admin = new EUser { Email = FirstAdminEmail, UserName = FirstAdminEmail, FIO=FirstAdminFio };
                IdentityResult result = await userManager.CreateAsync(admin, FirstAdminPassword);
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
            
            var adminRole = await roleManager.FindByNameAsync("Admin");
            var managerRole = await roleManager.FindByNameAsync("Manager");

            //context.RolesToGrants.Add(new ERolesToGrants()
            //{
            //    RoleId = managerRole.Id,
            //    GrantId = grant.Entity.Id.ToString()
            //});

            if ((from g in context.Grants where g.Name == Grants.Grants.Roles.Edit select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Roles.Edit,
                    Description = "Право редактировать роли пользователей"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == Grants.Grants.Roles.EditRole select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Roles.EditRole,
                    Description = "Право редактировать роли"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == Grants.Grants.Roles.Delete select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Roles.Delete,
                    Description = "Право удалять роли"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == Grants.Grants.Roles.View select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Roles.View,
                    Description = "Право просматривать страницу \"Список ролей\""
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == Grants.Grants.Roles.Create select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Roles.Create,
                    Description = "Право создавать роли"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }

            if ((from g in context.Grants where g.Name == Grants.Grants.Institutions.View select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Institutions.View,
                    Description = "Право просматривать страницу \"Управление институтами\""
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == Grants.Grants.Institutions.Edit select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Institutions.Edit,
                    Description = "Право редактировать институты"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == Grants.Grants.Institutions.Create select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Institutions.Create,
                    Description = "Право создавать институты"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == Grants.Grants.Institutions.Delete select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Institutions.Delete,
                    Description = "Право удалять институты"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }

            if ((from g in context.Grants where g.Name == Grants.Grants.Departments.Create select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Departments.Create,
                    Description = "Право создавать кафедры"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == Grants.Grants.Departments.Delete select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Departments.Delete,
                    Description = "Право удалять кафедры"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == Grants.Grants.Departments.Edit select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Departments.Edit,
                    Description = "Право изменять кафедры"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }

            if ((from g in context.Grants where g.Name == Grants.Grants.Specialities.Create select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Specialities.Create,
                    Description = "Право создавать специальности"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == Grants.Grants.Specialities.Delete select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Specialities.Delete,
                    Description = "Право удалять специальности"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == Grants.Grants.Specialities.Edit select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Specialities.Edit,
                    Description = "Право изменять специальности"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }

            if ((from g in context.Grants where g.Name == Grants.Grants.Disciplines.EnrollStudent select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Disciplines.EnrollStudent,
                    Description = "Право зачислять студента на специальность"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == Grants.Grants.Disciplines.Create select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Disciplines.Create,
                    Description = "Право создавать дисциплины"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == Grants.Grants.Disciplines.Delete select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Disciplines.Delete,
                    Description = "Право удалять дисциплины"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == Grants.Grants.Disciplines.Edit select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Disciplines.Edit,
                    Description = "Право изменять дисциплины"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }

            if ((from g in context.Grants where g.Name == Grants.Grants.DisciplinesDetails.Files select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.DisciplinesDetails.Files,
                    Description = "Право управлять файлами"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == Grants.Grants.DisciplinesDetails.FilePath select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.DisciplinesDetails.FilePath,
                    Description = "Право просматривать путь до файла на сервере"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }

            if ((from g in context.Grants where g.Name == Grants.Grants.Schedule.Edit select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Schedule.Edit,
                    Description = "Право изменять расписание"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == Grants.Grants.Schedule.Delete select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Schedule.Delete,
                    Description = "Право удалять расписание"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == Grants.Grants.Schedule.Create select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Schedule.Create,
                    Description = "Право создавать расписание"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }

            if ((from g in context.Grants where g.Name == Grants.Grants.UsersAdmin.View select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.UsersAdmin.View,
                    Description = "Право просматривать страницу \"Список пользователей\""
                });

                context.RolesToGrants.Add(new ERolesToGrants()
				{
					RoleId = adminRole.Id,
					GrantId = grant.Entity.Id.ToString()
				});
            }
            if ((from g in context.Grants where g.Name == Grants.Grants.UsersAdmin.CreateUsers select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.UsersAdmin.CreateUsers,
                    Description = "Право регистрировать пользователей"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == Grants.Grants.UsersAdmin.Delete select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.UsersAdmin.Delete,
                    Description = "Право удалять пользователей"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == Grants.Grants.UsersAdmin.Edit select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.UsersAdmin.Edit,
                    Description = "Право изменять пользователей"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == Grants.Grants.UsersAdmin.ResetPassword select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.UsersAdmin.ResetPassword,
                    Description = "Право сбрасывать пароль пользователей"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == Grants.Grants.UsersAdmin.Roles select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.UsersAdmin.Roles,
                    Description = "Право менять роли пользователей"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }

            if ((from g in context.Grants where g.Name == Grants.Grants.Message.Admin select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Message.Admin,
                    Description = "Право отправлять сообщение всем пользователям"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == Grants.Grants.Message.Departments select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Message.Departments,
                    Description = "Право делать рассылку для всех пользователей данной кафедры"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == Grants.Grants.Message.Specialities select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Message.Specialities,
                    Description = "Право делать рассылку для всех пользователей данной специальности"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == Grants.Grants.Message.Disciplines select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Message.Disciplines,
                    Description = "Право делать рассылку для всех пользователей данной дисциплины"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == Grants.Grants.Message.DisciplinesDetails select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Message.DisciplinesDetails,
                    Description = "Право делать рассылку для всех студентов данной дисциплины"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }

            if ((from g in context.Grants where g.Name == Grants.Grants.Complains.View select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Complains.View,
                    Description = "Право просматривать жалобы"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == Grants.Grants.Complains.Details select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Complains.Details,
                    Description = "Право управлять жалобами"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }

            if ((from g in context.Grants where g.Name == Grants.Grants.Forum.CreateNode select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Forum.CreateNode,
                    Description = "Право создавать разделы и темы на форуме"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == Grants.Grants.Forum.EditNode select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Forum.EditNode,
                    Description = "Право изменять разделы и темы на форуме"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == Grants.Grants.Forum.EditEndpoint select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Forum.EditEndpoint,
                    Description = "Право редактировать комментарии"
                });

                context.RolesToGrants.Add(new ERolesToGrants()
                {
                    RoleId = adminRole.Id,
                    GrantId = grant.Entity.Id.ToString()
                });
            }
            if ((from g in context.Grants where g.Name == Grants.Grants.Forum.DeleteComment select g).FirstOrDefault() == null)
            {
                var grant = context.Grants.Add(new EGrant()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Grants.Grants.Forum.DeleteComment,
                    Description = "Право удалять комментарии"
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