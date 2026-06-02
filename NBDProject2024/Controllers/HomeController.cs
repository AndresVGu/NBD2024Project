using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using NBDProject2024.Data;
using NBDProject2024.Models;
using NBDProject2024.ViewModels;
using System.Diagnostics;

namespace NBDProject2024.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly NBDContext _context;

        public HomeController(ILogger<HomeController> logger, NBDContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Dashboard(bool? mine)
        {
            string userEmail = User?.Identity?.Name ?? string.Empty;

            var vm = new DashboardVM
            {
                IsAdmin = User.IsInRole("Admin"),
                IsSupervisor = User.IsInRole("Supervisor"),
                IsDesigner = User.IsInRole("Designer"),
                IsSales = User.IsInRole("Sales")
            };

            vm.CanToggleScope = vm.IsDesigner || vm.IsSales;
            vm.UseMyScope = vm.CanToggleScope && (mine ?? true);

            vm.TotalClients = await _context.Clients.CountAsync();
            vm.TotalProjects = await _context.Projects.CountAsync();
            vm.TotalBids = await _context.Bids.CountAsync();
            vm.TotalEmployees = await _context.Employees.CountAsync();
            vm.ActiveEmployees = await _context.Employees.CountAsync(e => e.Active);
            vm.InactiveEmployees = vm.TotalEmployees - vm.ActiveEmployees;

            vm.MyCreatedClients = await _context.Clients.CountAsync(c => c.CreatedBy == userEmail);
            vm.MyCreatedProjects = await _context.Projects.CountAsync(p => p.CreatedBy == userEmail);
            vm.MyCreatedBids = await _context.Bids.CountAsync(b => b.CreatedBy == userEmail);

            vm.ScopedClients = vm.UseMyScope ? vm.MyCreatedClients : vm.TotalClients;
            vm.ScopedProjects = vm.UseMyScope ? vm.MyCreatedProjects : vm.TotalProjects;

            bool personalRecentFeed = vm.IsDesigner || vm.IsSales;
            if (vm.UseMyScope)
            {
                vm.ProjectsWithoutBids = await _context.Projects.CountAsync(p => p.CreatedBy == userEmail && !p.Bids.Any());
            }
            else
            {
                vm.ProjectsWithoutBids = await _context.Projects.CountAsync(p => !p.Bids.Any());
            }

            DateTime monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            if (vm.UseMyScope)
            {
                vm.BidsThisMonth = await _context.Bids.CountAsync(b => b.BidDate >= monthStart && b.CreatedBy == userEmail);
            }
            else
            {
                vm.BidsThisMonth = await _context.Bids.CountAsync(b => b.BidDate >= monthStart);
            }

            var recentProjectsQuery = _context.Projects.AsNoTracking();
            if (personalRecentFeed)
            {
                recentProjectsQuery = recentProjectsQuery.Where(p => p.CreatedBy == userEmail);
            }

            vm.RecentProjects = await recentProjectsQuery
                .OrderByDescending(p => p.CreatedOn)
                .Select(p => new ActivityItemVM
                {
                    Id = p.ID,
                    Title = p.ProjectName,
                    SubTitle = p.ProjectSite,
                    CreatedOn = p.CreatedOn,
                    CreatedBy = p.CreatedBy
                })
                .Take(5)
                .ToListAsync();

            var recentBidsQuery = _context.Bids
                .AsNoTracking()
                .Include(b => b.Project)
                .AsQueryable();
            if (personalRecentFeed)
            {
                recentBidsQuery = recentBidsQuery.Where(b => b.CreatedBy == userEmail);
            }

            vm.RecentBids = await recentBidsQuery
                .OrderByDescending(b => b.CreatedOn)
                .Select(b => new ActivityItemVM
                {
                    Id = b.ID,
                    Title = "Bid #" + b.ID,
                    SubTitle = b.Project.ProjectName,
                    CreatedOn = b.CreatedOn,
                    CreatedBy = b.CreatedBy
                })
                .Take(5)
                .ToListAsync();

            var recentClientsQuery = _context.Clients.AsNoTracking();
            if (personalRecentFeed)
            {
                recentClientsQuery = recentClientsQuery.Where(c => c.CreatedBy == userEmail);
            }

            vm.RecentClients = await recentClientsQuery
                .OrderByDescending(c => c.CreatedOn)
                .Select(c => new ActivityItemVM
                {
                    Id = c.ID,
                    Title = c.CompanyName,
                    SubTitle = c.FullName,
                    CreatedOn = c.CreatedOn,
                    CreatedBy = c.CreatedBy
                })
                .Take(5)
                .ToListAsync();

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
