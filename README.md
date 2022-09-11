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
					
								var user = await  userManager.FindByEmailAsync(defaultUser.Email);
					
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
					
								var user = await  userManager.FindByEmailAsync(defaultUser.Email);
					
								if (user == null)
								{
									await userManager.CreateAsync(defaultUser, "Aa123456!");
									await userManager.AddToRolesAsync(defaultUser, new List<string> { Roles.Basic.ToString(), Roles.Admin.ToString(), Roles.SuperAdmin.ToString() });
								}
					
								//Seed Claims
								await roleManager.SeedClaimsForSuperUser();
							}
					
							private static async Task SeedClaimsForSuperUser(this RoleManager<IdentityRole> roleManager)
							{
								var adminRole = await roleManager.FindByNameAsync(Roles.SuperAdmin.ToString());
								await roleManager.AddPermissionClaims(adminRole, "Products");
							}
					
							public static async Task AddPermissionClaims(this RoleManager<IdentityRole> roleManager, IdentityRole role, string module)
							{
								var allClaims = await roleManager.GetClaimsAsync(role);
								var allPermissions = Permissions.GeneratePermissionList(module);
					
								foreach(var permission in allPermissions)
								{
									if(!allClaims.Any(c => c.Type == "Permission" && c.Value == permission))
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
				
			- Modify Program/Main class to seed database
				```cs
					namespace PBaseWebADotNet5.Web
					{
						public class Program
						{
							public static async Task Main(string[] args)
							{
								var host = CreateHostBuilder(args).Build();
								using var scope = host.Services.CreateScope();
								var services = scope.ServiceProvider;
								var loggerFactory = services.GetRequiredService<ILoggerProvider>();
								var logger = loggerFactory.CreateLogger("app");
								try
								{
									var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
									var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
									await Seeds.DefaultRoles.SeedAsyn(roleManager);
									await Seeds.DefaultUsers.SeedBasicUserAsync(userManager);
									await Seeds.DefaultUsers.SeedSuperAdminUserAsync(userManager, roleManager);
									logger.LogInformation("Data Seeded");
									logger.LogInformation("Application Started");
								}
								catch (System.Exception ex)
								{
					
									logger.LogWarning(ex, "An Error occurred while seeding data");
								}
					
					
								host.Run();
							}
					
							....
						}
					}
				```
				
			- Create a new migration and migrate to data base 
				- add-migration InitialMigration
				- update-database -v
				
			- Run application to seed information. 
				select * from dbo.AspNetRoleClaims;
				select * from dbo.AspNetRoles;
				select * from dbo.AspNetUserClaims;
				select * from dbo.AspNetUserLogins;
				select * from dbo.AspNetUserRoles;
				select * from dbo.AspNetUsers;
				select * from dbo.AspNetUserTokens;
				
		- UserController V5.
			- ViewModels
				- Create a new class ViewModel/CheckBoxViewModel
				```cs
					namespace PBaseWebADotNet5.Web.ViewModel
					{
						public class CheckBoxViewModel
						{
							public string DisplayValue { get; set; }
							public bool IsSelected { get; set; }
						}
					}
				```
				
				- Create a new class ViewModel/UserRolesViewModel
				```cs
					namespace PBaseWebADotNet5.Web.ViewModel
					{
						public class UserRolesViewModel
						{
							public string UserId { get; set; }
							public string UserName { get; set; }
							public List<CheckBoxViewModel> Roles { get; set; }
						}
					}
				```

				- Create a new class ViewModel/UserViewModel
				```cs
					namespace PBaseWebADotNet5.Web.ViewModel
					{
						public class UserViewModel
						{
							public string Id { get; set; }
							public string UserName { get; set; }
							public string Email { get; set; }
							public IEnumerable<string> Roles { get; set; }
						}
					}
				```
