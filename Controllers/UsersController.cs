using com.ruda.Efile.BusinessLogic;
using com.ruda.Efile.Domain;
using DAL;
using FCMS_RUDA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FCMS_RUDA.Controllers
{
    public class UsersController : Controller
    {
        private readonly WebAPI webAPI;
        public UsersController(IOptions<WebAPI> _webAPI)
        {
            this.webAPI = _webAPI.Value;
        }

        public IActionResult Index()
        {
            try
            {
                if (HttpContext.Session.GetString("ID") == null)
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
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    ViewBag.Message = "Email / Password missing!";
                    return View();
                }

                Users objUser = new UsersDAL().SignIn(email, password);
                if (objUser != null && objUser.UserStatus == 1)
                {

                    Employees EmplDetails = await new UsersBLL().GetEmployeeDetails(email, webAPI);

                    if (EmplDetails != null & EmplDetails.EmpStatus != 6)
                    {
                        List<UserPermissions> ObjPermissions = new UsersDAL().GetUserPermissions(objUser.UserRole);

                        if (Convert.ToInt32(HttpContext.Session.GetString("ApproverID")) != 0 && Convert.ToInt32(HttpContext.Session.GetString("ApproverID")) == objUser.EmpID)
                        {
                            string RedirectURL = HttpContext.Session.GetString("RedirectURL");

                            HttpContext.Session.Clear();
                            SetSession(objUser, ObjPermissions, EmplDetails);
                            return Redirect(RedirectURL);
                        }

                        SetSession(objUser, ObjPermissions, EmplDetails);
                        return RedirectToAction("Index", "Home");
                    }
                    else if (objUser != null && objUser.UserStatus == 0)
                    {
                        ViewBag.Message = "Your account is disabled as per your employement Status. Please Contact HR Department!";
                        return View();
                    }
                    else
                    {
                        ViewBag.Message = "Invalid email or password. Try again!";
                        return View();
                    }
                }
                else
                {
                    ViewBag.Message = "Your account is disabled!";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Error Occured while Processing Your request.\n " + ex.Message + ".\n Please Try again!";
                return View();
            }
        }

        public IActionResult AddNewUser()
        {
            if (HttpContext.Session.GetString("ID") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }

            List<UserRoles> listRoles = new UsersDAL().GetAllUserRoles();
            ViewBag.ListRoles = listRoles;

            return View();
        }

        [HttpPost]
        public async Task<JsonResult> LoadEmployeeDetails(string EmailID)
        {
            Employees EmplDetails = await new UsersBLL().GetEmployeeDetails(EmailID, webAPI);
            var data = JsonConvert.SerializeObject(EmplDetails);
            return Json(data);
        }

        [HttpPost]
        public IActionResult AddNewUser(Users user)
        {
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
                return View();
            }
        }

        public IActionResult UpdateUser(int UserID)
        {
            if (HttpContext.Session.GetString("ID") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }

            Users user = new UsersDAL().GetUserByID(UserID);

            List<UserRoles> listRoles = new UsersDAL().GetAllUserRoles();
            ViewBag.ListRoles = listRoles;

            Department dept = HttpContext.Session.GetComplexData<List<Department>>("Departments").ToList().First(x => x.ID == user.Department);
            ViewBag.Department = dept.Name + " (" + dept.Initials + ")";

            return View(user);
        }

        [HttpPost]
        public IActionResult UpdateUser(Users user)
        {
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
                return View();
            }

        }

        public IActionResult UserRoles()
        {
            if (HttpContext.Session.GetString("ID") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }

            List<UserRoles> roles = new UsersDAL().GetAllUserRoles();

            return View(roles);
        }

        public IActionResult UserPermissions()
        {
            if (HttpContext.Session.GetString("ID") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }

            List<UserRoles> RolesList = new UsersDAL().GetAllUserRoles();
            ViewBag.RoleList = RolesList;

            return View();
        }

        [HttpPost]
        public JsonResult LoadPermissions(string RoleID)
        {
            List<UserPermissions> permissions = new UsersDAL().GetUserPermissions(Convert.ToInt32(RoleID));
            var data = JsonConvert.SerializeObject(permissions);
            return Json(data);
        }

        public IActionResult Logout()
        {
            int ApproverID = 0;
            string RedirectURL = string.Empty;

            if (TempData["ApproverID"] != null)
            {
                ApproverID = (int)TempData["ApproverID"];
                RedirectURL = TempData["RedirectURL"].ToString();
                TempData.Clear();
            }

            HttpContext.Session.Clear();

            HttpContext.Session.SetString("ApproverID", ApproverID.ToString());
            HttpContext.Session.SetString("RedirectURL", RedirectURL);

            return RedirectToAction("Login");
        }

        public IActionResult Departments()
        {
            if (HttpContext.Session.GetString("ID") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }

            List<Department> listDept = HttpContext.Session.GetComplexData<List<Department>>("Departments");
            ViewBag.UserRole = Convert.ToInt32(HttpContext.Session.GetString("UserRole"));

            return View(listDept);
        }

        public IActionResult Designations()
        {
            if (HttpContext.Session.GetString("ID") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }

            List<Designation> listDesig = HttpContext.Session.GetComplexData<List<Designation>>("Designations");
            ViewBag.UserRole = Convert.ToInt32(HttpContext.Session.GetString("UserRole"));

            return View(listDesig);
        }

        [HttpPost]
        public JsonResult LoadUsersForFileMarkByDept(string DeptID)
        {
            Employees emp = HttpContext.Session.GetComplexData<Employees>("Employee");
            List<Users> listUsers = new UsersDAL().GetUsersForNewFileMark(Convert.ToInt32(HttpContext.Session.GetString("ID")), emp.Department, emp.Grade);
            var data = JsonConvert.SerializeObject(listUsers);

            return Json(data);
        }

        #region Session Methods

        public void SetSession(Users user, List<UserPermissions> permissions, Employees employees)
        {
            try
            {
                HttpContext.Session.SetString("ID", user.UserID.ToString());
                HttpContext.Session.SetString("Email", user.Email.ToString());
                HttpContext.Session.SetString("password", user.Password.ToString());
                HttpContext.Session.SetString("Name", user.Name.ToString());
                HttpContext.Session.SetString("Designation", user.Designation.ToString());
                HttpContext.Session.SetString("Department", user.Department.ToString());
                HttpContext.Session.SetString("UserStatus", user.UserStatus.ToString());
                HttpContext.Session.SetString("UserRole", user.UserRole.ToString());
                HttpContext.Session.SetComplexData("Permissions", permissions);
                HttpContext.Session.SetComplexData("Employee", employees);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        public Users GetSession()
        {
            Users user = new Users();

            user.UserID = Convert.ToInt32(HttpContext.Session.GetString("ID"));
            user.Email = HttpContext.Session.GetString("Email").ToString();
            user.Password = HttpContext.Session.GetString("password").ToString();
            user.Name = HttpContext.Session.GetString("Name").ToString();
            user.Designation = Convert.ToInt32(HttpContext.Session.GetString("Designation"));
            user.Department = Convert.ToInt32(HttpContext.Session.GetString("Department"));
            user.UserStatus = Convert.ToInt32(HttpContext.Session.GetString("UserStatus"));
            user.UserRole = Convert.ToInt32(HttpContext.Session.GetString("UserRole"));
            return user;
        }

        public Employees GetSessionEmployee()
        {
            Employees emp = HttpContext.Session.GetComplexData<Employees>("Employee");
            return emp;
        }

        public List<UserPermissions> GetSessionPermissions()
        {
            List<UserPermissions> permissions = HttpContext.Session.GetComplexData<List<UserPermissions>>("Permissions");
            return permissions;
        }

        #endregion
    }
}
