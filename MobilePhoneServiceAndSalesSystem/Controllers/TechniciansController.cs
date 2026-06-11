using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobilePhoneServiceAndSalesSystem.DAL;
using MobilePhoneServiceAndSalesSystem.Models;
using MobilePhoneServiceAndSalesSystem.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace MobilePhoneServiceAndSalesSystem.Controllers
{
    [Route("technicians")]
    [Authorize(Roles = "Admin")]
    public class TechniciansController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<AppUser> _userManager;

        public TechniciansController(AppDbContext dbContext, UserManager<AppUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        [Route("")]
        public IActionResult Index()
        {
            var technicians = _dbContext.Technicians
                .Where(t => !t.IsDeleted)
                .Include(t => t.RepairJobs)
                .ToList();

            return View(technicians);
        }

        [Route("{id:int}")]
        public IActionResult Details(int id)
        {
            var technician = _dbContext.Technicians
                .Where(t => !t.IsDeleted)
                .Include(t => t.RepairJobs)
                .FirstOrDefault(t => t.Id == id);

            if (technician is null)
            {
                return NotFound();
            }

            return View(technician);
        }

        [HttpGet]
        [Route("search-list")]
        public IActionResult SearchList(string? term)
        {
            var query = term?.Trim() ?? string.Empty;

            var technicians = _dbContext.Technicians
                .Where(t => !t.IsDeleted
                    && (t.FirstName + " " + t.LastName).Contains(query))
                .Include(t => t.RepairJobs)
                .ToList();

            return PartialView("_TechnicianCards", technicians);
        }

        [HttpGet]
        [Route("create")]
        public IActionResult Create()
        {
            return View(new TechnicianCreateViewModel { HireDate = System.DateTime.Now });
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create(TechnicianCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                OIB = model.OIB,
                JMBG = model.JMBG
            };

            var createUserResult = await _userManager.CreateAsync(user, model.Password);
            if (!createUserResult.Succeeded)
            {
                foreach (var error in createUserResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(user, "Worker");
            if (!addToRoleResult.Succeeded)
            {
                foreach (var error in addToRoleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                await _userManager.DeleteAsync(user);
                return View(model);
            }

            var technician = new Technician
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Specialization = model.Specialization,
                HireDate = model.HireDate,
                Salary = model.Salary,
                UserId = user.Id
            };

            try
            {
                _dbContext.Technicians.Add(technician);
                _dbContext.SaveChanges();
            }
            catch
            {
                await _userManager.DeleteAsync(user);
                ModelState.AddModelError(string.Empty, "Failed to create the technician record.");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("edit/{id:int}")]
        public IActionResult Edit(int id)
        {
            var technician = _dbContext.Technicians.FirstOrDefault(t => t.Id == id && !t.IsDeleted);

            if (technician is null)
            {
                return NotFound();
            }

            return View(technician);
        }

        [HttpPost]
        [Route("edit/{id:int}")]
        public IActionResult Edit(int id, Technician technician)
        {
            if (id != technician.Id)
            {
                return NotFound();
            }

            var existingTechnician = _dbContext.Technicians.FirstOrDefault(t => t.Id == id && !t.IsDeleted);
            if (existingTechnician is null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(technician);
            }

            existingTechnician.FirstName = technician.FirstName;
            existingTechnician.LastName = technician.LastName;
            existingTechnician.Specialization = technician.Specialization;
            existingTechnician.HireDate = technician.HireDate;
            existingTechnician.Salary = technician.Salary;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("delete/{id:int}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var technician = _dbContext.Technicians
                .Where(t => !t.IsDeleted)
                .Include(t => t.RepairJobs)
                .FirstOrDefault(t => t.Id == id);

            if (technician is null)
            {
                return NotFound();
            }

            ViewBag.HasDependencies = technician.RepairJobs.Any();
            return View(technician);
        }

        [HttpPost]
        [Route("delete/{id:int}")]
        [ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id, string deleteMode)
        {
            var technician = _dbContext.Technicians
                .Include(t => t.RepairJobs)
                .FirstOrDefault(t => t.Id == id && !t.IsDeleted);

            if (technician is null)
            {
                return NotFound();
            }

            var hasDependencies = technician.RepairJobs.Any();
            if (hasDependencies
                && !string.Equals(deleteMode, "soft", System.StringComparison.OrdinalIgnoreCase)
                && !string.Equals(deleteMode, "hard", System.StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Choose a delete option for records with related data.";
                return RedirectToAction(nameof(Index));
            }

            if (string.Equals(deleteMode, "soft", System.StringComparison.OrdinalIgnoreCase))
            {
                technician.IsDeleted = true;
                technician.DeletedAt = System.DateTime.UtcNow;
            }

            if (!string.IsNullOrWhiteSpace(technician.UserId))
            {
                var user = await _userManager.FindByIdAsync(technician.UserId);
                if (user != null)
                {
                    technician.UserId = null;
                    _dbContext.SaveChanges();
                    var deleteUserResult = await _userManager.DeleteAsync(user);
                    if (!deleteUserResult.Succeeded)
                    {
                        TempData["Error"] = "Failed to delete the linked account.";
                        return RedirectToAction(nameof(Delete), new { id });
                    }
                }
            }

            if (!string.Equals(deleteMode, "soft", System.StringComparison.OrdinalIgnoreCase)
                && technician.RepairJobs.Any())
            {
                _dbContext.RepairJobs.RemoveRange(technician.RepairJobs);
            }

            if (string.Equals(deleteMode, "soft", System.StringComparison.OrdinalIgnoreCase))
            {
                _dbContext.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            _dbContext.Technicians.Remove(technician);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}