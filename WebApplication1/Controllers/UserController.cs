using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Data.Entities;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers;

public class UserController : Controller
{
    private readonly ILogger<UserController> _logger;
    private readonly AppDBContext _context;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly IUserStore<AppUser> _userStore;
    private readonly RoleManager<IdentityRole> _roleManager;
    
    public UserController(
        ILogger<UserController> logger, 
        AppDBContext context, 
        UserManager<AppUser> userManager,
        IUserStore<AppUser> userStore,
        SignInManager<AppUser> signInManager,
        RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _userStore = userStore;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }
    
    // GET
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Register()
    {
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl="")
    {
        returnUrl ??= Url.Content("~/");
        // ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        if (ModelState.IsValid)
        {
            var user = CreateUser();
            user.Email = model.Email;
            user.FullName = model.FullName;
            user.PhoneNumber = model.Phone;
            user.Address = model.Address;
            user.EmailConfirmed = true;
            
            await _userStore.SetUserNameAsync(user, model.Email, CancellationToken.None);
            
            // await _emailStore.SetEmailAsync(user, model.Email, CancellationToken.None);
            
            user.Role = "User"; // Set default role property
            
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Ensure roles exist
                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                }
                if (!await _roleManager.RoleExistsAsync("User"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("User"));
                }

                // Assign role to user
                await _userManager.AddToRoleAsync(user, "User");
                _logger.LogInformation("User created a new account with password.");

                // var userId = await _userManager.GetUserIdAsync(user);
                // var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                // code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                // var callbackUrl = Url.Page(
                //     "/Account/ConfirmEmail",
                //     pageHandler: null,
                //     values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                //     protocol: Request.Scheme);
                //
                // await _emailSender.SendEmailAsync(model.Email, "Confirm your email",
                //     $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                
                // if (_userManager.Options.SignIn.RequireConfirmedAccount)
                // {
                //     return RedirectToPage("RegisterConfirmation",
                //         new { email = model.Email, returnUrl = returnUrl });
                // }
                // else
                // {
                //     await _signInManager.SignInAsync(user, isPersistent: false);
                //     return LocalRedirect(returnUrl);
                // }
                
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Shop");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View();
    }

    public IActionResult Login()
    {
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = "")
    {
        // returnUrl ??= Url.Content("~/");
        // ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        if (ModelState.IsValid)
        {
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return RedirectToAction("Index", "Product", new { area = "Admin" });
                }
                
                return RedirectToAction("Index", "Shop");
            }
            // if (result.RequiresTwoFactor)
            // {
            //     return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
            // }
            // if (result.IsLockedOut)
            // {
            //     _logger.LogWarning("User account locked out.");
            //     return RedirectToPage("./Lockout");
            // }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View();
            }
        }
        
        return View(model);
    }
    
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Shop");
    }

    // Tạo hoặc cập nhật Admin user - truy cập: /User/SeedAdmin
    public async Task<IActionResult> SeedAdmin()
    {
        // Đảm bảo role Admin tồn tại
        if (!await _roleManager.RoleExistsAsync("Admin"))
        {
            await _roleManager.CreateAsync(new IdentityRole("Admin"));
        }
        if (!await _roleManager.RoleExistsAsync("User"))
        {
            await _roleManager.CreateAsync(new IdentityRole("User"));
        }

        // Tìm hoặc tạo admin user
        var adminEmail = "admin@admin.com";
        var adminUser = await _userManager.FindByEmailAsync(adminEmail);
        
        if (adminUser == null)
        {
            // Tạo admin mới
            adminUser = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Administrator",
                EmailConfirmed = true,
                Role = "Admin"
            };
            
            var result = await _userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
                return Content("Admin user created successfully! Email: admin@admin.com, Password: Admin@123");
            }
            else
            {
                return Content("Failed to create admin: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            // Đảm bảo user đã có role Admin
            if (!await _userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
            }
            adminUser.Role = "Admin";
            await _userManager.UpdateAsync(adminUser);
            return Content("Admin role assigned to existing user: " + adminEmail);
        }
    }

    private AppUser CreateUser()
    {
        try
        {
            return Activator.CreateInstance<AppUser>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(AppUser)}'. " +
                                                $"Ensure that '{nameof(AppUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                                                $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
        }
    }
}