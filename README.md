# 061_PermisionBasedAuthorization_DevCreed

- DevCreed: Permission Based Authorization In .Net 5 (Core) 
	- https://www.youtube.com/watch?v=slTben1Djz0&list=PL62tSREI9C-fGaDsCUvu5OaPWrv-mMzy-
	
		- Github: https://github.com/muhammadelhelaly/PermissionBasedAuthorizationIntDotNet5
		- V002 Crear aplicación ASP.NET Web Application(.NET Framework)
			- Carpeta relacionada 
				061_PermisionBasedAuthorization_DevCreed\NET50
				
			- ASP.NET Core Web App (Model-View_Controller)
			- Nombre:
				- Project: PBaseWebADotNet5.Web
				- Solution: PBaseWebADotNet5
			- Tipo:
				- .NET 5.0
				- Authentication Type: Individual Accounts
				- Configure for HTPPS
				- Enable Razor runtime compilation
				
		- Agregar conectionString
			```cs
				{
				  "ConnectionStrings": {
					"DefaultConnection": "Server=localhost;Database=db_DebCreed;User ID=sa;Password=123456;MultipleActiveResultSets=true",
				  },
				  ....
				}
			```
		- Seeds
			- Create enum Roles en "./Constants/Roles.cs
				```cs
					namespace PBaseWebADotNet5.Web.Constants
					{
						public enum Roles
						{
							SuperAdmin,
							Admin,
							Basic
						}
					}
				```
			- Create class  DefaultRoles en "./Seeds/DefaultRoles.cs
				```cs
					namespace PBaseWebADotNet5.Web.Seeds
					{
						public static class DefaultRoles
						{
							public static async Task SeedAsyn(RoleManager<IdentityRole> roleManager)
							{
								if (!roleManager.Roles.Any())
								{
									await roleManager.CreateAsync(new IdentityRole(Roles.SuperAdmin.ToString()));
									await roleManager.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
									await roleManager.CreateAsync(new IdentityRole(Roles.Basic.ToString()));
								}
								
							}
						}
					}
				```
			- Create class Permissions en "./Constants/Permissions.cs
				```cs
					namespace PBaseWebADotNet5.Web.Constants
					{
						public static class Permissions
						{
							public static List<string> GeneratePermissionList(string module)
							{
								return new List<string>
								{
									$"Permissions.{module}.View",
									$"Permissions.{module}.Create",
									$"Permissions.{module}.Edit",
									$"Permissions.{module}.Delete"
								};
							}
						}
					}
				```
				
			- Create class  DefaultUsers en "./Seeds/DefaultUsers.cs
				```cs
					namespace PBaseWebADotNet5.Web.Seeds
					{
						public static  class DefaultUsers
						{
							public static async Task SeedBasicUserAsync(UserManager<IdentityUser> userManager)
							{
								var defaultUser = new IdentityUser
								{
									UserName = "basicuser@domain.com",
									Email = "basicuser@domain.com",
									EmailConfirmed = true
								};
					
								var user = userManager.FindByEmailAsync(defaultUser.Email);
					
								if(user == null)
								{
									await userManager.CreateAsync(defaultUser, "Aa123456!");
									await userManager.AddToRoleAsync(defaultUser, Roles.Basic.ToString());
								}
							}
					
							public static async Task SeedSuperAdminUserAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
							{
								var defaultUser = new IdentityUser
								{
									UserName = "superadmin@domain.com",
									Email = "superadmin@domain.com",
									EmailConfirmed = true
								};
					
								var user = userManager.FindByEmailAsync(defaultUser.Email);
					
								if (user == null)
								{
									await userManager.CreateAsync(defaultUser, "Aa123456!");
									await userManager.AddToRolesAsync(defaultUser, new List<string> { Roles.Basic.ToString(), Roles.Admin.ToString(), Roles.SuperAdmin.ToString() });
								}
					
								//Seed Claims
								await roleManager.SeedClaimsForSuperUser();
							}
					
							public static async Task SeedClaimsForSuperUser(this RoleManager<IdentityRole> roleManager)
							{
								var adminRole = await roleManager.FindByIdAsync(Roles.SuperAdmin.ToString());
								await roleManager.AddPermissionClaims(adminRole, "Products");
							}
					
							public static async Task AddPermissionClaims(this RoleManager<IdentityRole> roleManager, IdentityRole role, string module)
							{
								var allClaims = await roleManager.GetClaimsAsync(role);
								var allPermissions = Permissions.GeneratePermissionList(module);
					
								foreach(var permission in allPermissions)
								{
									if(allClaims.Any(c => c.Type == "Permission" && c.Value == permission))
										await roleManager.AddClaimAsync(role, new Claim("Permission", permission));
								}
					
							}
						}
					}
				```
				
			- Modify StartUp class to use AddIdentity<> instead of AddDefaultIdentity
				```cs
					namespace PBaseWebADotNet5.Web
					{
						public class Startup
						{
							....
							public void ConfigureServices(IServiceCollection services)
							{
								....
								services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
									.AddEntityFrameworkStores<ApplicationDbContext>()
									.AddDefaultUI();
								....
							}
							....
						}
					}
				```