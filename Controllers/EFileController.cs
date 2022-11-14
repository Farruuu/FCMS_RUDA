using com.ruda.Efile.BusinessLogic;
using com.ruda.Efile.Domain;
using DAL;
using FCMS_RUDA.Models;
using FCMS_RUDA.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FCMS_RUDA.Controllers
{
    public class EFileController : Controller
    {
        private readonly SftpConfig sftpConfig;
        private readonly WebAPI webAPI;

        public EFileController(IOptions<SftpConfig> _SftpConfig, IOptions<WebAPI> _webAPI)
        {
            this.sftpConfig = _SftpConfig.Value;
            this.webAPI = _webAPI.Value;
        }


        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("ID") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }
            Employees emp = HttpContext.Session.GetComplexData<Employees>("Employee");

            DataTable dt = new EFileDAL().GetMyFiles(emp.EmpID);

            //dt.Columns.Add("DesignationName", typeof(string));
            dt.Columns.Add("DepartmentName", typeof(string));
            dt.Columns.Add("DeptInitials", typeof(string));

            foreach (DataRow row in dt.Rows)
            {
                Department dept = HttpContext.Session.GetComplexData<List<Department>>("Departments").ToList().First(x => x.ID == Convert.ToInt32(row["MarkedTodept"]));
                //Designation design = HttpContext.Session.GetComplexData<List<Designation>>("Designations").ToList().First(x => x.ID == Convert.ToInt32(row["MarkedToDesig"]));
                //row["DesignationName"] = row["MarkedToDesig"].ToString();
                row["DepartmentName"] = dept.Name;
                row["DeptInitials"] = dept.Initials;
            }

            return View(dt);
        }

        public IActionResult MyDeptFiles()
        {
            if (HttpContext.Session.GetString("ID") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }

            Employees emp = HttpContext.Session.GetComplexData<Employees>("Employee");
            DataTable dt = new EFileDAL().GetMyDeptFiles(emp.Department);

            //dt.Columns.Add("DesignationName", typeof(string));
            dt.Columns.Add("DepartmentName", typeof(string));
            dt.Columns.Add("DeptInitials", typeof(string));

            foreach (DataRow row in dt.Rows)
            {
                Department dept = HttpContext.Session.GetComplexData<List<Department>>("Departments").ToList().First(x => x.ID == Convert.ToInt32(row["MarkedTodept"]));
                //Designation design = HttpContext.Session.GetComplexData<List<Designation>>("Designations").ToList().First(x => x.ID == Convert.ToInt32(row["MarkedToDesig"]));
                //row["DesignationName"] = design.Title;
                row["DepartmentName"] = dept.Name;
                row["DeptInitials"] = dept.Initials;
            }

            return View(dt);
        }

        public IActionResult FilesClosed()
        {
            if (HttpContext.Session.GetString("ID") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }
            Employees emp = HttpContext.Session.GetComplexData<Employees>("Employee");
            DataTable dt = new EFileDAL().GetMyClosedFiles(emp.EmpID);

            //dt.Columns.Add("DesignationName", typeof(string));
            dt.Columns.Add("DepartmentName", typeof(string));
            dt.Columns.Add("DeptInitials", typeof(string));

            foreach (DataRow row in dt.Rows)
            {
                Department dept = HttpContext.Session.GetComplexData<List<Department>>("Departments").ToList().First(x => x.ID == Convert.ToInt32(row["MarkedTodept"]));
                //Designation design = HttpContext.Session.GetComplexData<List<Designation>>("Designations").ToList().First(x => x.ID == Convert.ToInt32(row["MarkedToDesig"]));
                //row["DesignationName"] = design.Title;
                row["DepartmentName"] = dept.Name;
                row["DeptInitials"] = dept.Initials;
            }

            return View(dt);
        }

        public IActionResult PendingFiles()
        {
            if (HttpContext.Session.GetString("ID") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }
            Employees emp = HttpContext.Session.GetComplexData<Employees>("Employee");
            DataTable dt = new EFileDAL().GetMyPendingFiles(emp.EmpID);

            //dt.Columns.Add("DesignationName", typeof(string));
            dt.Columns.Add("DepartmentName", typeof(string));
            dt.Columns.Add("DeptInitials", typeof(string));

            foreach (DataRow row in dt.Rows)
            {
                Department dept = HttpContext.Session.GetComplexData<List<Department>>("Departments").ToList().First(x => x.ID == Convert.ToInt32(row["MarkedTodept"]));
                //Designation design = HttpContext.Session.GetComplexData<List<Designation>>("Designations").ToList().First(x => x.ID == Convert.ToInt32(row["MarkedToDesig"]));
                //row["DesignationName"] = design.Title;
                row["DepartmentName"] = dept.Name;
                row["DeptInitials"] = dept.Initials;
            }

            return View(dt);
        }

        public IActionResult FileRequests()
        {
            if (HttpContext.Session.GetString("ID") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }
            Employees emp = HttpContext.Session.GetComplexData<Employees>("Employee");
            DataTable dt = new EFileDAL().GetPendingFileRequests(emp.EmpID);
            return View(dt);
        }

        public IActionResult ViewFile(int FileID)
        {
            if (HttpContext.Session.GetString("ID") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }

            List<Department> Depts = HttpContext.Session.GetComplexData<List<Department>>("Departments");
            //List<Designation> Designs = HttpContext.Session.GetComplexData<List<Designation>>("Designations");

            EFile file = new EFileDAL().GetFilebyIDforView(FileID, Depts);

            List<DocumentPriority> listPriority = new FilesDAL().GetAllDocumentPriorities();
            ViewBag.ListPriority = listPriority;

            List<DocumentType> listtypes = new FilesDAL().GetAllDocumentTypes();
            ViewBag.ListTypes = listtypes;

            List<DocumentStatus> listStatuses = new FilesDAL().GetAllDocumentStatus();
            ViewBag.ListStatus = listStatuses;

            return View(file);
        }

        public IActionResult AddNewFile()
        {
            if (HttpContext.Session.GetString("ID") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }

            List<Department> lstdept = HttpContext.Session.GetComplexData<List<Department>>("Departments");
            ViewBag.ListDepts = lstdept;

            List<DocumentPriority> listPriority = new FilesDAL().GetAllDocumentPriorities();
            ViewBag.ListPriority = listPriority;

            List<DocumentType> listtypes = new FilesDAL().GetAllDocumentTypes();
            ViewBag.ListTypes = listtypes;

            List<DocumentStatus> listStatuses = new FilesDAL().GetAllDocumentStatus();
            ViewBag.ListStatus = listStatuses;

            Employees emp = HttpContext.Session.GetComplexData<Employees>("Employee");
            ViewBag.MarkedToDept = Convert.ToInt32(emp.Department);

            List<Employees> listEmp = new UsersBLL().GetEmployeesForNewFileMark(emp.Department, emp.Grade, webAPI);
            ViewBag.ListEmp = listEmp;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddNewFile(EFileViewModel viewModel)
        {
            List<DocumentPriority> listPriority = new FilesDAL().GetAllDocumentPriorities();
            ViewBag.ListPriority = listPriority;

            List<DocumentType> listtypes = new FilesDAL().GetAllDocumentTypes();
            ViewBag.ListTypes = listtypes;

            List<DocumentStatus> listStatuses = new FilesDAL().GetAllDocumentStatus();
            ViewBag.ListStatus = listStatuses;

            Employees emp = HttpContext.Session.GetComplexData<Employees>("Employee");

            List<Employees> listUsers = new UsersBLL().GetEmployeesForNewFileMark(emp.Department, emp.Grade, webAPI);
            ViewBag.ListUsers = listUsers;

            int UserID = Convert.ToInt32(HttpContext.Session.GetString("ID"));

            try
            {
                if (!ModelState.IsValid)
                {
                    return View();
                }

                int maxnumber = new EFileDAL().GetMaxDocNumber(Convert.ToInt32(HttpContext.Session.GetString("Department")));
                viewModel.File.FileNumber = "RUDA-" + emp.WingInitials + "-" + emp.DeptInitials + "-" + DateTime.Now.Year + "-" + maxnumber;

                if (viewModel != null && viewModel.FileAttachments.FileAttachment != null && viewModel.FileAttachments.FileAttachment.Length > 0)
                {
                    viewModel.FileAttachments.AttachmentName = viewModel.File.FileNumber + "_" + viewModel.FileAttachments.AttachmentName + Path.GetExtension(viewModel.FileAttachments.FileAttachment.FileName);
                    viewModel.FileAttachments.AttachmentPath = UploadFile(viewModel.FileAttachments.FileAttachment, viewModel.File.FileNumber, viewModel.FileAttachments.AttachmentName);
                }
                else
                {
                    viewModel.FileAttachments.AttachmentName = string.Empty;
                    viewModel.FileAttachments.AttachmentPath = string.Empty;
                }

                viewModel.File.InitiatedBy = emp.EmpID;
                viewModel.File.FileStatus = 1;
                viewModel.FileDetails.MarkedDate = DateTime.Now.Date;
                viewModel.FileDetails.Comments = string.Empty;

                Int64 result = new EFileDAL().CreateFile(viewModel);

                if (result > 0)
                {
                    TempData["Success"] = "File Created successfully!";

                    try
                    {
                        string FileViewURL = $"{this.Request.Scheme}://{Request.Host}{Request.PathBase}/EFile/ViewFile?FileID=" + viewModel.File.ID.ToString() + "&ApproverID=" + viewModel.FileDetails.MarkedTo.ToString();

                        EmailRequest ApproverMail = new EmailRequest()
                        {
                            DisplayName = listUsers.Find(x => x.EmpID == viewModel.FileDetails.MarkedTo).Name.ToString(),
                            ToEmail = listUsers.Find(x => x.EmpID == viewModel.FileDetails.MarkedTo).EmailID.ToString(),
                            Subject = "Leave Application",
                            Body = @"<b>Dear " + listUsers.Find(x => x.EmpID == viewModel.FileDetails.MarkedTo).Name.ToString() + ",</b> <br>" +
                                    "<br>" +
                                    "You have an Efile submitted/Marked for Approval/Further Processing by " + HttpContext.Session.GetString("EmpName") + ".<br>" +
                                    "<br>" +
                                    "<br>" +
                                    "Please Click on below URL to Login to EFiling System and View File.<br>" +
                                    "<br>" +
                                     FileViewURL + " <br>" +
                                    "<br>" +
                                    "<b>Kind Regards</b><br>" +
                                    "<b>HR Employee Portal - RUDA</b>"
                        };

                        await new Notifications().SendEmail(ApproverMail);
                    }
                    catch (Exception ex) { }
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
                ViewBag.Message = "Error Occured while saving Record \n " + ex.Message + ".\n Please Try Again!";
                return View();
            }
        }

        public IActionResult UpdateFile(int FileID, int ApproverID = 0)
        {
            if (HttpContext.Session.GetString("ID") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";

                if (ApproverID != 0)
                {
                    TempData["ApproverID"] = ApproverID;
                    TempData["RedirectURL"] = $"~/Efile/UpdateFile?FileID=" + FileID;
                }

                return RedirectToAction("Logout", "Users");
            }

            EFileViewModel viewModel = new EFileDAL().GetFilebyIDforUpdate(FileID);

            List<DocumentPriority> listPriority = new FilesDAL().GetAllDocumentPriorities();
            ViewBag.ListPriority = listPriority;

            List<Department> lstdept = new UsersDAL().GetAllDepartments();
            ViewBag.ListDepts = lstdept;

            List<DocumentType> listtypes = new FilesDAL().GetAllDocumentTypes();
            ViewBag.ListTypes = listtypes;

            List<DocumentStatus> listStatuses = new FilesDAL().GetAllDocumentStatus();
            ViewBag.ListStatus = listStatuses;

            Employees emp = HttpContext.Session.GetComplexData<Employees>("Employee");

            List<Employees> listEmp = new UsersBLL().GetEmployeesForNewFileMark(emp.Department, emp.Grade, webAPI);
            ViewBag.ListEmp = listEmp;

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateFile(EFileViewModel viewModel)
        {
            List<DocumentPriority> listPriority = new FilesDAL().GetAllDocumentPriorities();
            ViewBag.ListPriority = listPriority;

            List<DocumentType> listtypes = new FilesDAL().GetAllDocumentTypes();
            ViewBag.ListTypes = listtypes;

            List<DocumentStatus> listStatuses = new FilesDAL().GetAllDocumentStatus();
            ViewBag.ListStatus = listStatuses;

            Employees emp = HttpContext.Session.GetComplexData<Employees>("Employee");

            List<Employees> listEmp = new UsersBLL().GetEmployeesForNewFileMark(emp.Department, emp.Grade, webAPI);
            ViewBag.listEmp = listEmp;

            int UserID = Convert.ToInt32(HttpContext.Session.GetString("ID"));

            try
            {
                if (!ModelState.IsValid)
                {
                    return View();
                }

                if (viewModel != null && viewModel.FileAttachments.FileAttachment != null && viewModel.FileAttachments.FileAttachment.Length > 0)
                {
                    viewModel.FileAttachments.AttachmentName = viewModel.File.FileNumber + "_" + viewModel.FileAttachments.AttachmentName + Path.GetExtension(viewModel.FileAttachments.FileAttachment.FileName);
                    viewModel.FileAttachments.AttachmentPath = UploadFile(viewModel.FileAttachments.FileAttachment, viewModel.File.FileNumber, viewModel.FileAttachments.AttachmentName);
                }
                else
                {
                    viewModel.FileAttachments.AttachmentName = string.Empty;
                    viewModel.FileAttachments.AttachmentPath = string.Empty;
                }

                viewModel.FileDetails.MarkedDate = DateTime.Now.Date;
                viewModel.FileDetails.Comments = string.Empty;

                Int64 result = new EFileDAL().UpdateFile(viewModel, emp.EmpID);

                if (result > 0)
                {
                    TempData["Success"] = "File Updated successfully!";

                    try
                    {
                        string FileViewURL = $"{this.Request.Scheme}://{Request.Host}{Request.PathBase}/EFile/ViewFile?FileID=" + viewModel.File.ID.ToString() + "&ApproverID=" + viewModel.FileDetails.MarkedTo.ToString();

                        EmailRequest ApproverMail = new EmailRequest()
                        {
                            DisplayName = listEmp.Find(x => x.EmpID == viewModel.FileDetails.MarkedTo).Name.ToString(),
                            ToEmail = listEmp.Find(x => x.EmpID == viewModel.FileDetails.MarkedTo).EmailID.ToString(),
                            Subject = "Leave Application",
                            Body = @"<b>Dear " + listEmp.Find(x => x.EmpID == viewModel.FileDetails.MarkedTo).Name.ToString() + ",</b> <br>" +
                                    "<br>" +
                                    "You have an Efile submitted/Marked for Approval/Further Processing by " + HttpContext.Session.GetString("EmpName") + ".<br>" +
                                    "<br>" +
                                    "<br>" +
                                    "Please Click on below URL to Login to EFiling System and View File.<br>" +
                                    "<br>" +
                                    FileViewURL + " <br>" +
                                    "<br>" +
                                    "<b>Kind Regards</b><br>" +
                                    "<b>HR Employee Portal - RUDA</b>"
                        };

                        await new Notifications().SendEmail(ApproverMail);
                    }
                    catch (Exception ex) { }
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
                ViewBag.Message = "Error Occured while saving Record \n " + ex.Message + ".\n Please Try Again!";
                return View();
            }
        }

        #region SFTP Functions

        public string UploadFile(IFormFile Attachment, string DocNumber, string attachmentName)
        {
            using var client = new SftpClient(sftpConfig.Host, sftpConfig.Port, sftpConfig.UserName, sftpConfig.Password);

            try
            {
                client.Connect();
                client.ChangeDirectory(sftpConfig.BaseDirectory);

                string FolderPath = sftpConfig.BaseDirectory + "//" + DocNumber;
                string AttachmentPath = FolderPath + "//" + attachmentName;

                if (!client.Exists(FolderPath))
                    client.CreateDirectory(FolderPath);

                using (var filestream = Attachment.OpenReadStream())
                {
                    client.UploadFile(filestream, AttachmentPath);
                }

                return "//" + sftpConfig.Host + AttachmentPath;
            }
            catch (Exception) { return null; }
            finally { client.Disconnect(); }
        }

        public IActionResult DownloadFile(string remoteFilename, string remoteFilePath)
        {
            using (SftpClient client = new SftpClient(sftpConfig.Host, sftpConfig.Port == 0 ? 22 : sftpConfig.Port, sftpConfig.UserName, sftpConfig.Password))
            {
                string desktoppath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                try
                {
                    client.Connect();

                    string RemoteFilePath = remoteFilePath.Replace("//172.16.0.6", ""); //ftp.BaseDirectory + "//" + remoteFilename;
                    byte[] fileBytes = client.ReadAllBytes(RemoteFilePath);
                    return File(fileBytes, "application/force-download", remoteFilename);
                }
                catch (Exception ex) { return Json("Error Downloading file." + ex.Message + "\n" + desktoppath + " Please Try again!"); }
                finally { client.Disconnect(); }
            }
        }

        #endregion
    }
}
