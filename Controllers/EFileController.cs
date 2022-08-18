using com.ruda.Efile.Domain;
using DAL;
using FCMS_RUDA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace FCMS_RUDA.Controllers
{
    public class EFileController : Controller
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("ID") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }

            DataTable dt = new EFileDAL().GetMyFiles(Convert.ToInt32(HttpContext.Session.GetString("ID")));

            return View(dt);
        }

        public IActionResult FileRequests()
        {
            if (HttpContext.Session.GetString("ID") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }

            DataTable dt = new EFileDAL().GetPendingFileRequests(Convert.ToInt32(HttpContext.Session.GetString("ID")));
            return View(dt);
        }

        public IActionResult AddNewFile()
        {
            if (HttpContext.Session.GetString("ID") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }
            Employees emp = HttpContext.Session.GetComplexData<Employees>("Employee");

            ViewBag.MarkedToDept = Convert.ToInt32(emp.Department);

            List<Department> lstdept = HttpContext.Session.GetComplexData<List<Department>>("Departments");
            ViewBag.ListDepts = lstdept;

            List<Users> listUsers = new UsersDAL().GetUsersForNewFileMark(Convert.ToInt32(HttpContext.Session.GetString("ID")), emp.Department);
            ViewBag.ListUsers = listUsers;

            List<DocumentPriority> listPriority = new FilesDAL().GetAllDocumentPriorities();
            ViewBag.ListPriority = listPriority;

            List<DocumentType> listtypes = new FilesDAL().GetAllDocumentTypes();
            ViewBag.ListTypes = listtypes;

            List<DocumentStatus> listStatuses = new FilesDAL().GetAllDocumentStatus();
            ViewBag.ListStatus = listStatuses;

            return View();
        }

        [HttpPost]
        public IActionResult AddNewFile(EFileViewModel viewModel)
        {
            List<Users> listUsers = new UsersDAL().GetUsersForNewFileMark(Convert.ToInt32(HttpContext.Session.GetString("Department")), Convert.ToInt32(HttpContext.Session.GetString("ID")));
            ViewBag.ListUsers = listUsers;

            List<DocumentPriority> listPriority = new FilesDAL().GetAllDocumentPriorities();
            ViewBag.ListPriority = listPriority;

            List<DocumentType> listtypes = new FilesDAL().GetAllDocumentTypes();
            ViewBag.ListTypes = listtypes;

            List<DocumentStatus> listStatuses = new FilesDAL().GetAllDocumentStatus();
            ViewBag.ListStatus = listStatuses;

            int UserID = Convert.ToInt32(HttpContext.Session.GetString("ID"));
            try
            {
                if (!ModelState.IsValid)
                {
                    return View();
                }

                string WingInitials = HttpContext.Session.GetString("WingInitials").ToString();
                string DeptInitials = HttpContext.Session.GetString("DeptInitials").ToString();

                int maxnumber = new EFileDAL().GetMaxDocNumber(Convert.ToInt32(HttpContext.Session.GetString("Department")));
                viewModel.File.FileNumber = "RUDA-" + WingInitials + "-" + DeptInitials + "-" + DateTime.Now.Year + "-" + maxnumber;

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

                viewModel.File.InitiatedBy = UserID;
                viewModel.File.FileStatus = 1;
                viewModel.FileDetails.MarkedDate = DateTime.Now.Date;
                viewModel.FileDetails.Comments = string.Empty;

                Int64 result = new EFileDAL().CreateFile(viewModel);

                if (result > 0)
                {
                    TempData["Success"] = "File Created successfully!";
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
                ViewBag.Message = "Error Occured while saving Record. Please Try Again!";
                return View();
            }
        }

        public IActionResult ViewFile(int FileID)
        {
            if (HttpContext.Session.GetString("ID") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }

            EFile file = new EFileDAL().GetFilebyIDforView(FileID);

            List<DocumentPriority> listPriority = new FilesDAL().GetAllDocumentPriorities();
            ViewBag.ListPriority = listPriority;

            List<DocumentType> listtypes = new FilesDAL().GetAllDocumentTypes();
            ViewBag.ListTypes = listtypes;

            List<DocumentStatus> listStatuses = new FilesDAL().GetAllDocumentStatus();
            ViewBag.ListStatus = listStatuses;

            return View(file);
        }

        public IActionResult UpdateFile(int FileID)
        {
            if (HttpContext.Session.GetString("ID") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }

            EFileViewModel viewModel = new EFileDAL().GetFilebyIDforUpdate(FileID);

            List<DocumentPriority> listPriority = new FilesDAL().GetAllDocumentPriorities();
            ViewBag.ListPriority = listPriority;

            List<Department> lstdept = new UsersDAL().GetAllDepartments();
            ViewBag.ListDepts = lstdept;

            List<Users> listUsers = null;// new UsersDAL().GetUsersForNewFileMark(Convert.ToInt32(HttpContext.Session.GetString("Department")), Convert.ToInt32(HttpContext.Session.GetString("ID")));
            ViewBag.ListUsers = listUsers;

            List<DocumentType> listtypes = new FilesDAL().GetAllDocumentTypes();
            ViewBag.ListTypes = listtypes;

            List<DocumentStatus> listStatuses = new FilesDAL().GetAllDocumentStatus();
            ViewBag.ListStatus = listStatuses;

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult UpdateFile(EFileViewModel viewModel)
        {
            List<DocumentPriority> listPriority = new FilesDAL().GetAllDocumentPriorities();
            ViewBag.ListPriority = listPriority;

            List<Users> listUsers = new UsersDAL().GetUsersForNewFileMark(Convert.ToInt32(HttpContext.Session.GetString("Department")), Convert.ToInt32(HttpContext.Session.GetString("ID")));
            ViewBag.ListUsers = listUsers;

            List<DocumentType> listtypes = new FilesDAL().GetAllDocumentTypes();
            ViewBag.ListTypes = listtypes;

            List<DocumentStatus> listStatuses = new FilesDAL().GetAllDocumentStatus();
            ViewBag.ListStatus = listStatuses;

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

                Int64 result = new EFileDAL().UpdateFile(viewModel, UserID);

                if (result > 0)
                {
                    TempData["Success"] = "File Updated successfully!";
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
                ViewBag.Message = "Error Occured while saving Record. Please Try Again!";
                return View();
            }
        }

        #region SFTP Functions

        public string UploadFile(IFormFile Attachment, string DocNumber, string attachmentName)
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.Development.json").Build();

            SftpConfig ftp = new SftpConfig
            {
                Host = config["SftpSetting:Host"],
                Port = Convert.ToInt32(config["SftpSetting:Port"]),
                UserName = config["SftpSetting:UserName"],
                Password = config["SftpSetting:Password"],
                BaseDirectory = config["SftpSetting:BaseDirectory"],
            };

            using var client = new SftpClient(ftp.Host, ftp.Port, ftp.UserName, ftp.Password);

            try
            {
                client.Connect();
                client.ChangeDirectory(ftp.BaseDirectory);

                string FolderPath = ftp.BaseDirectory + "//" + DocNumber;
                string AttachmentPath = FolderPath + "//" + attachmentName;

                if (!client.Exists(FolderPath))
                    client.CreateDirectory(FolderPath);

                using (var filestream = Attachment.OpenReadStream())
                {
                    client.UploadFile(filestream, AttachmentPath);
                }

                return "//" + ftp.Host + AttachmentPath;
            }
            catch (Exception) { return null; }
            finally { client.Disconnect(); }
        }

        public IActionResult DownloadFile(string remoteFilename, string remoteFilePath)
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.Development.json").Build();

            SftpConfig ftp = new SftpConfig
            {
                Host = config["SftpSetting:Host"],
                Port = Convert.ToInt32(config["SftpSetting:Port"]),
                UserName = config["SftpSetting:UserName"],
                Password = config["SftpSetting:Password"],
                BaseDirectory = config["SftpSetting:BaseDirectory"],
            };

            using (SftpClient client = new SftpClient(ftp.Host, ftp.Port == 0 ? 22 : ftp.Port, ftp.UserName, ftp.Password))
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
