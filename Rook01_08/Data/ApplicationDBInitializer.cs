using Microsoft.AspNetCore.Identity;
using Rook01_08.Models.Auth;
using Rook01_08.Models.Auth.Constans;

namespace Rook01_08.Data
{
    public class ApplicationDBInitializer
    {
        public static async Task IdentitySeed(IApplicationBuilder applicationBuilder)
        {
            using(var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

                if(!await roleManager.RoleExistsAsync(UserRoles.Admin))
                    await roleManager.CreateAsync(new IdentityRole<int>(UserRoles.Admin));

                if (!await roleManager.RoleExistsAsync(UserRoles.CelebEditor))
                    await roleManager.CreateAsync(new IdentityRole<int>(UserRoles.CelebEditor));

                //Admin user
                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                //todo: cont.
            }
        }

        public static async Task Seed(IApplicationBuilder applicationBuilder)
        {
            await IdentitySeed(applicationBuilder);
        }

    }
}
