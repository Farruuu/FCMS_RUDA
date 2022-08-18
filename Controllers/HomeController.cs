using com.ruda.Efile.BusinessLogic;
using com.ruda.Efile.Domain;
using DAL;
using FCMS_RUDA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace FCMS_RUDA.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("ID") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }

            List<Department> ListDepartments = await new OrgBLL().GetDepartmentsList();
            HttpContext.Session.SetComplexData("Departments", ListDepartments);

            List<Designation> ListDesignations = await new OrgBLL().GetDesignationsList();
            HttpContext.Session.SetComplexData("Designations", ListDesignations);

            Employees emp = HttpContext.Session.GetComplexData<Employees>("Employee");

            DataTable dtdashboard = new FilesDAL().GetDashboardStats(Convert.ToInt32(HttpContext.Session.GetString("ID")), emp.Department);
            return View(dtdashboard);
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
