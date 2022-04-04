using BLL;
using DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FCMS_RUDA.Controllers
{
    public class UsersController : Controller
    {
        private readonly ILogger<UsersController> _logger;

        public UsersController(ILogger<UsersController> logger)
        {
            _logger = logger;
        }
        public IActionResult Index()
        {
            try
            {
                if (HttpContext.Session.GetString("Name") == null)
                {
                    TempData["Session"] = "Your Session has expired. Please Login again";
                    return RedirectToAction("Logout", "Users");
                }

                List<Users> usersList = new UsersDAL().GetAllUsers();
                ViewBag.UserRole = Convert.ToInt32(HttpContext.Session.GetString("UserRole"));

                return View(usersList);
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View();
            }
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            try
            {
                Users objUser = new UsersDAL().SignIn(email, password);

                if (objUser != null && objUser.UserStatus == 1)
                {
                    SetSession(objUser);
                    return RedirectToAction("Index", "Home");
                }
                else if (objUser != null && objUser.UserStatus == 0)
                {
                    ViewBag.Message = "Your account is disabled!";
                    return View();
                }
                else
                {
                    ViewBag.Message = "Invalid email or password. Try again!";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Invalid email or password. Try again!";
                return View();
            }
        }

        public IActionResult AddNewUser()
        {
            if (HttpContext.Session.GetString("Name") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }

            List<Departments> listDept = new UsersDAL().GetAllDepartments();
            ViewBag.ListDept = listDept;

            List<UserRoles> listRoles = new UsersDAL().GetAllUserRoles();
            ViewBag.ListRoles = listRoles;

            return View();
        }

        [HttpPost]
        public IActionResult AddNewUser(Users user)
        {
            List<Departments> listDept = new UsersDAL().GetAllDepartments();
            ViewBag.ListDept = listDept;

            List<UserRoles> listRoles = new UsersDAL().GetAllUserRoles();
            ViewBag.ListRoles = listRoles;

            try
            {
                if (!ModelState.IsValid)
                {
                    return View();
                }

                int UserID = Convert.ToInt32(HttpContext.Session.GetString("ID"));
                Int64 result = new UsersDAL().CreateNewUser(user, UserID);

                if (result > 0)
                {
                    TempData["Success"] = "User Created Successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Message = "Error Occured while saving Record. Please Try Again!";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                _logger.LogError(ex.Message);
                return View();
            }
        }

        public IActionResult UpdateUser(int UserID)
        {
            if (HttpContext.Session.GetString("Name") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }
            Users user = new UsersDAL().GetUserByID(UserID);

            List<Departments> listDept = new UsersDAL().GetAllDepartments();
            ViewBag.ListDept = listDept;

            List<UserRoles> listRoles = new UsersDAL().GetAllUserRoles();
            ViewBag.ListRoles = listRoles;

            return View(user);
        }

        [HttpPost]
        public IActionResult UpdateUser(Users user)
        {
            List<Departments> listDept = new UsersDAL().GetAllDepartments();
            ViewBag.ListDept = listDept;

            List<UserRoles> listRoles = new UsersDAL().GetAllUserRoles();
            ViewBag.ListRoles = listRoles;

            try
            {
                if (!ModelState.IsValid)
                {
                    return View();
                }

                int UserID = Convert.ToInt32(HttpContext.Session.GetString("ID"));
                Int64 result = new UsersDAL().UpdateUser(user, UserID);

                if (result == 0)
                {
                    TempData["Success"] = "User Updated Successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Message = "Error Occured while saving Record. Please Try Again!";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                _logger.LogError(ex.Message);
                return View();
            }

        }

        public IActionResult UserRoles()
        {
            if (HttpContext.Session.GetString("Name") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }

            List<UserRoles> roles = new UsersDAL().GetAllUserRoles();

            return View(roles);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public void SetSession(Users user)
        {
            HttpContext.Session.SetString("ID", user.ID.ToString());
            HttpContext.Session.SetString("Email", user.Email.ToString());
            HttpContext.Session.SetString("password", user.Password.ToString());
            HttpContext.Session.SetString("UserStatus", user.UserStatus.ToString());
            HttpContext.Session.SetString("UserRole", user.UserRole.ToString());
            HttpContext.Session.SetString("Name", user.Name.ToString());
            HttpContext.Session.SetString("Designation", user.Designation.ToString());
            HttpContext.Session.SetString("Department", user.Department.ToString());
        }

        public Users GetSession()
        {
            Users user = new Users();

            user.ID = Convert.ToInt32(HttpContext.Session.GetString("ID"));
            user.Email = HttpContext.Session.GetString("Email").ToString();
            user.Password = HttpContext.Session.GetString("password").ToString();
            user.UserStatus = Convert.ToInt32(HttpContext.Session.GetString("UserStatus"));
            user.UserRole = Convert.ToInt32(HttpContext.Session.GetString("UserRole"));
            user.Name = HttpContext.Session.GetString("Name").ToString();
            user.Designation = HttpContext.Session.GetString("Designation").ToString();
            user.Department = Convert.ToInt32(HttpContext.Session.GetString("Department"));

            return user;
        }
    }
}
