using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MobilePhoneServiceAndSalesSystem.DAL;
using MobilePhoneServiceAndSalesSystem.Models;

namespace MobilePhoneServiceAndSalesSystem.Areas.Identity.Pages.Account
{
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<ExternalLoginModel> _logger;
        private readonly AppDbContext _dbContext;

        public ExternalLoginModel(
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager,
            ILogger<ExternalLoginModel> logger,
            AppDbContext dbContext)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _dbContext = dbContext;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ProviderDisplayName { get; set; }
        public string? ReturnUrl { get; set; }
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(100)]
            public string FirstName { get; set; } = string.Empty;

            [Required]
            [StringLength(100)]
            public string LastName { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [Phone]
            [StringLength(50)]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; } = string.Empty;

            [Required]
            [StringLength(250)]
            public string Address { get; set; } = string.Empty;
        }

        public IActionResult OnPost(string provider, string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl ??= Url.Content("~/");

            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var signInResult = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                isPersistent: false,
                bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrWhiteSpace(email))
            {
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    var linkResult = await _userManager.AddLoginAsync(existingUser, info);
                    if (linkResult.Succeeded)
                    {
                        await EnsureCustomerRoleAsync(existingUser);
                        await EnsureCustomerLinkAsync(existingUser, email);
                        await _signInManager.SignInAsync(existingUser, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }

                    foreach (var error in linkResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;

            Input.Email = email ?? string.Empty;

            return Page();
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (!ModelState.IsValid)
            {
                ProviderDisplayName = info.ProviderDisplayName;
                ReturnUrl = returnUrl;
                return Page();
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(string.Empty, "Email not found from provider.");
                return Page();
            }

            var user = new AppUser
            {
                UserName = email,
                Email = email,
                FirstName = Input.FirstName,
                LastName = Input.LastName
            };

            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                var addToRoleResult = await _userManager.AddToRoleAsync(user, "Customer");
                if (!addToRoleResult.Succeeded)
                {
                    foreach (var error in addToRoleResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    await _userManager.DeleteAsync(user);
                    return Page();
                }

                result = await _userManager.AddLoginAsync(user, info);
                if (result.Succeeded)
                {
                    var customer = new Customer
                    {
                        FirstName = Input.FirstName,
                        LastName = Input.LastName,
                        Email = Input.Email,
                        PhoneNumber = Input.PhoneNumber,
                        Address = Input.Address,
                        UserId = user.Id
                    };

                    _dbContext.Customers.Add(customer);
                    try
                    {
                        _dbContext.SaveChanges();
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError(string.Empty, "Failed to create the customer profile.");
                        await _userManager.DeleteAsync(user);
                        return Page();
                    }

                    _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }

        private async Task EnsureCustomerRoleAsync(AppUser user)
        {
            if (!await _userManager.IsInRoleAsync(user, "Customer"))
            {
                await _userManager.AddToRoleAsync(user, "Customer");
            }
        }

        private async Task EnsureCustomerLinkAsync(AppUser user, string email)
        {
            var customer = _dbContext.Customers.FirstOrDefault(c => !c.IsDeleted && c.UserId == user.Id);
            if (customer != null)
            {
                return;
            }

            customer = _dbContext.Customers.FirstOrDefault(c => !c.IsDeleted && c.Email == email);
            if (customer == null)
            {
                return;
            }

            customer.UserId = user.Id;
            await _dbContext.SaveChangesAsync();
        }
    }
}
