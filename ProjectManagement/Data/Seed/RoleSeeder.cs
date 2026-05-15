using Microsoft.AspNetCore.Identity;
using ProjectManagement.Common;

namespace ProjectManagement.Data.Seed
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles =
            {
                RoleConstants.Admin,
                RoleConstants.PM,
                RoleConstants.Leader,
                RoleConstants.Member
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
