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

			- Create a new UserController Class
				- A nivel de clase tendrá un RoleBasedAuthentication
					- [Authorize(Roles = "SuperAdmin")]
				```cs
					namespace PBaseWebADotNet5.Web.Controllers
					{
						[Authorize(Roles = "SuperAdmin")]
						public class UsersController : Controller
						{
							private readonly UserManager<IdentityUser> _userManager;
							private readonly RoleManager<IdentityRole> _roleManager;
							private readonly SignInManager<IdentityUser> _signInManager;
					
							public UsersController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<IdentityUser> signInManager)
							{
								_userManager = userManager;
								_roleManager = roleManager;
								_signInManager = signInManager;
							}
					
							public async Task<IActionResult> Index()
							{
								var users = await _userManager.Users
									.Select(user => new UserViewModel { Id = user.Id, UserName = user.UserName, Email = user.Email, Roles = _userManager.GetRolesAsync(user).Result })
									.ToListAsync();
					
								return View(users);
							}
					
							public async Task<IActionResult> ManageRoles(string userId)
							{
								var user = await _userManager.FindByIdAsync(userId);
					
								if (user == null)
									return NotFound();
					
								var roles = await _roleManager.Roles.ToListAsync();
					
								var viewModel = new UserRolesViewModel
								{
									UserId = user.Id,
									UserName = user.UserName,
									Roles = roles.Select(role => new CheckBoxViewModel
									{
										DisplayValue = role.Name,
										IsSelected = _userManager.IsInRoleAsync(user, role.Name).Result
									}).ToList()
								};
					
								return View(viewModel);
							}
					
							[HttpPost]
							[ValidateAntiForgeryToken]
							public async Task<IActionResult> UpdateRoles(UserRolesViewModel model)
							{
								var user = await _userManager.FindByIdAsync(model.UserId);
					
								if (user == null)
									return NotFound();
					
								var userRoles = await _userManager.GetRolesAsync(user);
					
								await _userManager.RemoveFromRolesAsync(user, userRoles);
								await _userManager.AddToRolesAsync(user, model.Roles.Where(r => r.IsSelected).Select(r => r.DisplayValue));
					
								//foreach (var role in model.Roles)
								//{
								//    if (userRoles.Any(r => r == role.RoleName) && !role.IsSelected)
								//        await _userManager.RemoveFromRoleAsync(user, role.RoleName);
					
								//    if (!userRoles.Any(r => r == role.RoleName) && role.IsSelected)
								//        await _userManager.AddToRoleAsync(user, role.RoleName);
								//}
					
								return RedirectToAction(nameof(Index));
							}
						}
					}
				```
				
				- Views
					- Add Index view
					```cs
						@model IEnumerable<UserViewModel>
						
						@{
							ViewData["Title"] = "Users";
						}
						
						
						
						<h1>User List</h1>
						
						<hr />
						
						<table class="table table-striped" id="userTable">
							<thead>
								<tr>
									<th>User</th>
									<th>Email</th>
									<th>Roles</th>
									<th>Actions</th>
								</tr>
							</thead>
							<tbody>
								@foreach (var user in Model)
								{
									<tr>
										<td>@user.UserName</td>
										<td>@user.Email</td>
										<td>@string.Join(" , ", user.Roles.ToList())</td>
										<td>
											<a class="btn btn-primary" asp-action="ManageRoles" asp-route-userId="@user.Id">Manage Roles</a>
										</td>
									</tr>
								}
							</tbody>
						</table>
					```
				- Add ManageRoles view
					```cs
						@model UserRolesViewModel
						
						@{
							ViewData["Title"] = "Manage Roles";
						}
						
						<form asp-action="UpdateRoles" method="post">
							<div class="card mt-4">
								<div class="card-header">
									<h2>@Model.UserName</h2>
									Add/Remove Roles
								</div>
								<div class="card-body">
									<input type="hidden" asp-for="UserId" />
									@for (int i = 0; i < Model.Roles.Count; i++)
									{
										<div class="form-check m-1">
											<input type="hidden" asp-for="@Model.Roles[i].DisplayValue" />
											<input asp-for="@Model.Roles[i].IsSelected" class="form-check-input" />
											<label class="form-check-label" asp-for="@Model.Roles[i].IsSelected">
												@Model.Roles[i].DisplayValue
											</label>
										</div>
									}
									<div asp-validation-summary="All" class="text-danger"></div>
								</div>
								<div class="card-footer">
									<button type="submit" class="btn btn-primary">Save</button>
									<a asp-action="Index" class="btn btn-secondary">Cancel</a>
								</div>
							</div>
						</form>
					```
					
		- RolesController V8.
			- ViewModels
				- Create a new class ViewModel/RoleFormViewModel
				```cs
					namespace PBaseWebADotNet5.Web.ViewModel
					{
						public class RoleFormViewModel
						{
							[Required, StringLength(256)]
							public string Name { get; set; }
						}
					}
				```

			- Create a new RolesController Class
				- A nivel de clase tendrá un RoleBasedAuthentication
					- [Authorize(Roles = "SuperAdmin")]
				```cs
					namespace PBaseWebADotNet5.Web.Controllers
					{
						[Authorize(Roles = "SuperAdmin")]
						public class RolesController : Controller
						{
							private readonly RoleManager<IdentityRole> _roleManager;
					
							public RolesController(RoleManager<IdentityRole> roleManager)
							{
								_roleManager = roleManager;
							}
					
					
							public async Task<IActionResult> Index()
							{
								var roles = await _roleManager.Roles.ToListAsync();
								return View(roles);
							}
					
							[HttpPost]
							[ValidateAntiForgeryToken]
							public async Task<IActionResult> Add(RoleFormViewModel model)
							{
								if (!ModelState.IsValid)
									return View("Index", await _roleManager.Roles.ToListAsync());
					
								if (await _roleManager.RoleExistsAsync(model.Name))
								{
									ModelState.AddModelError("Name", "Role is exists!");
									return View("Index", await _roleManager.Roles.ToListAsync());
								}
					
								await _roleManager.CreateAsync(new IdentityRole(model.Name.Trim()));
					
								return RedirectToAction(nameof(Index));
							}
						}
					}
				```
				
				- Views
					- Add Index view
					```cs
						@model IEnumerable<IdentityRole>
						
						@using PBaseWebADotNet5.Web.Constants
						
						@{
							ViewData["Title"] = "Roles";
						}
						
						<h1>Roles</h1>
						
						<partial name="_RoleForm" model="new RoleFormViewModel()" />
						
						<table class="table table-striped mt-4">
							<thead>
								<tr class="bg-primary text-white">
									<th>Id</th>
									<th>Role Name</th>
									<th>Actions</th>
								</tr>
							</thead>
							<tbody>
								@foreach (var role in Model)
								{
									<tr>
										<td>@role.Id</td>
										<td>@role.Name</td>
										<td>
											<a class="btn btn-primary" asp-action="ManagePermissions" asp-route-roleId="@role.Id">Manage Permissions</a>
										</td>
									</tr>
								}
							</tbody>
						</table>
						
						@section Scripts {
							<partial name="_ValidationScriptsPartial" />
						}
					```
				- Add partial view _RoleForm
					```cs
						@model RoleFormViewModel
						
						<form method="post" asp-action="Add">
							<div class="input-group mb-3">
								<input asp-for="Name" class="form-control" placeholder="Role name..." />
								<div class="input-group-append">
									<button type="submit" class="btn btn-primary">Add New Role</button>
								</div>
							</div>
							<span asp-validation-for="Name" class="text-danger"></span>
						</form>
					```
					
		- Permissions V10.
			- ViewModels
				- Create a new class ViewModel/PermissionsFormViewModel
				```cs
					namespace PBaseWebADotNet5.Web.ViewModel
					{
						public class PermissionsFormViewModel
						{
							public string RoleId { get; set; }
							public string RoleName { get; set; }
							public List<CheckBoxViewModel> RoleCalims { get; set; }
						}
					}
				```
				
			- Contants
				- Add enum Constants/Modules with a list of all modules
				```cs
					namespace PBaseWebADotNet5.Web.Constants
					{
						public enum Modules
						{
							Products,
							Stock,
							Categories
						}
					}
				```
				
			- Add method: GenerateAllPermissions  to Constants/Permissions class
				```cs
					namespace PBaseWebADotNet5.Web.Constants
					{
						public static class Permissions
						{
							....
					
							public static List<string> GenerateAllPermissions()
							{
								var allPermissions = new List<string>();
					
								var modules = Enum.GetValues(typeof(Modules));
					
								foreach (var module in modules)
									allPermissions.AddRange(GeneratePermissionList(module.ToString()));
					
								return allPermissions;
							}
						}
					}
				```
				
			- Controller
				- Add new methods to RolesController Class
				```cs
					namespace PBaseWebADotNet5.Web.Controllers
					{
						[Authorize(Roles = "SuperAdmin")]
						public class RolesController : Controller
						{	
							....
							
							public async Task<IActionResult> ManagePermissions(string roleId)
							{
								var role = await _roleManager.FindByIdAsync(roleId);
					
								if (role == null)
									return NotFound();
					
								var roleClaims = _roleManager.GetClaimsAsync(role).Result.Select(c => c.Value).ToList();
								var allClaims = Permissions.GenerateAllPermissions();
								var allPermissions = allClaims.Select(p => new CheckBoxViewModel { DisplayValue = p }).ToList();
					
								foreach (var permission in allPermissions)
								{
									if (roleClaims.Any(c => c == permission.DisplayValue))
										permission.IsSelected = true;
								}
					
								var viewModel = new PermissionsFormViewModel
								{
									RoleId = roleId,
									RoleName = role.Name,
									RoleCalims = allPermissions
								};
					
								return View(viewModel);
							}
					
							[HttpPost]
							[ValidateAntiForgeryToken]
							public async Task<IActionResult> ManagePermissions(PermissionsFormViewModel model)
							{
								var role = await _roleManager.FindByIdAsync(model.RoleId);
					
								if (role == null)
									return NotFound();
					
								var roleClaims = await _roleManager.GetClaimsAsync(role);
					
								foreach (var claim in roleClaims)
									await _roleManager.RemoveClaimAsync(role, claim);
					
								var selectedClaims = model.RoleCalims.Where(c => c.IsSelected).ToList();
					
								foreach (var claim in selectedClaims)
									await _roleManager.AddClaimAsync(role, new Claim("Permission", claim.DisplayValue));
					
								return RedirectToAction(nameof(Index));
							}
						}
					}
				```
				
				- Views
					- Add ManagePermissions view
					```cs
						@model PermissionsFormViewModel
						
						@{
							ViewData["Title"] = "Manage Permissions";
						}
						
						<form asp-action="ManagePermissions" method="post">
							<div class="card mt-4">
								<div class="card-header">
									<h2>@Model.RoleName</h2>
									Add/Remove Permissions
								</div>
								<div class="card-body">
									<input type="hidden" asp-for="RoleId" />
									<table class="table table-striped">
										<thead>
											<tr>
												<th></th>
												<th>Module</th>
												<th>Permission</th>
											</tr>
										</thead>
										<tbody>
											@for (int i = 0; i < Model.RoleCalims.Count; i++)
											{
												<tr>
													<td>
														<div class="form-check m-1">
															<input type="hidden" asp-for="@Model.RoleCalims[i].DisplayValue" />
															<input asp-for="@Model.RoleCalims[i].IsSelected" class="form-check-input" />
															@*<label class="form-check-label" asp-for="@Model.Roles[i].IsSelected">
														@Model.Roles[i].DisplayValue
														</label>*@
														</div>
													</td>
													<td>@Model.RoleCalims[i].DisplayValue.Split('.')[1]</td>
													<td>@Model.RoleCalims[i].DisplayValue.Split('.')[2]</td>
												</tr>
											}
										</tbody>
									</table>
									<div asp-validation-summary="All" class="text-danger"></div>
								</div>
								<div class="card-footer">
									<button type="submit" class="btn btn-primary">Save</button>
									<a asp-action="Index" class="btn btn-secondary">Cancel</a>
								</div>
							</div>
						</form>
					```	
					
			- Add Customs Policy V.13
				- Add Filters
					- Add Filters/PermissionAuthorizationHandler
						```cs
							namespace PBaseWebADotNet5.Web.Filters
							{
								public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
								{
									public PermissionAuthorizationHandler()
									{
							
									}
							
									protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
									{
										if (context.User == null)
											return;
							
										var canAccess = context.User.Claims.Any(c => c.Type == "Permission" && c.Value == requirement.Permission && c.Issuer == "LOCAL AUTHORITY");
							
										if (canAccess)
										{
											context.Succeed(requirement);
											return;
										}
									}
								}
							}
						```	
						
					- Add Filters/PermissionPolicyProvider
						```cs
							namespace PBaseWebADotNet5.Web.Filters
							{
								public class PermissionPolicyProvider : IAuthorizationPolicyProvider
								{
									public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }
							
									public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
									{
										FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
									}
							
									public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
									{
										return FallbackPolicyProvider.GetDefaultPolicyAsync();
									}
							
									public Task<AuthorizationPolicy> GetFallbackPolicyAsync()
									{
										return FallbackPolicyProvider.GetDefaultPolicyAsync();
									}
							
									public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
									{
										if (policyName.StartsWith("Permission", StringComparison.OrdinalIgnoreCase))
										{
											var policy = new AuthorizationPolicyBuilder();
											policy.AddRequirements(new PermissionRequirement(policyName));
											return Task.FromResult(policy.Build());
										}
							
										return FallbackPolicyProvider.GetPolicyAsync(policyName);
									}
								}
							}
						```	
						
					- Add Filters/PermissionRequirement
						```cs
							namespace PBaseWebADotNet5.Web.Filters
							{
								public class PermissionRequirement : IAuthorizationRequirement
								{
									public string Permission { get; private set; }
							
									public PermissionRequirement(string permission)
									{
										Permission = permission;
									}
								}
							}
						```	
						
				- Add Filters like a service.
					- Add services to StartUp.cs 
						```cs
							namespace PBaseWebADotNet5.Web
							{
								public class Startup
								{
									....
							
									public void ConfigureServices(IServiceCollection services)
									{
										services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
										services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
							
										....
									}
									
									....
						```	
						
				- ProductsController V14.
					- Add class: Products  to Constants/Permissions class
						```cs
							namespace PBaseWebADotNet5.Web.Constants
							{
								public static class Permissions
								{
									....
							
									public static List<string> GenerateAllPermissions()
									{
										....
									}
							
									public static class Products
									{
										public const string View = "Permissions.Products.View";
										public const string Create = "Permissions.Products.Create";
										public const string Edit = "Permissions.Products.Edit";
										public const string Delete = "Permissions.Products.Delete";
									}
								}
							}
						```
						
					- Controller
						- Add new methods to ProductsController Class
						```cs
							namespace PBaseWebADotNet5.Web.Controllers
							{
								public class ProductsController : Controller
								{
									public IActionResult Index()
									{
										return View();
									}
							
									[Authorize(Permissions.Products.Edit)]
									public IActionResult Edit()
									{
										return View();
									}
								}
							}
						```
						- Views
							- Add Index view
							```cs
								@inject IAuthorizationService AuthorizationService
								
								@using PBaseWebADotNet5.Web.Constants
								
								@{
									ViewData["Title"] = "Products";
								}
								
								<h1>Manage Products</h1>
								<hr />
								
								@if (AuthorizationService.AuthorizeAsync(User, Permissions.Products.View).Result.Succeeded)
								{
									<a href="#" class="btn btn-primary">View</a>
								}
								
								@if (AuthorizationService.AuthorizeAsync(User, Permissions.Products.Create).Result.Succeeded)
								{
									<a href="#" class="btn btn-secondary">Create</a>
								}
								
								@if (AuthorizationService.AuthorizeAsync(User, Permissions.Products.Edit).Result.Succeeded)
								{
									<a asp-action="Edit" class="btn btn-success">Edit</a>
								}
								
								@if (AuthorizationService.AuthorizeAsync(User, Permissions.Products.Delete).Result.Succeeded)
								{
									<a href="#" class="btn btn-danger">Delete</a>
								}
							```
							- Add Edit view
							```cs
								<p>Hello from Edit view!</p>

							```
							
				- Security Stamp Validator Options
					```cs
						namespace PBaseWebADotNet5.Web
						{
							public class Startup
							{
								....
								public void ConfigureServices(IServiceCollection services)
								{
									....
						
									services.Configure<SecurityStampValidatorOptions>(options =>
									{
										options.ValidationInterval = TimeSpan.Zero;
									});
						
									services.AddControllersWithViews();
								}

					```