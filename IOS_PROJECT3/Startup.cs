using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IOS_PROJECT3.Models;
using IOS_PROJECT3.Grants;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace IOS_PROJECT3
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DBMergedContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<EUser, IdentityRole>()
                .AddEntityFrameworkStores<DBMergedContext>().AddDefaultTokenProviders();//providers here <----

            services.AddControllersWithViews();

            services.AddScoped<IAuthorizationHandler, GrantAuthorizationHandler>();
            services.AddSingleton<IAuthorizationPolicyProvider, GrantPolicyProvider>();
            services.AddScoped<GrantCheckService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            
                if (env.IsDevelopment())
                {
                app.UseDeveloperExceptionPage();
               // app.UseExceptionHandler("/Home/Index");
            }
                else
                {
                    app.UseExceptionHandler("/Home/Index");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }
                app.UseHttpsRedirection();
                app.UseStaticFiles();

                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=PersonalPage}/{action=Index}/{id?}");
                });
            }
            
            }
    }


