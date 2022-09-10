using Microsoft.AspNetCore.Identity;
using PBaseWebADotNet5.Web.Constants;
using System.Threading.Tasks;

namespace PBaseWebADotNet5.Web.Seeds
{
    public static class DefaultRoles
    {
        public static async Task SeedAsyn(RoleManager<IdentityRole> roleManager)
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.SuperAdmin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Basic.ToString()));
        }
    }
}
