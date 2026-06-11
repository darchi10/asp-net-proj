using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MobilePhoneServiceAndSalesSystem.DAL;
using MobilePhoneServiceAndSalesSystem.Models;

namespace MobilePhoneServiceAndSalesSystem.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly AppDbContext _dbContext;

        public RegisterModel(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ILogger<RegisterModel> logger,
            AppDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _dbContext = dbContext;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

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

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = new AppUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                FirstName = Input.FirstName,
                LastName = Input.LastName
            };

            var result = await _userManager.CreateAsync(user, Input.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");
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

                var existingCustomer = _dbContext.Customers.FirstOrDefault(c => !c.IsDeleted && c.Email == Input.Email);
                if (existingCustomer != null)
                {
                    if (!string.IsNullOrWhiteSpace(existingCustomer.UserId))
                    {
                        ModelState.AddModelError(string.Empty, "Customer record is already linked to another account.");
                        await _userManager.DeleteAsync(user);
                        return Page();
                    }

                    existingCustomer.FirstName = Input.FirstName;
                    existingCustomer.LastName = Input.LastName;
                    existingCustomer.PhoneNumber = Input.PhoneNumber;
                    existingCustomer.Address = Input.Address;
                    existingCustomer.UserId = user.Id;
                }
                else
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
                }

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

                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(ReturnUrl ?? "~/");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}
