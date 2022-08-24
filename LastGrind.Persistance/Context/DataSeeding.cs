using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using LastGrind.Domain.Entities;

namespace LastGrind.Persistance.Context
{
    public static class DataSeeding
    {
        public static async Task UseSeed(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            //var context = app.ApplicationServices.GetService<AppDbContext>();
            var roleManager = app.ApplicationServices.GetService<RoleManager<IdentityRole>>();

            
         
            if (!roleManager.Roles.Any(x => x.Name == "Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            if(!roleManager.Roles.Any(x=>x.Name == "User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }
           
        }
    }
}
